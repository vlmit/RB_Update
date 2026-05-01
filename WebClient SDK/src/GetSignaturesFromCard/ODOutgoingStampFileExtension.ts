import { UIContext, MenuAction, showLoadingOverlay,showNotEmpty, showMessage } from 'tessa/ui';
import { CardRequest, CardService } from 'tessa/cards/service';
import { Card } from 'tessa/cards';
import { DotNetType, Guid, createTypedField } from 'tessa/platform';
import { FileExtension, IFileExtensionContext } from 'tessa/ui/files';



export class ODOutgoingStamp extends FileExtension {
  public async openingMenu(context: IFileExtensionContext) {
    //let uiContext = UIContext.current;
    //let editor = uiContext.cardEditor;
    let card: Card = context.control.model.card;
    if (
      !Guid.equals(card.typeName, 'Outgoing') && !Guid.equals(card.typeName, 'Outgoing_ACGP') &&
      !Guid.equals(card.typeName, 'Outgoing OP1') && !Guid.equals(card.typeName, 'Outgoing OP2') &&
      !Guid.equals(card.typeName, 'Outgoing OP3') && !Guid.equals(card.typeName, 'Outgoing OP4') &&
      !Guid.equals(card.typeName, 'Outgoing OP5') && !Guid.equals(card.typeName, 'Protocol') &&
      !Guid.equals(card.typeName, 'ORD') && !Guid.equals(card.typeName, 'Order OP1') &&
      !Guid.equals(card.typeName, 'Order OP2') && !Guid.equals(card.typeName, 'Order OP3') &&
      !Guid.equals(card.typeName, 'Order OP4') && !Guid.equals(card.typeName, 'Order OP5') && !Guid.equals(card.typeName, 'Attorney')

      && !Guid.equals(card.typeName, 'RB_Outgoing_AGIP')  && !Guid.equals(card.typeName, 'RB_Internal_BurPrirodNadzor')
      && !Guid.equals(card.typeName, 'RB_Internal_AGIP')  && !Guid.equals(card.typeName, 'RB_Internal')
        ) {
      return;
    }

    
    

    const file = context.file.model;
    const editCollapsed = !context.file.model.permissions.canEdit;
    
    
    // Добавляем пункт меню для получения файлов
    context.actions.push(
        new MenuAction(
          'OutgoingStamp',
          'Проставить регистрационный штамп',
          "ta icon-thin-254",
          null,
            [
            new MenuAction(
              'CommonBlank',
              'Общий бланк',
              "ta icon-thin-020",
              async () => {
                const editor = UIContext.current.cardEditor;
                if (!editor
                ) {
                  return;
                }
      
                showLoadingOverlay(async (splashResolve) => {
                  //await editor.saveCard();
                  let getIncomingStampRequest = new CardRequest();
                  getIncomingStampRequest.requestType = "ff3eaffd-fcef-43a1-a426-5ae632a7c3d5";
                  getIncomingStampRequest.info["cardID"] = createTypedField(card.id, DotNetType.Guid);
                  getIncomingStampRequest.info["versionRowID"] = createTypedField(file.lastVersion.id, DotNetType.Guid);
                  getIncomingStampRequest.info["fileName"] = createTypedField(file.name, DotNetType.String);
                  
                  let [commonBlank_indentLeft, commonBlank_indentTop, commonBlank_monthMode] = this.calculateStampCoordinate(card.typeName);

                  getIncomingStampRequest.info["indentLeft"] = createTypedField(commonBlank_indentLeft, DotNetType.Double);
                  getIncomingStampRequest.info["indentTop"] = createTypedField(commonBlank_indentTop, DotNetType.Double);
                  getIncomingStampRequest.info["monthMode"] = createTypedField(commonBlank_monthMode, DotNetType.Boolean);
                  
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
              null),
            new MenuAction(
              'GDAlignment',
              'Бланк ГД',
              "ta icon-thin-020",
              async () => {
                const editor = UIContext.current.cardEditor;
                if (!editor
                ) {
                  return;
                }
      
                showLoadingOverlay(async (splashResolve) => {
                  //await editor.saveCard();
                  let getIncomingStampRequest = new CardRequest();
                  getIncomingStampRequest.requestType = "ff3eaffd-fcef-43a1-a426-5ae632a7c3d5";
                  getIncomingStampRequest.info["cardID"] = createTypedField(card.id, DotNetType.Guid);
                  getIncomingStampRequest.info["versionRowID"] = createTypedField(file.lastVersion.id, DotNetType.Guid);
                  getIncomingStampRequest.info["fileName"] = createTypedField(file.name, DotNetType.String);
                  getIncomingStampRequest.info["indentLeft"] = createTypedField(90, DotNetType.Double);
                  getIncomingStampRequest.info["indentTop"] = createTypedField(234, DotNetType.Double);
                  getIncomingStampRequest.info["monthMode"] = createTypedField(false, DotNetType.Boolean);
                  
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
              null,
              !Guid.equals(card.typeName, 'Outgoing') && !Guid.equals(card.typeName, 'Outgoing_ACGP') && !Guid.equals(card.typeName, 'RB_Outgoing_AGIP')),
            new MenuAction(
                'ZGDAlignment',
                'Бланк ЗГД',
                "ta icon-thin-020",
                async () => {
                  const editor = UIContext.current.cardEditor;
                  if (!editor
                  ) {
                    return;
                  }
        
                  showLoadingOverlay(async (splashResolve) => {
                    //await editor.saveCard();
                    let getIncomingStampRequest = new CardRequest();
                    getIncomingStampRequest.requestType = "ff3eaffd-fcef-43a1-a426-5ae632a7c3d5";
                    getIncomingStampRequest.info["cardID"] = createTypedField(card.id, DotNetType.Guid);
                    getIncomingStampRequest.info["versionRowID"] = createTypedField(file.lastVersion.id, DotNetType.Guid);
                    getIncomingStampRequest.info["fileName"] = createTypedField(file.name, DotNetType.String);
                    getIncomingStampRequest.info["indentLeft"] = createTypedField(85, DotNetType.Double);
                    getIncomingStampRequest.info["indentTop"] = createTypedField(252, DotNetType.Double);
                    getIncomingStampRequest.info["monthMode"] = createTypedField(false, DotNetType.Boolean);
                    
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
                null,
                !Guid.equals(card.typeName, 'Outgoing') && !Guid.equals(card.typeName, 'Outgoing_ACGP') && !Guid.equals(card.typeName, 'RB_Outgoing_AGIP')),
            new MenuAction(
                'AccountantAlignment',
                'Бланк Главный бухгалтер',
                "ta icon-thin-020",
                async () => {
                  const editor = UIContext.current.cardEditor;
                  if (!editor
                  ) {
                    return;
                  }
        
                  showLoadingOverlay(async (splashResolve) => {
                    //await editor.saveCard();
                    let getIncomingStampRequest = new CardRequest();
                    getIncomingStampRequest.requestType = "ff3eaffd-fcef-43a1-a426-5ae632a7c3d5";
                    getIncomingStampRequest.info["cardID"] = createTypedField(card.id, DotNetType.Guid);
                    getIncomingStampRequest.info["versionRowID"] = createTypedField(file.lastVersion.id, DotNetType.Guid);
                    getIncomingStampRequest.info["fileName"] = createTypedField(file.name, DotNetType.String);
                    getIncomingStampRequest.info["indentLeft"] = createTypedField(90, DotNetType.Double);
                    getIncomingStampRequest.info["indentTop"] = createTypedField(234, DotNetType.Double);
                    getIncomingStampRequest.info["monthMode"] = createTypedField(false, DotNetType.Boolean);
                    
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
                null,
                !Guid.equals(card.typeName, 'Outgoing') && !Guid.equals(card.typeName, 'Outgoing_ACGP') && !Guid.equals(card.typeName, 'RB_Outgoing_AGIP')),
            new MenuAction(
                  'ChiefEngineer',
                  'Бланк Главный инженер',
                  "ta icon-thin-020",
                  async () => {
                    const editor = UIContext.current.cardEditor;
                    if (!editor
                    ) {
                      return;
                    }
          
                    showLoadingOverlay(async (splashResolve) => {
                      //await editor.saveCard();
                      let getIncomingStampRequest = new CardRequest();
                      getIncomingStampRequest.requestType = "ff3eaffd-fcef-43a1-a426-5ae632a7c3d5";
                      getIncomingStampRequest.info["cardID"] = createTypedField(card.id, DotNetType.Guid);
                      getIncomingStampRequest.info["versionRowID"] = createTypedField(file.lastVersion.id, DotNetType.Guid);
                      getIncomingStampRequest.info["fileName"] = createTypedField(file.name, DotNetType.String);
                      getIncomingStampRequest.info["indentLeft"] = createTypedField(80, DotNetType.Double);
                      getIncomingStampRequest.info["indentTop"] = createTypedField(268, DotNetType.Double);
                      getIncomingStampRequest.info["monthMode"] = createTypedField(false, DotNetType.Boolean);
                      
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
                  null,
                  !Guid.equals(card.typeName, 'Outgoing') && !Guid.equals(card.typeName, 'Outgoing_ACGP') &&
                  !Guid.equals(card.typeName, 'Outgoing OP1') && !Guid.equals(card.typeName, 'Outgoing OP2') &&
                  !Guid.equals(card.typeName, 'Outgoing OP3') && !Guid.equals(card.typeName, 'Outgoing OP4') &&
                  !Guid.equals(card.typeName, 'Outgoing OP5') && !Guid.equals(card.typeName, 'RB_Outgoing_AGIP')),
            ],
          context.files.length > 2 || file.getExtension() !== 'pdf' || editCollapsed
      ));
  }

  
  private calculateStampCoordinate(cardTypeName: string): [ number, number, boolean] {

    let commonBlank_indentLeft = 90;
    let commonBlank_indentTop = 211; // выставлены дефолтные значения по Общему бланку для исходящих
    let commonBlank_monthMode = false;

    if(Guid.equals(cardTypeName, 'Protocol') || Guid.equals(cardTypeName, 'RB_Internal_AGIP'))
    {
      commonBlank_indentLeft = 63;
      commonBlank_indentTop = 152; 
      commonBlank_monthMode = true;
    }
    else if(Guid.equals(cardTypeName, 'ORD') || Guid.equals(cardTypeName, 'Order OP1') ||
            Guid.equals(cardTypeName, 'Order OP2') || Guid.equals(cardTypeName, 'Order OP3') ||
            Guid.equals(cardTypeName, 'Order OP4') || Guid.equals(cardTypeName, 'Order OP5') || Guid.equals(cardTypeName, 'RB_Internal')
           )
    {
      commonBlank_indentLeft = 77;
      commonBlank_indentTop = 190;  
      commonBlank_monthMode = true;
    }
    else if(Guid.equals(cardTypeName, 'Attorney'))
    {
      commonBlank_indentLeft = 65;
      commonBlank_indentTop = 218; 
      commonBlank_monthMode = true;
    }
    else if(Guid.equals(cardTypeName, 'ORD LS')  || Guid.equals(cardTypeName, 'RB_Internal_BurPrirodNadzor') )
    {
      commonBlank_indentLeft = 78;
      commonBlank_indentTop = 232; 
      commonBlank_monthMode = true;
    }

    return [commonBlank_indentLeft, commonBlank_indentTop, commonBlank_monthMode]
  }



}