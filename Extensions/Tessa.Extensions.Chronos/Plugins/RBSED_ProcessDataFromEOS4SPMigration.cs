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
using DocumentFormat.OpenXml.Wordprocessing;


namespace Tessa.Extensions.Chronos.Plugins
{
    /// <summary>
    /// Пример плагина, который может работать через серверное API.
    /// </summary>
    [Plugin(
        Name = "RBSED_ProcessDataFromEOS4SPMigration",
        Description = "Плагин создание карточкек и загрузку данных из СЭД EOS4SP",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class RBSED_ProcessDataFromEOS4SPMigration :
        Plugin
    {
        #region Constants
        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RBSED_ProcessDataFromEOS4SPMigration.xml";

        //введено
        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Base Overrides

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("Starting plugin RBSED_ProcessDataFromEOS4SPMigration");


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
            ICardServerPermissionsProvider permissionsProvider = container.Resolve<ICardServerPermissionsProvider>();

            // создаём карточку СЭД
            // определяем вид карточки документа
            // уточнить тип данных!
            var typeID = Guid.Parse("9f9e8879-9b7f-4a55-8d22-82c6bdca66a8");
            var docTypes = await typesCache.GetDocTypesAsync();
            var docType = docTypes.FirstOrDefault(x => x.ID == typeID);

            //поменять путь
            var mainDir = new DirectoryInfo("/home/tessa/tessa/tessa/share/EosMigrationSED");

            foreach (var directory in mainDir.GetDirectories())
            {
                FileParseSEDCardMigration fp = new FileParseSEDCardMigration();

                IDbScope dbScope = container.Resolve<IDbScope>();
                //var HeadUserID = await GetHeadUserIDAsync(directory.Name, dbScope);
                //if (HeadUserID == null)
                //{
                //    logger.Info(" Department " + directory.Name + " has no Head. Stop sending");
                //    continue;
                //}

                
                var SPCards = fp.GetSPCardsFromDir(directory.FullName);

                foreach (var c in SPCards)
                {

                    switch(c["DocGroup"])
                    {
                        case "105":
                            logger.Info($"DocGroup: {c["DocGroup"]}");
                            //typeID = Guid.Parse("9f9e8879-9b7f-4a55-8d22-82c6bdca66a8");
                            //docTypes = await typesCache.GetDocTypesAsync();
                            //docType = docTypes.FirstOrDefault(x => x.ID == typeID);
                            break;
                        default:
                            logger.Info($"Неверное значение DocGroup: {c["DocGroup"]}");
                            continue;
                    }

                    var cardID = await GetExternalGuidAsync(c["UniqueId"], dbScope);

                    //if (cardID != null)
                    //{
                    //    logger.Info("Card with UID: " + c["UniqueId"] + " is already exist");
                        
                        
                    //        var checkID = await CheckTaskHistoryAsync((Guid)cardID, (Guid)HeadUserID, dbScope);
                    //        if (checkID != null)
                    //        {
                    //            logger.Info("Card with UID: " + c["UniqueId"] + " department " + directory.Name  + " is under consideration.");
                    //            continue;
                    //        }
                    //        var cardGetRequest = new CardGetRequest
                    //        {
                    //            CardID = cardID
                    //        };

                    //        permissionsProvider.SetFullPermissions(cardGetRequest);
                    //        var cardGetResponse = await cardRepository.GetAsync(cardGetRequest, cancellationToken);

                    //        var cardOld = cardGetResponse.Card;

                    //        if (!cardGetResponse.ValidationResult.IsSuccessful())
                    //        {
                    //            logger.Error(cardGetResponse.ValidationResult.Build());
                    //            return;
                    //        }
                    //        cardOld.Sections["DocumentCommonInfo"].Fields["LastDepartmentSend"] = directory.Name;

                    //        await using (var fileContainer = await manager.CreateContainerAsync(cardOld))
                    //        {
                    //            var storeResponse = await fileContainer.StoreAsync();
                    //            if (!storeResponse.ValidationResult.IsSuccessful())
                    //            {
                    //                ValidationResult result = storeResponse.ValidationResult.Build();
                    //                logger.LogResult(result); // пишем ошибку или другие сообщения в лог
                    //                logger.Info( "SP UID: " + c["UniqueId"] + " saved with error, department " + directory.Name);
                    //            }
                    //            else
                    //            {
                    //                logger.Info("SP UID: " + c["UniqueId"] + " send to consideration, department " + directory.Name);
                    //                continue;
                    //            }
                    //        }
                        
                        
                    //}

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
                    var cardNewID = Guid.NewGuid();
                    card.ID = cardNewID;

                    card.Sections["DocumentCommonInfo"].Fields["Subject"] = c["Title"]; //Title
                    card.Sections["DocumentCommonInfo"].Fields["FullNumber"] = c["RegNumber"]; //RegNumber

                    card.Sections["DocumentCommonInfo"].Fields["ExternalGuid"] = c["UniqueId"];
                    // card.Sections["DocumentCommonInfo"].Fields["Comment"] = c["Comments"];
                    card.Sections["DocumentCommonInfo"].Fields["Notes"] = c["Annotation"];
                    card.Sections["DocumentCommonInfo"].Fields["IsMigration"] = true;
                    card.Sections["DocumentCommonInfo"].Fields["CurrentStateSP"] = c["CurrentState"];

                    if (!String.IsNullOrEmpty(c["RegDate"]) || !String.IsNullOrWhiteSpace(c["RegDate"]))
                    {
                        card.Sections["DocumentCommonInfo"].Fields["DocDate"] = DateTime.ParseExact(c["RegDate"], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture).AddHours(8); //RegDate
                    }
                    //if (!String.IsNullOrEmpty(c["ProjectDate"]) || !String.IsNullOrWhiteSpace(c["ProjectDate"]))
                    //{
                    //    card.Sections["DocumentCommonInfo"].Fields["CreationDate"] = DateTime.ParseExact(c["ProjectDate"], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture).AddHours(8); //RegDate
                    //}

                    //Исходящий номер и дата
                    card.Sections["DocumentCommonInfo"].Fields["OutgoingNumber"] = c["outgoingNumber"];
                    if (!String.IsNullOrEmpty(c["outgoingDate"]) || !String.IsNullOrWhiteSpace(c["outgoingDate"]))
                    {
                        card.Sections["DocumentCommonInfo"].Fields["OutgoingDate"] = DateTime.ParseExact(c["outgoingDate"], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture).AddHours(8);
                    }

                    card.Sections["DocumentCommonInfo"].Fields["LastDepartmentSend"] = directory.Name;

                    switch (c["DeliveryTypeId"])
                    {
                        case "0"://ok
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("f9825c49-de34-44aa-9e15-a23a022f4391");
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Нет";
                            break;
                        case "1"://ok
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("6d471a90-1f9a-4430-a8d7-90ab622c031d");
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Почта России";
                            break;
                        case "2"://ok
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("6a7b976a-a503-4b33-9c54-7dfb7084b429");
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "E-mail";
                            break;
                        case "3"://ok
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("ddff8eb5-5db8-4e09-8479-b0b6dcb04219");
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Курьер";
                            break;
                        case "4"://ok
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("0c85e4c0-3745-4934-8e97-d6709d402dc0");
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Факс";
                            break;
                        case "51"://ok
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("eb8fbd06-a6e1-46ee-8959-d91e120c392d");
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Телеграмма";
                            break;
                        case "52": //ok
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("8f2affda-f1a4-4410-b059-c487cd0d2768");
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Региональный портал ОГВ(интернет-приемная)";
                            break;
                        case "53"://ok
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("278231f7-c617-47ea-b7e2-7dc3e13fbad2");
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Интернет (без обратного адреса)";
                            break;
                        case "54"://ok
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("cc08854e-7384-4eff-9650-785fe37ec05c");
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Прямой теле-эфир";
                            break;
                        case "55"://ok
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("b17c080a-99d1-4c15-9998-a651ce9189cf");
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Телефон (горячая линия)";
                            break;
                        case "56"://ok
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("c3c6372f-2a12-4864-8839-6e608b70934b");
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "Личное обращение гражданина";
                            break;
                        case "101":
                            break;
                        case "102"://ok
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("bee2d870-374d-46d3-b483-b9ccf0e48273");
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "ЕПГУ";
                            break;
                        case "103":
                            break;
                        case "201"://ok
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("c1329507-99d6-4139-bedc-d351b065c683");
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "МЭДО";
                            break;
                        case "202"://ok
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("90a03697-3344-40a3-83ba-2c236e040068");
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "СЭВ";
                            break;
                        case "203"://ok
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdID"] = Guid.Parse("e2f97f8e-e2b6-465e-b94c-150c6ae7529c");
                            card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeIdName"] = "VipNet";
                            break;
                        case "300":
                            break;
                    }

                    switch (c["DocCategoryId"])
                    {
                        case "0"://ok
                            card.Sections["DocumentCommonInfo"].Fields["CategoryID"] = Guid.Parse("54f91a0a-85c3-4739-a388-23060c2ffff6");
                            card.Sections["DocumentCommonInfo"].Fields["CategoryName"] = "Не определено";
                            break;
                        case "1"://ok
                            card.Sections["DocumentCommonInfo"].Fields["CategoryID"] = Guid.Parse("54f91a0a-85c3-4739-a388-23060c2ffff6");
                            card.Sections["DocumentCommonInfo"].Fields["CategoryName"] = "Приказ";
                            break;
                        case "2"://ok
                            card.Sections["DocumentCommonInfo"].Fields["CategoryID"] = Guid.Parse("54f91a0a-85c3-4739-a388-23060c2ffff6");
                            card.Sections["DocumentCommonInfo"].Fields["CategoryName"] = "Распоряжение";
                            break;
                        case "3"://ok
                            card.Sections["DocumentCommonInfo"].Fields["CategoryID"] = Guid.Parse("54f91a0a-85c3-4739-a388-23060c2ffff6");
                            card.Sections["DocumentCommonInfo"].Fields["CategoryName"] = "Письмо";
                            break;
                        case "4"://ok
                            card.Sections["DocumentCommonInfo"].Fields["CategoryID"] = Guid.Parse("54f91a0a-85c3-4739-a388-23060c2ffff6");
                            card.Sections["DocumentCommonInfo"].Fields["CategoryName"] = "Служебная записка";
                            break;
                        case "5"://ok
                            card.Sections["DocumentCommonInfo"].Fields["CategoryID"] = Guid.Parse("54f91a0a-85c3-4739-a388-23060c2ffff6");
                            card.Sections["DocumentCommonInfo"].Fields["CategoryName"] = "Договор";
                            break;
                        case "6": //ok
                            card.Sections["DocumentCommonInfo"].Fields["CategoryID"] = Guid.Parse("54f91a0a-85c3-4739-a388-23060c2ffff6");
                            card.Sections["DocumentCommonInfo"].Fields["CategoryName"] = "Протокол";
                            break;
                        case "7"://ok
                            card.Sections["DocumentCommonInfo"].Fields["CategoryID"] = Guid.Parse("54f91a0a-85c3-4739-a388-23060c2ffff6");
                            card.Sections["DocumentCommonInfo"].Fields["CategoryName"] = "Акт";
                            break;
                        case "8"://ok
                            card.Sections["DocumentCommonInfo"].Fields["CategoryID"] = Guid.Parse("54f91a0a-85c3-4739-a388-23060c2ffff6");
                            card.Sections["DocumentCommonInfo"].Fields["CategoryName"] = "Заявка";
                            break;
                        case "9"://ok
                            card.Sections["DocumentCommonInfo"].Fields["CategoryID"] = Guid.Parse("54f91a0a-85c3-4739-a388-23060c2ffff6");
                            card.Sections["DocumentCommonInfo"].Fields["CategoryName"] = "Реестр";
                            break;
                    }

                    /*   switch (c["ComplaintReasonId"])
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
                       }*/

                    /*   switch (c["SolutionTypeId"])
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
                       }*/

                    /* switch (c["CitizenRequestTypeId"])
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
                     }*/


                    // указываем информацию по подразделению
                    var department = new RBDepartmentRow();
                    //IDbScope dbScope = container.Resolve<IDbScope>();
                    department = await GetDepartmentInfoAsync(c["PlaceCreateion"], dbScope);
                    if (department != null)
                    {
                        card.Sections["DocumentCommonInfo"].Fields["DepartmentID"] = department.ID;
                        card.Sections["DocumentCommonInfo"].Fields["DepartmentName"] = department.Name;
                        card.Sections["DocumentCommonInfo"].Fields["DepartmentIndexDep"] = department.Index;
                    }

                    //var assignedTo = new RBAssignedToRow();

                    //assignedTo = await GetAssignedToInfoAsync(c["assignedTo"], dbScope);

                    //if (assignedTo != null)
                    //{
                    //    card.Sections["DocumentCommonInfo"].Fields["assignedToID"] = assignedTo.ID;
                    //    card.Sections["DocumentCommonInfo"].Fields["assignedToName"] = assignedTo.Name;
                    //}
                    //else
                    //{
                    //    card.Sections["DocumentCommonInfo"].Fields["assignedToID"] = Guid.Parse("3db19fa0-228a-497f-873a-0250bf0a4ccb");
                    //    card.Sections["DocumentCommonInfo"].Fields["assignedToName"] = c["assignedTo"];
                    //}

                    var partner = await GetPartnerIDAsync(c["AddresseeOrganiz"], dbScope);
                    if (partner != null)
                    {
                        card.Sections["DocumentCommonInfo"].Fields["PartnerID"] = partner;
                        card.Sections["DocumentCommonInfo"].Fields["PartnerName"] = c["AddresseeOrganiz"];
                    }
                    else
                    {
                        card.Sections["DocumentCommonInfo"].Fields["PartnerID"] = Guid.Parse("5155812d-711c-4f63-9199-3673b61a93de");
                        card.Sections["DocumentCommonInfo"].Fields["PartnerName"] = c["AddresseeOrganiz"];
                    }

                    logger.Info($"c[executors]:{c["Executors"]}");

                    if (!String.IsNullOrWhiteSpace(c["Executors"]))
                    {
                        //Поиск исполнителя по ExtNumber
                        var executor = await GetUserInfoAsync(Convert.ToInt32(c["Executors"].Trim(' ')), dbScope);
                        if (executor != null)
                        {
                            logger.Info($"Executor found CardID: {c["UniqueId"]} ExtNumber: {c["Executors"]} ID: {executor.ID} Name: {executor.Name}");

                            card.Sections["DocumentCommonInfo"].Fields["ExecutorID"] = executor.ID;
                            card.Sections["DocumentCommonInfo"].Fields["ExecutorName"] = executor.Name;
                        }
                        else
                        {
                            logger.Info($"Executor not found CardID: {c["UniqueId"]} ExtNumber: {c["Executors"]}");

                            card.Sections["DocumentCommonInfo"].Fields["ExecutorName"] = c["Executors"];
                        }
                    }

                    

                    //card.Sections["DocumentCommonInfo"].Fields["DocDate"] = DateTime.ParseExact(c["RegDate"], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture); //RegDate

                    /*  if (!String.IsNullOrEmpty(c["DocDateCompleted"]) || !String.IsNullOrWhiteSpace(c["DocDateCompleted"]))
                      {
                          card.Sections["DocumentCommonInfo"].Fields["FinishDate"] = DateTime.ParseExact(c["DocDateCompleted"], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture); //DocDateCompleted
                      }*/

                    //card.Sections["DocumentCommonInfo"].Fields["FinishDate"] = DateTime.ParseExact(c["DocDateCompleted"], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture); //DocDateCompleted

                    /*   //Формирование ФИО
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
                       }*/


                    //Поиск исполнителя переделать

                    RBUserNew rbUser = await GetUserInfoAsync(Convert.ToInt32(c["CreatedBy"]), dbScope);

                    if (rbUser != null)
                    {
                        logger.Info($"Registrator found CardID: {c["UniqueId"]} ExtNumber: {c["CreatedBy"]} ID: {rbUser.ID} Name: {rbUser.Name}");

                        card.Sections["DocumentCommonInfo"].Fields["RegistratorID"] = rbUser.ID;
                        card.Sections["DocumentCommonInfo"].Fields["RegistratorName"] = rbUser.Name;
                    }
                    else
                    {
                        logger.Info($"Registrator not found CardID: {c["UniqueId"]} ExtNumber: {c["CreatedBy"]}");
                        card.Sections["DocumentCommonInfo"].Fields["RegistratorName"] = c["CreatedBy"];
                    }
                    

                    // Обходим директорию с файлами, добавляем файлы в карточку
                    var dir = new DirectoryInfo(@$"/home/tessa/tessa/tessa/share/EosMigrationSED/{directory.Name}/Files/" + c["UniqueId"]);

                    if (dir.Exists)
                    {
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
                                logger.Info("Files for card with SP UID: " + c["UniqueId"] + " attached with error 1010111, department " + directory.Name);
                            }
                            else
                            {
                                logger.Info("Files for card with SP UID: " + c["UniqueId"] + " attached succesful, department " + directory.Name);
                            }
                        }

                    }

                    logger.Info($"Создана карточка по метаданным {c["UniqueId"]}, department {directory.Name}");
                }
            }

            

            
            logger.Info("Shutting down RBSED_ProcessDataFromEOS4SPMigration");
        }

