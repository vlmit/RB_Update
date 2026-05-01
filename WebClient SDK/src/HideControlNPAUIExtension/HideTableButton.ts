import { CardRowsListener } from 'tessa/cards';
import { DotNetType, Guid, Visibility } from 'tessa/platform';
import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
import { GridViewModel } from 'tessa/ui/cards/controls';

export class HideTableButton extends CardUIExtension {
  private _listener: CardRowsListener | null;
  public initialized(_context: ICardUIExtensionContext): void {
    if (!Guid.equals(_context.card.typeId, '4e8d39ac-381e-4111-bc3a-66179ace7ecb')) return;

    if (this._listener) {
      this._listener.stop();
    }

    const approvalStagesTable = _context.model.controls.get('TableApprovalStages') as GridViewModel;
    const section = _context.card.sections.get('ApprovalStages');

    if (!section || !approvalStagesTable) return;

    if (approvalStagesTable.rows.length > 0) {
      approvalStagesTable.addButton.setVisibility(Visibility.Collapsed);
    }

    this._listener = new CardRowsListener();

    this._listener.rowDeleted.add(() => {
      if (approvalStagesTable.rows.length === 0) {
        approvalStagesTable.addButton.setVisibility(Visibility.Visible);
      } else {
        approvalStagesTable.addButton.setVisibility(Visibility.Collapsed);
      }
    });

    this._listener.rowInserted.add((_, row) => {
      approvalStagesTable.addButton.setVisibility(Visibility.Collapsed);

      const sectionDCI = _context.card.sections.get('DocumentCommonInfo');
      const fieldUrgencyNPAName = sectionDCI?.fields.get('UrgencyNPAName');
      const planedDate = Date.now();
      const newDatet = new Date(planedDate);

      if (fieldUrgencyNPAName === 'Срочно') {
        newDatet.setUTCDate(newDatet.getUTCDate() + 3);
        row.set('Planned', newDatet.toISOString(), DotNetType.DateTime);
        row.set('TimeLimit', null);
      } else if (fieldUrgencyNPAName === 'Весьма срочно') {
        newDatet.setUTCDate(newDatet.getUTCDate() + 2);
        row.set('Planned', newDatet.toISOString(), DotNetType.DateTime);
        row.set('TimeLimit', null);
      } else {
        row.set('Planned', null);
        row.set('TimeLimit', 3, DotNetType.Int32);
      }
    });

    this._listener.start(section.rows);
  }

  public finalized() {
    if (this._listener) {
      this._listener.stop();
      this._listener = null;
    }
  }
}
