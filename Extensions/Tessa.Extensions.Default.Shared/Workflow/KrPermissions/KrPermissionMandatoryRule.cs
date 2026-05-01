using System;
using System.Collections.Generic;

namespace Tessa.Extensions.Default.Shared.Workflow.KrPermissions
{
    public sealed class KrPermissionMandatoryRule
    {
        #region Fields

        private ICollection<Guid> columnIDs;
        private ICollection<Guid> taskTypes;
        private ICollection<Guid> completionOptions;

        #endregion

        #region Constructors

        public KrPermissionMandatoryRule(
            Guid sectionID,
            string text,
            int validationType)
        {
            this.SectionID = sectionID;
            this.Text = text;
            this.ValidationType = validationType;
        }

        #endregion

        #region Properties

        public Guid SectionID { get; }

        public bool HasColumns => this.columnIDs?.Count > 0;

        public ICollection<Guid> ColumnIDs => this.columnIDs ??= new List<Guid>();

        public string Text { get; }

        public int ValidationType { get; }

        public bool HasTaskTypes => this.taskTypes?.Count > 0;

        public ICollection<Guid> TaskTypes => this.taskTypes ??= new List<Guid>();

        public bool HasCompletionOptions => this.taskTypes?.Count > 0;

        public ICollection<Guid> CompletionOptions => this.completionOptions ??= new List<Guid>();

        #endregion
    }
}
