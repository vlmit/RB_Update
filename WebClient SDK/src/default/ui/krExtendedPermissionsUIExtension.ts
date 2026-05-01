import {
  CardUIExtension,
  IBlockViewModel,
  ICardModel,
  ICardUIExtensionContext,
  IControlViewModel,
  IFormViewModel
} from 'tessa/ui/cards';
import {
  ControlType,
  KrPermissionSectionSettings,
  KrPermissionVisibilitySettings,
  KrToken
} from 'tessa/workflow';
import { GridRowEventArgs, GridViewModel } from 'tessa/ui/cards/controls';
import { isTopLevelForm, showNotEmpty } from 'tessa/ui';
import { ValidationResult } from 'tessa/platform/validation';
import { TaskViewModel } from 'tessa/ui/cards/tasks';
import { unseal, Visibility } from 'tessa/platform';
import { LocalizationManager } from 'tessa/localization';
import { DefaultFormTabWithTasksViewModel } from 'tessa/ui/cards/forms';
import { CardRowState } from 'tessa/cards';

class VisibilitySetting {
  public text: string;
  public isHidden: boolean;
  public isPattern: boolean;

  constructor(text: string, isHidden: boolean, isPattern: boolean) {
    this.text = text;
    this.isHidden = isHidden;
    this.isPattern = isPattern;
  }
}

class VisibilitySettings {
  public blockSettings: Array<VisibilitySetting> = [];
  public controlSettings: Array<VisibilitySetting> = [];
  public tabSettings: Array<VisibilitySetting> = [];

  public fill(visibilitySettings: KrPermissionVisibilitySettings[]) {
    for (const visibilitySetting of visibilitySettings) {
      let patternList: Array<VisibilitySetting>;
      switch (visibilitySetting.controlType) {
        case ControlType.Tab:
          patternList = this.tabSettings;
          break;

        case ControlType.Block:
          patternList = this.blockSettings;
          break;

        case ControlType.Control:
          patternList = this.controlSettings;
          break;

        default:
          continue;
      }
      this.fillSettings(patternList, visibilitySetting);
    }
  }

  private fillSettings(
    patternList: Array<VisibilitySetting>,
    visibilitySetting: KrPermissionVisibilitySettings
  ) {
    const alias: string = visibilitySetting.alias;
    const length: number = alias.length;
    const wildStart: boolean = alias[0] === '*';
    const wildEnd: boolean = alias[length - 1] === '*';
    if (wildStart || wildEnd) {
      const startIndex = wildStart ? 1 : 0;
      const endIndex = Math.max(0, length - (wildStart ? 1 : 0) - (wildEnd ? 1 : 0));
      const escapedAlias =
        // TODO: escape
        // endIndex + 1: substring возвращает подстроку от startIndex до endIndex не включительно (+1 для того чтобы включить последний элемент)
        alias.substring(startIndex, endIndex + 1);
      patternList.push(new VisibilitySetting(escapedAlias, visibilitySetting.isHidden, true));
    } else {
      patternList.push(new VisibilitySetting(alias, visibilitySetting.isHidden, false));
    }
  }
}

class PermissionsControlsVisitor {
  public hideSections: Set<guid> = new Set();
  public hideFields: Set<guid> = new Set();
  public showFields: Set<guid> = new Set();
  public mandatorySections: Set<guid> = new Set();
  public mandatoryFields: Set<guid> = new Set();
  public disallowedSections = new Set();
  private _visibilitySettings: VisibilitySettings;

