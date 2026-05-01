using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tessa.Extensions.Default.Shared.Notices
{
    /// <summary>
    /// Объект, выполняющий отправку уведомлений.
    /// </summary>
    public interface INotificationSender
    {
        INotification DeserializeFrom(IDictionary<string, object> storage);

        Task SendMessageAsync(
            INotificationContext context,
            IList<INotification> notifications,
            CancellationToken cancellationToken = default);
    }
}
