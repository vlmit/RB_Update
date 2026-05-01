import { CardUIExtension, ICardUIExtensionContext, CardToolbarAction } from 'tessa/ui/cards';
import { getTessaIcon } from 'common/utility';
import { UIContext, showLoadingOverlay, showMessage } from 'tessa/ui';
import { DotNetType, createTypedField } from 'tessa/platform';
import { CardRequest, CardService } from 'tessa/cards/service';

/*
 * Добавить кнопку в верхем меню табов "Обновить сертификат" для web-клиента - показывает окно выбора сертификата, сохраняет выбранный сертификат в профиль.
 */
  export class MEDOPreviewBtn extends CardUIExtension {
  //private _disposer: Function | null = null;

  public initialized(context: ICardUIExtensionContext) {
    // если это не карточка сотрудника, то ничего не делаем
    //if (!Guid.equals(context.card.typeId, '929ad23c-8a22-09aa-9000-398bf13979b2')) {
    //  return;
    //}
    
     // Добавляем кнопку
    if (context.toolbar) {
      
      context.toolbar.removeItemIfExists('MEDOPreviewBtn');
      context.toolbar.addItem(
        new CardToolbarAction({
          name: 'MEDOPreviewBtn',
          caption: 'Нанести штамп',
          icon: getTessaIcon('icon-thin-005'),
          command: async _ => {
            const editor = UIContext.current.cardEditor;
            if (!editor
            ) {
              return;
            }
            await editor.saveCard();
            
            showLoadingOverlay(async (splashResolve) => 
            {
              let previewRequest = new CardRequest();
              previewRequest.requestType = "04643995-3ff1-477e-b683-ec65a9021c4e";
              previewRequest.info["cardID"] = createTypedField(context.card.id, DotNetType.Guid);
              
              let previewResponse = await CardService.instance.request(previewRequest);
              if (!previewResponse.validationResult.isSuccessful)
              {
                  showMessage('Файл предпросмотра не прикреплен к карточке');
                      return;
              }
              splashResolve;
            }); 
          }
        })
      );
    }
  }
}