  public visit(cardModel: ICardModel, parentGrids: Array<GridViewModel>) {
    let disableGrids = false;
    if (cardModel.table && cardModel.table.row.state !== CardRowState.Inserted) {
      for (const grid of parentGrids) {
        // Если хотя бы одна из родительских таблиц недоступна для редактирования через KrPermissions, то и все таблицы текущей строки должны быть недоступны.
        const parentGridSource = grid.cardTypeControl.getSourceInfo();
        if (this.disallowedSections.has(parentGridSource.sectionId)) {
          disableGrids = true;
          break;
        }
      }
    }
    for (const control of cardModel.controlsBag) {
      this.visitControl(control);
      let grid: GridViewModel | null = null;
      if (control instanceof GridViewModel) {
        grid = control as GridViewModel;
      }
      if (disableGrids && grid) {
        grid.isReadOnly = true;
      }
    }

    for (const block of cardModel.blocksBag) {
      this.visitBlock(block);
    }

    for (const form of cardModel.formsBag) {
      this.visitForm(cardModel, form);
    }
  }

  public fill(
    sectionSettings: KrPermissionSectionSettings[],
    visibilitySettings: VisibilitySettings
  ) {
    for (const sectionSetting of sectionSettings) {
      if (sectionSetting.isHidden) {
        this.hideSections.add(sectionSetting.id);
      } else {
        sectionSetting.hiddenFields.forEach(x => this.hideFields.add(x));
      }

      sectionSetting.visibleFields.forEach(x => this.showFields.add(x));

      if (sectionSetting.isMandatory) {
        this.mandatorySections.add(sectionSetting.id);
      } else {
        sectionSetting.mandatoryFields.forEach(x => this.mandatoryFields.add(x));
      }
      if (sectionSetting.isDisallowed) {
        this.disallowedSections.add(sectionSetting.id);
      }
    }

    this._visibilitySettings = visibilitySettings;
  }

  private visitForm(model: ICardModel, form: IFormViewModel) {
    if (form.name) {
      const hidden: boolean | null = this.checkIsHidden(
        this._visibilitySettings.tabSettings,
        form.name
      );
      if (hidden) {
        this.hideForm(model, form);
      }
      // Нет возможности добавлять скрытую форму, т.к. она не была сгенерирована
    }
  }

  private visitBlock(block: IBlockViewModel) {
    if (block.name) {
      const hidden: boolean | null = this.checkIsHidden(
        this._visibilitySettings.blockSettings,
        block.name
      );
      if (hidden !== null) {
        if (hidden) {
          this.hideBlock(block);
        } else {
          this.showBlock(block);
        }
      }
    }
  }

  private visitControl(control: IControlViewModel) {
    const sourceInfo = control.cardTypeControl.getSourceInfo();
    if (
      (this.hideSections.has(sourceInfo.sectionId) &&
        !sourceInfo.columnIds.some(x => this.showFields.has(x))) ||
      sourceInfo.columnIds.some(x => this.hideFields.has(x))
    ) {
      this.hideControl(control);
    }
    if (control.name) {
      const hidden: boolean | null = this.checkIsHidden(
        this._visibilitySettings.controlSettings,
        control.name
      );
      if (hidden !== null) {
        if (hidden) {
          this.hideControl(control);
        } else {
          this.showControl(control);
        }
      }
    }

    if (
      this.mandatorySections.has(sourceInfo.sectionId) ||
      sourceInfo.columnIds.some(x => this.mandatoryFields.has(x))
    ) {
      this.makeControlMandatory(control);
    }
  }

  private hideControl(control: IControlViewModel) {
    control.controlVisibility = Visibility.Collapsed;
  }

  private showControl(control: IControlViewModel) {
    control.controlVisibility = Visibility.Visible;
  }

  private hideBlock(block: IBlockViewModel) {
    block.blockVisibility = Visibility.Collapsed;
  }

  private showBlock(block: IBlockViewModel) {
    block.blockVisibility = Visibility.Visible;
  }

  private hideForm(_model: ICardModel, formViewModel: IFormViewModel) {
    if (isTopLevelForm(unseal(formViewModel.cardTypeForm))) {
      formViewModel.isCollapsed = true;
    } else {
      formViewModel.close();
    }
  }

  private makeControlMandatory(controlViewModel: IControlViewModel) {
    controlViewModel.isRequired = true;
    controlViewModel.requiredText = LocalizationManager.instance.format(
      '$KrPermissions_MandatoryControlTemplate',
      controlViewModel.caption
    );
  }

