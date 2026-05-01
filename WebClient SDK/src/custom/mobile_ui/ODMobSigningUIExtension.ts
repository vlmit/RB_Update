import { CardUIExtension ,ICardModel,ICardUIExtensionContext } from 'tessa/ui/cards';
import { UIContext } from 'tessa/ui';
//import { IFile } from 'tessa/files';
import { CardFile} from 'tessa/cards';
import { showError} from 'tessa/ui';
//import { FileListViewModel, selectFileCerts, setSign, SelectFileCertResult } from 'tessa/ui/cards/controls';
//import { ValidationResult } from 'tessa/platform/validation';


/**Утверждение
 * Расширение на задание "Утверждение".
 */
export class ODMobSigningUIExtension extends CardUIExtension {

  public async saving(context: ICardUIExtensionContext) {
    const card = context.card;

    const singingTask = card.tasks.find(t => t.typeId === '968d68b3-a7c5-4b5d-bfa4-bb0f346880b6');

    if (!singingTask) {
      return;
    }

    if (singingTask.optionId === '45d6f756-d30b-4c98-9d72-6adf1a15d075'
    || singingTask.optionId === '4de44ffd-c2ca-4fad-835b-631222b076e1') {
      const taskSettings = singingTask.settings;
      if (!taskSettings) {
        return;
      }
      const editor = UIContext.current.cardEditor;
      if (!editor
      ) {
        return;
      }
      // let comment = '';
      // if (savingAndSendingTask.optionId === '45d6f756-d30b-4c98-9d72-6adf1a15d075')
      // {
      //   comment = 'Утверждено';
      // }
      // if (savingAndSendingTask.optionId === '4de44ffd-c2ca-4fad-835b-631222b076e1')
      // {
      //   comment = 'Не утверждено';
      // }

      const signProcessResult = await this.signingProcessingUIAsync(
        context.model,
        //comment
      );
      if (!signProcessResult) {
        context.cancel = true;
        const editor = UIContext.current.cardEditor;
        if (!editor) {
          return;
        }
        editor.closePending = false;
      }
    }
  }
  public async signingProcessingUIAsync(
    cardModel: ICardModel,
    //comment: string,

  ): Promise<boolean> {
    const currentCard = cardModel.card;
   

    let cardFiles: CardFile[];
   
      cardFiles = currentCard.files.filter(f => !f.isVirtual);

    if (cardFiles.length === 0) {
      await showError('Отсутствует файл для подписи!');
    }

    return await this.signFileAsync(cardModel);

  }

   /**
   * Подписать файл ЭП. <br />
   * Открывает типовое окно выбора подписи.
   * @param diadocSatellite Карточка сателлит диадок.
   * @param cardModel Модель текущей карточки.
   * @param fileFromSigningId Идентификатор файла для подписания.
   * @param diadocFlowDirectionId Идентификатор направления документа Диадок.
   */
    private async signFileAsync(
      //diadocSatellite: Card,
      cardModel: ICardModel,
      //comment: string,
      //fileFromSigningId: guid | null//,
      //diadocFlowDirectionId: number
    ): Promise<boolean> {

      //const cert = certResult.cert;
      ////////////////////////////////////////////
      const editor = UIContext.current.cardEditor;
            if (!editor
            ) {
              return false;
            }
      const userCard = cardModel.card;

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
      /////////////////////////////////
     /////////////////////////////////////////////////////////
     return true;
    }

}


