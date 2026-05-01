using System;
using System.Collections.Generic;
using System.Linq;
using Tessa.Cards;
using Tessa.Extensions.Default.Shared.Workflow.KrPermissions;
using Tessa.Platform.Collections;
using Tessa.Platform.IO;

namespace Tessa.Extensions.Default.Server.Workflow.KrPermissions
{
    /// <inheritdoc cref="IKrPermissionsManagerResult" />
    public sealed class KrPermissionsManagerResult : IKrPermissionsManagerResult
    {
        #region Fields

        private readonly KrPermissionsDescriptor descriptor;
        private readonly bool withExtendedPermissions;

        private HashSet<Guid, IKrPermissionSectionSettings> extendedCardSettings;
        private Dictionary<Guid, HashSet<Guid, IKrPermissionSectionSettings>> extendedTasksSettings;

        #endregion

        #region Constructors

        public KrPermissionsManagerResult(
            KrPermissionsDescriptor descriptor,
            bool withExtendedPermissions,
            long version)
        {
            this.descriptor = descriptor;
            this.withExtendedPermissions = withExtendedPermissions;
            this.Version = version;
        }

        private KrPermissionsManagerResult()
        {
            this.descriptor = new KrPermissionsDescriptor();
        }

        #endregion

        #region IKrPermissionsManagerResult Implementation

        /// <inheritdoc />
        public long Version { get; }

        /// <inheritdoc />
        public bool WithExtendedSettings => withExtendedPermissions;

        /// <inheritdoc />
        public ICollection<KrPermissionFlagDescriptor> Permissions => descriptor.Permissions;

        /// <inheritdoc />
        public HashSet<Guid, IKrPermissionSectionSettings> ExtendedCardSettings => this.extendedCardSettings
            ??= new HashSet<Guid, IKrPermissionSectionSettings>(x => x.ID, descriptor.ExtendedCardSettings.Select(x => x.Build()));

        /// <inheritdoc />
        public Dictionary<Guid, HashSet<Guid, IKrPermissionSectionSettings>> ExtendedTasksSettings => this.extendedTasksSettings
            ??= this.descriptor.ExtendedTasksSettings.ToDictionary(
                x => x.Key,
                x => new HashSet<Guid, IKrPermissionSectionSettings>(y => y.ID, descriptor.ExtendedTasksSettings[x.Key].Select(y => y.Build())));

        /// <inheritdoc />
        public ICollection<KrPermissionFileRule> FileRules
            => descriptor.FileRules;

        /// <inheritdoc />
        public bool Has(KrPermissionFlagDescriptor krPermission)
        {
            return descriptor.Has(krPermission);
        }

