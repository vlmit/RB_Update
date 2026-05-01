using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using AngleSharp.Dom;
using AngleSharp.Io.Dom;
using Chronos.Contracts;
using LinqToDB.Common;
using NLog;
using Org.BouncyCastle.Crypto.Generators;
using Tessa.Cards;
using Tessa.Extensions.Chronos.Medo.MedoRecieve.Helpers;
using Tessa.Extensions.Chronos.Medo.MedoSend.Helpers;
using Tessa.Extensions.Chronos.Medo.MedoSend.OutgoingTypes;
using Tessa.Extensions.Default.Shared.EDS;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Extensions.Shared.Helpers.Medo;
using Tessa.Files;
using Tessa.PdfSharp.Drawing;
using Tessa.PdfSharp.Pdf;
using Tessa.PdfSharp.Pdf.IO;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.EDS;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using Unity;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using LinqToDB.Data;
using QRCoder.Extensions;
using Tessa.PdfSharp.Pdf.Content.Objects;
using System.Security.Cryptography.X509Certificates;
using Tessa.SmartMerge;
using Tessa.Extensions.Shared.Info;

namespace Tessa.Extensions.Chronos.Medo.MedoRecieve
{
    [Plugin(
        Name = "MedoRecievePlugin",
        Description = "Plugin recieve medo messages",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class MedoRecievePlugin : Plugin
    {
        #region Const

        /// <summary>
        ///     Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/MedoRecievePlugin.xml";

        //const string successPath = "/home/tessa/tessa/share/MEDO/OUT/";

        //const string errorPath = "/home/tessa/tessa/share/MEDO/ERROR/";
        //const string classifiedPath = "/home/tessa/tessa/share/MEDO/CLASSIFIED_IN/";
        //const string noPartnerPath = "/home/tessa/tessa/share/MEDO/NOPARTNER/";
        //const string incorrectVersionPath = "/home/tessa/tessa/share/MEDO/INCORRECTVERSION/";

        //const string notificationPath = "/home/tessa/tessa/share/MEDO/NOTIFICATION/";

        //const string inPath = "/home/tessa/tessa/share/MEDO/IN_common/";
        //const string resultMedoPath = "/home/tessa/tessa/share/MEDO/in_result.txt";

        const string MedoDeliveryType = "c1329507-99d6-4139-bedc-d351b065c683";

        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static string GlobalPath = ConfigurationManager.Settings.TryGet<string>(MedoConst.GlobalInPathSettingName);

        private static string successPath = Path.Combine(GlobalPath, ProcessingResult.SUCCESS.ToString("g"));
        private static string errorPath = Path.Combine(GlobalPath, ProcessingResult.ERROR.ToString("g"));
        private static string classifiedPath = Path.Combine(GlobalPath, ProcessingResult.CLASSIFIED.ToString("g"));
        private static string noPartnerPath = Path.Combine(GlobalPath, ProcessingResult.NOPARTNER.ToString("g"));
        private static string incorrectVersionPath = Path.Combine(GlobalPath, ProcessingResult.INCORRECTVERSION.ToString("g"));
        private static string notificationPath = Path.Combine(GlobalPath, ProcessingResult.NOTIFICATION.ToString("g"));
        private static string notificationErrorPath = Path.Combine(notificationPath, "ERROR");

        private static string inPath = Path.Combine(GlobalPath, "in");
        #endregion

        #region EntryPoint

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            await TessaPlatform.InitializeFromConfigurationAsync(cancellationToken: cancellationToken);

            IUnityContainer container = await new UnityContainer().RegisterServerForPluginAsync();

            ICAdESManager cadesManager = container.Resolve<ICAdESManager>();
            ICardRepository cardRepository = container.Resolve<ICardRepository>();
            ICardFileManager manager = container.Resolve<ICardFileManager>();
            IKrTypesCache typesCache = container.Resolve<IKrTypesCache>();
            ICardServerPermissionsProvider permissionsProvider = container.Resolve<ICardServerPermissionsProvider>();

            //List<string> failureDirs= new List<string>();
            //List<string> okDirs = new List<string>();
            //List<string> classifiedDirs = new List<string>();
            //List<string> noPartnerDirs = new List<string>();

            List<Guid> createdCardsIds = new List<Guid>();

            var typeID = Guid.Parse("9a7a302d-35bc-4026-b647-f8b4eb7e9204");//"584984fd-11dd-43ff-91a5-98dd3d713e66");

            var docTypes = await typesCache.GetDocTypesAsync();
            var docType = docTypes.FirstOrDefault(x => x.ID == typeID);

            var dbScope = container.Resolve<IDbScope>();

            //поменять путь
            var mainDir = new DirectoryInfo(inPath);

            if (!mainDir.Exists)
            {
                logger.Error($"Folder {mainDir.FullName} not exists");
                return;
            }

            var directories = mainDir.GetDirectories();

            var directoriesLength = directories.Length;

            int cardCount = 0;

            var lastDir = string.Empty;
            var resultType = ProcessingResult.ERROR;

            logger.Info($"Start recieve MEDO messages. To recieve: {directoriesLength} messages");
            //System.IO.File.AppendAllText(resultMedoPath, "------------------------------------------------------------------- " + DateTime.Now + "\n");
            foreach (var dir in directories)
            {
                try
                {
                    if (cardCount != 0)
                    {
                        MoveDirAfterProcessing(lastDir, resultType);
                    }

                    cardCount++;

                    //System.IO.File.AppendAllText(resultMedoPath, $"---------------{dir.Name}----------------\n");
                    //System.IO.File.AppendAllText(resultMedoPath, $"Card {cardCount} of {directoriesLength}\n");

                    lastDir = dir.Name;
                    resultType = ProcessingResult.ERROR;

                    logger.Info("-------------------");


                    logger.Info($"Card {cardCount} of {directoriesLength}");
                    logger.Info("start " + dir.Name);

                    #region Check in folders

                    if (isDirExist(dir.Name))
                    {
                        //System.IO.File.AppendAllText(resultMedoPath, "No import " + dir.Name + " directory already imported\n");
                        ////continue;
                    }

                    #endregion

                    #region Parse envelope file

                    //bool isExistEnvelope = false;
                    string envelopeFileName = "";

                    // Парсинг envelope.ltr для поиска xml файла
                    //if (!System.IO.File.Exists(Path.Combine(dir.FullName, "envelope.ltr")) && !System.IO.File.Exists(Path.Combine(dir.FullName, "envelope.ini")))
                    //{
                    //    logger.Info($"Not exist {Path.Combine(dir.FullName, "envelope.ltr")}");
                    //    System.IO.File.AppendAllText(resultMedoPath, "No import " + dir.Name + " not exist envelope.ini\n");
                    //    continue;
                    //}

                    if (System.IO.File.Exists(Path.Combine(dir.FullName, "envelope.ltr")))
                    {
                        //isExistEnvelope = true;
                        envelopeFileName = "envelope.ltr";
                    }
                    else if (System.IO.File.Exists(Path.Combine(dir.FullName, "envelope.ini")))
                    {
                        //isExistEnvelope = true;
                        envelopeFileName = "envelope.ini";
                    }
                    else
                    {
                        logger.Info($"Not exist {Path.Combine(dir.FullName, "envelope.ltr")} or {Path.Combine(dir.FullName, "envelope.ini")}");
                        //System.IO.File.AppendAllText(resultMedoPath, "No import " + dir.Name + " not exist envelope file\n");
                        continue;
                    }

                    EnvelopeParse envelopeParse = new EnvelopeParse();

                    var envelopeFiles = envelopeParse.GetFilesFromEnvelope(Path.Combine(dir.FullName, envelopeFileName));

                    // Если нет сведений о файлах
                    if (envelopeFiles.Count == 0)
                    {
                        logger.Info($"Envelope doesn't have information about files {Path.Combine(dir.FullName, envelopeFileName)}");
                        //System.IO.File.AppendAllText(resultMedoPath, "No import " + dir.Name + " no files in envelope\n");
                        continue;
                    }

                    string communicationFileName = "";

                    foreach (var envelopeFile in envelopeFiles)
                    {
                        if (envelopeFile.EndsWith(".xml"))
                        {
                            communicationFileName = envelopeFile;
                            break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(communicationFileName))
                    {
                        logger.Info($"Envelope doesn't have information about communication file {Path.Combine(dir.FullName, envelopeFileName)}");
                        //System.IO.File.AppendAllText(resultMedoPath, "No import " + dir.Name + " no xml file in envelope\n");
                        continue;
                    }

                    string communicationFilePath = Path.Combine(dir.FullName, communicationFileName);

                    if (!System.IO.File.Exists(communicationFilePath))
                    {
                        logger.Info($"Not exist {communicationFilePath}");
                        //System.IO.File.AppendAllText(resultMedoPath, "No import " + dir.Name + " not exist xml file\n");
                        continue;
                    }

                    #endregion

                    #region Parse communication file

                    //Парсинг файла коммуникации (транспортный контейнер)
                    XDocument communicationFile = XDocument.Load(communicationFilePath);

                    var xsdVersion = MedoHelper.GetXsdVersion(communicationFile);

                    logger.Info($"xsd version: {xsdVersion}");

                    IValidationResultBuilder validationResults = new ValidationResultBuilder();

                    bool xmlValidation = false;

                    switch (xsdVersion)
                    {
                        case XsdVersion.NewVersion:
                            xmlValidation = MedoHelper.CheckXml(
                                communicationFile,
                                xsdVersion,
                                XmlType.Message,
                                validationResults,
                                ConfigurationManager.Settings.TryGet<string>(MedoConst.CommunicationSettingName271));
                            break;
                        case XsdVersion.OldVersion:
                            xmlValidation = MedoHelper.CheckXml(
                                communicationFile,
                                xsdVersion,
                                XmlType.Message,
                                validationResults,
                                ConfigurationManager.Settings.TryGet<string>(MedoConst.CommunicationSettingName));
                            break;
                        case XsdVersion.Version22:
                            xmlValidation = true;
                            break;
                        case XsdVersion.Version25:
                            xmlValidation = true;
                            break;
                        case XsdVersion.Version26:
                            xmlValidation = true;
                            break;
                        case XsdVersion.Version20:
                            xmlValidation = true;
                            break;
                        default:
                            logger.Error($"Version |{xsdVersion.ToString("g")}| is not supported");
                            //System.IO.File.AppendAllText(resultMedoPath, "No import " + dir.Name + " Old version is not supported\n");
                            resultType = ProcessingResult.INCORRECTVERSION;
                            continue;
                    }

                    if (!xmlValidation)
                    {
                        logger.Error("Container didn't pass validation by scheme");
                        //System.IO.File.AppendAllText(resultMedoPath, $"No import  {dir.Name} "+ 
                        //    $"Container didn't pass validation by scheme\n Validation result {validationResults.ToString()}");
                        continue;
                    }

                    //получение корневого элемента
                    XElement rootElementCommunication = communicationFile.Root;

                    XNamespace communicationNamespace = rootElementCommunication.Name.Namespace;

                    if (communicationNamespace == null)
                    {
                        logger.Info($"No communicationNamespace");
                        //System.IO.File.AppendAllText(resultMedoPath, "No import " + dir.Name + " xml doesn't have namespace\n");
                        continue;
                    }

                    #endregion

                    #region Checking partner

                    // Поиск контрагента
                    MedoPartner partner = null;

                    var sourceMedo = XmlParse.GetDescendantAttributeValue(rootElementCommunication, "source", "uid");

                    if (!string.IsNullOrWhiteSpace(sourceMedo))
                    {
                        partner = await GetPartnerAsync(sourceMedo.ToLower(), dbScope);
                    }

                    if (partner == null)
                    {
                        logger.Info($"Partner not found dirName {dir.FullName}");
                        //System.IO.File.AppendAllText(resultMedoPath, "No import " + dir.Name + " Partner not found\n");
                    }

                    #endregion

                    #region Check if acknowledgment

                    var acknowledgeElement = rootElementCommunication.Descendants(communicationNamespace + "acknowledgment").FirstOrDefault();

                    if (acknowledgeElement != null)
                    {
                        resultType = ProcessingResult.NotificationError;

                        //if (partner == null)
                        //{
                        //    logger.Info($"Partner not found {dir.Name}");
                        //    continue;
                        //}

                        //Guid? ackIDNotif = await GetMesIDCardNotif(partner.ID, cardIDNotif, dbScope);

                        //if (mesIDNotif == null)
                        //{
                        //    mesIDNotif = Guid.NewGuid();
                        //    logger.Info($"MessageID not found by partner {partner.ID} cardID: {cardIDNotif} new mesID {mesIDNotif} dir {dir.Name}");
                        //}
                        //else
                        //{
                        //    await UpdateMedoJournalStateAsync((Guid)mesIDNotif, status, dbScope);
                        //}
                    }

                    #endregion

                    #region Check if notification

                    var notificationElement = rootElementCommunication.Descendants(communicationNamespace + "notification").FirstOrDefault();

                    if (notificationElement != null)
                    {
                        resultType = ProcessingResult.NotificationError;

                        if (partner == null)
                        {
                            logger.Info($"Partner not found {dir.Name}");
                            continue;
                        }

                        var foundationElement = notificationElement.Descendants(communicationNamespace + "foundation").FirstOrDefault();

                        if (foundationElement == null)
                        {
                            logger.Info($"No foundation element in xml {dir.Name}");
                            continue;
                        }

                        var foundationNumElement = foundationElement.Descendants(communicationNamespace + "num").FirstOrDefault();

                        var num = XmlParse.GetDescendantValue(foundationNumElement, "number");
                        var date = XmlParse.GetDescendantValue(foundationNumElement, "date");

                        //Guid? cardIDNotif = await GetCardByRegNum(num.ToLower(), dbScope);

                        Guid? cardIDNotif = null;

                        List<MedoCardIDDocDate> cardIDDocDate = await GetCardByRegNum(num.ToLower(), dbScope);

                        if (cardIDDocDate.Count == 0)
                        {
                            logger.Info($"Card not found by reg number: {num}");
                            continue;
                        }

                        foreach(var c in cardIDDocDate)
                        {
                            if (c.DocDate.ToString("yyyy-MM-dd") == date.Trim(' '))
                            {
                                cardIDNotif = c.ID; 
                                break;
                            }
                        }

                        if (cardIDNotif == null)
                        {
                            logger.Info($"Card not found by reg number: {num} reg date: {date}");
                            continue;
                        }

                        Card cardNotif = await GetCardAsync((Guid)cardIDNotif, permissionsProvider, cardRepository, cancellationToken);

                        if (cardNotif == null)
                        {
                            logger.Info($"Can't get card by ID {cardIDNotif} dir {dir.Name}");
                            continue;
                        }

                        //if (cardNotif.TypeID != Guid.Parse("88e3ed72-100c-4fc8-8527-6558623aa0f7"))
                        //{
                        //    logger.Info($"Card type not OutgoingAGIP {cardIDNotif} dir {dir.Name}");
                        //    continue;
                        //}

                        string status = XmlParse.GetAttrValue(notificationElement, "type");
                        if (string.IsNullOrWhiteSpace(status))
                        {
                            logger.Info($"Fail to get notificationType {cardIDNotif} dir {dir.Name}");
                            continue;
                        }

                        Guid mesIDNotif = Guid.NewGuid();

                        //Guid? mesIDNotif = await GetMesIDCardNotif(partner.ID, cardIDNotif, dbScope);

                        //if (mesIDNotif == null)
                        //{
                        //    mesIDNotif = Guid.NewGuid();
                        //    logger.Info($"MessageID not found by partner {partner.ID} cardID: {cardIDNotif} new mesID {mesIDNotif} dir {dir.Name}");
                        //}
                        //else
                        //{
                        //    await UpdateMedoJournalStateAsync((Guid)mesIDNotif, status, dbScope);
                        //}

                        int newReportEntry = await InsertNewMedoReportEntryAsync(cardNotif.ID, mesIDNotif, partner, status, dbScope);

                        if (newReportEntry == 0)
                        {
                            logger.Info($"Fail to insert new MedoJournalReport Entry {dir.Name}");
                            continue;
                        }

                        var documentAcceptedElement = rootElementCommunication.Descendants(communicationNamespace + "documentAccepted").FirstOrDefault();

                        if (documentAcceptedElement != null)
                        {
                            var acceptedNumElement = documentAcceptedElement.Element(communicationNamespace + "num");

                            var acceptedNum = XmlParse.GetDescendantValue(acceptedNumElement, "number");
                            var acceptedDate = XmlParse.GetDescendantValue(acceptedNumElement, "date");

                            //var mesUid = XmlParse.GetDescendantAttributeValue(rootElementCommunication, "notification", "uid");

                            //var notificationGuid = Guid.Parse(mesUid);

                            //if (await GetCardIDByMesIDAsync(notificationGuid, dbScope) == null)
                            //{
                            //    logger.Info($"Not found card by messageID : {mesUid}");
                            //    continue;
                            //}

                            DateTime regDate;

                            if (!DateTime.TryParseExact(acceptedDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out regDate))
                            {
                                logger.Info($"Incorrect date: {acceptedDate}");
                                continue;
                            }

                            string regNum = $"{acceptedNum} от {regDate.ToString("dd.MM.yyyy")}";

                            await UpdateMedoJournalAcceptedAsync(mesIDNotif, regNum, dbScope);

                            //continue;
                        }

                        var documentRefusedElement = rootElementCommunication.Descendants(communicationNamespace + "documentRefused").FirstOrDefault();

                        if (documentRefusedElement != null)
                        {
                            var mesUid = XmlParse.GetDescendantAttributeValue(rootElementCommunication, "notification", "uid");

                            //var notificationGuid = Guid.Parse(mesUid);

                            //if (await GetCardIDByMesIDAsync(notificationGuid, dbScope) == null)
                            //{
                            //    logger.Info($"Not found card by messageID : {mesUid}");
                            //    continue;
                            //}

                            var notificationComment = XmlParse.GetDescendantValue(notificationElement, "comment");

                            var notificationReason = XmlParse.GetDescendantValue(documentRefusedElement, "reason");

                            StringBuilder sb = new StringBuilder();
                            sb.Append(notificationReason);
                            if (notificationComment != null)
                            {
                                sb.Append(' ');
                                sb.Append(notificationComment);
                            }

                            await UpdateMedoJournalRefusedAsync(mesIDNotif, sb.ToString(), dbScope);

                            //continue;
                        }

                        resultType = ProcessingResult.NOTIFICATION;

                        logger.Info($"Created MedoJournalReport entry status: {status} for card {cardNotif.Sections[SchemeInfo.DocumentCommonInfo].Fields[SchemeInfo.DocumentCommonInfo.FullNumber]}");
                    
                        continue;
                    }

                    #endregion

                    #region Get MainMesID

                    var headerUid = XmlParse.GetDescendantAttributeValue(rootElementCommunication, "header", "uid");

                    Guid MainMesID = Guid.Empty;

                    if (!string.IsNullOrWhiteSpace(headerUid))
                    {
                        MainMesID = Guid.Parse(headerUid);
                    }

                    //Guid? checkGuid = await GetCardIDByMesIDAsync(MainMesID, dbScope);

                    //if (checkGuid != null)
                    //{
                    //    logger.Info($"Container has been imported extGuid {checkGuid}");
                    //    System.IO.File.AppendAllText(resultMedoPath, "No import " + dir.Name + " extguid exist\n");
                    //    continue;
                    //}

                    #endregion

                    #region Card creation

                    Card card;
                    Guid cardID;

                    #endregion

                    #region Check container in communication file

                    // Проверка есть ли контейнер
                    bool isSigned = true;

                    string containerFileName = "";

                    var communicationContainer = communicationFile.Descendants(communicationNamespace + "container").FirstOrDefault();

                    // Если значение по умолчанию
                    if (communicationContainer == null)
                    {
                        logger.Info($"Container without sign {dir.Name}");

                        isSigned = false;
                    }
                    else
                    {
                        containerFileName = XmlParse.GetDescendantValue(communicationContainer, "body");

                        if (string.IsNullOrWhiteSpace(containerFileName))
                        {
                            logger.Info($"No container value");
                            //System.IO.File.AppendAllText(resultMedoPath, "No import " + dir.Name + " No container value\n");
                            continue;
                        }
                    }

                    #endregion

                    #region notSigned

                    // Заполнение полей карточки и прикрепление файлов без ЭП
                    if (!isSigned)
                    {
                        //Есть ли уже карточка с этим документом
                        var hasDocument = communicationFile.Descendants(communicationNamespace + "document").FirstOrDefault();

                        if (hasDocument == null)
                        {
                            logger.Info($"Отсутствует элемент document {dir.Name}");
                            //System.IO.File.AppendAllText(resultMedoPath, "No import " + dir.Name + " missing document element in communication\n");
                            continue;
                        }

                        var cardIDDirName = await GetCardByDirName(dir.Name.ToLower(), dbScope);

                        if (cardIDDirName != null)
                        {
                            logger.Error($"Документ был обработан ранее {dir.Name}");
                            continue;
                        }

                        #region Card creation

                        card = await NewCard(docType, typeID, cardRepository);

                        if (card == null)
                        {
                            logger.Error("Не удалось создать карточку документа");
                            //System.IO.File.AppendAllText(resultMedoPath, "No import " + dir.Name + " can't create card\n");
                            continue;
                        }

                        cardID = card.ID;

                        #endregion

                        var signatoryElement = hasDocument.Descendants(communicationNamespace + "signatory").FirstOrDefault();

                        if (signatoryElement != null)
                        {
                            var post = XmlParse.GetDescendantValue(signatoryElement, "post");
                            var person = XmlParse.GetDescendantValue(signatoryElement, "person");

                            string signatory = "";

                            if (string.IsNullOrEmpty(person) && !string.IsNullOrEmpty(post))
                            {
                                signatory = post;
                            }
                            else if (!string.IsNullOrEmpty(post) && !string.IsNullOrEmpty(person))
                            {
                                signatory = $"{person} {post}";
                            }
                            else if (string.IsNullOrEmpty(post) && !string.IsNullOrEmpty(person))
                            {
                                signatory = person;
                            }

                            if (!string.IsNullOrWhiteSpace(signatory))
                            {
                                card.Sections["DocumentCommonInfo"].Fields["SignedT"] = signatory;
                            }
                        }

                        //Поиск documentKind
                        //Переделать, версия 2.7?
                        var documentKindElement = hasDocument.Descendants(communicationNamespace + "kind").FirstOrDefault();

                        if (documentKindElement != null)
                        {
                            DocTypeMEDO checkCommunicationDocumentKind = null;

                            if (documentKindElement.Attribute(communicationNamespace + "id")?.Value != null)
                            {
                                checkCommunicationDocumentKind = await GetMedoDocTypeAsync(documentKindElement.Attribute(communicationNamespace + "id").Value, dbScope);
                            }

                            if (checkCommunicationDocumentKind != null)
                            {
                                logger.Info($"Found document kind id: {checkCommunicationDocumentKind.ID} Name: {checkCommunicationDocumentKind.Name}");
                                card.Sections["DocumentCommonInfo"].Fields["DocTypeMEDOID"] = checkCommunicationDocumentKind.ID;
                                card.Sections["DocumentCommonInfo"].Fields["DocTypeMEDOName"] = checkCommunicationDocumentKind.Name;
                                card.Sections["DocumentCommonInfo"].Fields["DocTypeMEDOIndex"] = checkCommunicationDocumentKind.Index;
                            }
                            else if (!string.IsNullOrEmpty(documentKindElement.Value))
                            {
                                logger.Info($"Not found document kind in xml");
                                card.Sections["DocumentCommonInfo"].Fields["DocTypeMEDOName"] = documentKindElement.Value;
                            }
                            else if (checkCommunicationDocumentKind == null)
                            {
                                logger.Info($"Not found document kind in DB");
                            }
                        }

                        //Добавление темы в карточку
                        var annotationElement = communicationFile.Descendants(communicationNamespace + "annotation").FirstOrDefault();

                        if (annotationElement != null)
                        {
                            card.Sections["DocumentCommonInfo"].Fields["Subject"] = annotationElement.Value;
                        }

                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeID"] = Guid.Parse(MedoDeliveryType);
                        card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeName"] = "МЭДО";

                        var numElement = communicationFile.Descendants(communicationNamespace + "num").FirstOrDefault();

                        if (numElement != null)
                        {
                            card.Sections["DocumentCommonInfo"].Fields["OutgoingNumber"] = numElement.Element(communicationNamespace + "number").Value;
                            card.Sections["DocumentCommonInfo"].Fields["OutgoingDate"] = DateTime.ParseExact(numElement.Element(communicationNamespace + "date").Value, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddDays(1);
                        }

                        List<string> attachmentsfToCard = new List<string>();

                        var filesElement = rootElementCommunication.Element(communicationNamespace + "files");

                        var mainFileElement = filesElement.Descendants(communicationNamespace + "file").FirstOrDefault();

                        if (mainFileElement == null)
                        {
                            logger.Error("Отсутствует описание документа в xml");
                            //System.IO.File.AppendAllText(resultMedoPath, "No import " + dir.Name + " no file element in xml\n");
                            continue;
                        }

                        var mainFileName = mainFileElement.Attribute(communicationNamespace + "localName").Value;

                        List<string> mainFileSignatures= new List<string>();

                        var signaturesElements = mainFileElement.Descendants(communicationNamespace + "signerInfo");

                        if (signaturesElements.Count() > 0)
                        {
                            foreach (var signature in signaturesElements)
                            {
                                mainFileSignatures.Add(signature.Attribute(communicationNamespace + "signature").Value);
                            }
                        }

                        var filesCollection = filesElement.Descendants(communicationNamespace + "file");

                        foreach (var f in filesCollection)
                        {
                            var fileLocalName = f.Attribute(communicationNamespace + "localName").Value;

                            if (fileLocalName == mainFileName)
                            {
                                continue;
                            }

                            attachmentsfToCard.Add(fileLocalName);
                        }

                        card.Sections["DocumentCommonInfo"].Fields["DocDate"] = null;
                        card.Sections["DocumentCommonInfo"].Fields["MEDOFolderName"] = dir.Name;

                        await using (var fileContainer = await manager.CreateContainerAsync(card))
                        {
                            logger.Info($"Save file: {Path.Combine(dir.FullName, mainFileName)}");
                            var(newMainFile, newMainFileValidationResult) = await fileContainer
                            .FileContainer
                            .BuildFile(mainFileName)
                            .SetCategory(new FileCategory(Guid.Parse("723bf3bd-31c2-4269-a76a-d323e082a2f1"), "Документ"))
                            .SetContent(Path.Combine(dir.FullName, mainFileName))
                            .AddWithNotificationAsync();

                            foreach (var s in mainFileSignatures)
                            {
                                await SignFile(Path.Combine(dir.FullName, s), newMainFile.TryGetActualVersion(), cadesManager, cancellationToken);
                            }

                            foreach (var s in attachmentsfToCard)
                            {
                                logger.Info($"Save file: {Path.Combine(dir.FullName, s)}");
                                await fileContainer
                                .FileContainer
                                .BuildFile(s)
                                .SetCategory(new FileCategory(Guid.Parse("cc650bc9-3e45-4480-9274-3bb3b13d20d5"), "Приложение"))
                                .SetContent(Path.Combine(dir.FullName, s))
                                .AddWithNotificationAsync();
                            }

                            logger.Info($"Save file: {Path.Combine(dir.FullName, communicationFileName)}");
                            await fileContainer
                            .FileContainer
                            .BuildFile(communicationFileName)
                            .SetContent(Path.Combine(dir.FullName, communicationFileName))
                            .AddWithNotificationAsync();

                            var storeResponse = await fileContainer.StoreAsync();
                            if (!storeResponse.ValidationResult.IsSuccessful())
                            {
                                ValidationResult result = storeResponse.ValidationResult.Build();
                                logger.LogResult(result); // пишем ошибку или другие сообщения в лог
                                logger.Info("Files for card with UID: " + " attached with error");
                            }
                            else
                            {
                                logger.Info("Files for card with UID: " + cardID + " attached succesful");
                            }

                        }

                        //okDirs.Add(dir.Name);

                        //System.IO.File.AppendAllText(resultMedoPath, "Ok import " + dir.Name + "\n");

                        createdCardsIds.Add(cardID);

                        resultType = partner == null ? ProcessingResult.NOPARTNER : ProcessingResult.SUCCESS;

                        int insertResult = await InsertNewMedoEntryAsync(cardID, MainMesID, partner, xsdVersion, dbScope);

                        //MedoHelper.CreateAcknowledgMes(xsdVersion, true, MainMesID)

                        if (partner != null)
                        {
                            await AddCorrespondentToCardAsync(cardID, partner, dbScope);

                            AcknowledgmentInfo acknowledgmentInfo = new AcknowledgmentInfo()
                            {
                                DbScope = dbScope,
                                CardID = cardID,
                                MainMesID = MainMesID,
                                MedoPartnerID = partner.ID,
                                MedoPartnerName = partner.Name
                            };

                            CreateAcknowledgmentNotification notification = 
                                new CreateAcknowledgmentNotification(acknowledgmentInfo);

                            await notification.InitializeAsync();
                        }

                        logger.Info($".{insertResult}");

                        continue;
                    }

                    #endregion

                    #region ifSigned

                    logger.Info($"Container with sign {dir.Name}");

                    //Работа с архивом при наличии ЭП
                    var zipPath = Path.Combine(dir.FullName, containerFileName);

                    if (!System.IO.File.Exists(zipPath))
                    {
                        logger.Info($"Zip folder {zipPath} not exists for container {dir.Name}");
                        //System.IO.File.AppendAllText(resultMedoPath, $"No import {dir.Name} Zip folder {zipPath} not exists\n");
                        continue;
                    }

                    logger.Info($"Zip folder {zipPath} exists for container {dir.Name}");
                    
                    using var zipContainer = ZipFile.Open(zipPath, ZipArchiveMode.Read);

                    string tempFilesFromZipPath = Path.Combine(dir.FullName, $"tempFilesFromZip{DateTime.Now}");

                    zipContainer.ExtractToDirectory(tempFilesFromZipPath);

                    var tempFilesFromZipDir = new DirectoryInfo(tempFilesFromZipPath);

                    if (!tempFilesFromZipDir.Exists)
                    {
                        logger.Error($"Extraction error. Path: {tempFilesFromZipDir.FullName}");
                        continue;
                    }

                    XDocument passport = XDocument.Load(Path.Combine(tempFilesFromZipDir.FullName, "passport.xml"));

                    XElement passportRootElement = passport.Root;

                    XNamespace passportNamespace = passportRootElement.Name.Namespace.NamespaceName;

                    logger.Info($"root namespace: {passportNamespace}");

                    var checkClassification = passportRootElement
                        .Descendants(passportNamespace + "classification")
                        .FirstOrDefault();

                    if (checkClassification != null)
                    {
                        if (checkClassification.Attribute(passportNamespace + "id") != null &&
                            (checkClassification.Attribute(passportNamespace + "id").Value == "DC00000001"
                            || checkClassification.Value == "Информация ограниченного распространения"))
                        {
                            logger.Info("Classified message " + dir.Name);
                            //classifiedDirs.Add(dir.Name);

                            resultType = ProcessingResult.CLASSIFIED;

                            logger.Info($"Classified container {dir.Name}");
                            //System.IO.File.AppendAllText(resultMedoPath, $"No import {dir.Name} Classified container\n");
                            continue;
                        }
                    }

                    if (passportNamespace == null)
                    {
                        logger.Info($"Null passportNamespace for container {dir.Name}");
                        //System.IO.File.AppendAllText(resultMedoPath, $"No import {dir.Name} Null passportNamespace\n");
                        continue;
                    }

                    var cardIDDir = await GetCardByDirName(dir.Name.ToLower(), dbScope);

                    if (cardIDDir != null)
                    {
                        logger.Error($"Документ был обработан ранее {dir.Name}");
                        continue;
                    }

                    #region Card creation

                    card = await NewCard(docType, typeID, cardRepository);

                    if (card == null)
                    {
                        logger.Error("Не удалось создать карточку документа");
                        //System.IO.File.AppendAllText(resultMedoPath, "No import " + dir.Name + " can't create card\n");
                        continue;
                    }

                    cardID = card.ID;

                    #endregion

                    var reqElement = passport.Descendants(passportNamespace + "requisites").FirstOrDefault();

                    var passportDocumentKind = reqElement.Descendants(passportNamespace + "documentKind").FirstOrDefault();

                    if (passportDocumentKind != null)
                    {
                        DocTypeMEDO checkPassportDocumentKind = null;

                        if (passportDocumentKind.Attribute(passportNamespace + "id") != null)
                        {
                            logger.Info($"Try get document kind id: {passportDocumentKind.Attribute(passportNamespace + "id").Value}");
                            checkPassportDocumentKind = await GetMedoDocTypeAsync(passportDocumentKind.Attribute(passportNamespace + "id").Value, dbScope);
                        }

                        if (checkPassportDocumentKind != null)
                        {
                            logger.Info($"Found document kind id: {checkPassportDocumentKind.ID} Name: {checkPassportDocumentKind.Name}");
                            card.Sections["DocumentCommonInfo"].Fields["DocTypeMEDOID"] = checkPassportDocumentKind.ID;
                            card.Sections["DocumentCommonInfo"].Fields["DocTypeMEDOName"] = checkPassportDocumentKind.Name;
                            card.Sections["DocumentCommonInfo"].Fields["DocTypeMEDOIndex"] = checkPassportDocumentKind.Index;
                        }
                        else if (!string.IsNullOrWhiteSpace(passportDocumentKind.Value))
                        {
                            logger.Info($"Not found document kind in xml");
                            card.Sections["DocumentCommonInfo"].Fields["DocTypeMEDOName"] = passportDocumentKind.Value;
                        }
                        else if (checkPassportDocumentKind == null)
                        {
                            logger.Info($"Not found document kind in DB");
                        }
                    }

                    var annotation = XmlParse.GetDescendantValue(reqElement, "annotation");

                    if (annotation != null)
                    {
                        card.Sections["DocumentCommonInfo"].Fields["Subject"] = annotation;
                    }

                    var authorsElement = passport.Descendants(passportNamespace + "authors").FirstOrDefault();

                    var outNumDate = authorsElement.Descendants(passportNamespace + "registration").FirstOrDefault();

                    if (outNumDate == null)
                    {
                        logger.Error($"Not exist registration element in xml {dir.Name}");
                        //System.IO.File.AppendAllText(resultMedoPath, $"No import {dir.Name} Not exist registration element in xml\n");
                        continue;
                    }

                    //Получение внешн. номера
                    var outNum = XmlParse.GetDescendantValue(outNumDate, "number");

                    if (string.IsNullOrWhiteSpace(outNum))
                    {
                        logger.Error($"Not exist registration number element in xml {dir.Name}");
                        //System.IO.File.AppendAllText(resultMedoPath, $"No import {dir.Name} Not exist registration number element in xml\n");
                        continue;
                    }

                    card.Sections["DocumentCommonInfo"].Fields["OutgoingNumber"] = outNum;

                    //Получение внешн. даты
                    var outDate = XmlParse.GetDescendantValue(outNumDate, "date");

                    if (string.IsNullOrWhiteSpace(outDate))
                    {
                        logger.Error($"Not exist registration date element in xml {dir.Name}");
                        //System.IO.File.AppendAllText(resultMedoPath, $"No import {dir.Name} Not exist registration date element in xml\n");
                        continue;
                    }

                    DateTime checkValidDate;
                    var isValidDate = DateTime.TryParseExact(outDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out checkValidDate);

                    if (!isValidDate)
                    {
                        logger.Error($"Incorrect format for registration date {dir.Name} input date: {checkValidDate}");
                        //System.IO.File.AppendAllText(resultMedoPath, $"No import {dir.Name} Incorrect format for registration date {checkValidDate}\n");
                        continue;
                    }

                    card.Sections["DocumentCommonInfo"].Fields["OutgoingDate"] = DateTime.ParseExact(outDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddDays(1);

                    card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeID"] = Guid.Parse(MedoDeliveryType);
                    card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeName"] = "МЭДО";


                    var mainDocumentName = passport
                        .Descendants(passportNamespace + "document")
                        .FirstOrDefault()
                        .Attribute(passportNamespace + "localName")
                        .Value;

                    IEnumerable<string> documentSignatureNames = null;

                    List<StampInfo> stampInfos = new List<StampInfo>();
                    var registrationStamps = passport.Descendants(passportNamespace + "registrationStamp");

                    if (registrationStamps.Count() > 0)
                    {
                        foreach (var regStamp in registrationStamps)
                        {
                            if (regStamp.Attribute(passportNamespace + "localName") == null)
                            {
                                continue;
                            }

                            StampInfo stampInfo = new StampInfo();

                            stampInfo.stampName = regStamp.Attribute(passportNamespace + "localName").Value;

                            stampInfo.page = Convert.ToInt32(regStamp.Descendants(passportNamespace + "page").FirstOrDefault().Value);
                            stampInfo.topLeftX = Convert.ToInt32(regStamp.Descendants(passportNamespace + "x").FirstOrDefault().Value);
                            stampInfo.topLeftY = Convert.ToInt32(regStamp.Descendants(passportNamespace + "y").FirstOrDefault().Value);
                            stampInfo.dimensionW = Convert.ToInt32(regStamp.Descendants(passportNamespace + "w").FirstOrDefault().Value);
                            stampInfo.dimensionH = Convert.ToInt32(regStamp.Descendants(passportNamespace + "h").FirstOrDefault().Value);

                            stampInfos.Add(stampInfo);
                        }
                    }

                    var documentSignatures = passport.Descendants(passportNamespace + "documentSignature");

                    if (documentSignatures.Count() > 0)
                    {
                        documentSignatureNames = documentSignatures.Select(x => x.Attribute(passportNamespace + "localName").Value);

                        foreach (var sign in documentSignatures)
                        {
                            StampInfo stampInfo = new StampInfo();

                            stampInfo.stampName = sign.Descendants(passportNamespace + "signatureStamp").FirstOrDefault().Attribute(passportNamespace + "localName").Value;
                            stampInfo.page = Convert.ToInt32(sign.Descendants(passportNamespace + "page").FirstOrDefault().Value);
                            stampInfo.topLeftX = Convert.ToInt32(sign.Descendants(passportNamespace + "x").FirstOrDefault().Value);
                            stampInfo.topLeftY = Convert.ToInt32(sign.Descendants(passportNamespace + "y").FirstOrDefault().Value);
                            stampInfo.dimensionW = Convert.ToInt32(sign.Descendants(passportNamespace + "w").FirstOrDefault().Value);
                            stampInfo.dimensionH = Convert.ToInt32(sign.Descendants(passportNamespace + "h").FirstOrDefault().Value);

                            stampInfos.Add(stampInfo);
                        }
                    }
                    else
                    {
                        logger.Info("No signatures for document");
                    }

                    var documentSignElements = passport.Descendants(passportNamespace + "sign");

                    foreach (var documentSign in documentSignElements)
                    {
                        var signType = XmlParse.GetDescendantAttributeValue(documentSign, "documentSignature", "type");

                        if (!string.IsNullOrEmpty(signType) && signType.ToLower().Contains("утверждающая", StringComparison.Ordinal))
                        {
                            var post = XmlParse.GetDescendantValue(documentSign, "post");
                            var name = XmlParse.GetDescendantValue(documentSign, "name");

                            string signatory = "";

                            if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(post))
                            {
                                signatory = post;
                            }
                            else if (!string.IsNullOrEmpty(post) && !string.IsNullOrEmpty(name))
                            {
                                signatory = $"{name} {post}";
                            }
                            else if (string.IsNullOrEmpty(post) && !string.IsNullOrEmpty(name))
                            {
                                signatory = name;
                            }

                            if (!string.IsNullOrWhiteSpace(signatory))
                            {
                                card.Sections["DocumentCommonInfo"].Fields["SignedT"] = signatory;
                                break;
                            }
                        }
                    }

                    List<string> attachmentsFilesToSignedCard = new List<string>();
                    Dictionary<string, string> attachmentFilesWithSignToSignedCard= new Dictionary<string, string>();

                    var attachments = passport.Descendants(passportNamespace + "attachment");

                    foreach (var att in attachments)
                    {
                        var attName = XmlParse.GetAttrValue(att, "localName");

                        if (string.IsNullOrEmpty(attName))
                        {
                            continue;
                        }

                        attachmentsFilesToSignedCard.Add(attName);
                        attachmentFilesWithSignToSignedCard.Add(attName, "");

                        var signature = XmlParse.GetDescendantAttributeValue(att, "signature", "localName");

                        if (!string.IsNullOrEmpty(attName))
                        {
                            attachmentFilesWithSignToSignedCard[attName] = signature;
                        }
                    }

                    MemoryStream inMemoryCopy = new MemoryStream();
                    using (FileStream fs = System.IO.File.OpenRead(Path.Combine(tempFilesFromZipPath, mainDocumentName)))
                    {
                        fs.CopyTo(inMemoryCopy);
                    }

                    using var pdfFile = PdfReader.Open(inMemoryCopy, PdfDocumentOpenMode.Modify);
                    
                    foreach (var stampInfo in stampInfos)
                    {
                        MemoryStream inMemoryStamp = new MemoryStream();
                        using (FileStream fsStamp = System.IO.File.OpenRead(Path.Combine(tempFilesFromZipPath, stampInfo.stampName)))
                        {
                            using var bitmap = new Bitmap(Path.Combine(tempFilesFromZipPath, stampInfo.stampName));
                            ///
                            Image img = Image.FromFile(Path.Combine(tempFilesFromZipPath, stampInfo.stampName));

                            Bitmap bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                g.Clear(System.Drawing.Color.White);
                                g.DrawImage(img, new Rectangle(new Point(), img.Size), new Rectangle(new Point(), img.Size), GraphicsUnit.Pixel);
                            }
                            bmp.Save(inMemoryStamp, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }

                        DrawStamp(pdfFile, stampInfo, inMemoryStamp, Path.Combine(tempFilesFromZipPath, stampInfo.stampName));
                    }

                    string newName = mainDocumentName;

                    string ext = newName.Substring(newName.LastIndexOf('.'));
                    string fileName = newName.Substring(0, newName.LastIndexOf("."));

                    string stampFileName = fileName + "_Копия документа с номером и штампом ЭП" + ext;

                    pdfFile.Save(Path.Combine(tempFilesFromZipPath, stampFileName));
                    attachmentFilesWithSignToSignedCard.Add(stampFileName, "");

                    //logger.Info($"stampFileName: {stampFileName}");

                    //Прикрепление файлов в карточке
                    //Обходим директорию с файлами, добавляем файлы в карточку
                    await using (var fileContainer = await manager.CreateContainerAsync(card))
                    {
                        //Сохранение документа
                        logger.Info($"Save file: {Path.Combine(tempFilesFromZipPath, mainDocumentName)}");
                        var (newMainFile, newMainFileValidationResult) = await fileContainer
                        .FileContainer
                        .BuildFile(mainDocumentName)
                        .SetCategory(new FileCategory(Guid.Parse("723bf3bd-31c2-4269-a76a-d323e082a2f1"), "Документ"))
                        .SetContent(Path.Combine(tempFilesFromZipPath, mainDocumentName))
                        .AddWithNotificationAsync();

                        var toSignVersion = newMainFile.TryGetActualVersion();

                        //foreach (var fileToCard in attachmentsFilesToSignedCard)
                        //{
                        //    logger.Info($"Save file: {Path.Combine(tempFilesFromZipPath, fileToCard)}");
                        //    var (newFile, newFileValidationResult) = await fileContainer
                        //    .FileContainer
                        //    .BuildFile(fileToCard)
                        //    .SetCategory(new FileCategory(Guid.Parse("cc650bc9-3e45-4480-9274-3bb3b13d20d5"), "Приложение"))
                        //    .SetContent(Path.Combine(tempFilesFromZipPath, fileToCard))
                        //    .AddWithNotificationAsync();
                        //}

                        foreach (var file in attachmentFilesWithSignToSignedCard)
                        {
                            logger.Info($"Save file: {Path.Combine(tempFilesFromZipPath, file.Key)}");
                            var (newFile, newFileValidationResult) = await fileContainer
                            .FileContainer
                            .BuildFile(file.Key)
                            .SetCategory(new FileCategory(Guid.Parse("cc650bc9-3e45-4480-9274-3bb3b13d20d5"), "Приложение"))
                            .SetContent(Path.Combine(tempFilesFromZipPath, file.Key))
                            .AddWithNotificationAsync();

                            if (!string.IsNullOrWhiteSpace(file.Value))
                            {
                                await SignFile(Path.Combine(tempFilesFromZipPath, file.Value), newFile.TryGetActualVersion(), cadesManager, cancellationToken);
                            }
                        }

                        logger.Info($"Save file: {Path.Combine(tempFilesFromZipPath, "passport.xml")}");
                        var (newPassportFile, newPassportFileValidationResult) = await fileContainer
                        .FileContainer
                        .BuildFile("passport.xml")
                        .SetContent(Path.Combine(tempFilesFromZipPath, "passport.xml"))
                        .AddWithNotificationAsync();

                        documentSignatureNames = documentSignatureNames.Count() > 0 ? documentSignatureNames.Distinct() : null;

                        if (documentSignatureNames != null)
                        {
                            foreach (var sign in documentSignatureNames)
                            {
                                logger.Info($"toSign Signature local name: {sign}");
                            }

                            foreach (var documentSignatureName in documentSignatureNames)
                            {
                                logger.Info($"beforeSign {documentSignatureName}");

                                bool signFile = await SignFile(Path.Combine(tempFilesFromZipPath, documentSignatureName), toSignVersion, cadesManager, cancellationToken);
                            }
                        }

                        card.Sections["DocumentCommonInfo"].Fields["DocDate"] = null;
                        card.Sections["DocumentCommonInfo"].Fields["MEDOFolderName"] = dir.Name;

                        var storeResponse = await fileContainer.StoreAsync();
                        if (!storeResponse.ValidationResult.IsSuccessful())
                        {
                            ValidationResult result = storeResponse.ValidationResult.Build();
                            logger.LogResult(result); // пишем ошибку или другие сообщения в лог
                            logger.Info("Files for card with UID: " + " attached with error");
                        }
                        else
                        {
                            logger.Info("Files for card with UID: " + cardID + " attached succesful");
                        }

                        //okDirs.Add(dir.Name);

                        //System.IO.File.AppendAllText(resultMedoPath, "Ok import " + dir.Name + "\n");

                        createdCardsIds.Add(cardID);

                        resultType = partner == null ? ProcessingResult.NOPARTNER : ProcessingResult.SUCCESS;

                        int insertResult = await InsertNewMedoEntryAsync(cardID, MainMesID, partner, xsdVersion, dbScope);

                        if(partner != null)
                        {
                            await AddCorrespondentToCardAsync(cardID, partner, dbScope);

                            AcknowledgmentInfo acknowledgmentInfo = new AcknowledgmentInfo()
                            {
                                DbScope = dbScope,
                                CardID = cardID,
                                MainMesID = MainMesID,
                                MedoPartnerID = partner.ID,
                                MedoPartnerName = partner.Name
                            };

                            CreateAcknowledgmentNotification notification =
                                new CreateAcknowledgmentNotification(acknowledgmentInfo);

                            await notification.InitializeAsync();
                        }

                        logger.Info($".{insertResult}");

                        Directory.Delete(tempFilesFromZipPath, true);
                    }

                    #endregion
                
                }
                catch (Exception e)
                {
                    logger.Error($"Error message: {e.Message}\nstacktrace: {e.StackTrace}");
                    //System.IO.File.AppendAllText(resultMedoPath, "No import " + dir.Name + " get exception\n");

                    if (resultType == ProcessingResult.NotificationError)
                    {
                        continue;
                    }
                    else if (resultType == ProcessingResult.NOTIFICATION)
                    {
                        resultType = ProcessingResult.NotificationError;
                        continue;
                    }
                    else if (resultType != ProcessingResult.ERROR)
                    {
                        resultType= ProcessingResult.ERROR;
                    }

                    continue;
                }
            }

            if (directoriesLength > 0)
            {
                MoveDirAfterProcessing(lastDir, resultType);
            }
            
            logger.Info("-----------------------------------------------");
            logger.Info($"Before import: {directoriesLength} directories\nAfter import");

            logger.Info($"Created {createdCardsIds.Count} cards");

            logger.Info($"IN_common_11: {mainDir.GetDirectories().Length}");

            var outDirInfo = new DirectoryInfo(successPath);
            logger.Info($"OUT: {outDirInfo.GetDirectories().Length} directories");

            var noPartnerInfo = new DirectoryInfo(noPartnerPath);
            logger.Info($"NOPARTNER: {noPartnerInfo.GetDirectories().Length} directories");

            var errorDirInfo = new DirectoryInfo(errorPath);
            logger.Info($"ERROR: {errorDirInfo.GetDirectories().Length}  directories");

            var classifiedDirInfo = new DirectoryInfo(classifiedPath);
            logger.Info($"CLASSIFIED: {classifiedDirInfo.GetDirectories().Length}  directories");

            var oldVersionDirInfo = new DirectoryInfo(incorrectVersionPath);
            logger.Info($"OLDVERSION: {oldVersionDirInfo.GetDirectories().Length}  directories");

            logger.Info("End recieve MEDO message");
            logger.Info("-----------------------------------------------");
        }

        #endregion

        #region Private

        private bool MoveDirAfterProcessing(string name, ProcessingResult result)
        {
            string source = Path.Combine(inPath, name);

            string outDirName = name;

            if (string.IsNullOrWhiteSpace(source) || !Directory.Exists(source))
            {
                logger.Info($"MoveDirAfterProcessing source dir {source} not exist");
                return false;
            }

            if(isDirExist(name))
            {
                logger.Info($"MoveDirAfterProcessing target dir {name} already exist in output");
                outDirName = name + "_" + DateTime.Now;
                //return false;
            }

            string target;

            switch (result)
            {
                case ProcessingResult.SUCCESS:
                    target = successPath;
                    break;
                case ProcessingResult.ERROR:
                    target = errorPath;
                    break;
                case ProcessingResult.NOPARTNER:
                    target = noPartnerPath;
                    break;
                case ProcessingResult.CLASSIFIED:
                    target = classifiedPath;
                    break;
                case ProcessingResult.INCORRECTVERSION:
                    target = incorrectVersionPath;
                    break;
                case ProcessingResult.NOTIFICATION:
                    target = notificationPath;
                    break;
                case ProcessingResult.NotificationError: 
                    target = notificationErrorPath;
                    break;
                default:
                    return false;
            }

            var outPath = Path.Combine(target, outDirName);

            try
            {
                if (!Directory.Exists(target))
                {
                    Directory.CreateDirectory(target);
                }

                Directory.Move(source, outPath);
            }
            catch (Exception e)
            {
                logger.Error($"MoveDirAfterProcessing Error message while moving: {e.Message}\nstacktrace: {e.StackTrace}");
                return false;
            }

            logger.Info($"MoveDirAfterProcessing dir move successfully to {target}");

            return true;
        }

        private async Task<Card> GetCardAsync(
            Guid CardID,
            ICardServerPermissionsProvider permissionsProvider,
            ICardRepository cardRepository,
            CancellationToken cancellationToken = default)
        {
            var request = new CardGetRequest { CardID = CardID };
            permissionsProvider.SetFullPermissions(request);

            var response = await cardRepository.GetAsync(request, cancellationToken);
            if (!response.ValidationResult.IsSuccessful())
            {
                logger.Error($"validationresult: {response.ValidationResult}");
                //this.ValidationResult.Add(response.ValidationResult);
                return null;
            }

            return response.Card;
        }

        private bool isDirExist(string name)
        {
            if (Directory.Exists(Path.Combine(successPath, name)))
            {
                logger.Info($"Already exist in {Path.Combine(successPath, name)}");
                return true;
            }
            if (Directory.Exists(Path.Combine(classifiedPath, name)))
            {
                logger.Info($"Already exist in {Path.Combine(classifiedPath, name)}");
                return true;
            }
            if (Directory.Exists(Path.Combine(errorPath, name)))
            {
                logger.Info($"Already exist in {Path.Combine(errorPath, name)}");
                return true;
            }
            if (Directory.Exists(Path.Combine(noPartnerPath, name)))
            {
                logger.Info($"Already exist in {Path.Combine(noPartnerPath, name)}");
                return true;
            }
            if (Directory.Exists(Path.Combine(incorrectVersionPath, name)))
            {
                logger.Info($"Already exist in {Path.Combine(incorrectVersionPath, name)}");
                return true;
            }
            if (Directory.Exists(Path.Combine(notificationPath, name)))
            {
                logger.Info($"Already exist in {Path.Combine(notificationPath, name)}");
                return true;
            }
            if (Directory.Exists(Path.Combine(notificationErrorPath, name)))
            {
                logger.Info($"Already exist in {Path.Combine(notificationErrorPath, name)}");
                return true;
            }

            logger.Info($"isDirExist dir {name} not exist in output ");

            return false;
        }

        private async Task<bool> SignFile(string path, IFileVersion toSignVersion, ICAdESManager cadesManager, CancellationToken cancellationToken)
        {
            logger.Info($"beforeSign {path}");

            byte[] signatureBytes = await cadesManager.GetSignatureBytesFromFileAsync(path, cancellationToken);

            var (certificate, errorText) = cadesManager.DecodeCertificateFromSignature(signatureBytes);

            if (certificate is null)
            {
                logger.Error($"Certificate is null, errorText: {errorText}");
                return false;
            }

            SignatureType signatureType = SignatureType.CAdES;
            SignatureProfile signatureProfile = SignatureProfile.BES;

            try
            {
                var validationInfos = await cadesManager.CheckExtendedSignatureAsync(new SignedData
                {
                    Signature = signatureBytes,
                    SignatureType = signatureType,
                    SignatureProfile = signatureProfile,
                }, cancellationToken);
                var firstValInfo = validationInfos?.FirstOrDefault();
                signatureType = firstValInfo?.ReachedSignatureType ?? SignatureType.CAdES;
                signatureProfile = firstValInfo?.ReachedSignatureProfile ?? SignatureProfile.BES;
            }
            catch (OperationCanceledException)
            {
                logger.Error($"OperationCanceledException");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error($"Can't parse certificate from file , {ex}");
                throw new InvalidOperationException($"Can't parse certificate from file \"\" stacktrace {ex.StackTrace} message {ex.Message}", ex);
            }

            IFileSignatureCreationToken signatureToken = await toSignVersion.Source.GetSignatureCreationTokenAsync(cancellationToken).ConfigureAwait(false);
            signatureToken.Comment = string.Empty;
            signatureToken.EventType = FileSignatureEventType.Imported;
            signatureToken.Company = certificate.Company;
            signatureToken.SubjectName = certificate.SubjectName;
            signatureToken.SerialNumber = certificate.SerialNumber;
            signatureToken.IssuerName = certificate.IssuerName;
            signatureToken.Data = signatureBytes;
            signatureToken.SignatureType = signatureType;
            signatureToken.SignatureProfile = signatureProfile;
            IFileSignature signature = await toSignVersion.Source.CreateSignatureAsync(signatureToken, toSignVersion, cancellationToken).ConfigureAwait(false);

            await toSignVersion.Signatures.AddWithNotificationAsync(signature, cancellationToken);

            return true;
        }

        private async Task<Card> NewCard(KrDocType docType, Guid typeID, ICardRepository cardRepository)
        {
            var newRequestTemp = new CardNewRequest();

            // проверяем используется ли тип документа в типовом решении

            if (docType != null)
            {
                logger.Info($"DocType not null\ndocType.Caption {docType.Caption}\ndocType.CardTypeID {docType.CardTypeID}\ntypeID {typeID}");
                newRequestTemp.CardTypeID = docType.CardTypeID;
                newRequestTemp.Info[KrConstants.Keys.DocTypeID] = typeID;
                newRequestTemp.Info[KrConstants.Keys.DocTypeTitle] = docType.Caption;
            }
            else
            {
                logger.Info("DocType null");
            }

            // для создания карточки из типового решения должны быть права создания для заданного типа у скрытого пользователя System
            CardNewResponse newResponseTemp = await cardRepository.NewAsync(newRequestTemp);

            ValidationResult newResultTemp = newResponseTemp.ValidationResult.Build();
            // логируем сообщения при создании, если они есть
            logger.LogResult(newResultTemp);
            // если не удалось создать карточку - выходим
            if (!newResultTemp.IsSuccessful)
            {
                return null;
            }

            // теперь у нас есть карточка card, в ней можно заполнить нужные поля и добавить файл
            Card card = newResponseTemp.Card;
            var cardID = Guid.NewGuid();
            card.ID = cardID;

            return card;
        }

        private async Task<int> UpdateMedoStateAsync(int stateId, string stateName, Guid mesID, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                var param = new[]
                {
                    new DataParameter("@mesID", mesID),
                    new DataParameter("@stID", stateId),
                    new DataParameter("@stName", stateName),
                };

                return await db.SetCommand
                    ("UPDATE \"MedoCommonInfo\" " +
                            "SET \"MedoStatusID\" = @stID, \"MedoStatusName\" = @stName " +
                            "WHERE \"MessageID\" = @mesID", param)
                        .LogCommand()
                        .ExecuteNonQueryAsync();
            }
        }

        private async Task<Guid?> GetMesIDCardNotif(Guid partnerID, Guid? cardID, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                var param = new[]
                {
                    new DataParameter("@cardID", cardID),
                    new DataParameter("@partnerID", partnerID),
                    new DataParameter("@stID", 1),
                };

                return await db.SetCommand
                    ("SELECT \"MessageID\" " +
                            "FROM \"MedoCommonInfo\" " +
                            "WHERE \"ID\" = @cardID AND \"MedoPartnerID\" = @partnerID LIMIT 1", param)
                        .LogCommand()
                        .ExecuteAsync<Guid?>();
            }
        }

        private async Task<List<MedoCardIDDocDate>> GetCardByRegNum(string regNum, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                var param = new[]
                {
                    new DataParameter("@num", regNum)
                };

                return await db.SetCommand
                    ("SELECT \"ID\", \"DocDate\" " +
                            "FROM \"DocumentCommonInfo\" " +
                            "WHERE LOWER(\"FullNumber\") = @num LIMIT 1", param)
                        .LogCommand()
                        .ExecuteListAsync<MedoCardIDDocDate>();
                        //.ExecuteAsync<Guid?>();
            }
        }

        private async Task<Guid?> GetCardByDirName(string dirName, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                var param = new[]
                {
                    new DataParameter("@dirName", dirName)
                };

                return await db.SetCommand
                    ("SELECT \"ID\" " +
                            "FROM \"DocumentCommonInfo\" " +
                            "WHERE LOWER(\"MEDOFolderName\") = @dirName", param)
                        .LogCommand()
                        //.ExecuteListAsync<MedoCardIDDocDate>();
                        .ExecuteAsync<Guid?>();
            }
        }

