import { CardUIExtension, ICardUIExtensionContext, CardToolbarAction } from 'tessa/ui/cards';
import { getTessaIcon } from 'common/utility';
import { UIContext } from 'tessa/ui';
import { Guid } from 'tessa/platform';


/**
 * Добавить кнопку "На главную" для web-клиента при запуске с мобильного устройства
 * - при нажатии закрывает карточку, возвращаясь на РМ пользователя
 */
 export class ODCloseCardUIExtension extends CardUIExtension {
  //private _disposer: Function | null = null;

  public initialized(context: ICardUIExtensionContext) {
    // если карточка не Протокол, то ничего не делаем
    if (!Guid.equals(context.card.typeId, 'de076ddc-70aa-4a4e-abbc-53b87567dc06') // исходящие
    && !Guid.equals(context.card.typeId, 'ed6d5ee1-9075-4ce0-bdae-76afe87544f2') // входящие
    && !Guid.equals(context.card.typeId, 'b8066da5-d9a6-4e14-a415-0422e36b4c04')// входящие АПиП
    && !Guid.equals(context.card.typeId, '88e3ed72-100c-4fc8-8527-6558623aa0f7') // исходящий АПиП
    && !Guid.equals(context.card.typeId, 'aa81c9f6-2326-44d6-b511-461d8eb8ea7b')// внутренний АПиП
    ) {
      return;
    }
   
     // Добавляем кнопку закрытия карточки.
     if (context.toolbar) {
      
      context.toolbar.removeItemIfExists('ToMainMenuButton');
      context.toolbar.addItem(
        new CardToolbarAction({
          name: 'ToMainMenuButton',
          caption: 'На главную',
          icon: getTessaIcon('Thin119'),
          command: async _ => {
            const editor = UIContext.current.cardEditor;
            if (!editor
            ) {
              return;
            }
           // await editor.saveCard();
            await editor.close(true);
          }
        })
      );
    }
     
  }
 
 
}
