using System;
using System.Collections.Generic;
using Tessa.Platform.Collections;

namespace Tessa.Extensions.Default.Shared.Workflow.KrPermissions
{
    public sealed class KrPermissionExtendedCardSettings : IKrPermissionExtendedCardSettings
    {
        #region Fields

        private HashSet<Guid, IKrPermissionSectionSettings> cardSettings;
        private Dictionary<Guid, ICollection<IKrPermissionSectionSettings>> tasksSettings;

        #endregion

        #region Constructors

        public KrPermissionExtendedCardSettings()
        {
        }

        #endregion

        #region Properties
        
        public Dictionary<Guid, ICollection<IKrPermissionSectionSettings>> TasksSettings
            => this.tasksSettings ??= new Dictionary<Guid, ICollection<IKrPermissionSectionSettings>>();

        public ICollection<IKrPermissionSectionSettings> CardSettings 
            => this.cardSettings ??= new HashSet<Guid, IKrPermissionSectionSettings>(x => x.ID);

        #endregion

        #region IKrPermissionExtendedCardSettings Implementation

        public ICollection<IKrPermissionSectionSettings> GetCardSettings()
        {
            return cardSettings;
        }

        public Dictionary<Guid, ICollection<IKrPermissionSectionSettings>> GetTasksSettings()
        {
            return tasksSettings;
        }

        public ICollection<KrPermissionVisibilitySettings> GetVisibilitySettings()
        {
            return EmptyHolder<KrPermissionVisibilitySettings>.Collection;
        }

        public ICollection<KrPermissionFileSettings> GetFileSettings()
        {
            return EmptyHolder<KrPermissionFileSettings>.Collection;
        }

        public void SetCardAccess(bool isAllowed, Guid sectionID, ICollection<Guid> fields)
        {
            if (cardSettings == null)
            {
                cardSettings = new HashSet<Guid, IKrPermissionSectionSettings>(x => x.ID);
            }
            else if (cardSettings.TryGetItem(sectionID, out var isectionSettingsExisted)
                && isectionSettingsExisted is KrPermissionSectionSettings sectionSettingsExisted)
            {
                if (fields != null
                    && fields.Count > 0)
                {
                    if (isAllowed)
                    {
                        sectionSettingsExisted.AllowedFields.AddRange(fields);
                        sectionSettingsExisted.DisallowedFields.RemoveRange(fields);
                    }
                    else
                    {
                        sectionSettingsExisted.DisallowedFields.AddRange(fields);
                        sectionSettingsExisted.AllowedFields.RemoveRange(fields);
                    }
                }
                else
                {
                    sectionSettingsExisted.IsAllowed = isAllowed;
                    sectionSettingsExisted.IsDisallowed = !isAllowed;
                }

                return;
            }

            var sectionSettings = new KrPermissionSectionSettings
            {
                ID = sectionID,
            };
            cardSettings.Add(sectionSettings);

            if (fields != null
                    && fields.Count > 0)
            {
                if (isAllowed)
                {
                    sectionSettings.AllowedFields = new HashSet<Guid>(fields);
                }
                else
                {
                    sectionSettings.DisallowedFields = new HashSet<Guid>(fields);
                }
            }
            else
            {
                sectionSettings.IsAllowed = isAllowed;
                sectionSettings.IsDisallowed = !isAllowed;
            }
        }

        #endregion
    }
}
