using System;
using System.Linq;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Extensions.Default.Shared.Workflow.KrPermissions;
using Tessa.Extensions.Platform.Client.UI;
using Tessa.Platform.Conditions;
using Tessa.Platform.Placeholders;
using Tessa.Platform.Runtime;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using Tessa.Scheme;
using Tessa.UI;
using Tessa.UI.Cards;
using Tessa.UI.Cards.Controls;
using Unity;

namespace Tessa.Extensions.Default.Client.UI
{
    public sealed class KrPermissionsUIExtension : CardUIExtension
    {
        #region Fields

        private readonly ISession session;
        private readonly IUnityContainer unityContainer;
        private readonly ICardRepairManager cardRepairManager;
        private readonly ICardMetadata cardMetadata;
        private readonly IConditionTypesProvider typesProvider;
        private readonly IPlaceholderManager placeholderManager;

        private static readonly string[] extendedSections = new string[]
        {
            "KrPermissionExtendedCardRuleFields",
            "KrPermissionExtendedCardRules",
            "KrPermissionExtendedTaskRuleFields",
            "KrPermissionExtendedTaskRuleTypes",
            "KrPermissionExtendedTaskRules",
            "KrPermissionExtendedMandatoryRuleFields",
            "KrPermissionExtendedMandatoryRuleTypes",
            "KrPermissionExtendedMandatoryRuleOptions",
            "KrPermissionExtendedMandatoryRules",
            "KrPermissionExtendedVisibilityRules",
            "KrPermissionExtendedFileRules",
            "KrPermissionExtendedFileRuleCategories",
        };

        private static readonly string[] extendedControls = new string[]
        {
            "Priority",
            "CardExtendedSettings",
            "TasksExtendedSettings",
            "MandatoryExtendedSettings",
            "VisibilityExtendedSettings",
            "FileExtendedPermissionsSettings",
        };


        #endregion

        #region Constructors

        public KrPermissionsUIExtension(
            ISession session,
            IUnityContainer unityContainer,
            ICardRepairManager cardRepairManager,
            ICardMetadata cardMetadata,
            IConditionTypesProvider typesProvider,
            IPlaceholderManager placeholderManager)
        {
            this.session = session;
            this.unityContainer = unityContainer;
            this.cardRepairManager = cardRepairManager;
            this.cardMetadata = cardMetadata;
            this.typesProvider = typesProvider;
            this.placeholderManager = placeholderManager;
        }

        #endregion

        #region Base Overrides

        public override Task Initialized(ICardUIExtensionContext context)
        {
            InitializeConditions(context);
            InitializeFlags(context);
            InitializeExtendedPermissions(context);

            return Task.CompletedTask;
        }

        #endregion

        #region Private Methods

        private void InitializeExtendedPermissions(ICardUIExtensionContext context)
        {
            var model = context.Model;
            var permissionsSection = context.Card.Sections.GetOrAdd("KrPermissions");
            bool isExtendedOnUpdating = false;

            permissionsSection.FieldChanged += (s, e) =>
            {
                if (e.FieldName == "IsExtended")
                {
                    if (isExtendedOnUpdating)
                    {
                        return;
                    }

                    isExtendedOnUpdating = true;
                    try
                    {
                        if (e.FieldValue as bool? ?? false)
                        {
                            EnableExtendedSettings(model);
                        }
                        else if (ClearExtendedSettings(model))
                        {
                            DisableExtendedSettings(model);
                        }
                        else
                        {
                            permissionsSection.Fields["IsExtended"] = true;
                        }
                    }
                    finally
                    {
                        isExtendedOnUpdating = false;
                    }
                }
            };

            if (!permissionsSection.RawFields.TryGet<bool?>("IsExtended") ?? false)
            {
                DisableExtendedSettings(model);
            }
            ExtendControls(model);
        }

