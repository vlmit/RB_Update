using System.Collections.Generic;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared.Notices;

namespace Tessa.Extensions.Default.Server.Notices
{
    public sealed class NotificationGetExtension : CardGetExtension
    {
        #region Constructor

        public NotificationGetExtension(INotificationResolver notificationResolver)
        {
            this.notificationResolver = notificationResolver;
        }

        #endregion

        #region Fields

        private readonly INotificationResolver notificationResolver;

        #endregion

        #region CardStoreExtension Methods

        public override Task AfterRequest(ICardGetExtensionContext context)
        {
            Dictionary<string, object> info;
            Card card;
            if (!context.RequestIsSuccessful
                || !NotificationHelper.HasNotifications(info = context.Request.TryGetInfo())
                || (card = context.Response.TryGetCard()) == null)
            {
                return Task.CompletedTask;
            }

            return NotificationHelper.SendNotificationsAsync(
                info,
                card.ID,
                card.TypeID,
                context.Request.TryGetDigest(),
                context.ValidationResult,
                this.notificationResolver,
                cancellationToken: context.CancellationToken);
        }

        #endregion
    }
}
