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

namespace Tessa.Extensions.Chronos.Plugins
{
    /// <summary>
    /// Пример плагина, который может работать через серверное API.
    /// </summary>
    [Plugin(
        Name = "RB_ProcessDataFromEOS4SP",
        Description = "Плагин создание карточкек и загрузку данных из СЭД EOS4SP",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class RB_ProcessDataFromEOS4SP :
        Plugin
    {
        #region Constants

        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RB_ProcessDataFromEOS4SP.xml";

        //введено
        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Base Overrides

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("Starting plugin RB_ProcessDataFromEOS4SP");

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

            // создаём карточку входящего документа
            var typeID = Guid.Parse("9f9e8879-9b7f-4a55-8d22-82c6bdca66a8");
            var docTypes = await typesCache.GetDocTypesAsync();
            var docType = docTypes.FirstOrDefault(x => x.ID == typeID);

            var rootFolder = new DirectoryInfo(@"/home/tessa/tessa/sync/in");
            var metaFiles = rootFolder.GetFiles("*.txt"); // берем только .txt файлы

            // Обходим каждый метафайл и по каждому создаем карточку входящего документа
            foreach (var metaFile in metaFiles)
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

                // Построчно обходим метафайл
                var lines = System.IO.File.ReadAllLines(metaFile.FullName);

                #region Past Code
                /*  string s;
                  using (var f = new StreamReader(metaFile.FullName, Encoding.GetEncoding(1251)))
                  {
                      f.Read
                      while (!f.EndOfStream)
                      {
                          s = f.ReadLine();
                          // что-нибудь делаем с прочитанной строкой s
                      }
                  }*/
                #endregion

                card.Sections["DocumentCommonInfo"].Fields["Subject"] = lines[2].Substring(13, lines[2].Length - 13); //Title
                //card.Sections["DocumentCommonInfo"].Fields["OutgoingNumber"] = lines[4].Substring(13, lines[4].Length - 13); //RegNumber
                // добавлено 20_10
                card.Sections["DocumentCommonInfo"].Fields["ExternalGuid"] = Path.GetFileNameWithoutExtension(metaFile.FullName);//lines[7].Substring(13, lines[7].Length - 13); //UID
                //card.Sections["DocumentCommonInfo"].Fields["DocDate"] = DateTime.ParseExact(lines[3].Substring(13, lines[3].Length - 13), "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture); //RegDate

                // Обходим директорию с файлами, добавляем файлы в карточку
                var dir = new DirectoryInfo($@"/home/tessa/tessa/sync/in/Files/{Path.GetFileNameWithoutExtension(metaFile.FullName)}") ;

                await using (var fileContainer = await manager.CreateContainerAsync(card))
                {
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        await fileContainer
                        .FileContainer
                        .BuildFile(file.Name)
                        .SetContent(file.FullName) 
                        .AddWithNotificationAsync();

                        file.Delete();
                    }

                    var storeResponse = await fileContainer.StoreAsync();
                    if (!storeResponse.ValidationResult.IsSuccessful())
                    {
                        ValidationResult result = storeResponse.ValidationResult.Build();
                        logger.LogResult(result); // пишем ошибку или другие сообщения в лог
                    }
                }

                //Файл обработан - перенесем в архив
               // metaFile.MoveTo($@"/home/tessa/tessa/sync/archive/{Path.GetFileName(metaFile.FullName)}");
                metaFile.CopyTo($@"/home/tessa/tessa/sync/archive/{Path.GetFileName(metaFile.FullName)}");
                metaFile.Delete();
                //Папку с файлами - также перенесем в архив
                //dir.MoveTo($@"/home/tessa/tessa/sync/archive/Files/{Path.GetFileNameWithoutExtension(metaFile.FullName)}");
                //dir.CopyTo($@"/home/tessa/tessa/sync/archive/Files/{Path.GetFileNameWithoutExtension(metaFile.FullName)}");
                dir.Delete();

                // Создали карточку, далее - выдадим поручение по ней
                // создаём бизнес-процесс и задачу "Постановка задачи" с указанным ID = mainTaskRowID
                /* Guid mainTaskRowID = Guid.NewGuid();

                 var storeInfo = new Dictionary<string, object>(StringComparer.Ordinal);
                 storeInfo.SetStartingProcessName(WfHelper.ResolutionProcessName);
                 storeInfo.SetStartingProcessTaskRowID(mainTaskRowID);

                 //card.RemoveAllButChanged();
                 var storeRequest = await cardRepository.StoreAsync(new CardStoreRequest { Card = card, Info = storeInfo }, cancellationToken);
                 if (!storeRequest.ValidationResult.IsSuccessful())
                 {
                     ValidationResult result = storeRequest.ValidationResult.Build();
                     logger.LogResult(result);
                     return;
                 }*.

                 // завершаем "Постановку задачи" с вариантом "Отправить"
                /* getResponse = await cardRepository.GetAsync(new CardGetRequest { CardID = cardID }, cancellationToken);
                 if (!getResponse.ValidationResult.IsSuccessful())
                 {
                     // ошибка, надо залогировать и выйти
                     return;
                 }*/

                // card = getResponse.Card;

                /*      var task = card.Tasks.FirstOrDefault(x => x.RowID == mainTaskRowID);
                      if (task == null)
                      {
                          logger.Error("Задача не сформирована");
                          return;
                      }

                      // кому отправляем, любая роль, в т.ч. конкретный сотрудник
                      Guid roleID = Guid.Parse("3db19fa0-228a-497f-873a-0250bf0a4ccb");
                      string roleName = "Admin";

                      var performerRows = task.Card.Sections[WfHelper.ResolutionPerformersSection].Rows;
                      var performer = performerRows.Add();
                      performer.RowID = Guid.NewGuid();
                      performer[WfHelper.ResolutionPerformerOrderField] = Int32Boxes.Zero;
                      performer[WfHelper.ResolutionPerformerRoleIDField] = roleID;
                      performer[WfHelper.ResolutionPerformerRoleNameField] = roleName;
                      performer.State = CardRowState.Inserted;

                      // комментарий
                      string comment = lines[6].Substring(13, lines[6].Length - 13); ;
                      var resolutionFields = task.Card.Sections[WfHelper.ResolutionSection].Fields;
                      resolutionFields[WfHelper.ResolutionCommentField] = comment;

                      // планируемая дата завершения
                      var dueDate = DateTime.ParseExact(lines[7].Substring(13, lines[7].Length - 13), "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                      resolutionFields[WfHelper.ResolutionDurationInDaysField] = null;
                      resolutionFields[WfHelper.ResolutionPlannedField] = dueDate;

                      //вид задачи
                      //resolutionFields[WfHelper.ResolutionKindIDField] = new Guid("..."); // из справочника видов задач
                      //resolutionFields[WfHelper.ResolutionKindCaptionField] = "Тестовое задание";

                      // вариант завершения - отправить
                      task.OptionID = DefaultCompletionOptions.SendToPerformer;
                      task.Action = CardTaskAction.Complete;
                      task.State = CardRowState.Deleted;

                      //card.RemoveAllButChanged();
                      storeRequest = await cardRepository.StoreAsync(new CardStoreRequest { Card = card }, cancellationToken);
                      if (!storeRequest.ValidationResult.IsSuccessful())
                      {
                          ValidationResult result = storeRequest.ValidationResult.Build();
                          logger.LogResult(result);
                          return;
                      }*/

            }
            
            logger.Info("Shutting down ODProcessDataFromEOS4SP");
        }
        #endregion
    }
}
