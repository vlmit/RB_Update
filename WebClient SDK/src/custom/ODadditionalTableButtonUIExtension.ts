import { autorun } from 'mobx';
import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
import { Guid } from 'tessa/platform';
import {
  GridViewModel,
  AutoCompleteEntryViewModel,
  TextBoxViewModel
} from 'tessa/ui/cards/controls';
import { UIButton } from 'tessa/ui';
import { CardRowState } from 'tessa/cards';

/**
 * Добавляем дополнительную кнопку справа под контролом таблицы.
 * При нажатии на кнопку, мы копируем выбранную строку и добавляем в секцию.
 */
export class AdditionalTableButtonUIExtension extends CardUIExtension {
  private _disposer: Function | null = null;

  public initialized(context: ICardUIExtensionContext) {
    // если не карточка "автомобиль", то ничего не делаем
    if (!Guid.equals(context.card.typeId, 'd0006e40-a342-4797-8d77-6501c4b7c4ac')) {
      return;
    }

    const carNameControl = context.model.controls.get('CarName') as TextBoxViewModel;
    const driverNameControl = context.model.controls.get(
      'DriverName'
    ) as AutoCompleteEntryViewModel;
    if (carNameControl && driverNameControl) {
      carNameControl.keyDown.add(e => {
        if (e.event.keyCode === 13) {
          // enter
          console.log('Enter key was pressed');

          if (driverNameControl) {
            driverNameControl.focus();
          }
        }
      });
    }

    // пытаемся найти грид по алиасу
    const table = context.model.controls.get('ShareList') as GridViewModel;
    if (!table) {
      return;
    }

    // пытаемся найти секцию
    const section = context.card.sections.tryGet('TEST_CarSales');
    if (!section) {
      return;
    }

    // создаем кнопку копирования строки
    const copyButton = new UIButton('CopyButton', () => {
      // получаем текущую выбранную строку
      const selectedRow = table.selectedRow;
      if (!selectedRow) {
        return;
      }

      // копируем всем данные и добавляем в секцию
      const clonedRow = selectedRow.row.clone();
      clonedRow.rowId = Guid.newGuid();
      clonedRow.state = CardRowState.Inserted;
      section.rows.push(clonedRow);
    });

    // добавляем к кнопкам справа
    table.rightButtons.push(copyButton);

    // следим за selectedRow; когда значение меняется, то мы устанавливаем доступность кнопки
    this._disposer = autorun(() => copyButton.setIsEnabled(!!table.selectedRow));
  }

  public finalized() {
    // подчищаем за собой
    if (this._disposer) {
      this._disposer();
      this._disposer = null;
    }
  }
}
