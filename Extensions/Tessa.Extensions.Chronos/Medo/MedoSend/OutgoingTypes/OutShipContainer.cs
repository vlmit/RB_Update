using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using LinqToDB.Common;
using LinqToDB.Data;
using NLog;
using Tessa.Cards;
using Tessa.Cards.Caching;
using Tessa.Extensions.Chronos.Helpers;
using Tessa.Extensions.Chronos.Medo.MedoSend.Helpers;
using Tessa.Extensions.Shared.Helpers.Medo;
using Tessa.Extensions.Shared.Info;
using Tessa.Files;
using Tessa.Platform;
using Tessa.Platform.Collections;
using Tessa.Platform.Data;
using Tessa.Platform.IO;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using Tessa.Properties.Resharper;
using Tessa.Views.Mapping;
using Unity;
using static LinqToDB.Reflection.Methods.LinqToDB.Insert;
using File = System.IO.File;
using FileData = System.Tuple<System.Guid, System.Guid, string, System.Guid>;
using StampType = Tessa.Extensions.Shared.Helpers.Medo.StampType;

namespace Tessa.Extensions.Chronos.Medo.MedoSend.OutgoingTypes
{
    public abstract class OutShipContainer : OutgoingContext
    {
        #region Constructor

        protected OutShipContainer(
            IUnityContainer container,
            IDbScope dbScope,
            string globalPath,
            NewMessageInfo newMessageInfo)
            : base(container,
                dbScope,
                globalPath,
                newMessageInfo)
        {
            this.fileManager = container.Resolve<ICardFileManager>();
            this.permissionsProvider = container.Resolve<ICardServerPermissionsProvider>();
            this.cardStreamRepository = container.Resolve<ICardStreamServerRepository>();
            this.tempPath = this.GetTempPath();
            this.newMessageInfo = newMessageInfo;
        }

        #endregion

        #region Fields

        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected readonly string tempPath;

        private readonly ICardFileManager fileManager;

        private readonly ICardServerPermissionsProvider permissionsProvider;

        private readonly ICardStreamServerRepository cardStreamRepository;

        #endregion

        #region Properties

        protected Card Card { get; set; }

        //protected Card MedoSettingsCard { get; set; }

        /// <summary>
        ///     регистрационный номер
        /// </summary>
        public string RegNumber => this.Card?.Sections[SchemeInfo.DocumentCommonInfo].Fields.Get<string>(SchemeInfo.DocumentCommonInfo.FullNumber);

        /// <summary>
        ///     регистрационная дата
        /// </summary>
        public DateTime? RegDate => this.Card?.Sections[SchemeInfo.DocumentCommonInfo].Fields.Get<DateTime?>(SchemeInfo.DocumentCommonInfo.DocDate);

        public NewMessageInfo newMessageInfo; 

        /// <summary>
        ///     главный файл
        /// </summary>
        [CanBeNull]
        public CardFile MainFile => this.Card?.Files.FirstOrDefault(x => x.CategoryID == FileCategories.MainDoc.ID && x.Name.ToLower().EndsWith(".pdf"));

        #endregion

        #region Abstract

        /// <summary>
        ///     получаем XElement с получателями (адрессаты мэдо)
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task<XElement> AddresseesElementAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     изменяем данные в информационной таблице МЭДО
        /// </summary>
        public virtual Task ChangeMedoCommonInfoAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public abstract Task<int> InsertMedoJournalEntryAsync(CancellationToken cancellationToken = default);

        #endregion

        #region Protected

        protected override async Task EnvelopeAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer EnvelopeAsync");

            var mails = this.Card.Sections["RecieversPartners"].Rows
                           //.Where(r => r.Get<int>(SchemeInfo.RecieversPartners.DeliveryTypeID) == SchemeInfo.DeliveryTypes.MEDO.ID)
                         //   .Concat(this.Card.Sections[SchemeInfo.CopyRecieversPartners].Rows
                         //               .Where(r => r.Get<int>(SchemeInfo.CopyRecieversPartners.DeliveryTypeID) == SchemeInfo.DeliveryTypes.MEDO.ID))
                            ;

            string partnerMedoAddress = await GetPartnerMedoAddress(this.MedoPartnerID, this.DbScope, cancellationToken);

            var sb = StringBuilderHelper
                     .Acquire()
                     .AppendLine("[ПИСЬМО КП ПС СЗИ]")
                     .AppendLine("АВТООТПРАВКА=1")
                     .AppendLine("ШИФРОВАНИЕ=0")
                     .AppendLine("ЭЦП=1")
                     .AppendLine("ДОСТАВЛЕНО=1")
                     .AppendLine("ПРОЧТЕНО=1")
                     .AppendLine($"ДАТА={DateTime.Now:dd.MM.yyyy HH:mm:ss}")
                     .AppendLine()
                     .AppendLine("[АДРЕСАТЫ]")
                     .AppendLine($"0={partnerMedoAddress}");

            //var i = 0;
            //foreach (var mail in mails)
            //{
            //    sb.AppendLine($"{i++}={mail["PartnerMedoAddress"]}");
            //}

            sb.AppendLine()
              .AppendLine("[ФАЙЛЫ]")
              .AppendLine("0=document.edc.zip")
              .AppendLine("1=communication.xml");

            await File.WriteAllTextAsync(Path.Combine(this.GlobalPath, "envelope.ini"), sb.ToString(), /*Encoding.UTF8*/ Encoding.GetEncoding(1251), cancellationToken);
            await File.WriteAllTextAsync(Path.Combine(this.ArchivePath, "envelope.ini"), sb.ToStringAndRelease(), /*Encoding.UTF8*/ Encoding.GetEncoding(1251), cancellationToken);
        }

        public override async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer InitializeAsync");

            var cardCachetemp = this.Container.Resolve<ICardCache>();

            if (cardCachetemp == null)
            {
                logger.Info("Can't resolve \"ICardCache\"");
            }

            var cardCache = this.Container.Resolve<ICardCache>() ?? throw new InvalidOperationException("Can't resolve \"ICardCache\"");
            this.Card = await this.GetCardAsync(
                this.permissionsProvider,
                this.Container.Resolve<ICardRepository>(),
                cancellationToken);

            logger.Info($"OutshipContainer InitializeAsync cardID: {this.CardID} fullNumber: {this.Card.Sections[SchemeInfo.DocumentCommonInfo].Fields[SchemeInfo.DocumentCommonInfo.FullNumber]}");

            //logger.Info($"InitializeAsync cardID {this.Card.ID} num: {this.Card.Sections["DocumentCommonInfo"].Fields["FullNumber"].ToString()}");

            //this.MedoSettingsCard = (await cardCache.Cards.GetAsync("MedoStampInfo", cancellationToken).ConfigureAwait(false)).GetValue();

