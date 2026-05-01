using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Tessa.Platform;
using Tessa.Platform.Collections;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Shared.Workflow.KrPermissions
{
    public sealed class KrPermissionSectionSettingsStorage : StorageObject, IKrPermissionSectionSettings
    {
        #region Nested Types

        private enum AccessFlag
        {
            None = 0,
            IsAllowed = 1,
            IsDisallowed = 2,
            IsSectionHidden = 4,
            IsSectionVisible = 8,
            DisallowRowAdding = 16,
            DisallowRowDeleting = 32,
            IsMandatory = 64,
            IsMasked = 128,
        }

        #endregion

        #region Constructors

        public KrPermissionSectionSettingsStorage(Dictionary<string, object> storage)
            :base(storage)
        {
            this.Init(nameof(this.ID), GuidBoxes.Empty);
            this.Init(nameof(this.Flag), Int32Boxes.Zero);
        }

        public KrPermissionSectionSettingsStorage(SerializationInfo info, StreamingContext context)
            :base(info, context)
        {
        }

        #endregion

        #region IKrPermissionSectionSettings Implementation

        public Guid ID
        {
            get => this.Get<Guid>(nameof(this.ID));
            set => this.Set(nameof(this.ID), value);
        }

        public bool DisallowRowAdding
        {
            get => this.Has(AccessFlag.DisallowRowAdding);
            set => this.Set(AccessFlag.DisallowRowAdding, value);
        }

        public bool DisallowRowDeleting
        {
            get => this.Has(AccessFlag.DisallowRowDeleting);
            set => this.Set(AccessFlag.DisallowRowDeleting, value);
        }

        public bool IsAllowed
        {
            get => this.Has(AccessFlag.IsAllowed);
            set => this.Set(AccessFlag.IsAllowed, value);
        }

        public bool IsDisallowed
        {
            get => this.Has(AccessFlag.IsDisallowed);
            set => this.Set(AccessFlag.IsDisallowed, value);
        }

        public bool IsHidden
        {
            get => this.Has(AccessFlag.IsSectionHidden);
            set => this.Set(AccessFlag.IsSectionHidden, value);
        }

        public bool IsVisible
        {
            get => this.Has(AccessFlag.IsSectionVisible);
            set => this.Set(AccessFlag.IsSectionVisible, value);
        }

        public bool IsMandatory
        {
            get => this.Has(AccessFlag.IsMandatory);
            set => this.Set(AccessFlag.IsMandatory, value);
        }

        public bool IsMasked
        {
            get => this.Has(AccessFlag.IsMasked);
            set => this.Set(AccessFlag.IsMasked, value);
        }

        public IReadOnlyCollection<Guid> AllowedFields
        {
            get => this.TryGet<IReadOnlyCollection<Guid>>(nameof(this.AllowedFields)) ?? EmptyHolder<Guid>.Collection;
            set => this.Set(nameof(this.AllowedFields), value);
        }

        public IReadOnlyCollection<Guid> DisallowedFields
        {
            get => this.TryGet<IReadOnlyCollection<Guid>>(nameof(this.DisallowedFields)) ?? EmptyHolder<Guid>.Collection;
            set => this.Set(nameof(this.DisallowedFields), value);
        }

        public IReadOnlyCollection<Guid> HiddenFields
        {
            get => this.TryGet<IReadOnlyCollection<Guid>>(nameof(this.HiddenFields)) ?? EmptyHolder<Guid>.Collection;
            set => this.Set(nameof(this.HiddenFields), value);
        }

        public IReadOnlyCollection<Guid> VisibleFields
        {
            get => this.TryGet<IReadOnlyCollection<Guid>>(nameof(this.VisibleFields)) ?? EmptyHolder<Guid>.Collection;
            set => this.Set(nameof(this.VisibleFields), value);
        }

        public IReadOnlyCollection<Guid> MandatoryFields
        {
            get => this.TryGet<IReadOnlyCollection<Guid>>(nameof(this.MandatoryFields)) ?? EmptyHolder<Guid>.Collection;
            set => this.Set(nameof(this.MandatoryFields), value);
        }

        public IReadOnlyCollection<Guid> MaskedFields
        {
            get => this.TryGet<IReadOnlyCollection<Guid>>(nameof(this.MaskedFields)) ?? EmptyHolder<Guid>.Collection;
            set => this.Set(nameof(this.MaskedFields), value);
        }

        #endregion

        #region Storage Properties

        private AccessFlag Flag
        {
            get => (AccessFlag)this.Get<int>(nameof(this.Flag));
            set => this.Set(nameof(this.Flag), (int)value);
        }

        #endregion

        #region Private Methods

        private bool Has(AccessFlag flag)
        {
            return (this.Flag & flag) == flag;
        }

        private void Set(AccessFlag flag, bool value)
        {
            this.Flag =
                value
                ? this.Flag | flag
                : (this.Flag & ~flag);
        }

        #endregion
    }
}