        private void ExtendControls(ICardModel model)
        {
            var card = model.Card;
            if (model.Controls.TryGet(extendedControls[1], out var control)
                && control is GridViewModel cardTable)
            {
                ExtendPermissionGrid(
                    cardTable,
                    card.Sections["KrPermissionExtendedCardRules"],
                    card.Sections["KrPermissionExtendedCardRuleFields"]);
            }

            if (model.Controls.TryGet(extendedControls[2], out control)
                && control is GridViewModel tasksTable)
            {
                ExtendPermissionGrid(
                    tasksTable,
                    card.Sections["KrPermissionExtendedTaskRules"],
                    card.Sections["KrPermissionExtendedTaskRuleFields"]);
            }

            if (model.Controls.TryGet(extendedControls[3], out control)
                && control is GridViewModel mandatoryTable)
            {
                ExtendMandatoryGrid(
                    mandatoryTable,
                    card);
            }
        }

        private void ExtendMandatoryGrid(GridViewModel grid, Card card)
        {
            ICardModel openedRowModel = null;
            var fieldsSection = card.Sections["KrPermissionExtendedMandatoryRuleFields"];
            var typesSection = card.Sections["KrPermissionExtendedMandatoryRuleTypes"];
            var optionsSection = card.Sections["KrPermissionExtendedMandatoryRuleOptions"];

            void SectionRowChanged(object sender, CardFieldChangedEventArgs args)
            {
                if (openedRowModel == null)
                {
                    return;
                }

                var row = (CardRow)sender;
                if (args.FieldName == "SectionID")
                {
                    ClearSection(row.RowID, fieldsSection);
                }
                else if (args.FieldName == "SectionTypeID"
                    && openedRowModel.Controls.TryGet("Fields", out var fieldsControl))
                {
                    if (args.FieldValue is int sectionType
                        && (SchemeTableContentType)sectionType == SchemeTableContentType.Entries)
                    {
                        fieldsControl.IsRequired = true;
                        fieldsControl.RequiredText = "$CardTypes_Validators_Fields";
                    }
                    else
                    {
                        fieldsControl.IsRequired = false;
                        fieldsControl.RequiredText = null;
                    }
                }
                else if (args.FieldName == "ValidationTypeID")
                {
                    if (!(args.FieldValue is int validationType
                            && validationType == KrPermissionsHelper.MandatoryValidationType.OnTaskCompletion))
                    {
                        ClearSection(row.RowID, typesSection);
                        ClearSection(row.RowID, optionsSection);
                        if (openedRowModel.Controls.TryGet("TaskTypes", out var typesControl)
                            && openedRowModel.Controls.TryGet("CompletionOptions", out var optionsControl))
                        {
                            typesControl.ControlVisibility = System.Windows.Visibility.Collapsed;
                            optionsControl.ControlVisibility = System.Windows.Visibility.Collapsed;

                            typesControl.Block.Rearrange();
                        }
                    }
                    else
                    {
                        if (openedRowModel.Controls.TryGet("TaskTypes", out var typesControl)
                            && openedRowModel.Controls.TryGet("CompletionOptions", out var optionsControl))
                        {
                            typesControl.ControlVisibility = System.Windows.Visibility.Visible;
                            optionsControl.ControlVisibility = System.Windows.Visibility.Visible;

                            typesControl.Block.Rearrange();
                        }
                    }
                }
            }

            grid.RowInitializing += (s, e) =>
            {
                e.Row.FieldChanged += SectionRowChanged;
                openedRowModel = e.RowModel;
                if (!(e.Row.TryGet<int?>("ValidationTypeID") is int validationType
                        && validationType == KrPermissionsHelper.MandatoryValidationType.OnTaskCompletion)
                    && openedRowModel.Controls.TryGet("TaskTypes", out var typesControl)
                    && openedRowModel.Controls.TryGet("CompletionOptions", out var optionsControl))
                {
                    typesControl.ControlVisibility = System.Windows.Visibility.Collapsed;
                    optionsControl.ControlVisibility = System.Windows.Visibility.Collapsed;
                }

                if (e.Row.TryGet<int?>("SectionTypeID") is int sectionType
                    && (SchemeTableContentType)sectionType == SchemeTableContentType.Entries
                    && openedRowModel.Controls.TryGet("Fields", out var fieldsControl))
                {
                    fieldsControl.IsRequired = true;
                    fieldsControl.RequiredText = "$CardTypes_Validators_Fields";
                }
            };
            grid.RowValidating += (s, e) =>
            {
                var row = e.Row;
                if (row.TryGet<int?>("ValidationTypeID") is int validationType
                    && validationType == KrPermissionsHelper.MandatoryValidationType.OnTaskCompletion
                    && !typesSection.Rows.Any(x => x.State != CardRowState.Deleted && x.Get<Guid>("RuleRowID") == row.RowID))
                {
                    e.ValidationResult.AddError(
                        this,
                        "$CardTypes_Validators_TaskTypes");
                }

                if (row.TryGet<int?>("SectionTypeID") is int sectionType
                    && (SchemeTableContentType)sectionType == SchemeTableContentType.Entries
                    && !fieldsSection.Rows.Any(x => x.State != CardRowState.Deleted && x.Get<Guid>("RuleRowID") == row.RowID))
                {
                    e.ValidationResult.AddError(
                        this,
                        "$CardTypes_Validators_Fields");
                }
            };
            grid.RowEditorClosed += (s, e) =>
            {
                e.Row.FieldChanged -= SectionRowChanged;
                openedRowModel = null;
            };
        }

