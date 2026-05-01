using System;
using Tessa.Platform;

namespace Tessa.Extensions.Default.Chronos.Notices
{
    /// <summary>
    /// Информация необходимая для построения уведомления по заданию.
    /// </summary>
    public class TaskNotificationInfo : TaskNotificationStorage, ITaskNotificationInfo
    {
        #region Constructors

        public TaskNotificationInfo()
            :base()
        {
            this.Init(nameof(CardNumber), null);
            this.Init(nameof(CardSubject), null);
            this.Init(nameof(InProgress), Int32Boxes.Zero);
            this.Init(nameof(TaskID), GuidBoxes.Empty);
            this.Init(nameof(Created), null);
            this.Init(nameof(Planned), null);
            this.Init(nameof(TaskInfo), null);
            this.Init(nameof(AuthorRole), null);
            this.Init(nameof(TypeCaption), null);
            this.Init(nameof(AutoApproveString), null);
            this.Init(nameof(AutoApproveDate), null);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Номер карточки.
        /// </summary>
        public string CardNumber
        {
            get { return this.Get<string>(nameof(CardNumber)); }
            set { this.Set(nameof(CardNumber), value); }
        }

	    /// <summary>
	    /// Тема карточки.
	    /// </summary>
        public string CardSubject
        {
            get { return this.Get<string>(nameof(CardSubject)); }
            set { this.Set(nameof(CardSubject), value); }
        }

        /// <summary>
        /// Признак, что задание взято в работу.
        /// </summary>
	    public int InProgress
        {
            get { return this.Get<int>(nameof(InProgress)); }
            set { this.Set(nameof(InProgress), value); }
        }

        /// <summary>
        /// ID задания.
        /// </summary>
        public Guid TaskID
        {
            get { return this.Get<Guid>(nameof(TaskID)); }
            set { this.Set(nameof(TaskID), value); }
        }

        /// <summary>
        /// Дата создания.
        /// </summary>
	    public DateTime? Created
        {
            get { return this.Get<DateTime?>(nameof(Created)); }
            set { this.Set(nameof(Created), value); }
        }

        /// <summary>
        /// Запланированная дата выполнения.
        /// </summary>
	    public DateTime? Planned
        {
            get { return this.Get<DateTime?>(nameof(Planned)); }
            set { this.Set(nameof(Planned), value); }
        }

        /// <summary>
        /// Инормация о задании.
        /// </summary>
	    public string TaskInfo
        {
            get { return this.Get<string>(nameof(TaskInfo)); }
            set { this.Set(nameof(TaskInfo), value); }
        }

        /// <summary>
        /// Автор задания.
        /// </summary>
	    public string AuthorRole
        {
            get { return this.Get<string>(nameof(AuthorRole)); }
            set { this.Set(nameof(AuthorRole), value); }
        }

        /// <summary>
        /// Тип задания.
        /// </summary>
        public string TypeCaption
        {
            get { return this.Get<string>(nameof(TypeCaption)); }
            set { this.Set(nameof(TypeCaption), value); }
        }

        /// <summary>
        /// Строка продупреждающая об автозаврешении.
        /// </summary>
        public string AutoApproveString
        {
            get { return this.Get<string>(nameof(AutoApproveString)); }
            set { this.Set(nameof(AutoApproveString), value); }
        }

        /// <summary>
        /// Планируемое время автозаврешении.
        /// </summary>
        public DateTime? AutoApproveDate
        {
            get { return this.Get<DateTime?>(nameof(AutoApproveDate)); }
            set { this.Set(nameof(AutoApproveDate), value); }
        }

        #endregion
    }
}
