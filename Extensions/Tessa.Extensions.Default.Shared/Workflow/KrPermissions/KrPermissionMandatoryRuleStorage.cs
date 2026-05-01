using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Tessa.Platform;
using Tessa.Platform.Collections;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Shared.Workflow.KrPermissions
{
    public sealed class KrPermissionMandatoryRuleStorage : StorageObject
    {
        #region Constructors

        public KrPermissionMandatoryRuleStorage(
            Guid sectionID,
            IEnumerable<Guid> columnIDs = null)
            : base(new Dictionary<string, object>(StringComparer.Ordinal))
        {
            this.Set(nameof(SectionID), sectionID);
            this.Set(nameof(ColumnIDs), columnIDs?.Cast<object>() ?? EmptyHolder<object>.Collection);
        }

        public KrPermissionMandatoryRuleStorage(Dictionary<string, object> storage)
            :base(storage)
        {
            this.Init(nameof(SectionID), GuidBoxes.Empty);
            this.Init(nameof(ColumnIDs), EmptyHolder<object>.Collection);
        }

        public KrPermissionMandatoryRuleStorage(SerializationInfo info, StreamingContext context)
            :base(info, context)
        {
        }

        #endregion

        #region Properties

        public Guid SectionID
        {
            get => this.Get<Guid>(nameof(SectionID));
        }

        public IReadOnlyCollection<object> ColumnIDs
        {
            get => this.Get<IReadOnlyCollection<object>>(nameof(ColumnIDs));
        }

        #endregion
    }
}
