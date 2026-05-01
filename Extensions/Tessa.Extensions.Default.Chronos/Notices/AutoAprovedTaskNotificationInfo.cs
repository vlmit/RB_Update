using System;
using Tessa.Platform;

namespace Tessa.Extensions.Default.Chronos.Notices
{
    public class AutoAprovedTaskNotificationInfo : TaskNotificationStorage, ITaskNotificationInfo
    {
        #region Constructors

        public AutoAprovedTaskNotificationInfo()
            : base()
        {
            this.Init(nameof(ID), GuidBoxes.Empty);
            this.Init(nameof(Date), null);
            this.Init(nameof(Comment), null);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Идентификатор записи
        /// </summary>
        public Guid ID
        {
            get { return this.Get<Guid>(nameof(ID)); }
            set { this.Set(nameof(ID), value); }
        }

        /// <summary>
        /// Дата автоматического завершения
        /// </summary>
        public DateTime? Date
        {
            get { return this.Get<DateTime?>(nameof(Date)); }
            set { this.Set(nameof(Date), value); }
        }

        /// <summary>
        /// Комментарий, с которым было завершено задание
        /// </summary>
        public string Comment
        {
            get { return this.Get<string>(nameof(Comment)); }
            set { this.Set(nameof(Comment), value); }
        }

        #endregion
    }
}