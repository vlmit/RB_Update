using System;
using System.Collections.Generic;
using System.Linq;

namespace Tessa.Extensions.Default.Shared.Workflow.KrPermissions
{
    /// <inheritdoc/>
    public sealed class KrPermissionSectionSettingsBuilder : IKrPermissionSectionSettingsBuilder
    {
        #region Fields

        private Dictionary<int, KrPermissionSectionSettings> sectionSettingsDict = new Dictionary<int, KrPermissionSectionSettings>();

        #endregion

        #region Constructors

        public KrPermissionSectionSettingsBuilder(Guid sectionID)
        {
            this.SectionID = sectionID;
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public Guid SectionID { get; }

        #endregion

        #region IKrPermissionsSectionSettingsBuilder Implementation

        /// <inheritdoc/>
        public IKrPermissionSectionSettingsBuilder Add(IKrPermissionSectionSettings sectionSettings, int priority = 0)
        {
            if (sectionSettingsDict.TryGetValue(priority, out var currentSettings))
            {
                currentSettings.MergeWith(sectionSettings);
            }
            else
            {
                sectionSettingsDict[priority] = KrPermissionSectionSettings.ConvertFrom(sectionSettings);
            }
            return this;
        }

        /// <inheritdoc/>
        public IKrPermissionSectionSettings Build()
        {
            if (sectionSettingsDict.Count == 0)
            {
                return null;
            }

            KrPermissionSectionSettings result = null;
            foreach (var (_, sectionSettings) in sectionSettingsDict.OrderBy(x => x.Key))
            {
                if (result is null)
                {
                    result = sectionSettings;
                }
                else
                {
                    result.MergeWith(sectionSettings, true);
                }
            }

            result.Clean();
            return result;
        }

        #endregion
    }
}
