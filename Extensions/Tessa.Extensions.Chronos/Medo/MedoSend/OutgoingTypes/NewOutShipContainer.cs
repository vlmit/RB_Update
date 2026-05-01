using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using LinqToDB.Common;
using LinqToDB.Data;
using Microsoft.Identity.Client;
using NLog;
using Tessa.Extensions.Chronos.Medo.MedoSend.Helpers;
using Tessa.Extensions.Shared.Helpers.Medo;
using Tessa.Extensions.Shared.Info;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using Unity;

namespace Tessa.Extensions.Chronos.Medo.MedoSend.OutgoingTypes
{
    public sealed class NewOutShipContainer : OutShipContainer
    {
        #region Constructor

        public NewOutShipContainer(IUnityContainer container,
            IDbScope dbScope,
            string globalPath,
            NewMessageInfo newMessageInfo)
        : base(container,
            dbScope,
            globalPath,
            newMessageInfo)
        {
        }

        
        #endregion

        #region Override

        public override async Task<XElement> AddresseesElementAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("New outshipcontainer AddresseesElementAsync");

            var addresse = this.Card?.Sections["Correspondents"].TryGetRows();
             //   .Where(x => x.Fields.Get<int>(SchemeInfo.RecieversPartners.DeliveryTypeID) == SchemeInfo.DeliveryTypes.MEDO.ID)
             //   .Concat(this.Card.Sections[SchemeInfo.CopyRecieversPartners].Rows
             //       .Where(r => r.Get<int>(SchemeInfo.CopyRecieversPartners.DeliveryTypeID) == SchemeInfo.DeliveryTypes.MEDO.ID)
             //       )
            
            if (addresse?.Count == 0)
            {
                this.ValidationResult.AddError(this, "Card hasnt addressees");

                logger.Info("New outshipcontainer AddresseesElementAsync Card hasnt addressees");

                return null;
            }

            var result = new XElement(this.Ns + MedoTag.TagAddressees);

            foreach (var address in addresse)
            {
                var orgID = address.Fields.Get<Guid>("PartnersID");
                string person = "";
                
                if (orgID != this.MedoPartnerID)
                {
                    continue;
                }

                //var person = address.Fields.Get<string>("PartnerHead");

                var org = await this.OrganizationElementAsync(orgID, MedoConst.GetOrg, cancellationToken);

                result.Add(
                    new XElement(this.Ns + MedoTag.TagAddressee,
                        org,
                        person.IsNullOrEmpty()
                            ? null
                            : new XElement(this.Ns + MedoTag.TagPerson,
                                new XElement(this.Ns + MedoTag.TagName, person))));
            }

            if (result.Value.IsNullOrEmpty())
            {
                logger.Info("New outshipcontainer AddresseesElementAsync Card hasnt addressees");

                this.ValidationResult.AddError(this, "Card hasnt addressees");
                return null;
            }

            return result;
        }

        public override async Task<int> InsertMedoJournalEntryAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("New outshipcontainer InsertMedoJournalEntryAsync");

            await using (this.DbScope.Create())
            {
                var db = this.DbScope.Db;
                var success = this.ValidationResult.IsSuccessful();

                const string command = "INSERT INTO \"MEDOJournalReports\" " +
                "(\"ID\", \"CardID\", \"Status\", \"PartnersID\", \"PartnersName\", \"MesID\", \"DateOfReceiving\") " +
                "VALUES(@ID, @cardID, @MedoStatusName, @MedoPartnerID, @MedoPartnerName, @MessageID, @date)";

                var param = new[]
                {
                    new DataParameter("@ID", Guid.NewGuid()),
                    new DataParameter("@cardID", this.CardID),
                    new DataParameter("@MedoStatusName", "Отправлено"),
                    new DataParameter("@MedoPartnerID", this.MedoPartnerID),
                    new DataParameter("@MedoPartnerName", newMessageInfo.MedoPartnerName),
                    new DataParameter("@MessageID", this.MainMesID),
                    new DataParameter("@date", DateTime.UtcNow)
                };

                foreach(var item in param )
                {
                    logger.Info($"{item.Name} {item.Value}");
                }

                return await db.SetCommand(command, param)
                        .LogCommand()
                        .ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public override async Task ChangeMedoCommonInfoAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("New outshipcontainer ChangeMedoCommonInfoAsync ");

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


        #endregion
    }
}