using System;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Shared.Notices
{
    public interface INotificationResolver
    {
        ValueTask<INotificationContext> CreateContextAsync(
            Guid cardID,
            Guid cardTypeID,
            string cardDigest,
            IValidationResultBuilder validationResult,
            bool withoutTransaction,
            CancellationToken cancellationToken = default);

        INotificationSender TryResolve(string notificationTypeKey);
    }
}
