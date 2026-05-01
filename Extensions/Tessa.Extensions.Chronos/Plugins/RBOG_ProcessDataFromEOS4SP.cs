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
        Name = "RBOG_ProcessDataFromEOS4SP",
        Description = "Плагин создание карточкек и загрузку данных из СЭД EOS4SP",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class RBOG_ProcessDataFromEOS4SP :
        Plugin
    {
        #region Constants

        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RBOG_ProcessDataFromEOS4SP.xml";

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

            // создаём карточку письменного обращения граждан
            // определяем вид карточки документа

            var typeID = Guid.Parse("47cc47f8-3bd5-40e4-8e8d-5c1a02286b12");
            var docTypes = await typesCache.GetDocTypesAsync();
            var docType = docTypes.FirstOrDefault(x => x.ID == typeID);


            FileParse fp = new FileParse();

            //Поменять путь
            var SPCards = fp.GetSPCardsFromDir(@"/home/tessa/tessa/share/ExportFromEOS/Incoming/2014");

            foreach (var c in SPCards)
            {
                IDbScope dbScope = container.Resolve<IDbScope>();
                var checkGUID = await GetExternalGuidAsync(c["UID"], dbScope);

                if (checkGUID != null)
                {
                    logger.Info("Card with UID: " + c["UID"] + " is already exist");
                    continue;
                }
                // определяем группу документов
                if (await IS_PO_DocAsync(c["DocGroup"], dbScope) != null) // письменное обращение
                {
                    typeID = Guid.Parse("47cc47f8-3bd5-40e4-8e8d-5c1a02286b12");
                    docTypes = await typesCache.GetDocTypesAsync();
                    docType = docTypes.FirstOrDefault(x => x.ID == typeID);
                }
                else if (await IS_UO_DocAsync(c["DocGroup"], dbScope) != null) // устное обращение
                {
                    typeID = Guid.Parse("3466e8be-fae7-40aa-b8ac-6f0191cb7d1b");
                    docTypes = await typesCache.GetDocTypesAsync();
                    docType = docTypes.FirstOrDefault(x => x.ID == typeID);
                }
                else if (await IS_LP_DocAsync(c["DocGroup"], dbScope) != null) // личный прием
                {
                    typeID = Guid.Parse("791db930-c8d0-42e1-ae25-a4bdd78347cd");
                    docTypes = await typesCache.GetDocTypesAsync();
                    docType = docTypes.FirstOrDefault(x => x.ID == typeID);
                }
                else if (await IS_O_DocAsync(c["DocGroup"], dbScope) != null) // ответ
                {
                    typeID = Guid.Parse("0b8568b7-4303-4fb6-9793-c29bdcfbee99");
                    docTypes = await typesCache.GetDocTypesAsync();
                    docType = docTypes.FirstOrDefault(x => x.ID == typeID);
                }
                else if (await IS_I_DocAsync(c["DocGroup"], dbScope) != null) // информационное письмо
                {
                    typeID = Guid.Parse("58e36a57-478a-449e-ad57-3a155fb8150a");
                    docTypes = await typesCache.GetDocTypesAsync();
                    docType = docTypes.FirstOrDefault(x => x.ID == typeID);
                }


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

                card.Sections["DocumentCommonInfo"].Fields["Subject"] = c["DocTitle"]; //Title
                card.Sections["DocumentCommonInfo"].Fields["FullNumber"] = c["RegNumber"]; //RegNumber

                card.Sections["DocumentCommonInfo"].Fields["ExternalGuid"] = c["UID"];
                card.Sections["DocumentCommonInfo"].Fields["Comment"] = c["Comments"];
                card.Sections["DocumentCommonInfo"].Fields["Notes"] = c["Annotation"];
                card.Sections["DocumentCommonInfo"].Fields["IsMigration"] = true;
                card.Sections["DocumentCommonInfo"].Fields["CurrentStateSP"] = c["CurrentState"];


                switch (c["DeliveryTypeId"])
                {
                    case "0"://ok
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("2474775a-3728-430f-93a4-7ae1bb60a7b8");
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Нет";
                        break;
                    case "1"://ok
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("33dab9f2-368d-405f-b808-f9c51321b23c");
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Почта России";
                        break;
                    case "2"://ok
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("71bfdbc3-6748-4ef7-9b12-eb8336bd0cc7");
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "E-mail";
                        break;
                    case "3"://ok
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("0df721d9-2636-4cb2-8fbb-8adbcdcefaf6");
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Курьер";
                        break;
                    case "4"://ok
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("6acafcce-a775-4719-aa78-cbd49872421c");
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Факс";
                        break;
                    case "51"://ok
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("437730ce-439d-4785-ac52-dd92cdb87256");
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Телеграмма";
                        break;
                    case "52": //ok
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("cbd0c800-4acf-424f-8af0-357dfc470a05");
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Региональный портал ОГВ(интернет-приемная)";
                        break;
                    case "53"://ok
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("278231f7-c617-47ea-b7e2-7dc3e13fbad2");
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Интернет (без обратного адреса)";
                        break;
                    case "54"://ok
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("a6e171ea-8585-4222-b4bd-8717fb5af56d");
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Прямой теле-эфир";
                        break;
                    case "55"://ok
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("912e104b-6325-48b4-b07a-9a9dd32f6d8d");
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Телефон (горячая линия)";
                        break;
                    case "56"://ok
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("66dba766-6f20-40f6-a21e-ba9f1b0cc82f");
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Личное обращение гражданина";
                        break;
                    case "101":
                        break;
                    case "102"://ok
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("3d13fce7-5b6a-472f-8ae7-db9cdb6eeb5c");
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "ЕПГУ";
                        break;
                    case "103":
                        break;
                    case "201"://ok
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("11b5adb7-0506-438d-a72e-413eef491ff4");
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "МЭДО";
                        break;
                    case "202"://ok
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("9247c796-ab16-4742-8853-b26cbb60a551");
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "СЭВ";
                        break;
                    case "203"://ok
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("0505d498-c5a5-40a9-bdaa-e5dfdc041f76");
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "VipNet";
                        break;
                    case "300":
                        break;
                }

                switch (c["ComplaintReasonId"])
                {
                    case "0"://ok
                        card.Sections["DocumentCommonInfo"].Fields["ComplaintReasonIdID"] = Guid.Parse("d2eb68db-692e-4488-b9de-f21c303e8c4f");
                        card.Sections["DocumentCommonInfo"].Fields["ComplaintReasonIdName"] = "Не определено";
                        break;
                    case "1"://ok
                        card.Sections["DocumentCommonInfo"].Fields["ComplaintReasonIdID"] = Guid.Parse("d3873847-7808-4cab-83e9-32668c685114");
                        card.Sections["DocumentCommonInfo"].Fields["ComplaintReasonIdName"] = "Ненадлежащее исполнение обязанностей должностными лицами ";
                        break;
                    case "2"://ok
                        card.Sections["DocumentCommonInfo"].Fields["ComplaintReasonIdID"] = Guid.Parse("6a294a85-3abd-49a0-873e-8009c43d6416");
                        card.Sections["DocumentCommonInfo"].Fields["ComplaintReasonIdName"] = "Недостатки в работе учреждений по предоставлению госуслуг";
                        break;
                    case "3"://ok
                        card.Sections["DocumentCommonInfo"].Fields["ComplaintReasonIdID"] = Guid.Parse("c9b670d7-7120-45ff-985d-cb553e642456");
                        card.Sections["DocumentCommonInfo"].Fields["ComplaintReasonIdName"] = "Нарушение законодательства";
                        break;
                    case "4"://ok
                        card.Sections["DocumentCommonInfo"].Fields["ComplaintReasonIdID"] = Guid.Parse("529d610b-9797-4379-9179-0998506ce87f");
                        card.Sections["DocumentCommonInfo"].Fields["ComplaintReasonIdName"] = "Непринятие во внимание законных интересов граждан";
                        break;
                    case "5"://ok
                        card.Sections["DocumentCommonInfo"].Fields["ComplaintReasonIdID"] = Guid.Parse("54e9d864-047d-4bd1-8f3a-25c589026452");
                        card.Sections["DocumentCommonInfo"].Fields["ComplaintReasonIdName"] = "Недостаточная информированность о деятельности учреждений";
                        break;
                }

                switch (c["SolutionTypeId"])
                {
                    case "0"://ok
                        card.Sections["DocumentCommonInfo"].Fields["ProcessingInfoOptIdID"] = Guid.Parse("b8e07985-1f82-44d7-8382-29231cbf51e6");
                        card.Sections["DocumentCommonInfo"].Fields["ProcessingInfoOptIdName"] = "Не определено";
                        break;
                    case "1"://ok
                        card.Sections["DocumentCommonInfo"].Fields["ProcessingInfoOptIdID"] = Guid.Parse("47a6f7d3-fc56-409e-b8b5-e83258de339b");
                        card.Sections["DocumentCommonInfo"].Fields["ProcessingInfoOptIdName"] = "Решено положительно";
                        break;
                    case "2"://ok
                        card.Sections["DocumentCommonInfo"].Fields["ProcessingInfoOptIdID"] = Guid.Parse("95c96d87-5608-4893-80e1-69b5e12043b3");
                        card.Sections["DocumentCommonInfo"].Fields["ProcessingInfoOptIdName"] = "Меры приняты";
                        break;
                    case "3"://ok
                        card.Sections["DocumentCommonInfo"].Fields["ProcessingInfoOptIdID"] = Guid.Parse("35310fe9-f62f-4e2a-a36f-fdb6d649685d");
                        card.Sections["DocumentCommonInfo"].Fields["ProcessingInfoOptIdName"] = "Разъяснено";
                        break;
                    case "4"://ok
                        card.Sections["DocumentCommonInfo"].Fields["ProcessingInfoOptIdID"] = Guid.Parse("008af190-fcd6-42b6-a747-75cf9ecce6d1");
                        card.Sections["DocumentCommonInfo"].Fields["ProcessingInfoOptIdName"] = "Отказано";
                        break;
                }

                switch (c["CitizenRequestTypeId"])
                {
                    case "1"://ok
                        card.Sections["DocumentCommonInfo"].Fields["CitizenRequestTypeIdID"] = Guid.Parse("62919bb5-a850-4a75-a0d7-6f99e0bc02cb");
                        card.Sections["DocumentCommonInfo"].Fields["CitizenRequestTypeIdName"] = "Жалоба";
                        break;
                    case "2"://ok
                        card.Sections["DocumentCommonInfo"].Fields["CitizenRequestTypeIdID"] = Guid.Parse("79e96720-bd51-4c1f-8f07-4ee624f57131");
                        card.Sections["DocumentCommonInfo"].Fields["CitizenRequestTypeIdName"] = "Заявление";
                        break;
                    case "3"://
                        card.Sections["DocumentCommonInfo"].Fields["CitizenRequestTypeIdID"] = Guid.Parse("1d53ed8c-cc0f-4874-a59b-c27193f3afb4");
                        card.Sections["DocumentCommonInfo"].Fields["CitizenRequestTypeIdName"] = "Предложение";
                        break;
                }


                // указываем информацию по подразделению
                var department = new RBDepartmentRow();
                //IDbScope dbScope = container.Resolve<IDbScope>();
                department = await GetDepartmentInfoAsync(c["DocGroup"], dbScope);
                if (department != null)
                {
                    card.Sections["DocumentCommonInfo"].Fields["DepartmentID"] = department.ID;
                    card.Sections["DocumentCommonInfo"].Fields["DepartmentName"] = department.Name;
                    card.Sections["DocumentCommonInfo"].Fields["DepartmentIndex"] = department.Index;
                }

                if (!String.IsNullOrEmpty(c["RegDate"]) || !String.IsNullOrWhiteSpace(c["RegDate"]))
                {
                    card.Sections["DocumentCommonInfo"].Fields["DocDate"] = DateTime.ParseExact(c["RegDate"], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture); //RegDate
                }

                //card.Sections["DocumentCommonInfo"].Fields["DocDate"] = DateTime.ParseExact(c["RegDate"], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture); //RegDate

                if (!String.IsNullOrEmpty(c["DocDateCompleted"]) || !String.IsNullOrWhiteSpace(c["DocDateCompleted"]))
                {
                    card.Sections["DocumentCommonInfo"].Fields["FinishDate"] = DateTime.ParseExact(c["DocDateCompleted"], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture); //DocDateCompleted
                }

                //card.Sections["DocumentCommonInfo"].Fields["FinishDate"] = DateTime.ParseExact(c["DocDateCompleted"], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture); //DocDateCompleted

                //Формирование ФИО
                StringBuilder sb = new StringBuilder();
                sb.Append(c["CitizenLastName"].Trim(' '));
                sb.Append(' ');
                sb.Append(c["CitizenFirstName"].Trim(' '));
                sb.Append(' ');
                sb.Append(c["CitizenMiddleName"].Trim(' '));

                //Поиск гражданина
                RBCitizen citizen = new RBCitizen();
                citizen = await GetCitizensFIOAsync(sb.ToString(), dbScope);

                if (citizen != null)
                {
                    card.Sections["DocumentCommonInfo"].Fields["CitizensID"] = citizen.ID;
                    card.Sections["DocumentCommonInfo"].Fields["CitizensFIO"] = citizen.FIO;
                    card.Sections["DocumentCommonInfo"].Fields["CitizensAddress"] = citizen.Address;
                    card.Sections["DocumentCommonInfo"].Fields["CitizensCiti"] = citizen.Citi;
                    card.Sections["DocumentCommonInfo"].Fields["CitizensIndex"] = citizen.Index;
                }


                //Поиск исполнителя
                card.Sections["DocumentCommonInfo"].Fields["RegistratorName"] = c["CreatedBy"];

                // Обходим директорию с файлами, добавляем файлы в карточку
                var dir = new DirectoryInfo(@"/home/tessa/tessa/share/ExportFromEOS/Incoming/2014/Files/" + c["UID"]);

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
                        logger.Info("Files for card with SP UID: " + c["UID"] + " attached with error 1000111");
                    }
                    else
                    {
                        logger.Info("Files for card with SP UID: " + c["UID"] + " attached succesful");
                    }
                }

                logger.Info($"Создана карточка по метаданным {c["UID"]}");



                logger.Info("Shutting down ODProcessDataFromEOS4SP");
            }
        }

        private async Task<string> GetExternalGuidAsync(string externalGuid, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("dci", "ExternalGuid")
                            .From("DocumentCommonInfo", "dci").NoLock()
                            .Where().C("dci", "ExternalGuid").Equals().P("externalGuid")
                            .Limit(1).Build(),
                        db.Parameter("externalGuid", externalGuid))
                    .LogCommand()
                    .ExecuteAsync<string>();

                return result;
            }
        }

        private async Task<string> IS_PO_DocAsync(string docGroup, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("ro", "ExternalID")
                            .From("Roles", "ro").NoLock()
                            .Where().C("ro", "ExternalID").Equals().P("externalID")
                            .Limit(1).Build(),
                        db.Parameter("externalID", docGroup))
                    .LogCommand()
                    .ExecuteAsync<string>();

                return result;
            }
        }

        private async Task<string> IS_UO_DocAsync(string docGroup, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("ro", "ExternalID2")
                            .From("Roles", "ro").NoLock()
                            .Where().C("ro", "ExternalID2").Equals().P("externalID")
                            .Limit(1).Build(),
                        db.Parameter("externalID", docGroup))
                    .LogCommand()
                    .ExecuteAsync<string>();

                return result;
            }
        }

        private async Task<string> IS_LP_DocAsync(string docGroup, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("ro", "ExternalID3")
                            .From("Roles", "ro").NoLock()
                            .Where().C("ro", "ExternalID3").Equals().P("externalID")
                            .Limit(1).Build(),
                        db.Parameter("externalID", docGroup))
                    .LogCommand()
                    .ExecuteAsync<string>();

                return result;
            }
        }

        private async Task<string> IS_O_DocAsync(string docGroup, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("ro", "ExternalID4")
                            .From("Roles", "ro").NoLock()
                            .Where().C("ro", "ExternalID4").Equals().P("externalID")
                            .Limit(1).Build(),
                        db.Parameter("externalID", docGroup))
                    .LogCommand()
                    .ExecuteAsync<string>();

                return result;
            }
        }

        private async Task<string> IS_I_DocAsync(string docGroup, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("ro", "ExternalID5")
                            .From("Roles", "ro").NoLock()
                            .Where().C("ro", "ExternalID5").Equals().P("externalID")
                            .Limit(1).Build(),
                        db.Parameter("externalID", docGroup))
                    .LogCommand()
                    .ExecuteAsync<string>();

                return result;
            }
        }

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
                            .Or().C("ro", "ExternalID2").Equals().P("externalID")
                            .Or().C("ro", "ExternalID3").Equals().P("externalID")
                            .Or().C("ro", "ExternalID4").Equals().P("externalID")
                            .Or().C("ro", "ExternalID5").Equals().P("externalID")
                            .Limit(1).Build(),
                        db.Parameter("externalID", docGroup))
                    .LogCommand()
                    .ExecuteAsync<RBDepartmentRow>();

                return result;
            }
        }

        private async Task<RBCitizen> GetCitizensFIOAsync(string citizensFIO, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = new RBCitizen();

                result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("cit", "ID", "FIO", "Address", "Citi", "Index", "Email")
                            .From("Citizens", "cit").NoLock()
                            .Where().C("cit", "FIO").Equals().P("citizensFIO")
                            .Limit(1).Build(),
                        db.Parameter("citizensFIO", citizensFIO))
                    .LogCommand()
                    .ExecuteAsync<RBCitizen>();

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
                            .Where().C("Name").Equals().P("userFIO")
                            .Limit(1).Build(),
                        db.Parameter("userFIO", userName))
                    .LogCommand()
                    .ExecuteAsync<Guid?>();

                return result;
            }
        }


        public class RBUser
        {
            public Guid ID { get; set; }
        }

        public class RBCitizen
        {
            public Guid ID { get; set; }
            public string FIO { get; set; }
            public string Address { get; set; }
            public string Citi { get; set; }
            public string Index { get; set; }
            public string Email { get; set; }
        }

        public class RBDepartmentRow
        {
            public Guid ID { get; set; }
            public string Name { get; set; }
            public string Index { get; set; }
        }

        #endregion
    }
}
