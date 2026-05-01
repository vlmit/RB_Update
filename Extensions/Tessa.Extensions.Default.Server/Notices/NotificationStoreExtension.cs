using System.Collections.Generic;
using System.Threading.Tasks;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared.Notices;
using Tessa.Cards;

namespace Tessa.Extensions.Default.Server.Notices
{
    public sealed class NotificationStoreExtension : CardStoreExtension
    {
        #region Constructor

        public NotificationStoreExtension(INotificationResolver notificationResolver)
        {
            this.notificationResolver = notificationResolver;
        }

        #endregion

        #region Fields

        private readonly INotificationResolver notificationResolver;

        #endregion

        #region CardStoreExtension Methods

        public override Task BeforeRequest(ICardStoreExtensionContext context)
        {
            if (NotificationHelper.HasNotifications(context.Request.TryGetInfo()))
            {
                context.Request.ForceTransaction = true;
            }

            return Task.CompletedTask;
        }

        public override Task BeforeCommitTransaction(ICardStoreExtensionContext context)
        {
            // уведомления могли появиться внутри транзакции, поэтому надо заново проверить их наличие в Info
            Dictionary<string, object> info;
            Card card;
            if (!NotificationHelper.HasNotifications(info = context.Request.TryGetInfo())
                || !context.ValidationResult.IsSuccessful()
                || (card = context.Request.TryGetCard()) == null)
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
                withoutTransaction: true,
                cancellationToken: context.CancellationToken);
        }

        #endregion
    }
}
