using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using LinqToDB.Common;
using LinqToDB.Data;
using NLog;
using NLog.Fluent;
using Tessa.Cards;
using Tessa.Extensions.Chronos.Medo.MedoSend.Helpers;
using Tessa.Extensions.Shared.Helpers;
using Tessa.Extensions.Shared.Helpers.Medo;
using Tessa.Extensions.Shared.Info;
using Tessa.Platform;
using Tessa.Platform.Collections;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using Unity;

namespace Tessa.Extensions.Chronos.Medo.MedoSend.OutgoingTypes
{
    public class OutNotification : OutgoingContext
    {
        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructor

        public OutNotification(
            IUnityContainer container,
            IDbScope dbScope,
            string globalPath,
            NewMessageInfo newMessageInfo)
            : base(container,
                dbScope,
                globalPath,
                newMessageInfo)
        {
            this.newMessage = newMessageInfo;
            this.responseID = Guid.NewGuid();
        }

        NewMessageInfo newMessage;
        Guid responseID;

        #endregion

        #region Override

        public override async Task CreateMedoContainerAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutNotification CreateMedoContainerAsync");

            await using var instance = this.DbScope.Create();

            var date = this.XsdVersion == XsdVersion.NewVersion
                ? (object)DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Local).ToString("yyyy-MM-ddTHH:mm:ssK")
                : DateTime.UtcNow;
            //var people = new Dictionary<int, string>()
            //{
            //    { 5, "Tom"},
            //    { 3, "Sam"},
            //    { 11, "Bob"}
            //};
            //var dest = await this.GetDestinationAsync(this.CardID);
            //var documentID = Guid.Parse("b61e2edb-b689-4605-941e-e730aab56953");
            //var temp = this.CardID;

            var documentID = this.MainMesID;

            PartnerMEDO tempPartner = new PartnerMEDO()
            { 
                PartnerMedoID = newMessage.MedoPartnerID.ToString(),
                PartnerName = newMessage.MedoPartnerName
            };

            var senderMedoID = await this.DbScope.Db.SetCommand
                  ("SELECT \"MedoID\" " +
                      "FROM \"Partners\" " +
                      "WHERE \"ID\" = @partnerID",
                  this.DbScope.Db.Parameter("@partnerID", this.MedoPartnerID))
                      .LogCommand()
                      .ExecuteAsync<Guid>(cancellationToken);

            var comment = await this.DbScope.Db.SetCommand
                ("SELECT \"MedoComment\" FROM \"MedoCommonInfo\" WHERE \"RowID\" = @ID",
                                       this.DbScope.Db.Parameter("@ID", this.RowID))
                                   .LogCommand()
                                   .ExecuteAsync<string>(cancellationToken);

            Guid respID = Guid.NewGuid();

            XDocument document;

            XDeclaration xd = new XDeclaration("1.0", "utf-8", null);

            xd.Standalone = null;

