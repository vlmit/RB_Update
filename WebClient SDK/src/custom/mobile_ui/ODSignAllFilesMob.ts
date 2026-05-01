import { CardUIExtension, ICardUIExtensionContext, CardToolbarAction } from 'tessa/ui/cards';
import { getTessaIcon } from 'common/utility';
import { UIContext } from 'tessa/ui';
//import { FileContainer } from 'tessa/files';
//import { selectFileCerts, setSign } from 'tessa/ui/cards/controls';
//import { Guid } from 'tessa/platform';
// import { ControlCaption } from 'tessa/ui/cards/components/controls';
// import { CardFile } from 'tessa/cards';


/**
 * Добавить кнопку в верхем меню "Подписать все файлы" для web-клиента - выполняет циклическое подписание всех вложенных в карточку файлов.
 * Работает только для карточки документа
 */
 export class SignAllFilesMob extends CardUIExtension {
  //private _disposer: Function | null = null;

  public initialized(context: ICardUIExtensionContext) {
    // если карточка не Протокол, то ничего не делаем
    //if (!Guid.equals(context.card.typeId, 'de076ddc-70aa-4a4e-abbc-53b87567dc06') // исходящие
    //&& !Guid.equals(context.card.typeId, 'ed6d5ee1-9075-4ce0-bdae-76afe87544f2') // входящие
   // && !Guid.equals(context.card.typeId, 'b8066da5-d9a6-4e14-a415-0422e36b4c04')// входящие АПиП
   // && !Guid.equals(context.card.typeId, '88e3ed72-100c-4fc8-8527-6558623aa0f7') // исходящий АПиП
   // && !Guid.equals(context.card.typeId, 'aa81c9f6-2326-44d6-b511-461d8eb8ea7b')// внутренний АПиП
   // && !Guid.equals(context.card.typeId, '93a392e7-097c-4420-85c4-db10b2df3c1d')// внутренний АПиП
  //  ) {
  //    return;
  //  }
   
     // Добавляем кнопку
    if (context.toolbar) {
      
      context.toolbar.removeItemIfExists('SignAllFilesMob');
      context.toolbar.addItem(
        new CardToolbarAction({
          name: 'SignAllFilesMob',
          caption: 'Подписать файлы (моб.)',
          icon: getTessaIcon('icon-thin-005'),
          command: async _ => {
            const editor = UIContext.current.cardEditor;
            if (!editor
            ) {
              return;
            }
            const userCard = context.card;

            userCard.files.forEach(async function(f){
              // alert (f.name + ' Версия файла: ' + f.lastVersion?.number);
              let tempUrl = `https://sedmob.govrb.ru/api/filelink?` +
                    `cardId=${userCard.id}` + 
                    `&fileId=${f.card.id}` +
                    `&versionId=${f.versionRowId}` + //check
                    `&cardTypeId=${userCard.typeId}` +
                    `&cardTypeName=${userCard.typeName}` +
                    `&fileName=${f.name}` +
                    `&fileTypeName=${f.typeName}`;
              
              // alert ('Test '+tempUrl);

              let request = {
                files: {
                  'file1': {
                    url: tempUrl
                  }
                }
              }; 

              let var1 = true;
              // @ts-ignore
              signFiles(request, function(response) {
                    if (!response.Result) {
                        alert (response.Error);
                        return;
                      }
                      if (var1) {}
                      alert (response.Signs["file1"]);
              });
              


               
            })
          }
        })
      );
    }
     
  }
 
 
}