            //logger.Info("InitializeAsync medoSettingsID" + this.Card.ID);
        }

        #endregion

        #region Override

        public override async Task CreateMedoContainerAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutShipContainer CreateMedoContainerAsync");

            var (successful, passport) = await this.TryGetPassportAsync(cancellationToken);

            if (successful)
            {
                logger.Info("OutShipContainer CreateMedoContainerAsync TryGetPassportAsync successfull");
            }

            if (successful)
            {
                Directory.CreateDirectory(this.tempPath);

                using (var writer = new XmlTextWriter(Path.Combine(this.tempPath, MedoConst.PassportXmlName), new UTF8Encoding(false)))
                {
                    
                    //writer.Settings.NewLineChars = "\r\n";
                    passport.Save(writer);
                }

                //passport.Save(Path.Combine(this.tempPath, MedoConst.PassportXmlName));

                logger.Info("OutShipContainer CreateMedoContainerAsync path to save: " + Path.Combine(this.tempPath, MedoConst.PassportXmlName));

                await this.GetMainFileAndSignatureAsync(cancellationToken);
                await this.CreateRegStampAsync(cancellationToken);
                await this.CreateAllSignStampAsync(cancellationToken);
                await this.AddAttachmentAndSignatureAsync(cancellationToken);
            }
            else
            {
                logger.Info("OutShipContainer CreateMedoContainerAsync TryGetPassport is not successfull");
            }
        }

        public override async Task AuxiliaryActionAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer AuxiliaryActionAsync");

            if (this.ValidationResult.IsSuccessful())
            {
                logger.Info("AuxiliaryActionAsync validation successful");

                this.AddFileAtZip();
                this.Communication();
                await this.EnvelopeAsync(cancellationToken);

                //var copyPath = ((await this.CardCache.Cards.GetAsync("MedoStampInfo", cancellationToken).ConfigureAwait(false))).GetValue()
                //    ?.Sections["MedoRosteh"].Fields.TryGet<string>("CopyFolder");

                string copyPath = string.Empty;

                if (!string.IsNullOrWhiteSpace(copyPath))
                {
                    Directory.CreateDirectory(copyPath);
                    var sourceDir = new DirectoryInfo(this.GlobalPath);
                    var outgoingCopyPath = Path.Combine(copyPath, "Исходящие", sourceDir.Name);
                    if (Directory.Exists(outgoingCopyPath))
                    {
                        outgoingCopyPath += DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss");
                    }
                    Directory.CreateDirectory(outgoingCopyPath);
                    sourceDir.GetFiles().ForEach(f => f.CopyTo(Path.Combine(outgoingCopyPath, f.Name)));
                }
            }
            else
            {
                logger.Info("AuxiliaryActionAsync validation not successful");
            }

            await this.ChangeMedoCommonInfoAsync(cancellationToken);

            await this.InsertMedoJournalEntryAsync(cancellationToken);

            logger.MedoLoggerResult(this.ValidationResult, this.MainMesID);

            if (Directory.Exists(this.tempPath))
            {
                Directory.Delete(this.tempPath, true);
            }
        }

        #endregion

        #region Private

        /// <summary>
        ///     получаем временную папку
        /// </summary>
        /// <returns></returns>
        private string GetTempPath()
        {
            logger.Info("OutshipContainer GetTempPath");

            var tempPath = ConfigurationManager.Settings.TryGet<string>(MedoConst.TempPathSettingName);
            if (tempPath.IsNullOrEmpty())
            {
                this.ValidationResult.AddError("Cant get TempPathSettingName from app.json");
                logger.Info("OutshipContainer Cant get TempPathSettingName from app.json");
                return null;
            }

            var mes = this.CardID.ToString().Replace("-", "");

            logger.Info("OutshipContainer GetTempPath: " + Path.Combine(tempPath, "outTemp", mes));

            return Path.Combine(tempPath, "outTemp", mes);
        }

        /// <summary>
        ///     создаем xml паспорт сообщения
        /// </summary>
        /// <returns></returns>
        private async Task<(bool successful, XDocument passport)> TryGetPassportAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutShipContainer TryGetPassportAsync");

            //var (accessLevelId, accessLevel) = this.Card.TypeID == TypeInfo.RB_OutgoingTypeID
            //    ? ("DC00000001", "Информация ограниченного распространения")
            //    : ("DC00000000", "Обычная информация");

            var (accessLevelId, accessLevel) = ("DC00000000", "Обычная информация");

            var griffId = (int?)this.Card.Sections["DocumentCommonInfo"].Fields?["GriffMedoID"];

            if(griffId == 1)
            {
                (accessLevelId, accessLevel) = ("DC00000001", "Информация ограниченного распространения");
            }
            
            // Create an XML declaration.

            XDeclaration xd = new XDeclaration("1.0", "utf-8", null);

            xd.Standalone = null;

            var passport = new XDocument(xd,//MedoConst.Declare,
                new XElement(this.Ns + MedoTag.TagContainer,
                    new XAttribute(XNamespace.Xmlns + "c", this.Ns),
                    new XAttribute(this.Ns + MedoTag.TagVersion, this.XsdVersion.GetDescription()),
                    new XAttribute(this.Ns + MedoTag.TagUid, this.Card.ID),
                    new XElement(this.Ns + MedoTag.TagRequisites,
                        this.GetRequisite(SchemeInfo.DocumentCommonInfo, SchemeInfo.DocumentCommonInfo.DocTypeMEDOName, MedoTag.TagDocumentKind),
                        new XElement(this.Ns + MedoTag.TagDocumentPlace, "Республика Бурятия", new XAttribute(this.Ns + "id", "DP00000003")),
                        new XElement(this.Ns + MedoTag.TagClassification,
                            new XAttribute(this.Ns + MedoTag.TagId,  accessLevelId),
                            accessLevel),
                        this.GetRequisite(SchemeInfo.DocumentCommonInfo, SchemeInfo.DocumentCommonInfo.Subject, MedoTag.TagAnnotation)),
                        //await this.LinksElementAsync(cancellationToken)),
                    new XElement(this.Ns + MedoTag.TagAuthors, await this.AuthorElementAsync(cancellationToken)),
                    await this.AddresseesElementAsync(cancellationToken),
                    this.DocumentElement(),
                    await this.AttchmentElementAsync(cancellationToken)
                ));

            logger.Info("OutShipContainer TryGetPassportAsync" +
                "\nthis.ValidationResult.IsSuccessful(): " + this.ValidationResult.IsSuccessful() +
                "\nthis.XsdVersion: " + this.XsdVersion.GetDescription() +
                "\nXmlType.Passport: " + XmlType.Passport +
                "\nthis.ValidationResult: " + this.ValidationResult +
                "\nthis.ContainerPath: " + this.ContainerPath);


            return (this.ValidationResult.IsSuccessful() &&
                passport.CheckXml(this.XsdVersion, XmlType.Passport, this.ValidationResult, this.ContainerPath), passport);
        }

        #region Passport

        /// <summary>
        ///     получаем реквизит
        /// </summary>
        /// <param name="section">имя таблицы</param>
        /// <param name="field">имя колонки</param>
        /// <param name="tag">тэг для сообщения</param>
        /// <returns></returns>
        private XElement GetRequisite(string section, string field, string tag)
        {
            logger.Info("OutshipContainer GetRequisite section: " + section + " field: " + field + " tag: " + tag);

            object result = null;
            var requisit = this.Card?.Sections[section]?.Fields.TryGetValue(field, out result);
            if (requisit == null || !(bool)requisit || result == null)
            {
                this.ValidationResult.AddError(this, $"Cant get requisite {tag} from section {section}, field {field}");

                logger.Info("OutshipContainer GetRequisite Cant get requisite section: " + section + " field: " + field + " tag: " + tag);

                return null;
            }

            XElement xElement;

            if (tag == MedoTag.TagDocumentKind)
            {
                var docTypeCode = this.Card.Sections[SchemeInfo.DocumentCommonInfo].Fields.TryGet<string>(SchemeInfo.DocumentCommonInfo.DocTypeMEDOIndex);
                xElement = new XElement(this.Ns + tag,
                    new XAttribute(this.Ns + MedoTag.TagId, string.IsNullOrEmpty(docTypeCode) ? "DK00000000" : docTypeCode),
                    result);
            }
            else
            {
                xElement = new XElement(this.Ns + tag, result);
            }

            return xElement;
        }

        //todo:
        /// <summary>
        ///     XElement по связанным карточкам
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<XElement> LinksElementAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer LinksElementAsync");

            var elements = new List<XElement>();
            foreach (var row in this.Card.Sections["OutgoingRefDocs"]?.Rows)
            {
                var docID = row.Fields.Get<Guid>("DocID");
                var org = await this.OrganizationElementAsync(docID, MedoConst.GetLinkOrg, cancellationToken);
                var reg = this.RegistrationElement(await this.GetRegInfoAsync(docID));

                if (org != null && reg != null)
                {
                    elements.Add(new XElement(this.Ns + MedoTag.TagLink, org, reg, new XAttribute(this.Ns + MedoTag.TagLinkUid, docID)));
                }
            }

            var result = new XElement(this.Ns + MedoTag.TagLinks);
            elements.ForEach(x => result.Add(x));

            return elements.Count == 0 ? null : result;
        }

        #region Organization

        /// <summary>
        ///     Получаем XElement по организации(контрагенту)
        /// </summary>
        /// <param name="id">id организации</param>
        /// <param name="command">комманда для получения информации по организации</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<XElement> OrganizationElementAsync(
            Guid id,
            string command,
            CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer OrganizationElementAsync");

            logger.Info($"id: {id} command: {command}");

            var orgInfo = await this.GetOrgInfoAsync(id, command, cancellationToken);

            if (orgInfo == null)
            {
                this.ValidationResult.AddError("Can't get DocumentRosteh_CommonInfo by id " + id);

                logger.Info("OutshipContainer OrganizationElementAsync Can't get DocumentRosteh_CommonInfo by id " + id);

                return null;
            }

            return new XElement(this.Ns + MedoTag.TagOrganization,
                new XElement(this.Ns + MedoTag.TagTitle, orgInfo[MedoTag.TagTitle]),
                !orgInfo[MedoTag.TagAddress].IsNullOrEmpty() ? new XElement(this.Ns + MedoTag.TagAddress, orgInfo[MedoTag.TagAddress]) : null,
                !orgInfo[MedoTag.TagPhone].IsNullOrEmpty() ? new XElement(this.Ns + MedoTag.TagPhone, orgInfo[MedoTag.TagPhone]) : null,
                !orgInfo[MedoTag.TagEmail].IsNullOrEmpty() ? new XElement(this.Ns + MedoTag.TagEmail, orgInfo[MedoTag.TagEmail]) : null
            );
        }

        /// <summary>
        ///     Получаем информацию по организации (контрагенту)
        ///     поле Sender в DocumentRosteh_CommonInfo
        /// </summary>
        /// <param name="id">id карточки</param>
        /// <param name="command">sql запрос</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, string>> GetOrgInfoAsync(
            Guid id,
            string command,
            CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer GetOrgInfoAsync");

            logger.Info($"id {id} command {command}");

            await using (this.DbScope.Create())
            {
                var db = this.DbScope.Db;
                var result = new Dictionary<string, string>();

                db.SetCommand(command, db.Parameter("CardID", id)).LogCommand();
                await using var reader = await db.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    var name = reader.GetValue<string>(0);

                    if (name.IsNullOrEmpty())
                    {
                        this.ValidationResult.AddError(this, $"Cant get organization name from link document {id}");

                        logger.Info("OutshipContainer GetOrgInfoAsync Cant get organization name from link document " + id);

                        return null;
                    }

                    result.Add(MedoTag.TagTitle, name);
                    result.Add(MedoTag.TagAddress, reader.GetValue<string>(1));
                    result.Add(MedoTag.TagPhone, reader.GetValue<string>(2));
                    result.Add(MedoTag.TagEmail, reader.GetValue<string>(3));

                    return result;
                }
            }

            return null;
        }

        #endregion

        #region Registration

        /// <summary>
        ///     регистрационный XElement документа
        /// </summary>
        /// <returns></returns>
        private async Task<XElement> DocRegElementAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer DocRegElementAsync");
            var regInfo = this.RegistrationElement(new Tuple<string, DateTime?>(this.RegNumber, this.RegDate));
            if (regInfo == null)
            {
                this.ValidationResult.AddError(this, $"Card {this.Card.ID} unregistered");

                logger.Info("OutshipContainer DocRegElementAsync card " + this.Card.ID + " unregistered");

                return null;
            }

            var regStamp = await this.RegStampElementAsync(
                StampType.Reg,
                MedoConst.RegStampName,
                MedoTag.TagRegStamp,
                this.CardID,
                cancellationToken);

            regInfo.Add(regStamp);
            return regInfo;
        }

        /// <summary>
        ///     Создаем регистрационный XElement
        /// </summary>
        /// <param name="regInfo">регистрационный номер и дата</param>
        /// <returns></returns>
        private XElement RegistrationElement(Tuple<string, DateTime?> regInfo)
        {
            logger.Info("OutshipContainer RegistrationElement");
            if (regInfo?.Item1 == null || regInfo.Item2 == null)
            {
                logger.Info("OutshipContainer RegistrationElement reginfo.item1 and reginfo.item2 null");
                return null;
            }

            var date = (DateTime)regInfo.Item2;

            return new XElement(this.Ns + MedoTag.TagRegistration,
                new XElement(this.Ns + MedoTag.TagNumber, regInfo.Item1),
                new XElement(this.Ns + MedoTag.TagDate, $"{date:yyyy-MM-dd}"));
        }

        /// <summary>
        ///     Получаем информацию о регистрационных данных документа
        /// </summary>
        /// <param name="id">id документа</param>
        /// <returns></returns>
        private async Task<Tuple<string, DateTime?>> GetRegInfoAsync(Guid id)
        {
            logger.Info("OutshipContainer GetRegInfoAsync");
            await using (this.DbScope.Create())
            {
                var db = this.DbScope.Db;

                const string command = "SELECT \"dci\".\"FullNumber\", \"dci\".\"DocDate\" " +
                        "FROM \"DocumentCommonInfo\" AS \"dci\" " +
                        //"INNER JOIN \"DocumentRosteh_CommonInfo\" AS \"drc\" ON \"drc\".\"ID\" = \"dci\".\"ID\" " +
                        "where \"dci\".\"id\" = @CardID";

                db.SetCommand(command, db.Parameter("@CardID", id)).LogCommand();
                await using (var reader = await db.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var number = reader.GetValue<string>(0);
                        var date = reader.GetValue<DateTime?>(1);

                        if (number.IsNullOrEmpty())
                        {
                            this.ValidationResult.AddWarning(this, $"registration number document {id} is null");

                            logger.Info("OutshipContainer GetRegInfoAsync Warning registration number document "+ id+ " is null");

                            return null;
                        }

                        if (date == null)
                        {
                            this.ValidationResult.AddWarning(this, $"registration date document {id} is null");

                            logger.Info("OutshipContainer GetRegInfoAsync Warning registration number document " + id + " is null");

                            return null;
                        }

                        return new Tuple<string, DateTime?>(number, (DateTime)date);
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     XElement по расположению штампа
        /// </summary>
        /// <param name="type">тип штампа</param>
        /// <param name="name">имя штампа</param>
        /// <param name="tag">имя тега</param>
        /// <param name="cardID">ID карточки</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<XElement> RegStampElementAsync(
            StampType type,
            string name,
            string tag,
            Guid cardID,
            CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer RegStampElementAsync");
            var validationResult = new ValidationResultBuilder();
            var stampInfo = await MedoHelper.GetStampDataAsync(
                this.DbScope,
                validationResult,
                type,
                MedoConst.GetRegStampInfo,
                cardID,
                cancellationToken: cancellationToken);


            if(validationResult.IsSuccessful())
            {
                logger.Info("OutshipContainer RegStampElementAsync validationResult.IsSuccessful()");
            }
            else
            {
                logger.Info("OutshipContainer RegStampElementAsync validationResult fail");
            }


            return !validationResult.IsSuccessful()
                ? null
                : new XElement(this.Ns + tag,
                    new XAttribute(this.Ns + MedoTag.TagLocalName, name),
                    new XElement(this.Ns + MedoTag.TagPosition,
                        new XElement(this.Ns + MedoTag.TagPage, stampInfo[MedoTag.TagPage]),
                        new XElement(this.Ns + MedoTag.TagTopLeft,
                            new XElement(this.Ns + MedoTag.TagX, stampInfo[MedoTag.TagX]),
                            new XElement(this.Ns + MedoTag.TagY, stampInfo[MedoTag.TagY])),
                        new XElement(this.Ns + MedoTag.TagDimension,
                            new XElement(this.Ns + MedoTag.TagW, stampInfo[MedoTag.TagW]),
                            new XElement(this.Ns + MedoTag.TagH, stampInfo[MedoTag.TagH]))));
        }

        #endregion

        #region Document

        private XElement DocumentElement()
        {
            logger.Info("OutshipContainer DocumentElement");

            if (this.MainFile == null)
            {
                this.ValidationResult.AddError(this, $"Card {this.Card.ID} havent main file");

                logger.Info($"OutshipContainer DocumentElement Card {this.Card.ID} havent main file");

                return null;
            }

            return new XElement(this.Ns + MedoTag.TagDocument,
                new XAttribute(this.Ns + MedoTag.TagLocalName, GetFileName(MainFile)),
                new XElement(this.Ns + MedoTag.TagPagesQuantity, this.Card.Sections[SchemeInfo.DocumentCommonInfo].Fields[SchemeInfo.DocumentCommonInfo.SheetsCount]),
                this.Card.Sections[SchemeInfo.DocumentCommonInfo].Fields[SchemeInfo.DocumentCommonInfo.SheetsAttachmentsCount] != null ? new XElement(this.Ns + "enclosurePagesQuantity", this.Card.Sections[SchemeInfo.DocumentCommonInfo].Fields[SchemeInfo.DocumentCommonInfo.SheetsAttachmentsCount]) : null);
        }

        #endregion

        #region Attachment

        /// <summary>
        ///     Получаем элемент описывающий приложения
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<XElement> AttchmentElementAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer AttchmentElementAsync");
            //var rows = this.Card.Sections["SendFiles"].Rows
            //               .Where(x => x.Fields.Get<bool>("MedoSend") && x.Fields.Get<Guid>("FileID") != this.MainFile?.RowID);

            //var rows = this.Card.Sections["Files"].Rows
            //               .Where(x => x.Fields.Get<Guid>("CategoryID") == Guid.Parse("723bf3bd-31c2-4269-a76a-d323e082a2f1"));

            //var files = rows
            //            .Select(row => this.Card.Files.FirstOrDefault(x => x.RowID == row.Fields.Get<Guid>("FileID")))
            //            .Where(x => x != null)
            //            .ToList();

            var cardFiles = this.Card.Files;
            var medoFiles = this.Card.Sections["MedoFiles"].TryGetRows();

            if (medoFiles == null)
            {
                return null;
            }

            var cardMedoFiles = medoFiles.Select(x => x).Where(x => (Guid)x.Fields["ParentRowID"] == this.RowID).ToList();

            //var tempCardFilesMedo = cardFiles.Select(x => x).Where

            var temp = this.Card.Files;

            List<CardFile> files = new List<CardFile>();

            foreach (var row in cardMedoFiles)
            {
                var f = cardFiles.Select(x => x).Where(x => x.RowID == (Guid)row.Fields["FilesID"]).FirstOrDefault();

                if (f != null)
                {
                    files.Add(f);

                    logger.Info($"OutshipContainer AttchmentElementAsync cardID: {this.Card.ID} fileID: {f.Card.ID} fileName: {f.Name} versionID: {f.VersionRowID}");
                }
            }

            //foreach (var tempFile in temp)
            //{
            //    tempCardFilesList.Add(tempFile);
            //}

            //var files = tempCardFilesList;

            if (files.IsNullOrEmpty() || files.Count == 0)
            {
                logger.Info("OutshipContainer AttchmentElementAsync no files");

                return null;
            }

            var order = 0;

            var elements = new List<XElement>(files.Count);
            foreach (var file in files)
            {
                if (file.CategoryID == FileCategories.MainDoc.ID) //|| file.CategoryID == FileCategories.Preview.ID)
                {
                    continue;
                }

                var element = new XElement(this.Ns + MedoTag.TagAttachment,
                    new XAttribute(this.Ns + MedoTag.TagLocalName, GetFileName(file)),
                    new XElement(this.Ns + MedoTag.TagOrder, order++),
                    await this.GetSignAsync(this.fileManager, file, cancellationToken));
                elements.Add(element);
            }

            var result = new XElement(this.Ns + MedoTag.TagAttachments);
            elements.ForEach(x => result.Add(x));

            return elements.IsNullOrEmpty() ? null : result;
        }

        /// <summary>
        ///     преобразование имени файа
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static string GetFileName(CardFile file)
        {
            logger.Info("OutshipContainer GetFileName");

            string newName = file.Name;

            string ext = newName.Substring(newName.LastIndexOf('.'));
            string fileName = newName.Substring(0, newName.LastIndexOf("."));

            logger.Info($"newName {newName} fileName {fileName} ext {ext}");

            //if (Regex.Match(fileName, "[А-Яа-яЁё]").Success)
            //{
            //    fileName = Transliteration.Translit(fileName);
            //}

            //Regex.Replace(fileName, @"[^0-9a-zA-Z_]+", "_");
            //Regex.Replace(fileName, @"[().-]", "_");

            newName = Transliteration.Translit(fileName) + ext;

            //string newName = file.Name;

            //if (Regex.Match(file.Name, "[А-Яа-яЁё]").Success)
            //{
            //    newName = Transliteration.Translit(file.Name);
            //}

            //Regex.Replace(newName, @"[^0-9a-zA-Z._]+", "_");

            return newName;

            var version = file.VersionRowID.ToString();
            return version.Replace("-", "") + Path.GetExtension(file.Name);
        }

        /// <summary>
        ///     Получаем информацию по подписям
        /// </summary>
        /// <param name="fileManager">ICardFileManager</param>
        /// <param name="file">CardFile</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<List<XElement>> GetSignAsync(
            ICardFileManager fileManager,
            CardFile file,
            CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer GetSignAsync");
            await using var container = await fileManager.CreateContainerAsync(this.Card, cancellationToken: cancellationToken);

            var lastVersion = container.FileContainer.Files.TryGet(file.RowID).Versions.Last;
            var signatures = (await lastVersion.Source.GetSignaturesAsync
                (lastVersion, FileSignatureLoadingMode.WithData, cancellationToken)).Signatures;

            if (signatures is { Count: > 0 })
            {
                logger.Info("OutshipContainer GetSignAsync signatures > 0");
                var result = signatures.Select(s =>
                    new XElement(this.Ns + MedoTag.TagAttSign,
                        new XAttribute(this.Ns + MedoTag.TagLocalName,
                            s.ID.ToString().Replace("-", "") + ".p7s"))).ToList();

                return result;
            }
            logger.Info("OutshipContainer GetSignAsync no signatures");
            return null;
        }

        #endregion

        #region Author

        private async Task<XElement> AuthorElementAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer AuthorElementAsync");

            Guid? partnerID = this.newMessageInfo.MedoPartnerID;

            var author =
                new XElement(this.Ns + MedoTag.TagAuthor,
                    new XElement(this.Ns + MedoTag.TagOrganization, 
                        new XElement(this.Ns + MedoTag.TagTitle, MedoConst.RBName)),
                    //await this.OrganizationElementAsync(
                    //    (Guid)partnerID,
                    //    //this.Card.Sections["DocumentCommonInfo"].Fields.Get<Guid>("PartnerID"),
                    //    //this.MedoSettingsCard.Sections["MedoRB"].Fields.Get<Guid>("PartnerID"),
                    //    //Guid.Parse("cf8bef81-fbb7-420a-8ef1-f48c07523e79"),
                    //    MedoConst.GetOrg,
                    //    cancellationToken),
                    await this.DocRegElementAsync(cancellationToken));

            var signs = await this.AllSignElementAsync();
            author.Add(signs);
            author.Add(await this.ExecutorElementAsync(cancellationToken));

            return author;
        }

        private async Task<List<XElement>> AllSignElementAsync()
        {
            logger.Info("OutshipContainer AllSignElementAsync");

            await GetAllSignStampsInfoAsync();

            var files = this.Card.TryGetFiles().ToList();
            var mainFile = files.FirstOrDefault(file =>
                file.CategoryID == FileCategories.MainDoc.ID && !file.IsVirtual && file.Name.ToLower().EndsWith(".pdf"));

            if (mainFile != null)
            {
                await using var container = await this.fileManager.CreateContainerAsync(this.Card);
                var lastVersion = container.FileContainer.Files.TryGet(mainFile.RowID).Versions.Last;
                var signatures = (await lastVersion.Source.GetSignaturesAsync
                    (lastVersion, FileSignatureLoadingMode.WithData)).Signatures;

                if (signatures.Count == 0)
                {
                    this.ValidationResult.AddError(this,
                        $"Main file {lastVersion} in card {this.CardID} is not sign");

                    logger.Info($"OutshipContainer AllSignElementAsync Main file {lastVersion} in card {this.CardID} is not sign");

                    return null;
                }

                var result = new List<XElement>();

                var signStampsList = await this.GetAllSignStampsInfoAsync();

                foreach (var signStamp in signStampsList)
                {
                    if (signStamp.StampTypeID != (int)StampType.Sign)
                    {
                        continue;
                    }

                    if (signStamp.FileSignaturesRowID == null)
                    {
                        logger.Error("OutshipContainer AllSignElementAsync signstamp without sign");
                        continue;
                    }

                    var signature = signatures[(Guid)signStamp.FileSignaturesRowID];

                    Dictionary<string, int?> signPosInfo = this.ConvertStampItemToDict(signStamp);
                    //    new Dictionary<string, int?>();

                    //signPosInfo.Add(MedoTag.TagPage, signStamp.Page);
                    //signPosInfo.Add(MedoTag.TagX, signStamp.X);
                    //signPosInfo.Add(MedoTag.TagY, signStamp.Y);
                    //signPosInfo.Add(MedoTag.TagW, signStamp.Width);
                    //signPosInfo.Add(MedoTag.TagH, signStamp.Height);

                    result.Add(await this.SignElementAsync(signature, signPosInfo));
                }


                //foreach (var signature in signatures)
                //{
                //    var signPosInfo = await MedoHelper.GetStampDataAsync(
                //        this.DbScope,
                //        this.ValidationResult,
                //        StampType.Sign,
                //        MedoConst.GetSignStampInfo,
                //        this.CardID,
                //        signature.ID);

                //    if (signPosInfo.IsNullOrEmpty())
                //    {
                //        this.ValidationResult.AddError(this, $"Cant get info about sign stamp, with sign count {signatures.Count}");

                //        logger.Info($"OutshipContainer AllSignElementAsync Cant get info about sign stamp, with sign count {signatures.Count}");

                //        continue;
                //    }

                //    result.Add(await this.SignElementAsync(signature, signPosInfo));
                //}

                return result;
            }

            return null;
        }

        private async Task<List<MedoStampInfo>> GetAllSignStampsInfoAsync()
        {
            await using (this.DbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = this.DbScope.Db;

                var builderFactory = this.DbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().C("MedoStampInfo","Page", "X", "Y", "Width", "Height", "StampTypeID", "FileSignaturesRowID")
                            .From("MedoStampInfo").NoLock()
                            .Where().C("ID").Equals().P("cardID")
                            //.And().C("StampTypeID").Equals().P("stampTypeId")
                            .Build(),
                        db.Parameter("cardID", this.CardID))
                    //    db.Parameter("stampTypeId", StampType.Sign))
                    .LogCommand()
                    .ExecuteListAsync<MedoStampInfo>();
                
                if (result == null || result.Count == 0)
                {
                    logger.Info($"GetAllStampsInfo no elements cardID: {this.CardID}");
                }

                foreach (var item in result)
                {
                    logger.Info($"Page: {item.Page}\nX: {item.X}\nY: {item.Y}\nStampTypeID: {item.StampTypeID}\nFileSignaturesRowID: {item.FileSignaturesRowID}");
                }

                return result;
            }
        }

        private Dictionary<string, int?> ConvertStampItemToDict(MedoStampInfo signStamp)
        {
            Dictionary<string, int?> signPosInfo = new Dictionary<string, int?>();

            signPosInfo.Add(MedoTag.TagPage, signStamp.Page);
            signPosInfo.Add(MedoTag.TagX, signStamp.X);
            signPosInfo.Add(MedoTag.TagY, signStamp.Y);
            signPosInfo.Add(MedoTag.TagW, signStamp.Width);
            signPosInfo.Add(MedoTag.TagH, signStamp.Height);

            return signPosInfo;
        }


        private async Task<XElement> SignElementAsync(
            IFileSignature sign,
            Dictionary<string, int?> info,
            CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer SignElementAsync");

            //var userData = await this.GetPersonDataByGuidAsync(sign.UserID, cancellationToken);

            var subjectData = await this.GetPersonDataByFullNameAsync(sign.SubjectName, cancellationToken);

            logger.Info($"{sign.SubjectName.Trim(' ')}");

            Dictionary<string, string> tempUserData = new Dictionary<string, string>();

            bool isUserFound = false;

            try
            {
                if (subjectData == null)
                {
                    logger.Error($"User not found {sign.SubjectName}");
                }
                else
                {
                    isUserFound = true;
                    foreach (var s in subjectData)
                    {
                        logger.Info($"s.Key {s.Key} s.Value {s.Value}");
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error($"{ex.Message}\n{ex.StackTrace}");
            }

            XElement person = null;

            if (isUserFound)
            {
                person = new XElement(this.Ns + MedoTag.TagPerson,
                subjectData.IsNullOrEmpty()
                ? null
                : new XAttribute(this.Ns + MedoTag.TagId, subjectData["ID"]),
                new XElement(this.Ns + MedoTag.TagPost,
                    subjectData.IsNullOrEmpty() || subjectData[MedoTag.TagPost].IsNullOrEmpty()
                        ? "-"
                        : subjectData[MedoTag.TagPost]),
                new XElement(this.Ns + MedoTag.TagName, sign.SubjectName),

                subjectData.IsNullOrEmpty() || !subjectData[MedoTag.TagPersonPhone].IsNullOrEmpty()
                    ? new XElement(this.Ns + MedoTag.TagPersonPhone, subjectData[MedoTag.TagPersonPhone])
                    : null,
                //: new XElement(this.Ns + MedoTag.TagPersonPhone, " "),
                //new XElement(this.Ns + MedoTag.TagPersonPhone, userData[MedoTag.TagPersonPhone].IsNullOrEmpty() ? "8 (3012) 21-02-51" : userData[MedoTag.TagPersonPhone]),
                //await this.GetPhoneNumberAsync(cancellationToken).ConfigureAwait(false)),
                subjectData.IsNullOrEmpty() || subjectData[MedoTag.TagPersonEmail].IsNullOrEmpty()
                    ? null
                    : new XElement(this.Ns + MedoTag.TagPersonEmail, subjectData[MedoTag.TagPersonEmail]));
            }
            else
            {
                person = new XElement(this.Ns + MedoTag.TagPerson,
                new XElement(this.Ns + MedoTag.TagPost, "-"),
                new XElement(this.Ns + MedoTag.TagName, sign.SubjectName));
            }


            return new XElement(this.Ns + MedoTag.TagSign,
                person,
                this.DocumentSignatureElement(sign, info));
        }

        /// <summary>
        ///     Получаем информацию о сотруднике
        /// </summary>
        /// <param name="id">id сотрудника</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, string>> GetPersonDataByGuidAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer GetPersonDataAsync");

            await using (this.DbScope.Create())
            {
                var db = this.DbScope.Db;
                var result = new Dictionary<string, string>();

                db.SetCommand(MedoConst.GetPersonData, db.Parameter("@id", id)).LogCommand();
                await using (var reader = await db.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        result.Add(MedoTag.TagPost, reader.GetValue<string>(0));
                        result.Add(MedoTag.TagPersonPhone, reader.GetValue<string>(1));
                        result.Add(MedoTag.TagPersonEmail, reader.GetValue<string>(2));
                        result.Add("FullName", reader.GetValue<string>(3));

                        return result;
                    }
                }
            }

            return null;
        }

        private async Task<Dictionary<string, string>> GetPersonDataByFullNameAsync(
            string fullName,
            CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer GetPersonDataAsync");

            await using (this.DbScope.Create())
            {
                var db = this.DbScope.Db;
                var result = new Dictionary<string, string>();

                string GetPersonData = "SELECT \"p\".\"Position\", \"p\".\"Phone\", \"p\".\"Email\", \"p\".\"FullName\", \"p\".\"ID\" " +
                                                "FROM \"PersonalRoles\" AS \"p\" " +
                                                "WHERE \"p\".\"FullName\" = @fullName";

                db.SetCommand(GetPersonData, db.Parameter("@fullName", fullName.Trim(' '))).LogCommand();
                await using (var reader = await db.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        result.Add(MedoTag.TagPost, reader.GetValue<string>(0));
                        result.Add(MedoTag.TagPersonPhone, reader.GetValue<string>(1));
                        result.Add(MedoTag.TagPersonEmail, reader.GetValue<string>(2));
                        result.Add("FullName", reader.GetValue<string>(3));
                        result.Add("ID", reader.GetValue<Guid>(4).ToString());

                        return result;
                    }
                }
            }

            return null;
        }

        private async Task<string> GetPhoneNumberAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer GetPhoneNumberAsync");
            return "911";
            //await using var _ = 
            //    this.DbScope.Create();
            //return await this.DbScope.Db.SetCommand(
            //        this.DbScope.BuilderFactory
            //            .Select().C("Phone")
            //            .From("MedoRB").NoLock()
            //            .Where().C("ID").Equals().V(SettingsInfo.StampInfo.ID)
            //            .Build())
            //    .LogCommand()
            //    .ExecuteAsync<string>(cancellationToken)
            //    .ConfigureAwait(false);
        }

        private async Task<XElement> ExecutorElementAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer ExecutorElementAsync");
            var (execId, execName) = (
                this.Card.Sections[SchemeInfo.DocumentCommonInfo].Fields.Get<Guid>(SchemeInfo.DocumentCommonInfo.AuthorID/*.SignedByID*/),
                this.Card.Sections[SchemeInfo.DocumentCommonInfo].Fields.Get<string>(SchemeInfo.DocumentCommonInfo.AuthorName/*.SignedByName*/));
            var executorInfo = await this.GetPersonDataByGuidAsync(
                this.Card.Sections[SchemeInfo.DocumentCommonInfo].Fields.Get<Guid>(SchemeInfo.DocumentCommonInfo.AuthorID/*.SignedByID*/),
                cancellationToken)
                                         .ConfigureAwait(false);
            return new XElement(this.Ns + MedoTag.TagExecutor,
                new XAttribute(this.Ns + MedoTag.TagId, execId),
                new XElement(this.Ns + MedoTag.TagPost,
                    executorInfo.IsNullOrEmpty() || executorInfo[MedoTag.TagPost].IsNullOrEmpty()
                        ? " "
                        : executorInfo[MedoTag.TagPost]),
                new XElement(this.Ns + MedoTag.TagName, executorInfo["FullName"]),
                !executorInfo[MedoTag.TagPersonPhone].IsNullOrEmpty() 
                    ? new XElement(this.Ns + MedoTag.TagPersonPhone, executorInfo[MedoTag.TagPersonPhone]) 
                    : new XElement(this.Ns + MedoTag.TagPersonPhone, " "),

                //new XElement(this.Ns + MedoTag.TagPersonPhone, executorInfo[MedoTag.TagPersonPhone].IsNullOrEmpty() ? "8 (3012) 21-02-51" : executorInfo[MedoTag.TagPersonPhone]),
                executorInfo.IsNullOrEmpty() || executorInfo[MedoTag.TagPersonEmail].IsNullOrEmpty()
                    ? null
                    : new XElement(this.Ns + MedoTag.TagPersonEmail, executorInfo[MedoTag.TagPersonEmail]));
        }

        /// <summary>
        ///     Элемент подписи
        /// </summary>
        /// <param name="sign">файл подписи</param>
        /// <param name="info">информация по положению подписи</param>
        /// <returns></returns>
        private XElement DocumentSignatureElement(IFileSignature sign, Dictionary<string, int?> info)
        {
            logger.Info("OutshipContainer DocumentSignatureElement");

            var signName = sign.ID.ToString().Replace("-", "");

            var stamp =
                new XElement(this.Ns + MedoTag.TagSignStamp,
                    new XAttribute(this.Ns + MedoTag.TagLocalName, signName + ".png"),
                    new XElement(this.Ns + MedoTag.TagPosition,
                        new XElement(this.Ns + MedoTag.TagPage, info[MedoTag.TagPage]),
                        new XElement(this.Ns + MedoTag.TagTopLeft,
                            new XElement(this.Ns + MedoTag.TagX, info[MedoTag.TagX]),
                            new XElement(this.Ns + MedoTag.TagY, info[MedoTag.TagY])),
                        new XElement(this.Ns + MedoTag.TagDimension,
                            new XElement(this.Ns + MedoTag.TagW, info[MedoTag.TagW]),
                            new XElement(this.Ns + MedoTag.TagH, info[MedoTag.TagH]))));

            return new XElement(this.Ns + MedoTag.TagDocumentSignature,
                new XAttribute(this.Ns + MedoTag.TagLocalName, signName + ".p7s"),
                new XAttribute(this.Ns + MedoTag.TagType, !string.IsNullOrWhiteSpace(sign.Comment) ? CheckSignComment(sign.Comment) : "Утверждающая"),
                stamp);
        }

        private string CheckSignComment(string comment)
        {
            string result;

            switch(comment.Trim(' ').ToLower())
            {
                case "утверждающая" or "утверждено":
                    result = "Утверждающая";
                    break;
                case "заверяющая" or "заверено":
                    result = "Заверяющая";
                    break;
                case "согласующая" or "согласовано":
                    result = "Согласующая";
                    break;
                default:
                    result = "Утверждающая";
                    break;
            }

            return result;
        }

        #endregion

        #endregion

        #region Main File

        /// <summary>
        ///     получаем главный файл и его подписи
        /// </summary>
        private async Task GetMainFileAndSignatureAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer GetMainFileAndSignatureAsync");

            if (this.MainFile?.LastVersion == null)
            {
                this.ValidationResult.AddError(this, "Cant get info about main file");

                logger.Info("OutshipContainer GetMainFileAndSignatureAsync Cant get info about main file");

                return;
            }

            if (Path.GetExtension(this.MainFile.Name)?.ToUpperInvariant() != ".PDF")
            {
                this.ValidationResult.AddError(this, "Main file is not in pdf format");

                logger.Info("OutshipContainer GetMainFileAndSignatureAsync Cant get info about main file");

                return;
            }

            var files = new List<FileData>
            {
                new(this.Card.ID, this.MainFile.RowID, this.MainFile.Name, this.MainFile.LastVersion.RowID)
            };

            WorkWithFileHelper.SaveFiles sf = SaveMainFileAsync;
            await WorkWithFileHelper.SaveFilesToTempFolderAsync(this.permissionsProvider, this.cardStreamRepository,
                this.ValidationResult, files, this.tempPath, sf, logger);

            var sCount = await this.GetSignatureAsync(this.MainFile, cancellationToken);
            if (sCount == 0)
            {
                this.ValidationResult.AddError(this, "Main file hasnt signature");

                logger.Info("OutshipContainer GetMainFileAndSignatureAsync Main file hasnt signature");
            }
        }

        #endregion

        #region Stamp

        /// <summary>
        ///     создаем регистрационный штамп
        /// </summary>
        private async Task CreateRegStampAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer CreateRegStampAsync");
            //todo: создается элемент 2 раза
            var regStamp = await this.RegStampElementAsync(StampType.Reg, MedoConst.RegStampName, MedoTag.TagRegStamp, this.CardID, cancellationToken);

            var w = regStamp?.Element(this.Ns + MedoTag.TagPosition)
                            ?.Element(this.Ns + MedoTag.TagDimension)
                            ?.Element(this.Ns + MedoTag.TagW)?.Value;
            var h = regStamp?.Element(this.Ns + MedoTag.TagPosition)
                            ?.Element(this.Ns + MedoTag.TagDimension)
                            ?.Element(this.Ns + MedoTag.TagH)?.Value;

            if (w.IsNullOrEmpty() || h.IsNullOrEmpty() || this.RegNumber.IsNullOrEmpty() || this.RegDate == null)
            {
                this.ValidationResult.AddError(this,
                    $"Cant create registration stamp: w:{w}, h:{h}, number:{this.RegNumber}, date{this.RegDate}");

                logger.Info($"OutshipContainer CreateRegStampAsync Cant create registration stamp: w:{w}, h:{h}, number:{this.RegNumber}, date{this.RegDate}");

                return;
            }

            var str = $"{this.RegNumber}        {this.RegDate ?? DateTime.Now:dd-MM-yyyy}";
            this.DrawRegStamp(w, h, str, Path.Combine(this.tempPath, MedoConst.RegStampName));
        }

        /// <summary>
        ///     создаем все штампы подписей
        /// </summary>
        private async Task CreateAllSignStampAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer CreateAllSignStampAsync");
            var signatures = await this.GetAllSignaturesAsync(this.MainFile, cancellationToken);
            if (signatures?.Count > 0)
            {
                var signStamps = await this.GetAllSignStampsInfoAsync();

                foreach(var signStamp in signStamps)
                {
                    if (signStamp.StampTypeID != (int)StampType.Sign)
                    {
                        continue;
                    }

                    var signPosInfo = this.ConvertStampItemToDict(signStamp);

                    var signature = signatures[(Guid)signStamp.FileSignaturesRowID];

                    if (signPosInfo.IsNullOrEmpty())
                    {
                        this.ValidationResult.AddError(this, $"Cant get position for sign {signature.ID}");

                        logger.Info($"OutshipContainer CreateAllSignStampAsync Cant get position for sign {signature.ID}");

                        return;
                    }

                    var signName = signature.ID.ToString().Replace("-", "") + ".png";

                    await this.DrawSignStampAsync(
                        (int)signPosInfo[MedoTag.TagW],
                        (int)signPosInfo[MedoTag.TagH],
                        signature,
                        Path.Combine(this.tempPath, signName),
                        cancellationToken);
                }

                return;
            }

            this.ValidationResult.AddError(this, $"Cant get main file signatures, card id; {this.CardID}");

            logger.Info($"OutshipContainer CreateAllSignStampAsync Cant get main file signatures, card id; {this.CardID}");
        }

        private static async Task<(DateTime?, DateTime?)> GetSignatureStampInfoAsync(
            IFileSignature signature,
            CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer GetSignatureStampInfoAsync");

            var contentInfo = new ContentInfo(await signature.Data.GetBytesAsync(cancellationToken));
            var signedCms = new SignedCms(contentInfo);

            try
            {
                signedCms.Decode(signedCms.ContentInfo.Content);
            }
            catch (CryptographicException ex)
            {
                logger.LogException(ex);
                return (null, null);
            }

            X509Certificate2 certificateClient = null;

            for (var index = signedCms.Certificates.Count - 1; index >= 0; index--)
            {
                var cert = signedCms.Certificates[index];

                if (!string.Equals(cert.SerialNumber, signature.SerialNumber, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                certificateClient = cert;
                break;
            }

            if (certificateClient is null)
            {
                throw new ArgumentNullException(nameof(X509Certificate2));
            }

            return (certificateClient.NotBefore, certificateClient.NotAfter);
        }

        /// <summary>
        ///     отрисовка png штампа
        /// </summary>
        /// <param name="w">ширина штампа</param>
        /// <param name="h">высота штампа</param>
        /// <param name="info">надпись для штампа</param>
        /// <param name="name">имя файла</param>
        private void DrawRegStamp(string w, string h, string info, string name)
        {
            logger.Info("OutshipContainer DrawRegStamp");

            if (!int.TryParse(w, out var width) || !int.TryParse(h, out var height))
            {
                this.ValidationResult.AddError(this, $"Cant parse width {w} or height {h}");

                logger.Info($"OutshipContainer DrawRegStamp Cant parse width {w} or height {h}");

                return;
            }

            logger.Info($"OutshipContainer DrawRegStamp w: {w} h: {h} info: {info} name: {name}");

            logger.Info("OutshipContainer DrawRegStamp TryParse pass");

            var font = new System.Drawing.Font("Times New Roman", 14);

            using var graphics = Graphics.FromImage(new Bitmap(1, 1));
            var size = graphics.MeasureString(info, font);

            logger.Info($"OutshipContainer DrawRegStamp size.Width: {size.Width} size.Height: {size.Height}");

            var bitmap = new Bitmap((int)size.Width, (int)size.Height);
            try
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    using (var sb = new SolidBrush(System.Drawing.Color.White))
                    {
                        g.FillRectangle(sb, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                    }

                    g.DrawString(info, font, Brushes.Black, PointF.Empty);
                }

                var ww = width * bitmap.HorizontalResolution / 25.4;
                var hh = height * bitmap.VerticalResolution / 25.4;
                
                logger.Info($"OutshipContainer DrawRegStamp width: {bitmap.HorizontalResolution} height: {bitmap.VerticalResolution} try to save ww: {ww} hh {hh}");

                var resize = new Bitmap(bitmap, (int)width, (int)height);

                var ms = new MemoryStream();
                ms.Position = 0;

                //resize.Save(name);
                //resize.Save("./MedoTempTest/regStamp.png");
                bitmap.Save(name);
            }
            catch (Exception e)
            {
                this.ValidationResult.AddError(this, e.Message);


                logger.Info($"OutshipContainer DrawRegStamp stacktrace {e.StackTrace} message {e.Message}");
            }
        }

        /// <summary>
        ///     отрисовка штампа поддписи
        /// </summary>
        /// <param name="w">ширина</param>
        /// <param name="h">высота</param>
        /// <param name="signature"></param>
        /// <param name="name">имя для файла</param>
        /// <param name="cancellationToken"></param>
        private async Task DrawSignStampAsync(int w, int h, IFileSignature signature, string name, CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer DrawSignStampAsync");

            if (System.IO.File.Exists(name))
            {
                logger.Info($"OutshipContainer DrawSignStampAsync Stamp {name} is already exist");
                return;
            }

            //получаем путь к файлу подписи из app.json
            var bitmapPath = ConfigurationManager.Settings.TryGet<string>(MedoConst.BitmapStampPath);
            if (bitmapPath.IsNullOrEmpty())
            {
                this.ValidationResult.AddError(this, "Cant get BitmapStamp from app.json");

                logger.Info("OutshipContainer DrawSignStampAsync Cant get BitmapStamp from app.json");

                return;
            }

            var bitmap = new Bitmap(bitmapPath);
            var (notBefore, notAfter) = await GetSignatureStampInfoAsync(signature, cancellationToken);

            try
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    using (var font = new System.Drawing.Font("Verdana", 24, FontStyle.Regular))
                    {
                        graphics.DrawString("Сертификат:", font, Brushes.Black, 45, 350);
                        graphics.DrawString(signature.SerialNumber, font, Brushes.Black, 305, 350);
                        graphics.DrawString("Владелец:", font, Brushes.Black, 45, 400);
                        graphics.DrawString(signature.SubjectName, font, Brushes.Black, 305, 400);
                        graphics.DrawString("Действителен", font, Brushes.Black, 45, 450);
                        graphics.DrawString($"с {notBefore ?? DateTime.UtcNow:dd.MM.yyyy} по {notAfter ?? DateTime.UtcNow:dd.MM.yyyy}", font, Brushes.Black, 305, 450);
                    }
                }

                var ww = w * bitmap.HorizontalResolution / 25.4;
                var hh = h * bitmap.VerticalResolution / 25.4;

                var resize = new Bitmap(bitmap, (int)ww, (int)hh);
                //resize.Save(name);
                //resize.Save("./MedoTempTest/signTest.png");
                bitmap.Save(name);
                //bitmap.Save("./MedoTempTest/signTest.png");
            }
            catch (Exception e)
            {
                this.ValidationResult.AddError(this, e.Message);
            }
        }

        #endregion

        #region Attachment

        /// <summary>
        ///     добавляем в папку информацию о приложениях
        /// </summary>
        private async Task AddAttachmentAndSignatureAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer AddAttachmentAndSignatureAsync");

            //var files = this.GetFilesInfo(this.Card.Sections["Files"].Rows);

            var files = this.GetFilesInfo();

            if (files == null)
            {
                return;
            }

            WorkWithFileHelper.SaveFiles sf = SaveAttachmentFileAsync;
            await WorkWithFileHelper.SaveFilesToTempFolderAsync(this.permissionsProvider, this.cardStreamRepository,
                this.ValidationResult, files, this.tempPath, sf, logger);

            //добавление подписей
            var allFiles = this.Card.TryGetFiles();

            var attachments = files.Select(file => allFiles.FirstOrDefault(x => x.RowID == file.Item2)).Where(x => x != null).ToList();
            foreach (var attachment in attachments)
            {
                await this.GetSignatureAsync(attachment, cancellationToken);
            }
        }

        /// <summary>
        ///     получаем файлы, которые надо отправить по мэдо
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        private IList<FileData> GetFilesInfo(/*ListStorage<CardRow> rows*/)
        {
            logger.Info("OutshipContainer GetFilesInfo");

            var result = new List<FileData>();

            var cardFiles = this.Card.Files;
            var medoFiles = this.Card.Sections["MedoFiles"].TryGetRows();

            if (medoFiles == null)
            {
                return null;
            }

            var cardMedoFiles = medoFiles.Select(x => x).Where(x => (Guid)x.Fields["ParentRowID"] == this.RowID).ToList();

            foreach(var row in cardMedoFiles)
            {
                var f = cardFiles.Select(x => x).Where(x => x.RowID == (Guid)row.Fields["FilesID"]).FirstOrDefault();

                if (f != null)
                {
                    result.Add(new FileData(
                        this.Card.ID,
                        f.Card.ID,
                        f.Name,
                        f.VersionRowID));

                    logger.Info($"OutshipContainer GetFilesInfo cardID: {this.Card.ID} fileID: {f.Card.ID} fileName: {f.Name} versionID: {f.VersionRowID}");
                }
            }

            //foreach (var f in Card.Files)
            //{
            //    if(f.CategoryID == FileCategories.MainDoc.ID || f.CategoryID == FileCategories.Preview.ID)
            //    {
            //        continue;
            //    }

            //    result.Add(new FileData(
            //            this.Card.ID,
            //            f.Card.ID,
            //            f.Name,
            //            f.VersionRowID));

            //    logger.Info($"OutshipContainer GetFilesInfo cardID: {this.Card.ID} fileID: {f.Card.ID} fileName: {f.Name} versionID: {f.VersionRowID}");
            //}


            //foreach (var row in rows)
            //{
            //    if (row.Fields.Get<Guid>("FileID") != this.MainFile?.RowID && row.Fields.Get<bool>("MedoSend"))
            //    {
            //        result.Add(new FileData(
            //            this.Card.ID,
            //            row.Fields.Get<Guid>("FileID"),
            //            row.Fields.Get<string>("FileName"),
            //            row.Fields.Get<Guid>("VersionID")));
            //    }
            //}

            return result;
        }

        /// <summary>
        ///     сохраняем файлы приложения во временную папку
        /// </summary>
        /// <param name="contentResult">ICardFileContentResult</param>
        /// <param name="directory">папка</param>
        /// <param name="name">имя файла</param>
        /// <param name="versionID">id версии файла</param>
        private static async Task SaveAttachmentFileAsync(
            ICardFileContentResult contentResult,
            string directory,
            string name,
            string versionID)
        {
            logger.Info($"OutshipContainer SaveAttachmentFileAsync directory: {directory} name: {name} versionID: {versionID}");

            await using var stream = await contentResult.GetContentOrThrowAsync();
            Directory.CreateDirectory(directory);

            string newName = name;

            string ext = newName.Substring(newName.LastIndexOf('.'));
            string fileName = newName.Substring(0, newName.LastIndexOf("."));

            //if (Regex.Match(fileName, "[А-Яа-яЁё]").Success)
            //{
            //    fileName = Transliteration.Translit(fileName);
            //}

            //Regex.Replace(fileName, @"[^0-9a-zA-Z_]+", "_");

            newName = Transliteration.Translit(fileName) + ext;

            //var newName = versionID.Replace("-", "") + Path.GetExtension(name);
            var p = Path.Combine(directory, newName);

            await using var f = File.Create(p);
            logger.Info($"OutshipContainer SaveAttachmentFileAsync save file {p}");
            await stream.WriteStreamAsync(f);
        }

        /// <summary>
        ///     сохраняем файлы приложения во временную папку
        /// </summary>
        /// <param name="contentResult">ICardFileContentResult</param>
        /// <param name="directory">папка</param>
        /// <param name="name">имя файла</param>
        /// <param name="versionID">id версии файла</param>
        private static async Task SaveMainFileAsync(ICardFileContentResult contentResult, string directory, string name, string versionID)
        {
            logger.Info("OutshipContainer SaveMainFileAsync");

            await using var stream = await contentResult.GetContentOrThrowAsync();
            Directory.CreateDirectory(directory);

            string newName = name;

            string ext = newName.Substring(newName.LastIndexOf('.'));
            string fileName = newName.Substring(0, newName.LastIndexOf("."));

            //if (Regex.Match(fileName, "[А-Яа-яЁё]").Success)
            //{
            //    fileName = Transliteration.Translit(fileName);
            //}

            //Regex.Replace(fileName, @"[^0-9a-zA-Z_]+", "_");

            newName = Transliteration.Translit(fileName) + ext;

            var p = Path.Combine(directory, newName);

            await using var f = File.Create(p);
            await stream.WriteStreamAsync(f);
        }

        #endregion

        /// <summary>
        /// Получение адреса корреспондента МЭДО
        /// </summary>
        /// <param name="partnerID"></param>
        /// <param name="dbScope"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task<string> GetPartnerMedoAddress(
            Guid partnerID,
            IDbScope dbScope,
            CancellationToken cancellationToken = default)
        {
            await using (dbScope.Create())
            {
                DataParameter[] param = new DataParameter[]
                {
                    new DataParameter("@partnerID", partnerID)
                };

                return await dbScope.Db.SetCommand
                    ("SELECT \"MedoAddress\" " +
                        "FROM \"Partners\" " +
                        "WHERE \"ID\" = @partnerID", param)
                        .LogCommand()
                        .ExecuteAsync<string>(cancellationToken);
            }
        }

        /// <summary>
        ///     Создание zip контейнер мэдо и сохраняем его в папку исходящих
        /// </summary>
        protected void AddFileAtZip()
        {
            logger.Info("OutshipContainer AddFileAtZip");

            var zipPath = Path.Combine(this.GlobalPath, "document.edc.zip");

            if (File.Exists(zipPath))
            {
                this.ValidationResult.AddWarning($"Zip folder {zipPath} already exist");

                logger.Info($"OutshipContainer AddFileAtZip Zip folder {zipPath} already exist");

                return;
            }

            var zipArchivePath = Path.Combine(this.ArchivePath, "document.edc.zip");

            if (File.Exists(zipArchivePath))
            {
                this.ValidationResult.AddWarning($"Zip folder {zipArchivePath} already exist");

                logger.Info($"OutshipContainer AddFileAtZip Zip folder {zipArchivePath} already exist");

                return;
            }

            try
            {
                //создаем директорию, если она не существует
                Directory.CreateDirectory(this.GlobalPath);

                using var newFile = ZipFile.Open(zipPath, ZipArchiveMode.Create);
                foreach (var file in Directory.GetFiles(this.tempPath))
                {
                    newFile.CreateEntryFromFile(file, Path.GetFileName(file));
                }

                //создаем директорию, если она не существует
                Directory.CreateDirectory(this.ArchivePath);

                using var newArchiveFile = ZipFile.Open(zipArchivePath, ZipArchiveMode.Create);
                foreach (var file in Directory.GetFiles(this.tempPath))
                {
                    newArchiveFile.CreateEntryFromFile(file, Path.GetFileName(file));
                }

            }
            catch (Exception e)
            {
                this.ValidationResult.AddError(e.StackTrace, e.Message);

                logger.Info($"OutshipContainer AddFileAtZip {e.Message}");
            }
        }

        /// <summary>
        ///     xml файл с описанием контейнера
        /// </summary>
        /// <returns>возвращает id сообщения МЭДО</returns>
        protected void Communication()
        {
            logger.Info("OutshipContainer Communication");

            var deliveryAdresses = this.Card.Sections["Correspondents"].Rows
                                            .Where(x => x.Get<Guid>("PartnersID") == this.MedoPartnerID)
                                            .FirstOrDefault();

            //var deliveryAdresses = this.Card.Sections["RecieversPartners"].Rows
                                       //.Where(r => r.Get<Guid>("DeliveryTypeID") == SchemeInfo.DeliveryTypes_MEDO_ID)
                                       //.Concat(this.Card.Sections[SchemeInfo.CopyRecieversPartners].Rows
                                      // .Where(r => r.Get<int>(SchemeInfo.CopyRecieversPartners.DeliveryTypeID) == SchemeInfo.DeliveryTypes.MEDO.ID))
                                       ;
            var communicationEl = new XElement(this.Xdms + MedoTag.TagCommunication,
                new XAttribute(XNamespace.Xmlns + "xdms", this.Xdms),
                new XAttribute(this.Xdms + MedoTag.TagVersion, this.XsdVersion.GetDescription()),
                new XElement(this.Xdms + MedoTag.TagHeader,
                    new XAttribute(this.Xdms + MedoTag.TagMesType,
                        MedoMessageType.ShippingContainer.GetDescription()),
                    new XAttribute(this.Xdms + MedoTag.TagMesUid, this.MainMesID),
                    new XAttribute(this.Xdms + MedoTag.TagCreated, DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Local).ToString("yyyy-MM-ddTHH:mm:ssK")),
                    new XElement(this.Xdms + MedoTag.TagSource,
                        new XElement(this.Xdms + MedoTag.TagComOrg, MedoConst.RBName),
                        new XAttribute(this.Xdms + MedoTag.TagOrgUid, MedoConst.RBMedoID))),
                new XElement(this.Xdms + MedoTag.TagContainer,
                    new XAttribute(this.Xdms + MedoTag.TagContainerType, MedoConst.ContainerType),
                    new XElement(this.Xdms + MedoTag.TagBody, "document.edc.zip"),
                    this.ShipContainerSignElement()));

            var deliveryTags = new XElement(this.Xdms + MedoTag.TagDeliveryIndex);

            //string PartnerMedoId = this.Card.Sections["DocumentCommonInfo"].Fields["PartnerMedoID"].ToString();
            //string PartnerName = this.Card.Sections["DocumentCommonInfo"].Fields["PartnerName"].ToString();

            //logger.Info($"PartnerMedoId: {PartnerMedoId} PartnerName: {PartnerName}");

            deliveryTags.Add(
                    new XElement(this.Xdms + MedoTag.TagDestination,
                    new XElement(this.Xdms + MedoTag.TagDestination, new XAttribute(this.Xdms + MedoTag.TagUid, deliveryAdresses["PartnersMedoID"]),
                    new XElement(this.Xdms + MedoTag.TagOrganization, deliveryAdresses["PartnersName"]))));

            //foreach (var row in deliveryAdresses)
            //{
            //    deliveryTags.Add(
            //        new XElement(this.Xdms + MedoTag.TagDestination,
            //        new XElement(this.Xdms + MedoTag.TagDestination, new XAttribute(this.Xdms + MedoTag.TagUid, deliveryAdresses["PartnersMedoID"]),
            //        new XElement(this.Xdms + MedoTag.TagOrganization, deliveryAdresses["PartnersName"]))));
            //}

            communicationEl.Add(deliveryTags);

            XDeclaration xd = new XDeclaration("1.0", "utf-8", null);

            xd.Standalone = null;

            var docXml = new XDocument(
                xd,
                //MedoConst.Declare, 
                communicationEl);

            if (docXml.CheckXml(this.XsdVersion, XmlType.Message, this.ValidationResult, this.CommunicationPath))
            {
                using (var writer = new XmlTextWriter(Path.Combine(this.GlobalPath, "communication.xml"), new UTF8Encoding(false)))
                {
                    //writer.Settings.NewLineChars = "\r\n";
                    docXml.Save(writer);
                }

                using (var writer = new XmlTextWriter(Path.Combine(this.ArchivePath, "communication.xml"), new UTF8Encoding(false)))
                {
                    //writer.Settings.NewLineChars = "\r\n";
                    docXml.Save(writer);
                }
                //docXml.Save(Path.Combine(this.GlobalPath, "communication.xml"));
            }
        }

        /// <summary>
        ///     Элемент с информацией о файле подписи контейнера
        /// </summary>
        /// <returns></returns>
        protected XElement ShipContainerSignElement() =>
            Directory.GetFiles(this.tempPath).Any(x => Path.GetFileName(x) == MedoConst.ContainerSign)
                ? new XElement(this.Xdms + MedoTag.TagSignature, MedoConst.ContainerSign)
                : null;

        /// <summary>
        ///     получаем и сохраняем подписи файла
        /// </summary>
        /// <param name="file">CardFile</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<int> GetSignatureAsync(CardFile file, CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer GetSignatureAsync");
            var signatures = await this.GetAllSignaturesAsync(file, cancellationToken);

            if (signatures == null)
            {
                logger.Info("OutshipContainer GetSignatureAsync signatures is null");

                return 0;
            }

            var signStampsList = await this.GetAllSignStampsInfoAsync();

            //var sign = signStampsList.Select(x => x.FileSignaturesRowID).Where(x => x == )

            foreach (var signature in signatures)
            {
                if (file.Card.ID == this.MainFile.Card.ID)
                {
                    var signByStamp = signStampsList.Select(x => x).Where(x => x.FileSignaturesRowID == signature.ID).FirstOrDefault();

                    if (signByStamp == null)
                    {
                        continue;
                    }
                }

                if (!signature.Data.IsEmpty)
                {
                    var str = signature.ID.ToString().Replace("-", "") + ".p7s";
                    await File.WriteAllBytesAsync(Path.Combine(this.tempPath, str), await signature.Data.GetBytesAsync(cancellationToken), cancellationToken);
                }
                else
                {
                    logger.Info($"OutshipContainer GetSignatureAsync Cant get data from signature {signature.ID}, file version {file.VersionRowID}");

                    this.ValidationResult.AddError(this, $"Cant get data from signature {signature.ID}, file version {file.VersionRowID}");
                }
            }

            return signatures.Count;
        }

        private async Task<IFileSignatureCollection> GetAllSignaturesAsync(CardFile file, CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer GetAllSignaturesAsync");
            await using var container = await this.fileManager.CreateContainerAsync(this.Card, cancellationToken: cancellationToken);
            var lastVersion = container.FileContainer.Files.TryGet(file.RowID).Versions.Last;
            return (await lastVersion.Source.GetSignaturesAsync(lastVersion, FileSignatureLoadingMode.WithData, cancellationToken))?.Signatures;
        }

        #endregion
    }

    public class MedoStampInfo
    {
        public int Page { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int StampTypeID { get; set; }
        public Guid? FileSignaturesRowID { get; set; }
    }
}
