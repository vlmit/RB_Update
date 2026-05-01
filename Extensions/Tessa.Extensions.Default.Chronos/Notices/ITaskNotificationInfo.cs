using System;

namespace Tessa.Extensions.Default.Chronos.Notices
{
    public interface ITaskNotificationInfo
    {
        /// <summary>
        /// ID карточки.
        /// </summary>
        Guid CardID { get; set; }

        /// <summary>
        /// ID исполнителя.
        /// </summary>
        Guid RoleID { get; set; }

        /// <summary>
        /// Имя исполнителя.
        /// </summary>
        string RoleName { get; set; }

        /// <summary>
        /// Текст для ссылки.
        /// </summary>
        string LinkText { get; set; }

        /// <summary>
        /// Ссылка.
        /// </summary>
        string Link { get; set; }

        /// <summary>
        /// Ссылка на ЛК. Может отсутствовать
        /// </summary>
        string WebLink { get; set; }
    }
}