            if (this.XsdVersion == XsdVersion.OldVersion)
            {
                document = new XDocument(xd,
                    new XElement(this.Xdms + MedoTag.TagCommunication,
                        new XAttribute(XNamespace.Xmlns + "xdms", this.Xdms),
                        new XAttribute(this.Xdms + MedoTag.TagVersion, this.XsdVersion.GetDescription()),
                        new XElement(this.Xdms + MedoTag.TagHeader,
                            new XAttribute(this.Xdms + MedoTag.TagMesType, MedoMessageType.Notification.GetDescription()),
                            new XAttribute(this.Xdms + MedoTag.TagMesUid, this.responseID.ToString().ToLowerInvariant()),
                            new XAttribute(this.Xdms + MedoTag.TagCreated, date),
                            new XElement(this.Xdms + MedoTag.TagSource,
                                new XElement(this.Xdms + MedoTag.TagComOrg, MedoConst.RBName),
                                new XAttribute(this.Xdms + MedoTag.TagOrgUid, MedoConst.RBMedoID))),
                        new XElement(this.Xdms + MedoTag.TagNotification,
                            new XAttribute(this.Xdms + MedoTag.TagNoticeType, this.Type.GetDescription()),
                            new XAttribute(this.Xdms + MedoTag.TagUid, documentID),
                            await this.EventNotificationAsync(cancellationToken),
                            new XElement(this.Xdms + MedoTag.TagComment, comment))));
            }
            else
            {
                document = new XDocument(xd,
                    new XElement(this.Xdms + MedoTag.TagCommunication,
                        new XAttribute(XNamespace.Xmlns + "xdms", this.Xdms),
                        new XAttribute(this.Xdms + MedoTag.TagVersion, this.XsdVersion.GetDescription()),
                        new XElement(this.Xdms + MedoTag.TagHeader,
                            new XAttribute(this.Xdms + MedoTag.TagMesType, MedoMessageType.Notification.GetDescription()),
                            new XAttribute(this.Xdms + MedoTag.TagMesUid, this.responseID.ToString().ToLowerInvariant()),
                            new XAttribute(this.Xdms + MedoTag.TagCreated, date),
                            new XElement(this.Xdms + MedoTag.TagSource,
                                new XElement(this.Xdms + MedoTag.TagComOrg, MedoConst.RBName),
                                new XAttribute(this.Xdms + MedoTag.TagOrgUid, MedoConst.RBMedoID))),
                        new XElement(this.Xdms + MedoTag.TagNotification,
                            new XAttribute(this.Xdms + MedoTag.TagMesType, this.Type.GetDescription()),
                            new XAttribute(this.Xdms + MedoTag.TagUid, documentID),
                            new XAttribute(this.Xdms + MedoTag.TagId, this.CardID),
                            await this.EventNotificationAsync(cancellationToken),
                            new XElement(this.Xdms + MedoTag.TagComment, comment)),
                        new XElement(this.Xdms + MedoTag.TagDeliveryIndex, 
                            new XElement(this.Xdms + MedoTag.TagDestination,
                                new XElement(this.Xdms + MedoTag.TagDestination, new XAttribute(this.Xdms + MedoTag.TagUid, senderMedoID),
                                    new XElement(this.Xdms + MedoTag.TagOrganization, tempPartner.PartnerName))))));
            }

            logger.Info("\nthis.ValidationResult.IsSuccessful()" + this.ValidationResult.IsSuccessful() +
                        "\ndocument.CheckXml: " + 
                        "\nthis.XsdVersion: " + this.XsdVersion +
                        "\nXmlType.Message: " + XmlType.Message +
                        "\nthis.ValidationResult: " + this.ValidationResult.ToString() +
                        "\nthis.CommunicationPath: " + this.CommunicationPath);