        private async Task<RBUserNew> GetUserInfoAsync(int externalID, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = new RBUserNew();

                result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("ro", "ID", "Name")
                            .From("Roles", "ro").NoLock()
                            .Where().C("ro", "ExternalID").Equals().P("extID")
                            .Limit(1).Build(),
                        db.Parameter("extID", externalID.ToString()))
                    .LogCommand()
                    .ExecuteAsync<RBUserNew>();

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

        public class RBUserNew
        {
            public Guid ID { get; set; }
            public string Name { get; set; }
        }

        public class RBDocInfo
        {
            public Guid ID { get; set; }
            public string FullNumber { get; set; }
            public string Subject { get; set; }
            //public string Login { get; set; }
        }

        private async Task<Guid?> GetExternalGuidAsync(string externalGuid, IDbScope dbScope)
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

        private async Task<string> GetExternalCommissionGuidAsync(string externalGuid, string commissionId, IDbScope dbScope)
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
                            .Select().Top(1).C("ro", "ID", "Name", "IndexDep")
                            .From("Roles", "ro").NoLock()
                            .Where().C("ro", "Name").Equals().P("name")
                            .Limit(1).Build(),
                        db.Parameter("name", docGroup))
                    .LogCommand()
                    .ExecuteAsync<RBDepartmentRow>();

                return result;
            }
        }

        private async Task<RBDepartmentRow> GetDepartmentDocGroupsAsync(string docGroup, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = new RBDepartmentRow();

                result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("ro", "ID", "Name", "ExternalID", "ExternalID2", "ExternalID3", "ExternalID4", "ExternalID5")
                            .From("Roles", "ro").NoLock()
                            .Where().C("ro", "ExternalID").Equals().P("docGroup")
                            .Or().C("ro", "ExternalID2").Equals().P("docGroup")
                            .Or().C("ro", "ExternalID3").Equals().P("docGroup")
                            .Or().C("ro", "ExternalID4").Equals().P("docGroup")
                            .Or().C("ro", "ExternalID5").Equals().P("docGroup")
                            .Limit(1).Build(),
                        db.Parameter("docGroup", docGroup))
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

        private async Task<Guid?> GetCommissionsUserCreatedByAsync(string userName, IDbScope dbScope)
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

        private async Task<Guid?> GetPartnerIDAsync(string userName, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("ID")
                            .From("Partners").NoLock()
                            .Where().C("Name").Equals().P("userFIO")
                            .Limit(1).Build(),
                        db.Parameter("userFIO", userName))
                    .LogCommand()
                    .ExecuteAsync<Guid?>();

                return result;
            }
        }

        private async Task<Guid?> GetExecutorIDAsync(string userName, IDbScope dbScope)
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

        private async Task<RBAssignedToRow> GetAssignedToInfoAsync(string login, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = new RBAssignedToRow();

                result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("ro", "ID", "Name")
                            .From("PersonalRoles", "ro").NoLock()
                            .Where().C("ro", "Login").Equals().P("login")
                            .Limit(1).Build(),
                        db.Parameter("login", login))
                    .LogCommand()
                    .ExecuteAsync<RBAssignedToRow>();

                return result;
            }
        }

        private async Task<Guid?> GetHeadUserIDAsync(string DepName, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;


                var result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("dp", "HeadUserID")
                            .From("DepartmentRoles", "dp").NoLock()
                            .InnerJoin("Roles", "ro").NoLock()
                            .On().C("ro", "ID").Equals().C("dp", "ID")
                            .Where().C("ro", "Name").Equals().P("depName")
                            .Limit(1).Build(),
                        db.Parameter("depName", DepName))
                    .LogCommand()
                    .ExecuteAsync<Guid?>();

                return result;
            }
        }

        private async Task<Guid?> CheckTaskHistoryAsync(Guid cardID, Guid HeadUserID,  IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;


                var result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("th", "ID")
                            .From("TaskHistory", "th").NoLock()
                            .Where().C("th", "ID").Equals().P("cardID")
                            .And().C("th", "RoleID").Equals().P("HeadUserID")
                            .Limit(1).Build(),
                        db.Parameter("cardID", cardID),
                        db.Parameter("HeadUserID", HeadUserID))
                    .LogCommand()
                    .ExecuteAsync<Guid?>();

                return result;
            }
        }

        public class RBAssignedToRow
        {
            public Guid ID { get; set; }
            public string Name { get; set; }
            //public string Login { get; set; }
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

        public class RBExecutorRow
        {
            public Guid ID { get; set; }
            public string Name { get; set; }
            //public string Login { get; set; }
        }

       

        #endregion
    }
}
