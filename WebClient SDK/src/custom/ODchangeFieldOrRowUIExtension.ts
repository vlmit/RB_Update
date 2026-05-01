import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
import { Guid, DotNetType } from 'tessa/platform';
import { CardRowState, CardRow } from 'tessa/cards';

/**
 * В зависимости от значения флажка на форме карточки менять другие поля карточки:
 * - в текстовое поле
 * - добавлять строчку в коллекционную секцию
 *
 * Когда жмакают галку "Базовый цвет", то:
 * - если галка установлена, то значение поля "Цвет" меняется на "Это базовый цвет"
 * - если галка установлена, то в секцию "Исполнители" добавляется новый сотрудник "Admin"
 * - если галка не установлена, то занчение поля "Цвет" меняется на "Это другой, не базовый цвет"
 */
export class ChangeFieldOrRowUIExtension extends CardUIExtension {

  public initialized(context: ICardUIExtensionContext) {
    // если карточка не для тестов, то ничего не делаем
    if (!Guid.equals(context.card.typeId, 'd0006e40-a342-4797-8d77-6501c4b7c4ac')) {
      return;
    }

    // находим секции
    const additionalInfo = context.card.sections.tryGet('TEST_CarAdditionalInfo');
    const performers = context.card.sections.tryGet('Performers');
    if (!additionalInfo
      || !performers
    ) {
      return;
    }

    // подписываемся на изменения полей в секции
    additionalInfo.fields.fieldChanged.add(e => {
      // если было изменено другое поле, то никак не реагируем
      if (e.fieldName !== 'IsBaseColor') {
        return;
      }

      const text = e.fieldValue ? 'Это базовый цвет' : 'Это другой, не базовый цвет';
      // ставим значения поля в секции
      additionalInfo.fields.set('Color', text, DotNetType.String);

      if (e.fieldValue) {
        // добавляем новую строчку в табличную секцию
        const newRow = new CardRow();
        newRow.rowId = Guid.newGuid();
        newRow.set('UserID', '3db19fa0-228a-497f-873a-0250bf0a4ccb', DotNetType.Guid);
        newRow.set('UserName', 'Admin', DotNetType.String);
        newRow.state = CardRowState.Inserted;
        performers.rows.push(newRow);
      }
    });
  }

}