        private async Task UpdateMedoJournalStateAsync(Guid mesID, string status, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                var param = new[]
                {
                    new DataParameter("@mesID", mesID),
                    new DataParameter("@status", status)
                };

                await db.SetCommand
                    ("UPDATE \"MEDOJournalReports\" " +
                            "SET \"Status\" = @status " +
                            "WHERE \"MesID\" = @mesID", param)
                        .LogCommand()
                        .ExecuteNonQueryAsync();
            }
        }

        private async Task UpdateMedoJournalAcceptedAsync(Guid mesID, string regNum, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                var param = new[]
                {
                    new DataParameter("@mesID", mesID),
                    //new DataParameter("@status", "Зарегистрирован"),
                    //new DataParameter("@regDate", regDate),
                    new DataParameter("@regNum", regNum)
                };

                await db.SetCommand
                    ("UPDATE \"MEDOJournalReports\" " +
                            "SET " + //\"Status\" = @status, " +
                            //" \"DateOfReceiving\" = @regDate, " +
                            " \"RegNum\" = @regNum " +
                            "WHERE \"MesID\" = @mesID", param)
                        .LogCommand()
                        .ExecuteNonQueryAsync();
            }
        }

        private async Task UpdateMedoJournalRefusedAsync(Guid mesID, string comment, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                var param = new[]
                {
                    new DataParameter("@mesID", mesID),
                    new DataParameter("@status", "Отказано в регистрации"),
                    new DataParameter("@comment", comment)
                };

                await db.SetCommand
                    ("UPDATE \"MEDOJournalReports\" " +
                            "SET " +
                            //"\"Status\" = @status, " +
                            "\"Comment\" = @comment " +
                            "WHERE \"MesID\" = @mesID", param)
                        .LogCommand()
                        .ExecuteNonQueryAsync();
            }
        }

        private async Task<int> AddCorrespondentToCardAsync(Guid CardID, MedoPartner partner, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;

                string InsertMedoInfo = "INSERT INTO \"Correspondents\" " +
                    "(\"ID\", \"RowID\"," +
                    "\"PartnersID\", \"PartnersName\")" +
                    "VALUES(@cID, @rowID, " +
                    "@partnerID, @partnerName)";

                DataParameter[] param = new DataParameter[]
                {
                    new DataParameter("@cID", CardID),
                    new DataParameter("@rowID", Guid.NewGuid()),
                    new DataParameter("@partnerID", partner != null ? partner.ID : null),
                    new DataParameter("@partnerName", partner != null ? partner.Name : null)
                };

                foreach (var p in param)
                {
                    logger.Info(p.Name + " " + p.Value);
                }
                return await db.SetCommand(InsertMedoInfo, param)
                        .LogCommand()
                        .ExecuteNonQueryAsync();
            }
        }

        private async Task<int> InsertNewMedoReportEntryAsync(Guid CardID, Guid? MainMesID, MedoPartner partner, string status, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;

                string InsertMedoInfo = "INSERT INTO \"MEDOJournalReports\" " +
                    "(\"ID\", \"CardID\", \"MesID\", \"PartnersID\", \"PartnersName\", \"Status\", \"DateOfReceiving\" ) " +
                    //"\"MedoStatusID\", \"MedoStatusName\", " +
                    //"\"MedoTypeID\", \"MedoTypeName\", " +
                    //"\"MedoComment\", \"MedoError\", \"MedoPartnerID\", \"MedoPartnerFullName\" , \"MedoXsdVersion\")" +
                    "VALUES(@reportID, @cardID, @mID, @partnersID, @partnersName, @stName, @recieveDate)";
                    //"@state, @stName, " +
                    //"@typeID, @typeName, " +
                    //"@comment, @error, @partnerID, @partnerName, @medoVersion)";

                DataParameter[] param = new DataParameter[]
                {
                    new DataParameter("@reportID", Guid.NewGuid()),
                    new DataParameter("@mID", (Guid)MainMesID),
                    new DataParameter("@cardID", CardID),
                    new DataParameter("@partnersID", partner.ID),
                    new DataParameter("@partnersName", partner.Name),
                    new DataParameter("@stName", status),
                    new DataParameter("@recieveDate", DateTime.UtcNow)
                    //new DataParameter("@typeID", NoticeType.Document.GetStringValue()),
                    //new DataParameter("@typeName", NoticeType.Document.GetDescription()),
                    //new DataParameter("@comment", ""),
                    //new DataParameter("@error", ""),
                    //new DataParameter("@partnerID", partner != null ? partner.ID : null),
                    //new DataParameter("@partnerName", partner != null ? partner.Name : null),
                    //new DataParameter("@medoVersion", xsdVersion == XsdVersion.NewVersion ? 1 : 0)
                };

                foreach (var p in param)
                {
                    logger.Info(p.Name + " " + p.Value);
                }
                return await db.SetCommand(InsertMedoInfo, param)
                        .LogCommand()
                        .ExecuteNonQueryAsync();
            }
        }

        private async Task<int> InsertNewMedoEntryAsync(Guid CardID, Guid? MainMesID, MedoPartner partner, XsdVersion xsdVersion, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;

                string InsertMedoInfo = "INSERT INTO \"MedoCommonInfo\" " +
                    "(\"ID\", \"RowID\", \"MessageID\", \"DateMessage\", " +
                    "\"MedoStatusID\", \"MedoStatusName\", " +
                    //"\"MedoTypeID\", \"MedoTypeName\", " +
                    "\"MedoComment\", \"MedoError\", \"MedoPartnerID\", \"MedoPartnerFullName\" , \"MedoXsdVersion\")" +
                    "VALUES(@cID, @rowID, @mID, @date, " +
                    "@state, @stName, " +
                    //"@typeID, @typeName, " +
                    "@comment, @error, @partnerID, @partnerName, @medoVersion)";

                DataParameter[] param = new DataParameter[]
                {
                    new DataParameter("@cID", CardID),
                    new DataParameter("@mID", (Guid)MainMesID),
                    new DataParameter("@rowID", Guid.NewGuid()),
                    new DataParameter("@date", DateTime.UtcNow),
                    new DataParameter("@state", 2),
                    new DataParameter("@stName", "Получено"),
                    //new DataParameter("@typeID", NoticeType.Document.GetStringValue()),
                    //new DataParameter("@typeName", NoticeType.Document.GetDescription()),
                    new DataParameter("@comment", ""),
                    new DataParameter("@error", ""),
                    new DataParameter("@partnerID", partner != null ? partner.ID : null),
                    new DataParameter("@partnerName", partner != null ? partner.Name : null),
                    new DataParameter("@medoVersion", xsdVersion == XsdVersion.NewVersion ? 1 : 0)
                };

                foreach (var p in param)
                {
                    logger.Info(p.Name + " " + p.Value);
                }
                return await db.SetCommand(InsertMedoInfo, param)
                        .LogCommand()
                        .ExecuteNonQueryAsync();
            }
        }

        private async Task<Guid?> GetCardIDByMesIDAsync(Guid mesID, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("dci", "ID")
                            .From("MedoCommonInfo", "dci").NoLock()
                            .Where().C("dci", "MessageID").Equals().P("mesID")
                            .Limit(1).Build(),
                        db.Parameter("mesID", mesID))
                    .LogCommand()
                    .ExecuteAsync<Guid?>();

                return result;
            }
        }

        private async Task<MedoPartner> GetPartnerAsync(string externalGuid, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = new MedoPartner();

                result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("partners", "ID", "Name")
                            .From("Partners", "partners").NoLock()
                            .Where().C("partners", "MedoID").Equals().P("externalGuid")
                            .Limit(1).Build(),
                        db.Parameter("externalGuid", externalGuid))
                    .LogCommand()
                    .ExecuteAsync<MedoPartner>();

                return result;
            }
        }

        private async Task<DocTypeMEDO> GetMedoDocTypeAsync(string index, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = new DocTypeMEDO();

                result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("medoType", "ID", "Name", "Index")
                            .From("DocTypeMEDO", "medoType").NoLock()
                            .Where().C("medoType", "Index").Equals().P("index")
                            .Limit(1).Build(),
                        db.Parameter("index", index))
                    .LogCommand()
                    .ExecuteAsync<DocTypeMEDO>();

                return result;
            }
        }
        
        private static void DrawStamp(PdfDocument pdfFile,
            StampInfo stampInfo,
            MemoryStream ms, string path)
        {
            var pageNumber = stampInfo.page - 1;
            pageNumber = pageNumber >= pdfFile.PageCount ? pdfFile.PageCount - 1 : pageNumber;
            var pdfPage = pdfFile.Pages[pageNumber];
            using var g = XGraphics.FromPdfPage(pdfPage, XGraphicsUnit.Millimeter);
            using var image = XImage.FromStream(() => ms);

            StringBuilder sb = new StringBuilder();

            g.DrawImage(image, stampInfo.topLeftX, stampInfo.topLeftY, stampInfo.dimensionW, stampInfo.dimensionH);
        }

        #region Models

        private class MedoCardIDDocDate
        {
            public Guid ID { get; set; }
            public DateTime DocDate { get; set; }
        }

        private class MedoPartner
        {
            public Guid ID { get; set; }
            public string Name { get; set; }
        }

        private class DocTypeMEDO
        {
            public Guid ID { get; set; }
            public string Name { get; set; }
            public string Index { get; set; }
        }

        private class StampInfo
        {
            public string stampName { get; set; }
            public int page { get; set; }
            public int topLeftX { get; set; }
            public int topLeftY { get; set; }
            public int dimensionW { get; set; }
            public int dimensionH { get; set; }
        }

        private enum ProcessingResult
        {
            [Description("Зарегистрирован")]
            SUCCESS = 0,

            [Description("Ошибка")]
            ERROR = 1,

            [Description("Не найден контрагент")]
            NOPARTNER = 2,

            [Description("Для служебного пользования")]
            CLASSIFIED = 3,

            [Description("Версия МЭДО не найдена")]
            INCORRECTVERSION = 4,

            [Description("Уведомления обработанные")]
            NOTIFICATION = 5,

            [Description("Уведомления с ошибками")]
            NotificationError = 6
        }

        #endregion

        #endregion
    }
}