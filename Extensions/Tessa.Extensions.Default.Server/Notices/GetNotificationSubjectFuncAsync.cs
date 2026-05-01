using System.Threading;
using System.Threading.Tasks;
using Tessa.Extensions.Default.Shared.Notices;

namespace Tessa.Extensions.Default.Server.Notices
{
    public delegate ValueTask<string> GetNotificationSubjectFuncAsync<in TNotification, in TNotificationData>(
        INotificationContext context,
        TNotification notification,
        TNotificationData data,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
        where TNotificationData : INotificationData<TNotification>;
}