  private checkIsHidden(settings: Array<VisibilitySetting>, checkName: string): boolean | null {
    if (settings && settings.length > 0) {
      for (const setting of settings) {
        if (setting.isPattern) {
          if (checkName.match(setting.text)) {
            return setting.isHidden;
          }
        } else {
          if (setting.text.toLowerCase() === checkName.toLowerCase()) {
            return setting.isHidden;
          }
        }
      }
    }
    return null;
  }
}

export class KrExtendedPermissionsUIExtension extends CardUIExtension {
  public initialized(context: ICardUIExtensionContext) {
    const token = KrToken.tryGet(context.card.info);
    // Если не используется типовое решение
    if (!token || !token.extendedCardSettings) {
      return;
    }

    const cardControlsVisitor = new PermissionsControlsVisitor();

    // Набор визиторов по гридам
    const visitors = new Map<GridViewModel, PermissionsControlsVisitor>();
    // Стек текущих открытых таблиц
    const parentGridStack: Array<GridViewModel> = new Array<GridViewModel>();

    // Инициализация видимости контролов по визитору
    const initModel = (initModel: ICardModel, visitor: PermissionsControlsVisitor) => {
      visitor.visit(initModel, parentGridStack);
      for (const control of initModel.controlsBag) {
        if (control instanceof GridViewModel) {
          visitors.set(control, visitor);
          control.rowInitializing.add(rowInitializaing);
          control.rowEditorClosed.add(rowClosed);
        }
      }
    };

    // Инициализация вдимости контролов по визитору при открытии строки таблицы
    const rowInitializaing = (e: GridRowEventArgs) => {
      try {
        if (e.rowModel) {
          parentGridStack.push(e.control);
          initModel(e.rowModel, visitors.get(e.control)!);
        }
      } catch (e) {
        showNotEmpty(ValidationResult.fromError(e));
      }
    };

    // Отписка от созданных подписок при закрытии строки грида
    const rowClosed = (e: GridRowEventArgs) => {
      if (e.rowModel) {
        parentGridStack.pop();
        for (const control of e.rowModel.controlsBag) {
          if (control instanceof GridViewModel) {
            visitors.delete(control);
            control.rowInitializing.remove(rowInitializaing);
            control.rowEditorClosed.remove(rowClosed);
          }
        }
      }
    };

    const extendedSettings = token.extendedCardSettings;
    const sectionSettings = extendedSettings.getCardSettings();
    const tasksSettings = extendedSettings.getTaskSettings();
    const visibilitySettings = extendedSettings.getVisibilitySettings();

    const model = context.model;

    if (
      (sectionSettings && sectionSettings.length > 0) ||
      (tasksSettings && tasksSettings.size > 0) ||
      (visibilitySettings && visibilitySettings.length > 0)
    ) {
      const uiVisibilitySettings = new VisibilitySettings();
      uiVisibilitySettings.fill(visibilitySettings);
      cardControlsVisitor.fill(sectionSettings, uiVisibilitySettings);

      initModel(model, cardControlsVisitor);
      if (tasksSettings && tasksSettings.size > 0) {
        this.modifyTask(model, tvm => {
          const taskSettings = tasksSettings.get(tvm.taskModel.cardTask!.typeId);
          if (!taskSettings) {
            return;
          }

          const taskVisitor = new PermissionsControlsVisitor();
          taskVisitor.fill(taskSettings, uiVisibilitySettings);

          tvm.modifyWorkspace(e => {
            initModel(e.task.taskModel, taskVisitor);
          });
        });
      }
    }
  }

  private modifyTask(
    model: ICardModel,
    modifyAction: (tvm: TaskViewModel, m: ICardModel) => void
  ): boolean {
    if (!(model instanceof DefaultFormTabWithTasksViewModel)) {
      return false;
    }

    for (const task of model.tasks.filter(x => x instanceof TaskViewModel)) {
      modifyAction(task, model);
    }

    return true;
  }
}
