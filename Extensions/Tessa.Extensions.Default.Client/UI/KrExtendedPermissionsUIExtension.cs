using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Extensions.Default.Shared.Workflow.KrPermissions;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Localization;
using Tessa.Platform.Collections;
using Tessa.UI;
using Tessa.UI.Cards;
using Tessa.UI.Cards.Controls;

namespace Tessa.Extensions.Default.Client.UI
{
    public sealed class KrExtendedPermissionsUIExtension : CardUIExtension
    {
        #region Nested Types

        private class VisibilitySetting
        {
            public string Text { get; set; }
            public bool IsHidden { get; set; }
            public bool IsPattern { get; set; }
        }

        private class VisibilitySettings
        {
            #region Fields

            public List<VisibilitySetting> BlockSettings;
            public List<VisibilitySetting> ControlSettings;
            public List<VisibilitySetting> TabSettings;

            #endregion

            #region Public Methods

            public void Fill(ICollection<KrPermissionVisibilitySettings> visibilitySettings)
            {
                foreach (var visibilitySetting in visibilitySettings)
                {
                    List<VisibilitySetting> patternList;
                    switch (visibilitySetting.ControlType)
                    {
                        case KrPermissionsHelper.ControlType.Tab:
                            patternList = this.TabSettings ??= new List<VisibilitySetting>();
                            break;

                        case KrPermissionsHelper.ControlType.Block:
                            patternList = this.BlockSettings ??= new List<VisibilitySetting>();
                            break;

                        case KrPermissionsHelper.ControlType.Control:
                            patternList = this.ControlSettings ??= new List<VisibilitySetting>();
                            break;
                        default: continue;
                    }

                    FillSettings(patternList, visibilitySetting);
                }
            }

            #endregion

            #region Private Methods

            private void FillSettings(List<VisibilitySetting> patternList, KrPermissionVisibilitySettings visibilitySetting)
            {
                string alias = visibilitySetting.Alias;
                int length = alias.Length;
                bool wildStart = alias[0] == '*',
                     wildEnd = alias[length - 1] == '*';

                if (wildStart || wildEnd)
                {
                    var escapedAlias =
                        Regex.Escape(alias.Substring(wildStart ? 1 : 0, Math.Max(0, length - (wildStart ? 1 : 0) - (wildEnd ? 1 : 0))));
                    patternList.Add(new VisibilitySetting { IsHidden = visibilitySetting.IsHidden, Text = escapedAlias, IsPattern = true });
                }
                else
                {
                    patternList.Add(new VisibilitySetting { IsHidden = visibilitySetting.IsHidden, Text = alias, IsPattern = false });
                }
            }

            #endregion
        }

        private class PermissionsControlsVisitor
        {
            #region Fields

            private HashSet<IFormViewModel> formsForRearange = new HashSet<IFormViewModel>();
            private VisibilitySettings visibilitySettings;

            #endregion

            #region Properties

            public HashSet<Guid> HideSections { get; } = new HashSet<Guid>();

            public HashSet<Guid> HideFields { get; } = new HashSet<Guid>();

            public HashSet<Guid> ShowFields { get; } = new HashSet<Guid>();

            public HashSet<Guid> MandatorySections { get; } = new HashSet<Guid>();

            public HashSet<Guid> MandatoryFields { get; } = new HashSet<Guid>();

            public HashSet<Guid> DisallowedSections { get; } = new HashSet<Guid>();

            #endregion

            #region Public Methods

            public async Task VisitAsync(ICardModel cardModel, Stack<GridViewModel> parentGrids)
            {
                this.formsForRearange.Clear();

                bool disableGrids = false;
                if (cardModel.Table != null
                    && cardModel.Table.Row.State != CardRowState.Inserted)
                {
                    foreach (var grid in parentGrids)
                    {
                        // Если хотя бы одна из родительских таблиц недоступна для редактирования через KrPermissions, то и все таблицы текущей строки должны быть недоступны.
                        var parentGridSource = grid.CardTypeControl.GetSourceInfo();
                        if (this.DisallowedSections.Contains(parentGridSource.SectionID))
                        {
                            disableGrids = true;
                            break;
                        }
                    }
                }

                foreach(var control in cardModel.ControlBag)
                {
                    this.VisitControl(control);
                    if (disableGrids 
                        && control is GridViewModel grid)
                    {
                        grid.IsReadOnly = true;
                    }
                }

                foreach(var block in cardModel.BlockBag)
                {
                    this.VisitBlock(block);
                }

                foreach (var form in cardModel.FormBag)
                {
                    await this.VisitFormAsync(cardModel, form);
                }

                foreach (var form in formsForRearange)
                {
                    form.Rearrange();
                }
            }

