using System;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Caching;
using Tessa.Notices;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Licensing;
using Tessa.Platform.Runtime;
using Tessa.Platform.Validation;
using Unity;

namespace Tessa.Extensions.Default.Shared.Notices
{
    public sealed class NotificationResolver :
        INotificationResolver
    {
        #region Constructors

        public NotificationResolver(
            IMailService mailService,
            IMailService mailServiceWithoutTransaction,
            IDbScope dbScope,
            ISession session,
            ICardCache cardCache,
            IUnityContainer unityContainer,
            ILicenseManager licenseManager)
        {
            this.mailService = mailService;
            this.mailServiceWithoutTransaction = mailServiceWithoutTransaction;
            this.dbScope = dbScope;
            this.session = session;
            this.cardCache = cardCache;
            this.unityContainer = unityContainer;
            this.licenseManager = licenseManager;
        }

        #endregion

        #region Fields

        private readonly IMailService mailService;

        private readonly IMailService mailServiceWithoutTransaction;

        private readonly IDbScope dbScope;

        private readonly ISession session;

        private readonly ICardCache cardCache;

        private readonly IUnityContainer unityContainer;

        private readonly ILicenseManager licenseManager;

        #endregion

        #region INotificationResolver Members

        public async ValueTask<INotificationContext> CreateContextAsync(
            Guid cardID,
            Guid cardTypeID,
            string cardDigest,
            IValidationResultBuilder validationResult,
            bool withoutTransaction,
            CancellationToken cancellationToken = default)
        {
            string webAddress = await CardHelper.TryGetWebAddressAsync(this.cardCache, cancellationToken: cancellationToken).ConfigureAwait(false);

            return new NotificationContext(
                cardID,
                cardTypeID,
                cardDigest,
                webAddress,
                validationResult,
                withoutTransaction ? this.mailServiceWithoutTransaction : this.mailService,
                this.dbScope,
                this.session,
                this.unityContainer,
                this.licenseManager);
        }


        public INotificationSender TryResolve(string notificationTypeKey)
        {
            return this.unityContainer.TryResolve<INotificationSender>(notificationTypeKey);
        }

        #endregion
    }
}
