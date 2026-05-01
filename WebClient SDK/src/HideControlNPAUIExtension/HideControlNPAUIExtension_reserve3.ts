import { DotNetType, Guid, Visibility } from 'tessa/platform';
import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
import { GridViewModel } from 'tessa/ui/cards/controls';

export class HideControlNPAUIExtension_reserve3 extends CardUIExtension {
  public initialized(_context: ICardUIExtensionContext): void {
    if (!Guid.equals(_context.card.typeId, '4e8d39ac-381e-4111-bc3a-66179ace7ecb')) return;

    const section = _context.card.sections.get('DocumentCommonInfo');

    const fieldUrgencyNPAName = section?.fields.get('UrgencyNPAName');

    const controlsArr = ['TimeLimitKrCard', 'PlannedKrCard'];

    const approvalStagesTable = _context.model.controls.get('TableApprovalStages') as GridViewModel;

    const sectionApprovalStages = _context.card.sections.get('ApprovalStages');
    if (!approvalStagesTable || !sectionApprovalStages) {
      return;
    }

    if (fieldUrgencyNPAName === 'Срочно' || fieldUrgencyNPAName === 'Весьма срочно') {
      this.hideControlsArrayInDialog(approvalStagesTable, controlsArr);
    }

    const rows = sectionApprovalStages.rows;

    section?.fields.fieldChanged.add((e) => {
      if (e.fieldName != 'UrgencyNPAName') return;
      const planedDate = Date.now();
      const newDate = new Date(planedDate);
      if (e.fieldValue === 'Срочно' || e.fieldValue === 'Весьма срочно') {
        this.hideControlsArrayInDialog(approvalStagesTable, controlsArr);
      } else {
        this.addVisibleControlsArrayInDialog(approvalStagesTable, controlsArr);
      }
      for (let row of rows) {
        if (e.fieldValue === 'Срочно') {
          newDate.setUTCDate(newDate.getUTCDate() + 3);
          row.set('Planned', newDate.toISOString(), DotNetType.DateTime);
          row.set('TimeLimit', null);
        } else if (e.fieldValue === 'Весьма срочно') {
          newDate.setUTCDate(newDate.getUTCDate() + 2);
          row.set('Planned', newDate.toISOString(), DotNetType.DateTime);
          row.set('TimeLimit', null);
        } else {
          row.set('Planned', null);
          row.set('TimeLimit', 3, DotNetType.Int32);
        }
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