            public void Fill(
                ICollection<IKrPermissionSectionSettings> sectionSettings,
                VisibilitySettings visibilitySettings)
            {
                if (sectionSettings != null)
                {
                    foreach (var sectionSetting in sectionSettings)
                    {
                        if (sectionSetting.IsHidden)
                        {
                            this.HideSections.Add(sectionSetting.ID);
                        }
                        else
                        {
                            this.HideFields.AddRange(sectionSetting.HiddenFields);
                        }

                        this.ShowFields.AddRange(sectionSetting.VisibleFields);

                        if (sectionSetting.IsMandatory)
                        {
                            this.MandatorySections.Add(sectionSetting.ID);
                        }
                        else
                        {
                            this.MandatoryFields.AddRange(sectionSetting.MandatoryFields);
                        }

                        if (sectionSetting.IsDisallowed)
                        {
                            this.DisallowedSections.Add(sectionSetting.ID);
                        }
                    }
                }

                this.visibilitySettings = visibilitySettings;
            }

            #endregion

            #region Private Methods

            private async Task VisitFormAsync(ICardModel model, IFormViewModel form)
            {
                if (!string.IsNullOrWhiteSpace(form.Name))
                {
                    var hidden = CheckIsHidden(visibilitySettings.TabSettings, form.Name);
                    if (hidden == true)
                    {
                        await HideFormAsync(model, form);
                    }
                    // Нет возможности добавлять скрытую форму, т.к. она не была сгенерирована
                }
            }

            private void VisitBlock(IBlockViewModel block)
            {
                if (!string.IsNullOrWhiteSpace(block.Name))
                {
                    var hidden = CheckIsHidden(visibilitySettings.BlockSettings, block.Name);
                    if (hidden.HasValue)
                    {
                        if (hidden.Value)
                        {
                            HideBlock(block);
                        }
                        else
                        {
                            ShowBlock(block);
                        }
                    }
                }
            }

            private void VisitControl(IControlViewModel control)
            {
                var sourceInfo = control.CardTypeControl.GetSourceInfo();
                if ((HideSections.Contains(sourceInfo.SectionID) && !sourceInfo.ColumnIDs.Any(x => ShowFields.Contains(x)))
                    || sourceInfo.ColumnIDs.Any(x => HideFields.Contains(x)))
                {
                    HideControl(control);
                }

                if (!string.IsNullOrWhiteSpace(control.Name))
                {
                    var hidden = CheckIsHidden(visibilitySettings.ControlSettings, control.Name);

                    if (hidden.HasValue)
                    {
                        if (hidden.Value)
                        {
                            HideControl(control);
                        }
                        else
                        {
                            ShowControl(control);
                        }
                    }
                }

                if (MandatorySections.Contains(sourceInfo.SectionID)
                    || sourceInfo.ColumnIDs.Any(x => MandatoryFields.Contains(x)))
                {
                    MakeControlMandatory(control);
                }
            }

            private void HideControl(IControlViewModel controlViewModel)
            {
                controlViewModel.ControlVisibility = System.Windows.Visibility.Collapsed;
                formsForRearange.Add(controlViewModel.Block.Form);
            }

            private void ShowControl(IControlViewModel controlViewModel)
            {
                controlViewModel.ControlVisibility = System.Windows.Visibility.Visible;
                formsForRearange.Add(controlViewModel.Block.Form);
            }

            private void HideBlock(IBlockViewModel blockViewModel)
            {
                blockViewModel.BlockVisibility = System.Windows.Visibility.Collapsed;
                formsForRearange.Add(blockViewModel.Form);
            }

            private void ShowBlock(IBlockViewModel blockViewModel)
            {
                blockViewModel.BlockVisibility = System.Windows.Visibility.Visible;
                formsForRearange.Add(blockViewModel.Form);
            }

            private async Task HideFormAsync(ICardModel model, IFormViewModel formViewModel)
            {
                if (formViewModel.CardTypeForm.IsTopLevelForm())
                {
                    model.Forms.Remove(formViewModel);
                }
                else
                {
                    await formViewModel.CloseAsync();
                }
            }

