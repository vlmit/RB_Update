import { UIContext, MenuAction, showLoadingOverlay,showNotEmpty, showMessage } from 'tessa/ui';
import { CardRequest, CardService } from 'tessa/cards/service';
import { Card } from 'tessa/cards';
import { DotNetType, Guid, createTypedField } from 'tessa/platform';
import { FileExtension, IFileExtensionContext } from 'tessa/ui/files';



export class ODIncomingStamp extends FileExtension {
  public async openingMenu(context: IFileExtensionContext) {
    //let uiContext = UIContext.current;
    //let editor = uiContext.cardEditor;
    // если это не карточка сотрудника, то ничего не делаем

    let card: Card = context.control.model.card;
    if (
      Guid.equals(card.typeName, 'Outgoing') || Guid.equals(card.typeName, 'Outgoing_ACGP') ||
      Guid.equals(card.typeName, 'Outgoing OP1') || Guid.equals(card.typeName, 'Outgoing OP2') ||
      Guid.equals(card.typeName, 'Outgoing OP3') || Guid.equals(card.typeName, 'Outgoing OP4') ||
      Guid.equals(card.typeName, 'Outgoing OP5') || Guid.equals(card.typeName, 'Protocol') ||
      Guid.equals(card.typeName, 'ORD') || Guid.equals(card.typeName, 'Order OP1') ||
      Guid.equals(card.typeName, 'Order OP2') || Guid.equals(card.typeName, 'Order OP3') ||
      Guid.equals(card.typeName, 'Order OP4') || Guid.equals(card.typeName, 'Order OP5') || Guid.equals(card.typeName, 'Attorney')
      //добавлениея 11_10_2023
      ||Guid.equals(card.typeName, 'ORD LS') || Guid.equals(card.typeName, 'Outgoing_OS')
      //|| Guid.equals(card.typeName, 'RB_Outgoing_AGIP')  || Guid.equals(card.typeName, 'RB_Internal_BurPrirodNadzor')
      //|| Guid.equals(card.typeName, 'RB_Internal_AGIP')  || Guid.equals(card.typeName, 'RB_Internal')
        ) {
      return;
    }
    const file = context.file.model;
    
    const editCollapsed = !context.file.model.permissions.canEdit;
    // Добавляем пункт меню для получения файлов
    
    context.actions.push(
        new MenuAction(
          'SignAll',
          'Проставить регистрационный штамп',
          "ta icon-thin-254",
          null,
          [
            new MenuAction(
            'SignAll',
            'Проставить регистрационный штамп',
            "ta icon-thin-254",
            async () => {
              const editor = UIContext.current.cardEditor;
              if (!editor
              ) {
                return;
              }

              
              if (file.lastVersion.state == 0 ) {
                  showMessage('Файл:  добавлен в карточку, но не сохранен. Сохраните карточку перед нанесением штампа.', 'Нанесение штампа на pdf');
                  return;
              }
    
              showLoadingOverlay(async (splashResolve) => {
                //await editor.saveCard();
                let getIncomingStampRequest = new CardRequest();
                getIncomingStampRequest.requestType = "a524840f-906b-48ae-9933-4e375ef2d296";file.lastVersion
                getIncomingStampRequest.info["cardID"] = createTypedField(card.id, DotNetType.Guid);
                getIncomingStampRequest.info["versionRowID"] = createTypedField(file.lastVersion.id, DotNetType.Guid);
                getIncomingStampRequest.info["fileName"] = createTypedField(file.name, DotNetType.String);
                getIncomingStampRequest.info["Mode"] = createTypedField("vertical", DotNetType.String);
                
                let getSignaturesResponse = await CardService.instance.request(getIncomingStampRequest);
                if (!getSignaturesResponse.validationResult.isSuccessful) {
                  await showNotEmpty(getSignaturesResponse.validationResult.build());
                  showMessage('Ошибка получения штампа, сохраните карточку и попробуйте снова!');
                 
                  return;
                }
                await editor.saveCard();
               
                splashResolve;
              });
            },
            null
        ),
        new MenuAction(
          'SignAll',
          'Проставить регистрационный штамп (горизонтальный бланк)',
          "ta icon-thin-254",
          async () => {
            const editor = UIContext.current.cardEditor;
            if (!editor
            ) {
              return;
            }

            
            if (file.lastVersion.state == 0 ) {
                showMessage('Файл:  добавлен в карточку, но не сохранен. Сохраните карточку перед нанесением штампа.', 'Нанесение штампа на pdf');
                return;
            }
  
            showLoadingOverlay(async (splashResolve) => {
              //await editor.saveCard();
              let getIncomingStampRequest = new CardRequest();
              getIncomingStampRequest.requestType = "a524840f-906b-48ae-9933-4e375ef2d296";file.lastVersion
              getIncomingStampRequest.info["cardID"] = createTypedField(card.id, DotNetType.Guid);
              getIncomingStampRequest.info["versionRowID"] = createTypedField(file.lastVersion.id, DotNetType.Guid);
              getIncomingStampRequest.info["fileName"] = createTypedField(file.name, DotNetType.String);
              getIncomingStampRequest.info["Mode"] = createTypedField("horizontal", DotNetType.String);
              
              let getSignaturesResponse = await CardService.instance.request(getIncomingStampRequest);
              if (!getSignaturesResponse.validationResult.isSuccessful) {
                await showNotEmpty(getSignaturesResponse.validationResult.build());
                showMessage('Ошибка получения штампа, сохраните карточку и попробуйте снова!');
               
                return;
              }
              await editor.saveCard();
             
              splashResolve;
            });
          },
          null
      )   
      ],
          context.files.length > 2 || file.getExtension() !== 'pdf' || editCollapsed
      ));
  }

  
}