        /// <inheritdoc />
        public IKrPermissionExtendedCardSettings CreateExtendedCardSettings(Guid userID, Card card)
        {
            KrPermissionExtendedCardSettingsStorage result = null;

            if (descriptor.ExtendedCardSettings.Count > 0)
            {
                result ??= new KrPermissionExtendedCardSettingsStorage();
                foreach (var settings in this.ExtendedCardSettings)
                {
                    var newSettings = result.SectionSettings.Add();

                    newSettings.ID = settings.ID;
                    newSettings.IsAllowed = settings.IsAllowed;
                    newSettings.IsDisallowed = settings.IsDisallowed;
                    newSettings.IsHidden = settings.IsHidden;
                    newSettings.IsVisible = settings.IsVisible;
                    newSettings.IsMandatory = settings.IsMandatory;
                    newSettings.DisallowRowAdding = settings.DisallowRowAdding;
                    newSettings.DisallowRowDeleting = settings.DisallowRowDeleting;
                    if (settings.AllowedFields.Count > 0)
                    {
                        newSettings.AllowedFields = settings.AllowedFields;
                    }
                    if (settings.DisallowedFields.Count > 0)
                    {
                        newSettings.DisallowedFields = settings.DisallowedFields;
                    }
                    if (settings.HiddenFields.Count > 0)
                    {
                        newSettings.HiddenFields = settings.HiddenFields;
                    }
                    if (settings.VisibleFields.Count > 0)
                    {
                        newSettings.VisibleFields = settings.VisibleFields;
                    }
                    if (settings.MandatoryFields.Count > 0)
                    {
                        newSettings.MandatoryFields = settings.MandatoryFields;
                    }
                    if (settings.MaskedFields.Count > 0)
                    {
                        newSettings.MaskedFields = settings.MaskedFields;
                    }
                }
            }

            if (descriptor.ExtendedTasksSettings.Count > 0)
            {
                result ??= new KrPermissionExtendedCardSettingsStorage();
                foreach (var settingsByTaskType in this.ExtendedTasksSettings)
                {
                    if (settingsByTaskType.Value.Count > 0)
                    {
                        result.TaskSettingsTypes.Add(settingsByTaskType.Key);
                        var taskSettings = result.TaskSettings.Add();
                        foreach (var settings in settingsByTaskType.Value)
                        {
                            var newSettings = taskSettings.Add();

                            newSettings.ID = settings.ID;
                            newSettings.IsAllowed = settings.IsAllowed;
                            newSettings.IsDisallowed = settings.IsDisallowed;
                            newSettings.IsHidden = settings.IsHidden;
                            newSettings.IsVisible = settings.IsVisible;
                            newSettings.DisallowRowAdding = settings.DisallowRowAdding;
                            newSettings.DisallowRowDeleting = settings.DisallowRowDeleting;
                            if (settings.AllowedFields.Count > 0)
                            {
                                newSettings.AllowedFields = settings.AllowedFields;
                            }
                            if (settings.DisallowedFields.Count > 0)
                            {
                                newSettings.DisallowedFields = settings.DisallowedFields;
                            }
                            if (settings.HiddenFields.Count > 0)
                            {
                                newSettings.HiddenFields = settings.HiddenFields;
                            }
                            if (settings.VisibleFields.Count > 0)
                            {
                                newSettings.VisibleFields = settings.VisibleFields;
                            }
                        }
                    }
                }
            }

            var visibilitySettings = descriptor.VisibilitySettings.Build();
            if (visibilitySettings is not null && visibilitySettings.Count > 0)
            {
                result ??= new KrPermissionExtendedCardSettingsStorage();
                foreach (var setting in visibilitySettings)
                {
                    result ??= new KrPermissionExtendedCardSettingsStorage();
                    result.VisibilitySettings.Add(
                        setting.ToStorage());
                }
            }

            if (descriptor.FileRules.Count > 0)
            {
                result ??= new KrPermissionExtendedCardSettingsStorage();
                for (int i = card.Files.Count - 1; i >= 0; i--)
                {
                    var file = card.Files[i];

                    // Данные правила не распространяются на виртуальные файлы
                    if (file.IsVirtual)
                    {
                        continue;
                    }

                    bool isOwnFile = userID == file.Card.CreatedByID;
                    string fileExtension = FileHelper.GetExtension(file.Name).TrimStart('.');
                    int currentAccessSetting = int.MaxValue;
                    foreach (var fileRule in descriptor.FileRules)
                    {
                        if (fileRule.AccessSetting < currentAccessSetting
                            && (!isOwnFile
                                || fileRule.CheckOwnFiles)
                            && (fileRule.Extensions.Count == 0
                                || fileRule.Extensions.Contains(fileExtension))
                            && (fileRule.Categories.Count == 0
                                || (file.CategoryID is Guid categoryID
                                    && fileRule.Categories.Any(x => x == categoryID))))
                        {
                            currentAccessSetting = fileRule.AccessSetting;
                            if (currentAccessSetting == KrPermissionsHelper.FileAccessSettings.FileNotAvailable)
                            {
                                // Если нашли правило с полным запретом, то нет смысла проверять дальше
                                break;
                            }
                        }
                    }

                    if (currentAccessSetting != int.MaxValue)
                    {
                        var fileSetting = result.FileSettings.Add();

                        fileSetting.FileID = file.RowID;
                        fileSetting.AccessSetting = currentAccessSetting;
                    }
                }
            }

            return result;
        }

        #endregion

        #region Static

        public static readonly IKrPermissionsManagerResult Empty = new KrPermissionsManagerResult();

        #endregion
    }
}
