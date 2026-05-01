using System;
using Tessa.Notices;
using Tessa.Platform.Data;
using Tessa.Platform.Licensing;
using Tessa.Platform.Runtime;
using Tessa.Platform.Validation;
using Unity;

namespace Tessa.Extensions.Default.Shared.Notices
{
    public sealed class NotificationContext :
        INotificationContext
    {
        #region Constructors

        public NotificationContext(
            Guid cardID,
            Guid cardTypeID,
            string cardDigest,
            string webAddress,
            IValidationResultBuilder validationResult,
            IMailService mailService,
            IDbScope dbScope,
            ISession session,
            IUnityContainer unityContainer,
            ILicenseManager licenseManager)
        {
            this.CardID = cardID;
            this.CardTypeID = cardTypeID;
            this.CardDigest = cardDigest;
            this.WebAddress = webAddress;
            this.ValidationResult = validationResult;
            this.MailService = mailService;
            this.DbScope = dbScope;
            this.Session = session;
            this.LicenseManager = licenseManager;
            this.UnityContainer = unityContainer;
        }

        #endregion

        #region INotificationContext Members

        public Guid CardID { get; }

        public Guid CardTypeID { get; }

        public string CardDigest { get; }

        public string WebAddress { get; }

        public IValidationResultBuilder ValidationResult { get; }

        public IMailService MailService { get; }

        public IDbScope DbScope { get; }

        public ISession Session { get; }

        public IUnityContainer UnityContainer { get; }

        public ILicenseManager LicenseManager { get; }

        #endregion
    }
}