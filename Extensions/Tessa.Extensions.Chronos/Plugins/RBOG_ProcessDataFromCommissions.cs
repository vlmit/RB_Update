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


namespace Tessa.Extensions.Chronos.Plugins
{
    /// <summary>
    /// Пример плагина, который может работать через серверное API.
    /// </summary>
    [Plugin(
        Name = "RBOG_ProcessDataFromCommissions",
        Description = "Плагин создание карточкек и загрузку данных из СЭД EOS4SP",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class RBOG_ProcessDataFromCommissions :
        Plugin
    {
        #region Constants

        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RBOG_ProcessDataFromCommissions.xml";

        //введено
        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Base Overrides

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("Starting plugin RBOG_ProcessDataFromCommissions");


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
            // определяем вид карточки поручения

            var typeID = Guid.Parse("aa85fd72-fb7e-4e98-a097-bb1d3216f667");
            var docTypes = await typesCache.GetDocTypesAsync();
            var docType = docTypes.FirstOrDefault(x => x.ID == typeID);


            FileParseComissions fpComissions = new FileParseComissions();


            //Поменять путь
            var commissionsFolders = Directory.GetDirectories("/home/tessa/tessa/share/ExportFromEOS/Incoming/2022_12_05/Commissions");
           // logger.Info($"длина массива {commissionsFolders.Length}");
            foreach (var f in commissionsFolders) 
            {
               // logger.Info($"путь {f}");
                IDbScope dbScope = container.Resolve<IDbScope>();

                var fSplit = f.Split('/');
                string cardExtGuid = fSplit.Last();
               


                var comissions = fpComissions.GetSPCardsFromDir(f);
                // logger.Info($"кол-во поручений {comissions.Count}");
                foreach (var c in comissions) // создаем карточки по каждому поручению в папке
                {
                    logger.Info($"Обрабатывается карточка из папки" + cardExtGuid);
                    //if (!String.IsNullOrEmpty(c["DocUID"]) || !String.IsNullOrWhiteSpace(c["DocUID"]))
                    //{
                    var checkGUID = await GetExternalGuidAsync(cardExtGuid, c["CommissionId"], dbScope);
                    if (checkGUID != null)
                    {
                        logger.Info("Card with UID: " + cardExtGuid + c["CommissionId"] + " is already exist");
                        continue;
                    }
                    //}
                    //logger.Info($"пример параметра {c["DocUID"]}");
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
                        logger.Error("Не удалось создать карточку поручения");
                        return;
                    }

                    // теперь у нас есть карточка card, в ней можно заполнить нужные поля и добавить файл
                    Card card = newResponse.Card;
                    var cardID = Guid.NewGuid();
                    card.ID = cardID;

                    //if (!String.IsNullOrEmpty(c["DocUID"]) || !String.IsNullOrWhiteSpace(c["DocUID"]))
                    //{
                    card.Sections["DocumentCommonInfo"].Fields["ExternalGuidForTask"] = cardExtGuid;//c["DocUID"];
                    //}
                    if (!String.IsNullOrEmpty(c["CommissionTitle"]) || !String.IsNullOrWhiteSpace(c["CommissionTitle"]))
                    {
                        card.Sections["DocumentCommonInfo"].Fields["Subject"] = c["CommissionTitle"];
                    }
                    card.Sections["DocumentCommonInfo"].Fields["FullNumber"] = c["CommissionId"];

                    if (!String.IsNullOrEmpty(c["CommissionCreated"]) || !String.IsNullOrWhiteSpace(c["CommissionCreated"]))
                    {
                        card.Sections["DocumentCommonInfo"].Fields["CreationDate"] = DateTime.ParseExact(c["CommissionCreated"], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture); //RegDate
                    }

                    if (!String.IsNullOrEmpty(c["CommissionDueDate"]) || !String.IsNullOrWhiteSpace(c["CommissionDueDate"]))
                    {
                        card.Sections["DocumentCommonInfo"].Fields["TaskDeadline"] = DateTime.ParseExact(c["CommissionDueDate"], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture); //RegDate
                    }


                    var commissionAuthorID = await GetUserCreatedByAsync(c["CommissionAuthor"], dbScope);
                    if (commissionAuthorID != null)
                    {
                        card.Sections["DocumentCommonInfo"].Fields["AuthorName"] = c["CommissionAuthor"];
                        card.Sections["DocumentCommonInfo"].Fields["AuthorID"] = commissionAuthorID;
                    }
                    else
                    {
                        card.Sections["DocumentCommonInfo"].Fields["AuthorName"] = c["CommissionAuthor"];
                    }

                    if (!String.IsNullOrEmpty(c["Controller"]) || !String.IsNullOrWhiteSpace(c["Controller"]))
                    {
                        var controller = await GetUserCreatedByAsync(c["Controller"], dbScope);
                        if (controller != null)
                        {
                            card.Sections["DocumentCommonInfo"].Fields["ControllerName"] = c["Controller"];
                            card.Sections["DocumentCommonInfo"].Fields["ControllerID"] = controller;
                        }
                        else
                        {
                            card.Sections["DocumentCommonInfo"].Fields["ControllerName"] = c["Controller"];
                            card.Sections["DocumentCommonInfo"].Fields["ControllerID"] = Guid.Parse("3db19fa0-228a-497f-873a-0250bf0a4ccb");
                        }
                    }

                    card.Sections["DocumentCommonInfo"].Fields["TreeLevel"] = c["TreeLevel"];
                    card.Sections["DocumentCommonInfo"].Fields["TreeIndex"] = c["TreeIndex"];
                    card.Sections["DocumentCommonInfo"].Fields["IsMigration"] = true;
                    //card.Sections["DocumentCommonInfo"].Fields["CurrentStateSP"] = c["StateId"];
                    switch (c["CommissionControl"])
                    {
                        case "0"://ok
                            card.Sections["DocumentCommonInfo"].Fields["CommissionControlID"] = 0;
                            card.Sections["DocumentCommonInfo"].Fields["CommissionControlName"] = "Нет контроля";
                            break;
                        case "1"://ok
                            card.Sections["DocumentCommonInfo"].Fields["CommissionControlID"] = 1;
                            card.Sections["DocumentCommonInfo"].Fields["CommissionControlName"] = "На контроле";
                            break;
                        case "2"://
                            card.Sections["DocumentCommonInfo"].Fields["CommissionControlID"] = 2;
                            card.Sections["DocumentCommonInfo"].Fields["CommissionControlName"] = "Снято с контроля";
                            break;
                    }


                    string parentIndex = null;
                    if (c["TreeLevel"] != "0" && c["TreeIndex"].Length > 10)
                    {
                        var temp = c["TreeIndex"];
                        parentIndex = temp.Remove(temp.Length - 10);
                    }
                    card.Sections["DocumentCommonInfo"].Fields["ParentTreeIndex"] = parentIndex;

                    if (!String.IsNullOrEmpty(c["Executors"]) || !String.IsNullOrWhiteSpace(c["Executors"]))
                    {
                        RBExecutorRow executor = await GetExecutorInfoAsync(c["Executors"], dbScope);

                        CardSection controlTaskDecisions = card.Sections["ControlTaskDecisions"];

                        CardRow row = controlTaskDecisions.Rows.Add();
                        row.RowID = Guid.NewGuid();
                        var parentRow = row.RowID;
                        row["Question"] = c["CommissionDescription"];
                        row.State = CardRowState.Inserted;

                        if (executor != null)
                        {
                            CardSection controlTaskPerformers = card.Sections["ControlTaskPerformers"];
                            CardRow rowP = controlTaskPerformers.Rows.Add();
                            rowP.RowID = Guid.NewGuid();
                            rowP["UserID"] = executor.ID;
                            rowP["UserName"] = executor.Name;
                            rowP["ParentRowID"] = parentRow;
                            rowP.State = CardRowState.Inserted;
                        }
                        else
                        {
                            CardSection controlTaskPerformers = card.Sections["ControlTaskPerformers"];
                            CardRow rowP = controlTaskPerformers.Rows.Add();
                            rowP.RowID = Guid.NewGuid();
                            rowP["UserID"] = Guid.Parse("3db19fa0-228a-497f-873a-0250bf0a4ccb"); ;
                            rowP["UserName"] = c["Executors"];
                            rowP["ParentRowID"] = parentRow;
                            rowP.State = CardRowState.Inserted;
                        }
                    }
                    // if (!String.IsNullOrEmpty(c["DocUID"]) || !String.IsNullOrWhiteSpace(c["DocUID"]))
                    // {
                    var MainCardID = await GetCardIDAsync(cardExtGuid, dbScope);
                    if (MainCardID != null) 
                    { 
                    card.Sections["DocumentCommonInfo"].Fields["ParentID"] = MainCardID.ID;
                    card.Sections["DocumentCommonInfo"].Fields["ParentDescription"] = MainCardID.FullNumber + " " + MainCardID.Subject;
                    }
                    //} 

                    var storeRequest = await cardRepository.StoreAsync(new CardStoreRequest { Card = card }, cancellationToken);
                    if (!storeRequest.ValidationResult.IsSuccessful())
                    {
                        ValidationResult result = storeRequest.ValidationResult.Build();
                        logger.LogResult(result);
                        return;
                    }
                  //  if (!String.IsNullOrEmpty(c["DocUID"]) || !String.IsNullOrWhiteSpace(c["DocUID"]))
                 //   {
                        logger.Info("Создано поручение к карточке " + cardExtGuid);
                  //  }
                  //  else
                  //  {
                  //      logger.Info("Создано поручение к карточке c неизвестным ID " + c["DocUID"]);
                  //  }
                }




            }
            logger.Info("Shutting down plugin RBOG_ProcessDataFromCommissions");
        
         }

        private async Task<string> GetExternalGuidAsync(string externalGuid, string commissionId, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("dci", "ExternalGuidForTask")
                            .From("DocumentCommonInfo", "dci").NoLock()
                            .Where().C("dci", "ExternalGuidForTask").Equals().P("externalGuid")
                            .And().C("dci", "FullNumber").Equals().P("commissionId")
                            .Limit(1).Build(),
                        db.Parameter("externalGuid", externalGuid),
                        db.Parameter("commissionId", commissionId))
                    .LogCommand()
                    .ExecuteAsync<string>();

                return result;
            }
        }

        private async Task<RBDocInfo> GetCardIDAsync(string externalGuid, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = new RBDocInfo();

                result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("dci", "ID", "FullNumber", "Subject")
                            .From("DocumentCommonInfo", "dci").NoLock()
                            .Where().C("dci", "ExternalGuid").Equals().P("externalGuid")
                            .Limit(1).Build(),
                        db.Parameter("externalGuid", externalGuid))
                    .LogCommand()
                    .ExecuteAsync<RBDocInfo>();

                return result;
            }
        }

        private async Task<Guid?> GetUserCreatedByAsync(string userName, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("ID")
                            .From("PersonalRoles").NoLock()
                            .Where().C("FullName").Equals().P("userFIO")
                            .Limit(1).Build(),
                        db.Parameter("userFIO", userName))
                    .LogCommand()
                    .ExecuteAsync<Guid?>();

                return result;
            }
        }

        private async Task<RBExecutorRow> GetExecutorInfoAsync(string login, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = new RBExecutorRow();

                result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("ro", "ID", "Name")
                            .From("PersonalRoles", "ro").NoLock()
                            .Where().C("ro", "Login").Equals().P("login")
                            .Limit(1).Build(),
                        db.Parameter("login", login))
                    .LogCommand()
                    .ExecuteAsync<RBExecutorRow>();

                return result;
            }
        }

        public class RBDocInfo
        {
            public Guid ID { get; set; }
            public string FullNumber { get; set; }
            public string Subject { get; set; }
            //public string Login { get; set; }
        }

        public class RBExecutorRow
        {
            public Guid ID { get; set; }
            public string Name { get; set; }
            //public string Login { get; set; }
        }


        #endregion
    }
}
