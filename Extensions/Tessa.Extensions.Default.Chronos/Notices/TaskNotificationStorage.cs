using System;
using System.Collections.Generic;
using Tessa.Platform;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Chronos.Notices
{
    public abstract class TaskNotificationStorage : StorageObject, ITaskNotificationInfo
    {
        #region Constructors

        public TaskNotificationStorage()
            : base(new Dictionary<string, object>(StringComparer.Ordinal))
        {
            this.Init(nameof(CardID), GuidBoxes.Empty);
            this.Init(nameof(RoleID), GuidBoxes.Empty);
            this.Init(nameof(RoleName), null);
            this.Init(nameof(LinkText), null);
            this.Init(nameof(Link), null);
            this.Init(nameof(WebLink), null);
        }

        #endregion

        #region ITaskNotificationInfo Members

        /// <summary>
        /// ID карточки.
        /// </summary>
        public Guid CardID
        {
            get { return this.Get<Guid>(nameof(CardID)); }
            set { this.Set(nameof(CardID), value); }
        }

        /// <summary>
        /// Исполнитель.
        /// </summary>
        public Guid RoleID
        {
            get { return this.Get<Guid>(nameof(RoleID)); }
            set { this.Set(nameof(RoleID), value); }
        }

        /// <summary>
        /// Исполнитель.
        /// </summary>
        public string RoleName
        {
            get { return this.Get<string>(nameof(RoleName)); }
            set { this.Set(nameof(RoleName), value); }
        }

        /// <summary>
        /// Текст для ссылки.
        /// </summary>
        public string LinkText
        {
            get { return this.Get<string>(nameof(LinkText)); }
            set { this.Set(nameof(LinkText), value); }
        }

        /// <summary>
        /// Ссылка.
        /// </summary>
        public string Link
        {
            get { return this.Get<string>(nameof(Link)); }
            set { this.Set(nameof(Link), value); }
        }

        /// <summary>
        /// Ссылка на веб-клиент.
        /// </summary>
        public string WebLink
        {
            get { return this.Get<string>(nameof(WebLink)); }
            set { this.Set(nameof(WebLink), value); }
        }

        #endregion
    }
}