            if (this.ValidationResult.IsSuccessful()
                && document.CheckXml(this.XsdVersion, XmlType.Message, this.ValidationResult, this.CommunicationPath))
            {
                logger.Info("Pass if");
                Directory.CreateDirectory(this.GlobalPath);
                logger.Info("Try to save file:" + Path.Combine(this.GlobalPath, "notification.xml"));
                //document.Save(Path.Combine(this.GlobalPath, this.CardID.ToString().Replace("-", "") + ".xml"));

                using (var writer = new XmlTextWriter(Path.Combine(this.GlobalPath, "notification.xml"), new UTF8Encoding(false)))
                {
                    //writer.Settings.NewLineChars = "\r\n";
                    document.Save(writer);
                }
            }
        }

        public override async Task AuxiliaryActionAsync(CancellationToken cancellationToken = default)
        {
            await using var instance = this.DbScope.Create();
            var correct = this.ValidationResult.IsSuccessful();

            var db = this.DbScope.Db;
            var param = new[]
            {
                new DataParameter("@ID", this.CardID),
                new DataParameter("@mesID", correct ? this.MainMesID : Guid.Empty),
                new DataParameter("@date", DateTime.UtcNow),
                new DataParameter("@stID", correct ? 1 : 4),
                new DataParameter("@stName", correct ? MedoState.Send.GetDescription() : MedoState.FormationError.GetDescription()),
                new DataParameter("@typeID", (int)this.Type),
                new DataParameter("@error", this.ValidationResult.ToString()),
                new DataParameter("@rowID", this.RowID),
                new DataParameter("@respID", this.responseID)
            };

            await db.SetCommand
                ("UPDATE \"MedoCommonInfo\" " +
                        "SET \"DateMessage\" = @date, \"MedoStatusID\" = @stID, \"MedoStatusName\" = @stName, \"MedoError\" = @error, \"ResponseMesID\" = @respID " +
                        "WHERE \"ID\" = @ID AND \"MedoTypeID\" = @typeID AND \"RowID\" = @rowID", param)
                    .LogCommand()
                    .ExecuteNonQueryAsync(cancellationToken);
            
            await this.EnvelopeAsync(cancellationToken);

            //var copyPath = (await this.CardCache.Cards.GetAsync("MedoStampInfo", cancellationToken).ConfigureAwait(false)).GetValue()
            //    ?.Sections["MedoRB"].Fields.TryGet<string>("CopyFolder");

            //var copyPath = "   ";

            //if (!string.IsNullOrWhiteSpace(copyPath))
            //{
            //    //Directory.CreateDirectory(copyPath);
            //    var sourceDir = new DirectoryInfo(this.GlobalPath);
            //    var outgoingCopyPath = Path.Combine(copyPath, "Notifications_test", sourceDir.Name);
            //    Directory.CreateDirectory(outgoingCopyPath);
            //    sourceDir.GetFiles().ForEach(f => f.CopyTo(Path.Combine(outgoingCopyPath, f.Name)));
            //}

            logger.Error(this.ValidationResult.Build());
        }

        protected override async Task EnvelopeAsync(CancellationToken cancellationToken = default)
        {
            var card = await this.GetCardAsync(
                this.Container.Resolve<ICardServerPermissionsProvider>(),
                this.Container.Resolve<ICardRepository>(),
                cancellationToken);
            var senderMedoAddress = "Тестовый адрес";
            //await this.DbScope.GetFieldAsync<string>(
            //card.Sections[SchemeInfo.DocumentRosteh_CommonInfo].Fields[SchemeInfo.DocumentRosteh_CommonInfo.SenderID],
            //  SchemeInfo.RostehPartners,
            // SchemeInfo.RostehPartners.MedoAddress,
            //  cancellationToken: cancellationToken);

            await using var instance = this.DbScope.Create();

            senderMedoAddress = await this.DbScope.Db.SetCommand
                  ("SELECT \"MedoAddress\" " +
                      "FROM \"Partners\" " +
                      "WHERE \"ID\" = @partnerID",
                  this.DbScope.Db.Parameter("@partnerID", this.MedoPartnerID))
                      .LogCommand()
                      .ExecuteAsync<string>(cancellationToken);

            var sb = StringBuilderHelper
                     .Acquire()
                     .AppendLine("[ПИСЬМО КП ПС СЗИ]")
                     .AppendLine("АВТООТПРАВКА=1")
                     .AppendLine("ШИФРОВАНИЕ=0")
                     .AppendLine("ЭЦП=1")
                     .AppendLine("ДОСТАВЛЕНО=0")
                     .AppendLine("ПРОЧТЕНО=1")
                     .AppendLine($"ДАТА={DateTime.Now:dd.MM.yyyy HH:mm:ss}")
                     .AppendLine()
                     .AppendLine("[АДРЕСАТЫ]")
                     .AppendLine($"0={senderMedoAddress}")
                     .AppendLine()
                     .AppendLine("[ФАЙЛЫ]")
                     .AppendLine($"0=notification.xml");
            
            await File.WriteAllTextAsync(Path.Combine(this.GlobalPath, "envelope.ini"), sb.ToStringAndRelease(), /*Encoding.UTF8*/ Encoding.GetEncoding(1251), cancellationToken);
        }

        #endregion

        #region Private

        /// <summary>
        ///     элемент уведомления
        /// </summary>
        /// <returns></returns>
        private async Task<XElement> EventNotificationAsync(CancellationToken cancellationToken = default)
        {
            await using var scope = this.DbScope.Create();
            //var comment = await this.DbScope.Db.SetCommand
            //    ("SELECT \"MedoComment\" FROM \"MedoCommonInfo\" WHERE \"ID\" = @ID AND \"MedoStatusID\" = 0 AND \"MedoTypeID\" = 1",
            //                           this.DbScope.Db.Parameter("@ID", this.CardID))
            //                       .LogCommand()
            //                       .ExecuteAsync<string>(cancellationToken);

            string reason = await this.DbScope.Db.SetCommand
                ("SELECT \"RefusalReasonName\" FROM \"DocumentCommonInfo\" WHERE \"ID\" = @ID",
                                       this.DbScope.Db.Parameter("@ID", this.CardID))
                                   .LogCommand()
                                   .ExecuteAsync<string>(cancellationToken);

            var element = new XElement(this.Xdms + MedoHelper.NoticeTypeDict[this.Type],
                new XElement(this.Xdms + MedoTag.TagTime, DateTime.UtcNow));
            var (number, regDate, secondaryNumber, creationDate) = await this.GetRegistrationValuesAsync(cancellationToken).ConfigureAwait(false);

            switch (this.Type)
            {
                case NoticeType.Registered:
                    element.Add(await this.NoticeRegAsync(secondaryNumber, creationDate, cancellationToken));
                    if (this.XsdVersion == XsdVersion.NewVersion)
                    {
                        element.Add(new XElement(this.Xdms + MedoTag.TagNum,
                            new XElement(this.Xdms + MedoTag.TagNumber, number),
                            new XElement(this.Xdms + MedoTag.TagDate, regDate.HasValue ? $"{regDate:yyyy-MM-dd}" : $"{DateTime.UtcNow:yyyy-MM-dd}")));
                    }

                    break;

                case NoticeType.RegDenied:
                    element.Add(await this.NoticeRegDeniedAsync(reason, secondaryNumber, creationDate, cancellationToken));
                    element.Add(new XElement(this.Xdms + MedoTag.TagReason, reason));
                    if (this.XsdVersion == XsdVersion.NewVersion)
                    {
                        //element.Add(new XElement(this.Xdms + MedoTag.TagReason, reason));
                    }

                    break;

                default:
                    this.ValidationResult.AddError(
                        $"Для уведомления типа {this.Type.GetDescription()} действий не предусмотренно");
                    return null;
            }

            return element;
        }

        private async Task<(string, DateTime?, string, DateTime?)> GetRegistrationValuesAsync(CancellationToken cancellationToken = default)
        {
            this.DbScope.Db.SetCommand
                ("SELECT \"dci\".\"FullNumber\", \"dci\".\"DocDate\", \"dci\".\"OutgoingNumber\", \"dci\".\"OutgoingDate\" " +
                 "FROM \"DocumentCommonInfo\" \"dci\" WHERE \"dci\".\"ID\" = @ID",
                this.DbScope.Db.Parameter("@ID", this.CardID));

            await using var reader = await this.DbScope.Db.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return (reader.GetNullableString(0), reader.GetNullableDateTime(1), reader.GetNullableString(2), reader.GetNullableDateTime(3));
            }

            return (null, null, null, null);
        }

        /// <summary>
        ///     Элемент уведомления - для зарегистрировано
        /// </summary>
        /// <returns></returns>
        private async Task<XElement> NoticeRegAsync(string secondaryNumber, DateTime? creationDate, CancellationToken cancellationToken = default)
        {
            if (secondaryNumber.IsNullOrEmpty())
            {
                this.ValidationResult.AddError($"Card {this.CardID} doesn't have registration information");
                return null;
            }

            if (this.XsdVersion == XsdVersion.OldVersion)
            {
                return new XElement(this.Xdms + MedoTag.TagNum,
                    new XElement(this.Xdms + MedoTag.TagNumber, secondaryNumber),
                    new XElement(this.Xdms + MedoTag.TagDate, creationDate.HasValue ? $"{creationDate:yyyy-MM-dd}" : $"{DateTime.UtcNow:yyyy-MM-dd}"));
            }

            var person = "Иванов Иван Иванович";//await this.GetPersonFullNameAsync(cancellationToken);

            return new XElement(this.Xdms + MedoTag.TagFoundation,
                //new XElement(this.Xdms + MedoTag.TagRegion, "Республика Бурятия"), //new XAttribute(this.Xdms + "id", "DP00000003"),
                new XElement(this.Xdms + MedoTag.TagOrganization, this.MedoPartnerName),// MedoConst.RBName),
                //new XElement(this.Xdms + MedoTag.TagPerson, person),
                new XElement(this.Xdms + MedoTag.TagNum,
                    new XElement(this.Xdms + MedoTag.TagNumber, secondaryNumber),
                    new XElement(this.Xdms + MedoTag.TagDate, creationDate.HasValue ? $"{creationDate:yyyy-MM-dd}" : $"{DateTime.UtcNow:yyyy-MM-dd}")));
        }

        private async Task<XElement> NoticeRegDeniedAsync(string reason, string secondaryNumber, DateTime? creationDate, CancellationToken cancellationToken = default)
        {
            if (reason.IsNullOrEmpty())
            {
                this.ValidationResult.AddError($"There is no reason for refusal in the card {this.CardID}");
                return null;
            }

            //if (this.XsdVersion == XsdVersion.OldVersion)
            //{
            //    return new XElement(this.Xdms + MedoTag.TagReason, reason);
            //}

            var person = "Иванов Иван Иванович";//await this.GetPersonFullNameAsync(cancellationToken);
            var tempXelement = new XElement(this.Xdms + MedoTag.TagFoundation,
                //new XElement(this.Xdms + MedoTag.TagRegion, "Республика Бурятия"), //new XAttribute(this.Xdms + "id", "DP00000003"),
                new XElement(this.Xdms + MedoTag.TagOrganization, this.MedoPartnerName),// MedoConst.RBName),
                //new XElement(this.Xdms + MedoTag.TagPerson, person),
                new XElement(this.Xdms + MedoTag.TagNum,
                    new XElement(this.Xdms + MedoTag.TagNumber, secondaryNumber ?? "Номер не был выделен"),
                    new XElement(this.Xdms + MedoTag.TagDate, creationDate.HasValue ? $"{creationDate:yyyy-MM-dd}" : $"{DateTime.UtcNow:yyyy-MM-dd}")));




            return tempXelement;
        }

        //private async Task<Dictionary<string, object>> GetDestinationAsync(CancellationToken cancellationToken = default) =>
        //    await this.DbScope.GetFieldsAsync(this.CardID,
        //        SchemeInfo.DocumentRosteh_CommonInfo,
        //        cancellationToken,
        //        SchemeInfo.DocumentRosteh_CommonInfo.SenderMedoID,
        //        SchemeInfo.DocumentRosteh_CommonInfo.SenderName);

        private async Task<List<PartnerMEDO>> GetDestinationAsync(Guid cardID)
        {
            await using (this.DbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = this.DbScope.Db;

                var builderFactory = this.DbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().C("DocumentCommonInfo", "PartnerMedoID", "PartnerName")
                            .From("DocumentCommonInfo").NoLock()
                            .Where().C("ID").Equals().P("cardID")
                            //.And().C("StampTypeID").Equals().P("stampTypeId")
                            .Build(),
                        db.Parameter("cardID", cardID))
                    //    db.Parameter("stampTypeId", StampType.Sign))
                    .LogCommand()
                    .ExecuteListAsync<PartnerMEDO>();

                if (result == null || result.Count == 0)
                {
                    logger.Info($"GetDestinationAsync no elements cardID: {CardID}");
                }

                foreach (var item in result)
                {
                    logger.Info($"PartnerMedoID: {item.PartnerMedoID}\nPartnerName: {item.PartnerName}");
                }

                return result;
            }
        }
            

        /*
        private async Task<string> GetPersonFullNameAsync(CancellationToken cancellationToken = default) =>
            await this.DbScope.GetFieldAsync<string>(this.RowID,
                SchemeInfo.MedoCommonInfo,
                SchemeInfo.MedoCommonInfo.PersonFullName,
                SchemeInfo.MedoCommonInfo.RowID,
                cancellationToken);

        private async Task<Guid> GetMedoDocumentIDAsync(CancellationToken cancellationToken = default) =>
            await this.DbScope.GetFieldAsync<Guid>(this.RowID,
                SchemeInfo.MedoCommonInfo,
                SchemeInfo.MedoCommonInfo.MedoDocID,
                SchemeInfo.MedoCommonInfo.RowID,
                cancellationToken);*/

        #endregion
    }

    public class PartnerMEDO
    {
        public string PartnerMedoID { get; set; }
        public string PartnerName { get; set; }
    }
}