using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Platform;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Shared.Notices
{
    /// <summary>
    /// Методы расширения.
    /// </summary>
    public static class NoticesExtensions
    {
        #region INotificationResolver Extensions

        /// <summary>
        /// Выполняет отправку почтовых уведомлений <see cref="INotification"/>
        /// без привязки к процессу создания или сохранения карточки.
        ///
        /// Возвращает результат валидации по отправке уведомлений, который не равен <c>null</c>.
        /// </summary>
        /// <typeparam name="T">Тип уведомления, реализующий интерфейс <see cref="INotification"/>.</typeparam>
        /// <param name="notificationResolver">Объект, выполняющий получение обработчиков сообщений с последующей отправкой.</param>
        /// <param name="cardID">Идентификатор карточки.</param>
        /// <param name="cardTypeID">Идентификатор типа карточки.</param>
        /// <param name="cardDigest">
        /// Digest карточки или <c>null</c>, если digest будет неизвестен в уведомлении.
        /// Необходимость наличия Digest определяется кодом <see cref="INotificationSender"/>.
        /// </param>
        /// <param name="info">
        /// Дополнительная информация, используемая объектами <see cref="INotificationSender"/> при отправке писем,
        /// или <c>null</c>, если дополнительная информация не передаётся.
        /// </param>
        /// <param name="withoutTransaction">
        /// Признак того, что отправка письма выполняется без транзакции. Актуально при использовании на сервере.
        /// Укажите <c>true</c>, если известно, что метод выполняется для уже открытой транзакции SQL.
        /// </param>
        /// <param name="notifications">Отправляемые уведомления.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>
        /// Результат валидации по отправке уведомлений. Не равен <c>null</c> и может содержать ошибки.
        /// </returns>
        public static async Task<ValidationResult> SendNotificationsAsync<T>(
            this INotificationResolver notificationResolver,
            Guid cardID,
            Guid cardTypeID,
            string cardDigest = null,
            IDictionary<string, object> info = null,
            bool withoutTransaction = false,
            CancellationToken cancellationToken = default,
            params T[] notifications)
            where T : INotification
        {
            Check.ArgumentNotNull(notificationResolver, "notificationResolver");
            Check.ArgumentNotNull(notifications, "notifications");

            if (notifications.Length == 0)
            {
                return ValidationResult.Empty;
            }

            IDictionary<string, object> actualInfo = info ?? new Dictionary<string, object>(StringComparer.Ordinal);
            NotificationHelper.AddNotification(actualInfo, notifications);

            var validationResult = new ValidationResultBuilder();

            await NotificationHelper.SendNotificationsAsync(
                actualInfo,
                cardID,
                cardTypeID,
                cardDigest,
                validationResult,
                notificationResolver,
                withoutTransaction,
                cancellationToken).ConfigureAwait(false);

            return validationResult.Build();
        }

        #endregion
    }
}
