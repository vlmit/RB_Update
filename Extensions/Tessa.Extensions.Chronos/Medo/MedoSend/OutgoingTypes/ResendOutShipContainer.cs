using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using LinqToDB.Common;
using LinqToDB.Data;
using Tessa.Extensions.Chronos.Medo.MedoSend.Helpers;
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
    public sealed class ResendOutShipContainer : OutShipContainer
    {
        #region Constructor

        public ResendOutShipContainer(
            IUnityContainer container,
            IDbScope dbScope,
            string globalPath,
            NewMessageInfo newMessageInfo)
            : base(container,
                dbScope,
                globalPath,
                newMessageInfo)
        {
            if (Directory.Exists(this.GlobalPath))
            {
                this.GlobalPath += " " + DateTime.Now.ToString(" yyyy_MM_dd HH_mm_ss_fffffff");
            }
        }

        #endregion

        #region Override

        public override async Task<XElement> AddresseesElementAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("ResendOutShipContainer AddresseesElementAsync");
            var addresse = this.Card?.Sections["Correspondents"].TryGetRows();

            foreach(var row in addresse)
            {
                
            }
              // .Where(x => x.Fields.Get<int>("DeliveryTypeID") == SchemeInfo.DeliveryTypes_MEDO_ID
              //      && x.Fields.Get<Guid>(SchemeInfo.RecieversPartners.PartnerID) == this.MedoPartnerID)
              //  .Concat(this.Card?.Sections[SchemeInfo.CopyRecieversPartners].Rows
              //  .Where(r => r.Get<int>(SchemeInfo.CopyRecieversPartners.DeliveryTypeID) == SchemeInfo.DeliveryTypes.MEDO.ID
              //      && r.Fields.Get<Guid>(SchemeInfo.CopyRecieversPartners.PartnerID) == this.MedoPartnerID))
              //  
              ;

            //if (addresse == null)
            //{
            //    logger.Info("ResendOutShipContainer AddresseesElementAsync Card hasnt necessary addressees");
            //    this.ValidationResult.AddError(this, "Card hasnt necessary addressees");
            //    return null;
            //}

            var org = await this.OrganizationElementAsync(this.MedoPartnerID, MedoConst.GetOrg, cancellationToken);
            //var person = addresse.Fields.Get<string>("PartnerHead");

            string person = null;

            return new XElement(this.Ns + MedoTag.TagAddressees,
                new XElement(this.Ns + MedoTag.TagAddressee,
                    org,
                    person.IsNullOrEmpty()
                        ? null
                        : new XElement(this.Ns + MedoTag.TagPerson,
                            new XElement(this.Ns + MedoTag.TagName, person))));
        }

        public override async Task ChangeMedoCommonInfoAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("ResendOutShipContainer ChangeMedoCommonInfoAsync");

            await using (this.DbScope.Create())
            {
                var db = this.DbScope.Db;
                var success = this.ValidationResult.IsSuccessful();

                const string command = "UPDATE \"MedoCommonInfo\" " +
                        "SET \"MessageID\" = @MessageID, \"DateMessage\" = @DateMessage, \"MedoStatusID\" = @MedoStatusID, " +
                        "\"MedoStatusName\" = @MedoStatusName, \"MedoError\" = @MedoError " +
                        "WHERE \"ID\" = @ID AND \"MedoPartnerID\" = @MedoPartnerID AND \"MedoTypeID\" = 8 AND \"MedoStatusID\" = 0";

                var param = new[]
                {
                    new DataParameter("@ID", this.CardID),
                    new DataParameter("@MessageID", this.MainMesID),
                    new DataParameter("@DateMessage", DateTime.UtcNow),
                    new DataParameter("@MedoStatusID", success ? 1 : 4),
                    new DataParameter("@MedoStatusName", success ? MedoState.Send.GetDescription() : MedoState.FormationError.GetDescription()),
                    new DataParameter("@MedoError", this.ValidationResult.ToString()),
                    new DataParameter("@MedoPartnerID", this.MedoPartnerID)
                };

                await db.SetCommand(command, param)
                        .LogCommand()
                        .ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public override Task<int> InsertMedoJournalEntryAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override async Task AuxiliaryActionAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("ResendOutShipContainer AuxiliaryActionAsync");

            if (this.ValidationResult.IsSuccessful())
            {
                this.AddFileAtZip();
                this.Communication();
                await this.EnvelopeAsync(cancellationToken);

                //var copyPath = (await this.CardCache.Cards.GetAsync("MedoStampInfo", cancellationToken).ConfigureAwait(false)).GetValue()
                //    ?.Sections["MedoRB"].Fields.TryGet<string>("CopyFolder");

                string copyPath = null;

                if (!string.IsNullOrWhiteSpace(copyPath))
                {
                    Directory.CreateDirectory(copyPath);
                    var sourceDir = new DirectoryInfo(this.GlobalPath);
                    var outgoingCopyPath = Path.Combine(copyPath, "Исходящие", sourceDir.Name);
                    if (Directory.Exists(outgoingCopyPath))
                    {
                        outgoingCopyPath += this.MainMesID + " " + DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss");
                    }
                    Directory.CreateDirectory(outgoingCopyPath);
                    sourceDir.GetFiles().ForEach(f => f.CopyTo(Path.Combine(outgoingCopyPath, f.Name)));
                }
            }

            await this.ChangeMedoCommonInfoAsync(cancellationToken);
            logger.MedoLoggerResult(this.ValidationResult, this.MainMesID);

            if (Directory.Exists(this.tempPath))
            {
                Directory.Delete(this.tempPath, true);
            }
        }

        #endregion

        /// <summary>
        ///     xml файл с описанием контейнера
        /// </summary>
        /// <returns>возвращает id сообщения МЭДО</returns>
        private new void Communication()
        {
            logger.Info("ResendOutShipContainer Communication");

            var deliveryAdresses = this.Card.Sections["RecieversPartners"].Rows
                                     //  .Where(r => r.Get<int>("DeliveryTypeID") == SchemeInfo.DeliveryTypes.MEDO.ID
                                     //   && r.Fields.Get<Guid>("PartnerID") == this.MedoPartnerID)
                                     //  .Concat(this.Card.Sections[SchemeInfo.CopyRecieversPartners].Rows
                                     //  .Where(r => r.Get<int>(SchemeInfo.CopyRecieversPartners.DeliveryTypeID) == SchemeInfo.DeliveryTypes.MEDO.ID
                                     //   && r.Fields.Get<Guid>(SchemeInfo.CopyRecieversPartners.PartnerID) == this.MedoPartnerID))
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

            var partnersRows = this.Card.Sections["Correspondents"].Rows;

            string PartnerMedoId = string.Empty;
            string PartnerName = string.Empty;

            foreach (var row in partnersRows)
            {
                if (row.Fields.Get<Guid>("PartnersID") == newMessageInfo.MedoPartnerID)
                {
                    PartnerMedoId = row.Fields["PartnersMedoID"].ToString();
                    PartnerName = row.Fields["PartnersName"].ToString();
                }
            }

            logger.Info($"PartnerMedoId: {PartnerMedoId} PartnerName: {PartnerName}");

            deliveryTags.Add(
                    new XElement(this.Xdms + MedoTag.TagDestination,
                    new XElement(this.Xdms + MedoTag.TagDestination, new XAttribute(this.Xdms + MedoTag.TagUid, PartnerMedoId),
                    new XElement(this.Xdms + MedoTag.TagOrganization, PartnerName))));

            //foreach (var row in deliveryAdresses)
            //{
            //    deliveryTags.Add(
            //        new XElement(this.Xdms + MedoTag.TagDestination,
            //        new XElement(this.Xdms + MedoTag.TagDestination, new XAttribute(this.Xdms + MedoTag.TagUid, row["PartnerID"]),
            //        new XElement(this.Xdms + MedoTag.TagOrganization, row["PartnerName"]))));
            //}

            communicationEl.Add(deliveryTags);

            var docXml = new XDocument(MedoConst.Declare, communicationEl);

            if (docXml.CheckXml(this.XsdVersion, XmlType.Message, this.ValidationResult, this.CommunicationPath))
            {
                docXml.Save(Path.Combine(this.GlobalPath, "communication.xml"));
            }
        }
    }
}