            private void MakeControlMandatory(IControlViewModel controlViewModel)
            {
                controlViewModel.IsRequired = true;
                controlViewModel.RequiredText = LocalizationManager.Format("$KrPermissions_MandatoryControlTemplate", controlViewModel.Caption);
            }

            private bool? CheckIsHidden(
                List<VisibilitySetting> settings,
                string checkName)
            {
                if (settings != null)
                {
                    foreach (var setting in settings)
                    {
                        if (setting.IsPattern)
                        {
                            if (Regex.IsMatch(checkName, setting.Text))
                            {
                                return setting.IsHidden;
                            }
                        }
                        else
                        {
                            if (string.Equals(checkName, setting.Text, StringComparison.OrdinalIgnoreCase))
                            {
                                return setting.IsHidden;
                            }
                        }
                    }
                }

                return null;
            }

            #endregion
        }

        #endregion

        #region Base Overrides

        public override async Task Initialized(ICardUIExtensionContext context)
        {
            var token = KrToken.TryGet(context.Card.Info);

            // Если не используется типовое решение
            if (token == null
                || token.ExtendedCardSettings == null)
            {
                return;
            }
            var cardControlsVisitor = new PermissionsControlsVisitor();

            // Набор визиторов по гридам
            Dictionary<GridViewModel, PermissionsControlsVisitor> visitors = new Dictionary<GridViewModel, PermissionsControlsVisitor>();
            // Стек текущих открытых таблиц
            Stack<GridViewModel> parentGridStack = new Stack<GridViewModel>();

            // Инициализация видимости контролов по визитору
            async Task InitModelAsync(ICardModel initModel, PermissionsControlsVisitor visitor)
            {
                await visitor.VisitAsync(initModel, parentGridStack);

                foreach (var control in initModel.ControlBag)
                {
                    if (control is GridViewModel grid)
                    {
                        visitors[grid] = visitor;
                        grid.RowInitializing += RowInitializaing;
                        grid.RowEditorClosed += RowClosed;
                    }
                }
            }

            // Инициализация вдимости контролов по визитору при открытии строки таблицы
            async void RowInitializaing(object sender, GridRowEventArgs e)
            {
                try
                {
                    using (e.Defer())
                    {
                        if (e.RowModel != null)
                        {
                            parentGridStack.Push(e.Control);
                            await InitModelAsync(e.RowModel, visitors[(GridViewModel)sender]);
                        }
                    }
                }
                catch(Exception ex)
                {
                    TessaDialog.ShowException(ex);
                }
            }

            // Отписка от созданных подписок при закрытии строки грида
            void RowClosed(object sender, GridRowEventArgs e)
            {
                if (e.RowModel != null)
                {
                    parentGridStack.Pop();
                    foreach (var control in e.RowModel.ControlBag)
                    {
                        if (control is GridViewModel grid)
                        {
                            visitors.Remove(grid);
                            grid.RowInitializing -= RowInitializaing;
                            grid.RowEditorClosed -= RowClosed;
                        }
                    }
                }
            }

            var extendedSettings = token.ExtendedCardSettings;
            var sectionSettings = extendedSettings.GetCardSettings();
            var tasksSettings = extendedSettings.GetTasksSettings();
            var visibilitySettings = extendedSettings.GetVisibilitySettings();

            var model = context.Model;

            if ((sectionSettings?.Count ?? 0) > 0
                || (visibilitySettings?.Count ?? 0) > 0
                || (tasksSettings?.Count ?? 0) > 0)
            {
                var uiVisibilitySettings = new VisibilitySettings();
                uiVisibilitySettings.Fill(visibilitySettings);
                cardControlsVisitor.Fill(sectionSettings, uiVisibilitySettings);

                await InitModelAsync(model, cardControlsVisitor);
                if (tasksSettings != null
                    && tasksSettings.Count > 0)
                {
                    await model.ModifyTasksAsync(async (tvm, m) =>
                    {
                        if (!tasksSettings.TryGetValue(tvm.TaskModel.CardTask.TypeID, out var taskSettings))
                        {
                            return;
                        }

                        var taskVisitor = new PermissionsControlsVisitor();
                        taskVisitor.Fill(taskSettings, uiVisibilitySettings);

                        await tvm.ModifyWorkspaceAsync(async (t, b) =>
                        {
                            await InitModelAsync(t.TaskModel, taskVisitor);
                        });
                    });
                }
            }
        }

        #endregion
    }
}
