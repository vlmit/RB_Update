import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
import { Guid, DotNetType } from 'tessa/platform';
import { CardRowsListener } from 'tessa/cards';

/**
 * Отслеживать добавление\удаление строк в коллекционной секции
 *
 * Когда изменяется секция "Получатели", то в поле "Тема" добавляется строка.
 */
export class TableSectionChangedUIExtension extends CardUIExtension {

  private _listener: CardRowsListener | null;

  public initialized(context: ICardUIExtensionContext) {
    // если карточка не для тестов, то ничего не делаем
    if (!Guid.equals(context.card.typeId, 'd0006e40-a342-4797-8d77-6501c4b7c4ac')) {
      return;
    }

    const recipients = context.card.sections.tryGet('Recipients');
    const commonInfo = context.card.sections.tryGet('DocumentCommonInfo');
    if (!recipients
      || !commonInfo
    ) {
      return;
    }

    if (this._listener) {
      this._listener.stop();
    }

    // можно подписаться на изменения в коллекции через recipients.rows.collectionChanged.
    // в этом случае handler будет вызываться только когда элемент добавляется или удаляется из коллекции.
    // чтобы правильно обрабатывать изменения состояния строк лучше использовать хелперный класс CardRowsListener.
    // в этом случае handler будет вызываться когда состояния строк меняются.

    this._listener = new CardRowsListener();
    this._listener.rowInserted.add((_, row) => {
      let text: string = commonInfo.fields.tryGet('Subject') || '';
      text += `\nRow with id = "${row.rowId}" has been added.`;
      commonInfo.fields.set('Subject', text, DotNetType.String);
    });

    this._listener.rowDeleted.add((_, row) => {
      let text: string = commonInfo.fields.tryGet('Subject') || '';
      text += `\nRow with id = "${row.rowId}" has been deleted.`;
      commonInfo.fields.set('Subject', text, DotNetType.String);
    });

    this._listener.start(recipients.rows);
  }

  public finalized() {
    if (this._listener) {
      this._listener.stop();
      this._listener = null;
    }
  }

}