using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Tessa.Platform;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Shared.Workflow.KrPermissions
{
    public sealed class KrPermissionFileSettings : StorageObject
    {
        #region Constructors

        public KrPermissionFileSettings()
            : this(new Dictionary<string, object>(StringComparer.Ordinal))
        {
        }

        public KrPermissionFileSettings(Dictionary<string, object> storage)
            :base(storage)
        {
            this.Init(nameof(FileID), GuidBoxes.Empty);
            this.Init(nameof(AccessSetting), Int32Boxes.Zero);
        }

        public KrPermissionFileSettings(SerializationInfo info, StreamingContext context)
            :base(info, context)
        {
        }

        #endregion

        #region Storage Properties

        public Guid FileID
        {
            get => this.Get<Guid>(nameof(FileID));
            set => this.Set(nameof(FileID), value);
        }

        public int AccessSetting
        {
            get => this.Get<int>(nameof(AccessSetting));
            set => this.Set(nameof(AccessSetting), value);
        }

        #endregion
    }
}
