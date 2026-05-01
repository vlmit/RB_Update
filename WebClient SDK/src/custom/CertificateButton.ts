import { CardUIExtension, ICardUIExtensionContext, CardToolbarAction } from 'tessa/ui/cards';
import { getTessaIcon } from 'common/utility';
import { UIContext } from 'tessa/ui';
import { Guid, DotNetType } from 'tessa/platform';
// import { IFile, createFakeFile } from 'tessa/files';
//import { CertData } from 'tessa/files';
import { selectFileCerts } from 'tessa/ui/cards/controls';
import { CardService, CardStoreRequest } from 'tessa/cards/service';
import { showNotEmpty } from 'tessa/ui';
import { userSession } from 'common/utility';
//import { FileContainer } from 'tessa/files';
//import { userSession } from 'common/utility';



/*
 * Добавить кнопку в верхем меню табов "Обновить сертификат" для web-клиента - показывает окно выбора сертификата, сохраняет выбранный сертификат в профиль.
 */
 export class ODTestButton extends CardUIExtension {
  //private _disposer: Function | null = null;

  public initialized(context: ICardUIExtensionContext) {
    // если это не карточка сотрудника, то ничего не делаем
    if (
   // !Guid.equals(context.card.typeId, '929ad23c-8a22-09aa-9000-398bf13979b2')
   // && 
    !Guid.equals(context.card.id, userSession.UserID)
    ) {
      return;
    }
   
     // Добавляем кнопку
    if (context.toolbar) {
      
      context.toolbar.removeItemIfExists('UpdateCertBtn');
      context.toolbar.addItem(
        new CardToolbarAction({
          name: 'UpdateCertBtn',
          caption: 'Установить сертификат',
          icon: getTessaIcon('icon-thin-005'),
          command: async _ => {
            const editor = UIContext.current.cardEditor;
            if (!editor
            ) {
              return;
            }
            
            const result = await  selectFileCerts(context.model.edsProvider);
            if (result.cancel || !result.cert) {
              alert ('Not found certs')
              return;
            }
            const cert = result.cert;
            //alert(JSON.stringify(cert));            
            
            const CertInfo = context.card.sections.tryGet('PersonalRoles');
            if (!CertInfo ) {
              return;
            }
            CertInfo.fields.set('EDSCertificateInfo', JSON.stringify(cert), DotNetType.String);
            //const saved = await editor.saveCard(context);
            //if (!saved) {
            //  return;
           // }
           const storeRequest = new CardStoreRequest();
           storeRequest.card = context.card;
           //storeRequest.forceTransaction = true;
           //storeRequest.forceReleaseLock = true;
           //storeRequest.removeUserInfo();
           const storeResponse = await CardService.instance.store(storeRequest)//, fileContainer.files);
           if (!storeResponse.validationResult.build().isSuccessful) {
             await showNotEmpty(storeResponse.validationResult.build());
             return;
           }

           editor.refreshCard();
           
          }
        })
      );
    }
     
  }
 
 
}
