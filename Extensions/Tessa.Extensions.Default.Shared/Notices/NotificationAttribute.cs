using System;

namespace Tessa.Extensions.Default.Shared.Notices
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class NotificationAttribute : Attribute
    {
        #region Constructors

        /// <summary>
        /// Создаёт атрибут с указанием его свойств.
        /// </summary>
        /// <param name="key">
        /// Ключ, по которому регистрируются обработчики <see cref="INotificationSender"/>
        /// и который определяет имя вложенной секции для сериализации сообщений.
        /// Ключ должен быть уникален для каждого типа уведомлений.
        /// </param>
        public NotificationAttribute(string key)
        {
            this.key = key;
        }

        #endregion

        #region Properties

        private readonly string key;

        /// <summary>
        /// Ключ, по которому регистрируются обработчики <see cref="INotificationSender"/>
        /// и который определяет имя вложенной секции для сериализации сообщений.
        /// Ключ должен быть уникален для каждого типа уведомлений.
        /// </summary>
        public string Key
        {
            get { return this.key; }
        }

        #endregion
    }
}
