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
using System.Runtime.CompilerServices;
using NLog.Fluent;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Tessa.Extensions.Chronos.Plugins
{
    /// <summary>
    /// Пример плагина, который может работать через серверное API.
    /// </summary>
    [Plugin(
        Name = "RBOG_ProcessDataOrganizationsExtNumDate",
        Description = "Плагин обновления карточек для добавление категорий (рубрик) обращения",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class RBOG_ProcessDataReviewResult :
        Plugin
    {
        #region Constants

        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RBOG_ProcessDataReviewResult.xml";

        //введено
        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Base Overrides

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("Starting plugin RBOG_ProcessDataReviewResult");

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
            ICardServerPermissionsProvider permissionsProvider = container.Resolve<ICardServerPermissionsProvider>();

            FileParseOGReviewResult fileParse = new FileParseOGReviewResult();

            // прописать путь
            //var dictCategories = fileParseCategories.GetCategoriesFromFile("/home/tessa/tessa_og/sync/testtt.txt");

            var listData = fileParse.GetData("/");

            foreach (var c in listData)
            {
                IDbScope dbScope = container.Resolve<IDbScope>();

                var cardId = await GetCardIDAsync(c.UID.ToLower(), dbScope);

                if (cardId == null)
                {
                    logger.Info("Card with UID: " + c.UID.ToLower() + " not exist");
                    continue;
                }

                var cardGetRequest = new CardGetRequest
                {
                    CardID = cardId
                };

                permissionsProvider.SetFullPermissions(cardGetRequest);
                var cardGetResponse = await cardRepository.GetAsync(cardGetRequest, cancellationToken);

                if (!cardGetResponse.ValidationResult.IsSuccessful())
                {
                    logger.Error(cardGetResponse.ValidationResult.Build());
                    return;
                }

                var card = cardGetResponse.Card;

                if (c.reviewResult == "NULL" || string.IsNullOrWhiteSpace(c.reviewResult))
                {
                    logger.Error("Результат рассмотрения не верный: " + c.reviewResult);
                }

                switch(c.reviewResult)
                {
                    case "0":
                        card.Sections["DocumentCommonInfo"].Fields["ReviewResultID"] = Guid.Parse("0e44fd1f-5b27-4a19-bdcf-6b633447054c");
                        card.Sections["DocumentCommonInfo"].Fields["ReviewResultName"] = "Не определено";
                        break;
                    case "1":
                        card.Sections["DocumentCommonInfo"].Fields["ReviewResultID"] = Guid.Parse("00e5ec91-bf76-4224-b85e-310f723378bb");
                        card.Sections["DocumentCommonInfo"].Fields["ReviewResultName"] = "Поддержано";
                        break;
                    case "3":
                        card.Sections["DocumentCommonInfo"].Fields["ReviewResultID"] = Guid.Parse("50ed2c41-7e69-47e0-aa0f-e6804a0ac247");
                        card.Sections["DocumentCommonInfo"].Fields["ReviewResultName"] = "Раъяснено";
                        break;
                    case "4":
                        card.Sections["DocumentCommonInfo"].Fields["ReviewResultID"] = Guid.Parse("bb6eba87-9a06-40f1-a3c0-06c9f3e87bc0");
                        card.Sections["DocumentCommonInfo"].Fields["ReviewResultName"] = "Не поддержано";
                        break;
                    default:
                        logger.Error("Номер результата рассмотрения неверный: ");
                        break;
                }

                //RBCategory TCategory = await GetCategoryAsync(c.category, dbScope);


                //if (TCategory == null)
                //{
                //    logger.Error("Рубрика " + c.category + " не найдена. Карточка: " + c.UID.ToLower() + " CatNotFound");
                //    continue;
                //}

                //await InsertSSTUInfRKRow((Guid)cardId, Guid.NewGuid(), TCategory, dbScope);

                /*  CardSection newOutgoingRefDocs = card.Sections["SSTUInfRK"];

                  CardRow row = newOutgoingRefDocs.Rows.Add();
                  row.RowID = Guid.NewGuid();
                  row["OTKGID"] = TCategory.ID;
                  row["OTKGKodRub"] = TCategory.KodRub;
                  row["OTKGName"] = TCategory.Name;
                  row.State = CardRowState.Inserted;*/

                /*    var storeRequest = await cardRepository.StoreAsync(new CardStoreRequest { Card = card }, cancellationToken);
                    if (!storeRequest.ValidationResult.IsSuccessful())
                    {
                        ValidationResult result = storeRequest.ValidationResult.Build();
                        logger.LogResult(result);
                        return;
                    }*/

                logger.Info("Card with external GUID " + c.UID + " updated successfully");
            }

            logger.Info("Shutting down RBOG_ProcessDataReviewResult");
        }

        private async Task<Guid?> GetCardIDAsync(string externalGuid, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("dci", "ID")
                            .From("DocumentCommonInfo", "dci").NoLock()
                            .Where().C("dci", "ExternalGuid").Equals().P("externalGuid")
                            .Limit(1).Build(),
                        db.Parameter("externalGuid", externalGuid))
                    .LogCommand()
                    .ExecuteAsync<Guid?>();

                return result;
            }
        }


        #endregion
    }
}
