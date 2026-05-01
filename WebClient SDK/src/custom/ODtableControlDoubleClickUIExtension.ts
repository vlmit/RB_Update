import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
import { Guid } from 'tessa/platform';
import { GridViewModel } from 'tessa/ui/cards/controls';

export class TableControlDoubleClickUIExtension extends CardUIExtension {

  public initialized(context: ICardUIExtensionContext) {
    // если не карточка "автомобиль", то ничего не делаем
    if (!Guid.equals(context.card.typeId, 'd0006e40-a342-4797-8d77-6501c4b7c4ac')) {
      return;
    }

    const tableControl = context.model.controls.get('ShareList') as GridViewModel;
    if (!tableControl) {
      return;
    }

    // подписываемся на открытие строки
    tableControl.rowInitializing.add(e => console.log(e));
  }

}