import { CardUIExtension ,ICardModel,ICardUIExtensionContext } from 'tessa/ui/cards';
import { UIContext } from 'tessa/ui';
//import { IFile } from 'tessa/files';
import { CardFile} from 'tessa/cards';
import { showError} from 'tessa/ui';
//import { FileListViewModel, selectFileCerts, setSign, SelectFileCertResult } from 'tessa/ui/cards/controls';
//import { ValidationResult } from 'tessa/platform/validation';


/**
 * Расширение на задание "Согласование".
 */
export class ODMobApprovalUIExtension extends CardUIExtension {

  public async saving(context: ICardUIExtensionContext) {
    const card = context.card;

    const approvalTask = card.tasks.find(t => t.typeId === 'e4d7f6bf-fea9-4a3b-8a5a-e1a0a40de74c');

    if (!approvalTask) {
      return;
    }

    if (approvalTask.optionId === '8cf5cf41-8347-05b4-b3b2-519e8e621225'
    || approvalTask.optionId === '811d41ef-5610-421e-a573-fcdfd821713e') {
      const taskSettings = approvalTask.settings;
      if (!taskSettings) {
        return;
      }
      const editor = UIContext.current.cardEditor;
      if (!editor
      ) {
        return;
      }
      // let comment = '';

      // if (savingAndSendingTask.optionId === '8cf5cf41-8347-05b4-b3b2-519e8e621225')
      // {
      //   comment = 'Согласовано';
      // }
      // if (savingAndSendingTask.optionId === '811d41ef-5610-421e-a573-fcdfd821713e')
      // {
      //   comment = 'Не согласовано';
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


