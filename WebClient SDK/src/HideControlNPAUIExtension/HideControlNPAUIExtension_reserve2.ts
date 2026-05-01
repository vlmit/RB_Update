import { Guid, Visibility } from 'tessa/platform';
import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
import { GridViewModel } from 'tessa/ui/cards/controls';

export class HideControlNPAUIExtension_reserve_2 extends CardUIExtension {
  public initialized(_context: ICardUIExtensionContext): void {
    if (!Guid.equals(_context.card.typeId, '4e8d39ac-381e-4111-bc3a-66179ace7ecb')) return;

    const section = _context.card.sections.get('DocumentCommonInfo');

    const fieldUrgencyNPAName = section?.fields.get('UrgencyNPAName');

    const controlsArr = ['TimeLimitKrCard', 'PlannedKrCard'];

    const approvalStagesTable = _context.model.controls.get('TableApprovalStages') as GridViewModel;
    if (!approvalStagesTable) {
      return;
    }

    if (fieldUrgencyNPAName === 'Срочно' || fieldUrgencyNPAName === 'Весьма срочно') {
      this.hideControlsArrayInDialog(approvalStagesTable, controlsArr);
    }

    section?.fields.fieldChanged.add((e) => {
      if (e.fieldName != 'UrgencyNPAName') return;
      if (e.fieldValue === 'Срочно' || e.fieldValue === 'Весьма срочно') {
        this.hideControlsArrayInDialog(approvalStagesTable, controlsArr);
      } else {
        this.addVisibleControlsArrayInDialog(approvalStagesTable, controlsArr);
      }
    });
  }

  public hideControlsArrayInDialog(approvalStagesTable: GridViewModel, controlsAllias: string[]) {
    approvalStagesTable.rowInitializing.addWithDispose((e) => {
      controlsAllias.forEach((controlAllias) => {
        if (!e.rowModel) return;
        const collapsedControl = e.rowModel.controls.get(controlAllias);
        if (collapsedControl) collapsedControl.controlVisibility = Visibility.Collapsed;
      });
    });
  }

  public addVisibleControlsArrayInDialog(
    approvalStagesTable: GridViewModel,
    controlsAllias: string[]
  ) {
    approvalStagesTable.rowInitializing.addWithDispose((e) => {
      controlsAllias.forEach((controlAllias) => {
        if (!e.rowModel) return;
        const collapsedControl = e.rowModel.controls.get(controlAllias);
        if (collapsedControl) collapsedControl.controlVisibility = Visibility.Visible;
      });
    });
  }
}
