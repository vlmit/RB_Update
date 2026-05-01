using System.Collections.Generic;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Shared.Notices
{
    /// <summary>
    /// Уведомление, не привязанное к заданию.
    /// </summary>
    [Notification(Key)]
    public sealed class CustomNotification :
        INotification
    {
        #region Constructors

        public CustomNotification(string email, string subject, string body)
        {
            this.Email = email;
            this.Subject = subject;
            this.Body = body;
        }

        public CustomNotification(IDictionary<string, object> storage)
        {
            this.Email = storage.Get<string>(EmailKey);
            this.Subject = storage.Get<string>(SubjectKey);
            this.Body = storage.Get<string>(BodyKey);
        }

        #endregion

        #region Private Constants

        private const string EmailKey = "Email";

        private const string SubjectKey = "Subject";

        private const string BodyKey = "Body";

        #endregion

        #region Public Constants

        /// <summary>
        /// Ключ, по которому должен быть зарегистрирован обработчик <see cref="INotificationSender"/>.
        /// Ключ также используется для указания секции при сериализации уведомлений.
        /// Ключ должен быть уникальным для каждого типа уведомлений.
        /// </summary>
        public const string Key = "custom";

        #endregion

        #region Properties

        public string Email { get; }

        public string Subject { get; }

        #endregion

        #region INotification Members

        public string Body { get; }

        
        public void SerializeTo(IDictionary<string, object> storage)
        {
            storage[EmailKey] = this.Email;
            storage[SubjectKey] = this.Subject;
            storage[BodyKey] = this.Body;
        }

        #endregion
    }
}
