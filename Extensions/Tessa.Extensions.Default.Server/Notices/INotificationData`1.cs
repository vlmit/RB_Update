using Tessa.Extensions.Default.Shared.Notices;
using Tessa.Notices;

namespace Tessa.Extensions.Default.Server.Notices
{
    public interface INotificationData<in TNotification>
        where TNotification : INotification
    {
        string Email { get; }

        string LanguageCode { get; }

        string CardNumber { get; }

        string CardSubject { get; }

        string CardDigest { get; }

        void Initialize(TNotification notification);

        bool IsApplicable(TNotification notification);

        MailInfo Info { get; }
    }
}
