import { DotNetType, Guid, Visibility } from 'tessa/platform';
import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
import { GridViewModel } from 'tessa/ui/cards/controls';

export class HideControlNPAUIExtension000 extends CardUIExtension {
  public initialized(_context: ICardUIExtensionContext): void {
    if (!Guid.equals(_context.card.typeId, '4e8d39ac-381e-4111-bc3a-66179ace7ecb')) return;

    const sectionDCI = _context.card.sections.get('DocumentCommonInfo');
    const fieldUrgencyNPAName = sectionDCI?.fields.get('UrgencyNPAName');
    const controlsArr = ['TimeLimitKrCard', 'PlannedKrCard'];
    const approvalStagesTable = _context.model.controls.get('TableApprovalStages') as GridViewModel;

    if (!approvalStagesTable) {
      return;
    }

    if (fieldUrgencyNPAName === 'Срочно' || fieldUrgencyNPAName === 'Весьма срочно (2 календарных дня)') {
      this.hideControlsArrayInDialog(approvalStagesTable, controlsArr);
    }

    sectionDCI?.fields.fieldChanged.add((e) => {
      if (e.fieldName != 'UrgencyNPAName') return;
      if (e.fieldValue === 'Срочно (3 календарных дня)' || e.fieldValue === 'Весьма срочно (2 календарных дня)') {
        this.hideControlsArrayInDialog(approvalStagesTable, controlsArr);
      } else {
        this.addVisibleControlsArrayInDialog(approvalStagesTable, controlsArr);
      }

      const rows = _context.card.sections.get('ApprovalStages')?.rows.filter((r) => r.state != 3);
      if (!rows) return;
      const planedDate = Date.now();
      const newDate = new Date(planedDate);
      for (let row of rows) {
        if (e.fieldValue === 'Срочно (3 календарных дня)') {
          newDate.setUTCDate(newDate.getUTCDate() + 3);
          row.set('Planned', newDate.toISOString(), DotNetType.DateTime);
          row.set('TimeLimit', null);
        } else if (e.fieldValue === 'Весьма срочно (2 календарных дня)') {
          newDate.setUTCDate(newDate.getUTCDate() + 2);
          row.set('Planned', newDate.toISOString(), DotNetType.DateTime);
          row.set('TimeLimit', null);
        } else {
          row.set('Planned', null);
          row.set('TimeLimit', 3, DotNetType.Int32);
        }
      }
    });

    if (approvalStagesTable.rows.length > 0) {
      approvalStagesTable.addButton.setVisibility(Visibility.Collapsed);
    }
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
