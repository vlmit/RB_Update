using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tessa.Platform.Collections;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Shared.Workflow.KrPermissions
{
    public sealed class KrPermissionExtendedCardSettingsStorage : StorageObject, IKrPermissionExtendedCardSettings
    {
        #region Fields

        private static readonly IStorageValueFactory<int, IKrPermissionSectionSettings> cardSettingsFactory =
            new DictionaryStorageValueFactory<int, IKrPermissionSectionSettings>(
                (index, storage) => new KrPermissionSectionSettingsStorage(storage));

        private static readonly IStorageValueFactory<int, ListStorage<IKrPermissionSectionSettings>> taskSettingsFactory =
            new ListStorageValueFactory<int, ListStorage<IKrPermissionSectionSettings>>(
                (index, storage) => new ListStorage<IKrPermissionSectionSettings>(storage, cardSettingsFactory));

        private static readonly IStorageValueFactory<int, KrPermissionFileSettings> fileSettingsFactory =
            new DictionaryStorageValueFactory<int, KrPermissionFileSettings>(
                (index, storage) => new KrPermissionFileSettings(storage));

        #endregion

        #region Constructors

        public KrPermissionExtendedCardSettingsStorage(Dictionary<string, object> storage)
            : base(storage)
        {
            this.Init(nameof(SectionSettings), new List<object>());
            this.Init(nameof(TaskSettings), new List<object>());
            this.Init(nameof(TaskSettingsTypes), new List<object>());
            this.Init(nameof(VisibilitySettings), new List<object>());
            this.Init(nameof(FileSettings), new List<object>());
        }

        public KrPermissionExtendedCardSettingsStorage()
            : this(new Dictionary<string, object>(StringComparer.Ordinal))
        {
        }

        #endregion

        #region Storage Properties

        public ListStorage<IKrPermissionSectionSettings> SectionSettings
        {
            get
            {
                return this.GetList(
                    nameof(SectionSettings),
                    x => new ListStorage<IKrPermissionSectionSettings>(x, cardSettingsFactory));
            }
            set { this.SetStorageValue(nameof(SectionSettings), value); }
        }

        public ListStorage<ListStorage<IKrPermissionSectionSettings>> TaskSettings
        {
            get
            {
                return this.GetList(
                    nameof(TaskSettings),
                    x => new ListStorage<ListStorage<IKrPermissionSectionSettings>>(x, taskSettingsFactory));
            }
            set { this.SetStorageValue(nameof(TaskSettings), value); }
        }

        public IList TaskSettingsTypes
        {
            get { return this.Get<IList>(nameof(TaskSettingsTypes)); }
            set { this.Set(nameof(TaskSettingsTypes), value); }
        }

        public IList VisibilitySettings
        {
            get { return this.Get<IList>(nameof(VisibilitySettings)); }
            set { this.Set(nameof(VisibilitySettings), value); }
        }

        public ListStorage<KrPermissionFileSettings> FileSettings
        {
            get
            {
                return this.GetList(
                    nameof(FileSettings),
                    x => new ListStorage<KrPermissionFileSettings>(x, fileSettingsFactory));
            }
            set { this.SetStorageValue(nameof(FileSettings), value); }
        }

        #endregion

        #region IKrPermissionExtendedCardSettings Implementation

        public ICollection<IKrPermissionSectionSettings> GetCardSettings()
        {
            return SectionSettings;
        }

        public Dictionary<Guid, ICollection<IKrPermissionSectionSettings>> GetTasksSettings()
        {
            if (TaskSettings != null
                && TaskSettings.Count > 0)
            {
                var result = new Dictionary<Guid, ICollection<IKrPermissionSectionSettings>>();
                for (int i = 0; i < TaskSettings.Count; i++)
                {
                    result[(Guid)TaskSettingsTypes[i]] = TaskSettings[i];
                }

                return result;
            }

            return null;
        }

        public ICollection<KrPermissionVisibilitySettings> GetVisibilitySettings()
        {
            if (VisibilitySettings.Count == 0)
            {
                return EmptyHolder<KrPermissionVisibilitySettings>.Collection;
            }

            var result = new KrPermissionVisibilitySettings[VisibilitySettings.Count];

            for(int i = 0; i < VisibilitySettings.Count; i++)
            {
                result[i] = KrPermissionVisibilitySettings.FromStorage((Dictionary<string, object>)VisibilitySettings[i]);
            }

            return result;
        }

        public ICollection<KrPermissionFileSettings> GetFileSettings()
        {
            return FileSettings;
        }

        public void SetCardAccess(
            bool isAllowed,
            Guid sectionID,
            ICollection<Guid> fields)
        {
            var sectionSettings = this.SectionSettings.FirstOrDefault(x => x.ID == sectionID);
            if (sectionSettings == null)
            {
                sectionSettings = this.SectionSettings.Add();
                sectionSettings.ID = sectionID;

                if (fields != null
                    && fields.Count > 0)
                {
                    if (isAllowed)
                    {
                        sectionSettings.AllowedFields = fields.ToArray();
                    }
                    else
                    {
                        sectionSettings.DisallowedFields = fields.ToArray();
                    }
                }
                else
                {
                    sectionSettings.IsAllowed = isAllowed;
                    sectionSettings.IsDisallowed = !isAllowed;
                }
            }
            else
            {
                if (fields != null
                    && fields.Count > 0)
                {
                    var to = isAllowed ? sectionSettings.AllowedFields : sectionSettings.DisallowedFields;
                    var from = isAllowed ? sectionSettings.DisallowedFields : sectionSettings.AllowedFields;

                    var toFields = new HashSet<Guid>(to);
                    var fromFields = new HashSet<Guid>(from);
                    toFields.AddRange(fields);
                    fromFields.RemoveWhere(x => toFields.Contains(x));

                    sectionSettings.AllowedFields = isAllowed ? toFields : fromFields;
                    sectionSettings.DisallowedFields = isAllowed ? fromFields : toFields;
                }
                else
                {
                    sectionSettings.IsAllowed = isAllowed;
                    sectionSettings.IsDisallowed = !isAllowed;
                }
            }
        }

        #endregion
    }
}

