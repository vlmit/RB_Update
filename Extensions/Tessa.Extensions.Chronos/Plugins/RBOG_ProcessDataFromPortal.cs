using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Chronos.Contracts;
using NLog;
using Tessa.Notices;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Validation;
using Unity;
using Tessa.Platform.Licensing;
using Tessa.Platform.Runtime;
using System.IO;
using Tessa.Cards;
using System.Text;
using Tessa.Extensions.Default.Shared;
using Tessa.Files;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using System.Linq;
using Tessa.Cards.Workflow;
using Tessa.Extensions.Default.Shared.Workflow.Wf;
using System.Globalization;
using Tessa.Extensions.Chronos.Helpers;
using System.Drawing.Imaging;
using Tessa.Roles;

namespace Tessa.Extensions.Chronos.Plugins
{
    /// <summary>
    /// Пример плагина, который может работать через серверное API.
    /// </summary>
    [Plugin(
        Name = "RBOG_ProcessDataFromPortal",
        Description = "Плагин создание карточкек и загрузку данных из СЭД EOS4SP",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class RBOG_ProcessDataFromPortal :
        Plugin
    {
        #region Constants

        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RBOG_ProcessDataFromPortal.xml";

        //введено
        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Base Overrides

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("Starting plugin RBOG_ProcessDataFromPortal");

            // конфигурируем контейнер Unity для использования стандартных серверных API (в т.ч. API карточек)
            // а также для получения прямого доступа к базе данных через IDbScope по строке подключения из app.config;
            // предполагаем, что все действия, совершаемые плагином, будут выполняться от имени пользователя System
            //logger.Trace("Configuring container");

            // настраиваем контейнер Unity для работы с карточками
            await TessaPlatform.InitializeFromConfigurationAsync();

            IUnityContainer container = await new UnityContainer()
                .RegisterServerForPluginAsync()
                ;

            ICardRepository cardRepository = container.Resolve<ICardRepository>();
            ICardFileManager manager = container.Resolve<ICardFileManager>();
            IKrTypesCache typesCache = container.Resolve<IKrTypesCache>();

            // создаём карточку письменного обращения граждан
            var typeID = Guid.Parse("47cc47f8-3bd5-40e4-8e8d-5c1a02286b12");
            var docTypes = await typesCache.GetDocTypesAsync();
            var docType = docTypes.FirstOrDefault(x => x.ID == typeID);

            FileParseFromPortal fp = new FileParseFromPortal();

            //Поменять путь
            var SPCards = fp.GetSPCardsFromDir(@"/home/tessa/tessa/portal");

            foreach (var c in SPCards)
            {
                var newRequest = new CardNewRequest();

                // проверяем используется ли тип документа в типовом решении

                if (docType != null)
                {
                    newRequest.CardTypeID = docType.CardTypeID;
                    newRequest.Info[KrConstants.Keys.DocTypeID] = typeID;
                    newRequest.Info[KrConstants.Keys.DocTypeTitle] = docType.Caption;
                }

                // для создания карточки из типового решения должны быть права создания для заданного типа у скрытого пользователя System
                CardNewResponse newResponse = await cardRepository.NewAsync(newRequest);

                ValidationResult newResult = newResponse.ValidationResult.Build();
                // логируем сообщения при создании, если они есть
                logger.LogResult(newResult);
                // если не удалось создать карточку - выходим
                if (!newResult.IsSuccessful)
                {
                    logger.Error("Не удалось создать карточку документа");
                    return;
                }

                // теперь у нас есть карточка card, в ней можно заполнить нужные поля и добавить файл
                Card card = newResponse.Card;
                var cardID = Guid.NewGuid();
                card.ID = cardID;

               // var subject = c["DocTitle"] + c["Annotation"];
                card.Sections["DocumentCommonInfo"].Fields["Subject"] = c["DocTitle"]; //Title
                card.Sections["DocumentCommonInfo"].Fields["ExternalGuid"] = c["UID"];
                card.Sections["DocumentCommonInfo"].Fields["Comment"] = c["Comments"];
                card.Sections["DocumentCommonInfo"].Fields["Notes"] = c["Annotation"];
                // опрделяем тип доставки
                if (c["DeliveryTypeId"] == "52")
                {
                    card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("cbd0c800-4acf-424f-8af0-357dfc470a05");
                    card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Региональный портал ОГВ(интернет-приемная)";
                }
                else if (c["DeliveryTypeId"] == "102")
                {
                    card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("3d13fce7-5b6a-472f-8ae7-db9cdb6eeb5c");
                    card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "ЕПГУ";
                }
                else if (c["DeliveryTypeId"] == "203")
                {
                    card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("0505d498-c5a5-40a9-bdaa-e5dfdc041f76");
                    card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "VipNet";
                }
                // указываем информацию по подразделению
                var department = new RBDepartmentRow();
                IDbScope dbScope = container.Resolve<IDbScope>();
                department = await GetDepartmentInfoAsync(c["DocGroup"], dbScope);
                if (department != null)
                {
                    card.Sections["DocumentCommonInfo"].Fields["DepartmentID"] = department.ID;
                    card.Sections["DocumentCommonInfo"].Fields["DepartmentName"] = department.Name;
                    card.Sections["DocumentCommonInfo"].Fields["DepartmentIndex"] = department.Index;
                }

                var storeRequest = await cardRepository.StoreAsync(new CardStoreRequest { Card = card }, cancellationToken);
                if (!storeRequest.ValidationResult.IsSuccessful())
                {
                    ValidationResult result = storeRequest.ValidationResult.Build();
                    logger.LogResult(result);
                    return;
                }

                // Обходим директорию с файлами, добавляем файлы в карточку
                var dir = new DirectoryInfo(@"/home/tessa/tessa/portal/Files/" + c["UID"]);

                await using (var fileContainer = await manager.CreateContainerAsync(card))
                {
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        await fileContainer
                        .FileContainer
                        .BuildFile(file.Name)
                        .SetContent(file.FullName)
                        .AddWithNotificationAsync();

                       //file.Delete();
                    }

                    var storeResponse = await fileContainer.StoreAsync();
                    if (!storeResponse.ValidationResult.IsSuccessful())
                    {
                        ValidationResult result = storeResponse.ValidationResult.Build();
                        logger.LogResult(result); // пишем ошибку или другие сообщения в лог
                    }
                }




                logger.Info($"Создана карточка по метаданным {c["UID"]}");

                string fileName = $@"/home/tessa/tessa/portal/{c["UID"]}.txt";

                if (System.IO.File.Exists(fileName))
                {
                    try
                    {
                        System.IO.File.Delete(fileName);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Не удалось удалить метаданные {c["UID"]}.txt");
                    }
                }

               // dir.Delete();

            }
            logger.Info($"Shutting down RBOG_ProcessDataFromPortal");
        }

        #endregion

        private async Task<RBDepartmentRow> GetDepartmentInfoAsync(string docGroup, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = new RBDepartmentRow();

                result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("ro", "ID", "Name", "Index")
                            .From("Roles", "ro").NoLock()
                            .Where().C("ro", "ExternalID").Equals().P("externalID")
                            .Limit(1).Build(),
                        db.Parameter("externalID", docGroup))
                    .LogCommand()
                    .ExecuteAsync<RBDepartmentRow>();

                return result;
            }
        }

        public class RBDepartmentRow
        {
            public Guid ID { get; set; }
            public string Name { get; set; }
            public string Index { get; set; }
        }

    }
}
