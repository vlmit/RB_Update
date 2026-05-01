using System;
using Tessa.Notices;
using Tessa.Platform.Data;
using Tessa.Platform.Licensing;
using Tessa.Platform.Runtime;
using Tessa.Platform.Validation;
using Unity;

namespace Tessa.Extensions.Default.Shared.Notices
{
    /// <summary>
    /// Контекст, в котором выполняется отправка уведомлений <see cref="INotification"/>.
    /// </summary>
    public interface INotificationContext
    {
        /// <summary>
        /// Идентификатор карточки.
        /// </summary>
        Guid CardID { get; }

        /// <summary>
        /// Идентификатор типа карточки.
        /// </summary>
        Guid CardTypeID { get; }

        /// <summary>
        /// Digest карточки или <c>null</c>, если Digest недоступен.
        /// Локализация должна быть выполнена индивидуально для каждого уведомления
        /// на языке пользователя, который получит уведомление.
        /// Локализацию требуется выполнять методом <see cref="Tessa.Localization.LocalizationManager.Format(string)"/>.
        /// </summary>
        string CardDigest { get; }

        /// <summary>
        /// Нормализованная ссылка на базовый адрес веб-сервера, заданная в настройках.
        /// </summary>
        string WebAddress { get; }

        /// <summary>
        /// Результат валидации, содержащий сообщения.
        /// </summary>
        IValidationResultBuilder ValidationResult { get; }

        /// <summary>
        /// Сервис отправки почтовых сообщений.
        /// </summary>
        IMailService MailService { get; }

        /// <summary>
        /// Объект, обеспечивающий взаимодействие с базой данных.
        /// </summary>
        IDbScope DbScope { get; }

        /// <summary>
        /// Сессия текущего пользователя.
        /// </summary>
        ISession Session { get; }

        /// <summary>
        /// Контейнер Unity.
        /// </summary>
        IUnityContainer UnityContainer { get; }

        /// <summary>
        /// Объект, управляющий лицензиями.
        /// </summary>
        ILicenseManager LicenseManager { get; }
    }
}