        private void ExtendPermissionGrid(GridViewModel grid, CardSection sectionsSection, CardSection fieldsSection)
        {
            ICardModel openedRowModel = null;
            void SectionRowChanged(object sender, CardFieldChangedEventArgs args)
            {
                if (openedRowModel == null)
                {
                    return;
                }

                var row = (CardRow)sender;
                if (args.FieldName == "SectionID")
                {
                    ClearSection(row.RowID, fieldsSection);
                }
                else if (args.FieldName == "SectionTypeID"
                    && args.FieldValue != null)
                {
                    var sectionType = (SchemeTableContentType)(int)args.FieldValue;
                    var accessSetting = row.TryGet<int?>("AccessSettingID");
                    if (accessSetting.HasValue
                        && sectionType >= SchemeTableContentType.Collections
                        && (accessSetting == KrPermissionsHelper.AccessSettings.DisallowRowAdding
                            || accessSetting == KrPermissionsHelper.AccessSettings.DisallowRowDeleting
                            || accessSetting == KrPermissionsHelper.AccessSettings.DisallowRowEdit))
                    {
                        row.Fields["AccessSettingID"] = null;
                        row.Fields["AccessSettingName"] = null;
                    }
                }
                else if (args.FieldName == "AccessSettingID")
                {
                    if (args.FieldValue is int accessSetting
                        && (accessSetting == KrPermissionsHelper.AccessSettings.DisallowRowAdding
                            || accessSetting == KrPermissionsHelper.AccessSettings.DisallowRowDeleting
                            || accessSetting == KrPermissionsHelper.AccessSettings.DisallowRowEdit))
                    {
                        ClearSection(row.RowID, fieldsSection);
                        if (openedRowModel.Controls.TryGet("Fields", out var control))
                        {
                            control.ControlVisibility = System.Windows.Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        if (openedRowModel.Controls.TryGet("Fields", out var control))
                        {
                            control.ControlVisibility = System.Windows.Visibility.Visible;
                        }
                    }

                    if (args.FieldValue as int? == KrPermissionsHelper.AccessSettings.MaskData)
                    {
                        if (openedRowModel.Controls.TryGet("Mask", out var control))
                        {
                            control.ControlVisibility = System.Windows.Visibility.Visible;
                        }
                    }
                    else
                    {
                        if (openedRowModel.Controls.TryGet("Mask", out var control))
                        {
                            control.ControlVisibility = System.Windows.Visibility.Collapsed;
                        }
                    }
                }
            }

            grid.RowInitializing += (s, e) =>
            {
                e.Row.FieldChanged += SectionRowChanged;
                openedRowModel = e.RowModel;
                var accessSetting = e.Row.TryGet<int?>("AccessSettingID");
                if (accessSetting.HasValue
                    && (accessSetting == KrPermissionsHelper.AccessSettings.DisallowRowAdding
                        || accessSetting == KrPermissionsHelper.AccessSettings.DisallowRowDeleting
                        || accessSetting == KrPermissionsHelper.AccessSettings.DisallowRowEdit)
                    && openedRowModel.Controls.TryGet("Fields", out var control))
                {
                    control.ControlVisibility = System.Windows.Visibility.Collapsed;
                }

                if (accessSetting != KrPermissionsHelper.AccessSettings.MaskData
                    && openedRowModel.Controls.TryGet("Mask", out control))
                {
                    control.ControlVisibility = System.Windows.Visibility.Collapsed;
                }
            };

            grid.RowEditorClosed += (s, e) =>
            {
                e.Row.FieldChanged -= SectionRowChanged;
                openedRowModel = null;
            };
        }

        private void EnableExtendedSettings(ICardModel model)
        {
            foreach (var controlName in extendedControls)
            {
                if (model.Controls.TryGet(controlName, out var control))
                {
                    control.ControlVisibility = System.Windows.Visibility.Visible;
                    control.Block.Rearrange();
                }
            }
        }

        private void DisableExtendedSettings(ICardModel model)
        {
            foreach(var controlName in extendedControls)
            {
                if (model.Controls.TryGet(controlName, out var control))
                {
                    control.ControlVisibility = System.Windows.Visibility.Collapsed;
                    control.Block.Rearrange();
                }
            }
        }

        private bool ClearExtendedSettings(ICardModel model)
        {
            bool HasRows(CardSection section)
            {
                return section.Rows.Count(x => x.State != CardRowState.Deleted) > 0;
            }

            var card = model.Card;
            if (!extendedSections.Any(x => HasRows(card.Sections[x]))
                || TessaDialog.Confirm("$KrPermissions_DisableExtendedSettingsConfirm"))
            {
                return true;
            }

            return false;
        }

        private void InitializeFlags(ICardUIExtensionContext context)
        {
            var model = context.Model;
            var permissionsSection = context.Card.Sections.GetOrAdd("KrPermissions");

            var flagsByName = KrPermissionFlagDescriptors.Full.IncludedPermissions.ToDictionary(
                x => x.SqlName,
                x => x);

            foreach (var flag in KrPermissionFlagDescriptors.Full.IncludedPermissions)
            {
                if (flag.IsVirtual
                    || flag.IncludedPermissions.Count == 0)
                {
                    continue;
                }

                if (permissionsSection.RawFields.TryGetValue(flag.SqlName, out var value)
                    && value is bool b
                    && b)
                {
                    foreach (var includedFlag in flag.IncludedPermissions)
                    {
                        TryUpdateFlag(model, permissionsSection, includedFlag, true);
                    }
                }
            }

            permissionsSection.FieldChanged += (s, e) =>
            {
                if (flagsByName.TryGetValue(e.FieldName, out var flag))
                {
                    foreach (var includedFlag in flag.IncludedPermissions)
                    {
                        TryUpdateFlag(
                            model,
                            permissionsSection,
                            includedFlag,
                            (bool)e.FieldValue);
                    }
                }
            };
        }

        private void InitializeConditions(ICardUIExtensionContext context)
        {
            var model = context.Model;

            var conditionContext = new ConditionsUIContext(
                session,
                unityContainer,
                cardRepairManager,
                cardMetadata,
                typesProvider,
                placeholderManager);

            conditionContext.Initialize(model);
        }

        private static void ClearSection(Guid parentRowID, CardSection section)
        {
            for (int i = section.Rows.Count - 1; i >= 0; i--)
            {
                var fieldRow = section.Rows[i];
                if (fieldRow.Get<Guid>("RuleRowID") == parentRowID)
                {
                    if (fieldRow.State == CardRowState.Inserted)
                    {
                        section.Rows.RemoveAt(i);
                    }
                    else
                    {
                        fieldRow.State = CardRowState.Deleted;
                    }
                }
            }
        }

        private static void TryUpdateFlag(
            ICardModel model,
            CardSection section,
            KrPermissionFlagDescriptor flag,
            bool isReadonly)
        {
            if (model.Controls.TryGet(flag.Name, out var control))
            {
                control.IsReadOnly = isReadonly;
                section.Fields[flag.SqlName] = isReadonly; // Соответстует value
            }
        }

        #endregion
    }
}
