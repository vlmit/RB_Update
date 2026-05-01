using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Extensions.Default.Shared.Notices;

namespace Tessa.Extensions.Default.Server.Notices
{
    public sealed class CustomNotificationSender :
        INotificationSender
    {
        #region INotificationSender Members

        public INotification DeserializeFrom(IDictionary<string, object> storage)
        {
            return new CustomNotification(storage);
        }

        public async Task SendMessageAsync(
            INotificationContext context,
            IList<INotification> notifications,
            CancellationToken cancellationToken = default)
        {
            foreach (CustomNotification notification in notifications.Cast<CustomNotification>())
            {
                await context.MailService.PostMessageAsync(
                    notification.Email,
                    notification.Subject,
                    notification.Body,
                    context.ValidationResult,
                    cancellationToken: cancellationToken);
            }
        }

        #endregion
    }
}
