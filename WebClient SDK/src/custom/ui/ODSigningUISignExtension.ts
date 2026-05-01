import { CardUIExtension ,ICardModel,ICardUIExtensionContext } from 'tessa/ui/cards';
import { UIContext } from 'tessa/ui';
//import { FileContainer } from 'tessa/files';
import { showLoadingOverlay } from 'tessa/ui';
//import { IStorage } from 'tessa/platform/storage';
import { CertData, IFile } from 'tessa/files';
import { CardFile} from 'tessa/cards';
import {  showError,  showMessage, showNotEmpty } from 'tessa/ui';
import { FileListViewModel  } from 'tessa/ui/cards/controls';
import { selectFileCerts, SelectFileCertResult } from 'tessa/ui/cards/controls';
import { ValidationResult, ValidationResultBuilder } from 'tessa/platform/validation';
import { CardGetRequest, CardService,CardStoreRequest } from 'tessa/cards/service';
import { userSession } from 'common/utility';
import { DotNetType } from 'tessa/platform';
import { openCard } from 'tessa/ui/uiHost';
import { ODFileSigningHelper } from '../helpers/ODFileSigningHelper';

interface PreSign  {
  subjectName: string;
  issuerName: string;
  validFrom: string;
  validTo: string;
  company:  string;
  serialNumber: string,
  certificateStr: string;
  thumbprint: string,
};
/**
 * Расширение на задание "Подписание".
 */
export class ODSigningUISignExtension extends CardUIExtension {
  //private _diadocSigningUIProcessService: DiadocSigningUIProcessService;

  public async saving(context: ICardUIExtensionContext) {
    //this._diadocSigningUIProcessService = new DiadocSigningUIProcessService();
    const card = context.card;

    const savingAndSendingTask = card.tasks.find(t => t.typeId === '968d68b3-a7c5-4b5d-bfa4-bb0f346880b6');

    if (!savingAndSendingTask) {
      return;
    }

    if (savingAndSendingTask.optionId === '45d6f756-d30b-4c98-9d72-6adf1a15d075'
    || savingAndSendingTask.optionId === '4de44ffd-c2ca-4fad-835b-631222b076e1') {
      const taskSettings = savingAndSendingTask.settings;
      if (!taskSettings) {
        return;
      }
      const editor = UIContext.current.cardEditor;
      if (!editor
      ) {
        return;
      }
      /*let comment = '';
      if (savingAndSendingTask.optionId === '45d6f756-d30b-4c98-9d72-6adf1a15d075')
      {
        comment = 'Утверждено';
      }
      if (savingAndSendingTask.optionId === '4de44ffd-c2ca-4fad-835b-631222b076e1')
      {
        comment = 'Не утверждено';
      }*/
      
      const signProcessResult = await this.signingProcessingUIAsync(
        context.model
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
    cardModel: ICardModel
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
      //fileFromSigningId: guid | null//,
      //diadocFlowDirectionId: number
    ): Promise<boolean> {

      let file = await ODSigningUISignExtension.getFileFromSigningAsync(cardModel, 0);
      if (!file) {
        return false;
      }
      let fileControl: FileListViewModel | undefined;
      let certResult : SelectFileCertResult | undefined;
      
      const cardGetRequest = new CardGetRequest();
      cardGetRequest.cardId = userSession.UserID;
      cardGetRequest.cardTypeId = '929ad23c-8a22-09aa-9000-398bf13979b2';
      cardGetRequest.cardTypeName = 'PersonalRole';
      // при запросе все расширения CardGetExtension будут вызываны
      const cardGetResponse = await CardService.instance.get(cardGetRequest);
      if (!cardGetResponse.validationResult.isSuccessful) {
        // если в validationResult есть ошибки, то показываем их
        await showNotEmpty(cardGetResponse.validationResult.build());
        return false;
      }
      
        // берем storage карточки как строку
        const CertInfoSection = cardGetResponse.card.sections.tryGet('PersonalRoles');
        if (!CertInfoSection ) {
          return false;
        }
      let certString: string = CertInfoSection.fields.tryGet('EDSCertificateInfo');

       var cert: CertData | undefined;
      
      if(certString != null)
      {
        let cert2: PreSign =JSON.parse(certString);
        const certDate = new Date(cert2.validTo);
        const now = new Date();
        if(certDate < now)
        {
          await showMessage("Срок действия вашего сертификата истек. Пожалуйста, укажите новый сертификат в профиле пользователя."
          );
          await showLoadingOverlay(async (splashResolve) => {
            const editor = await openCard({
              cardTypeId: '929ad23c-8a22-09aa-9000-398bf13979b2', // PersonalRole
              cardId: userSession.UserID,
              splashResolve
            });
      
            if (editor) {
              const workspaceInfo = '$UI_Tiles_Settings';
              if (editor.workspaceInfo !== workspaceInfo) {
                editor.workspaceInfo = workspaceInfo;
                editor.cardModelInitialized.add(async e => { e.workspaceInfo = workspaceInfo; });
              }
            }
          });
          return false;
        }
            
        cert = {subjectName: cert2.subjectName,//'Андрей',
              issuerName: cert2.issuerName,//'Officedoc',
              validFrom: cert2.validFrom,//'2022-01-01',
              validTo: cert2.validTo,//'2025-01-01',
              company: cert2.company,///'Office-Doc',
              serialNumber: cert2.serialNumber,//'01D8EF236759ECB0000001D00FBB0013', //cert2.certificateStr,//
              certificateStr: cert2.certificateStr,//'MIIFmDCCBUegAwIBAgIQAdjvI2dZ7LAAAAHQD7sAEzAIBgYqhQMCAgMwgfIxIzAhBgNVBAwMGtCQ0LTQvNC40L3QuNGB0YLRgNCw0YLQvtGAMUQwQgYDVQQLDDvQo9C00L7RgdGC0L7QstC10YDRj9GO0YnQuNC5INC4INC60LvRjtGH0LXQstC+0Lkg0YbQtdC90YLRgDFSMFAGA1UECgxJ0JDQtNC8LiDQn9GA0LXQt9C40LTQtdC90YLQsCDQuCDQn9GA0LDQstC40YLQtdC70YzRgdGC0LLQsCDQkdGD0YDRj9GC0LjQuDExMC8GA1UEAwwo0JDQtNC80LjQvdC40YHRgtGA0LDRgtC+0YAg0YHQtdGC0LggNDAyNzAeFw0yMjExMDMwMTI3MDBaFw0yNzExMDMwMTI3MDBaMIIBzTFSMFAGA1UECQxJ0L/RgC4g0JvQuNCz0L7QstGB0LrQuNC5LCDQtC4gMTE0LCDQu9C40YLQtdGAINCRLCDQv9C+0LzQtdGJ0LXQvdC40LUgNyDQnTEmMCQGA1UEBwwd0KHQsNC90LrRgi3Qn9C10YLQtdGA0LHRg9GA0LMxHTAbBgNVBCoMFNCV0LLQs9C10L3RjNC10LLQuNGHMRUwEwYDVQQEDAzQkNC90LTRgNC10LkxgYAwfgYDVQQMDHfQl9Cw0LzQtdGB0YLQuNGC0LXQu9GMINCz0LXQvdC10YDQsNC70YzQvdC+0LPQviDQtNC40YDQtdC60YLQvtGA0LAg0L/QviDRgNCw0LfRgNCw0LHQvtGC0LrQtSDQuCDQstC90LXQtNGA0LXQvdC40Y4g0JjQoTFqMGgGA1UECgxh0J7QsdGJ0LXRgdGC0LLQviDRgSDQvtCz0YDQsNC90LjRh9C10L3QvdC+0Lkg0L7RgtCy0LXRgtGB0YLQstC10L3QvdC+0YHRgtGM0Y4gwqvQntGE0LjRgS3QlNC+0LrCuzEVMBMGBSqFA2QEEgo3ODQyNTEyMDczMRMwEQYDVQQDDArQn9GP0YLQvtCyMGYwHwYIKoUDBwEBAQEwEwYHKoUDAgIkAAYIKoUDBwEBAgIDQwAEQMkyz/9zoFcagqkmKpQ46ARVDuDxgn57kiir4V17+PYv4Qw2mIoEirwJ0FitaMxMox4/Nln91TEUciZICtHmGPuBCQAwRkJCMDAxM4IJADBGQkIwNjQwo4IBvjCCAbowggEwBgNVHSMEggEnMIIBI4AUZCVjtzl9elzbg4K6/bp2arR/Za+hgfikgfUwgfIxIzAhBgNVBAwMGtCQ0LTQvNC40L3QuNGB0YLRgNCw0YLQvtGAMUQwQgYDVQQLDDvQo9C00L7RgdGC0L7QstC10YDRj9GO0YnQuNC5INC4INC60LvRjtGH0LXQstC+0Lkg0YbQtdC90YLRgDFSMFAGA1UECgxJ0JDQtNC8LiDQn9GA0LXQt9C40LTQtdC90YLQsCDQuCDQn9GA0LDQstC40YLQtdC70YzRgdGC0LLQsCDQkdGD0YDRj9GC0LjQuDExMC8GA1UEAwwo0JDQtNC80LjQvdC40YHRgtGA0LDRgtC+0YAg0YHQtdGC0LggNDAyN4IQAdhl2e/bYLAAAAGXD7sAEzALBgNVHQ8EBAMCA/gwHQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMEMAwGA1UdEwEB/wQCMAAwKwYDVR0QBCQwIoAPMjAyMjExMDMwMTI3MDBagQ8yMDI0MDIwMzAxMjcwMFowHQYDVR0OBBYEFFaUL0PI0DdJKf5a46ArUszjgeSoMAgGBiqFAwICAwNBAFH8Kv+aRbPM0pFc5kOpNzWW7oLqYNzhqjs2CPp4PiUl//OCjPBRAFxWVWsGxOg7D80gnzI3LEAJJfN0LwSUvuQ='
              thumbprint: cert2.thumbprint//'2D8A96AFC2154CB48AA02AFB7C3E60566B799177'
            };  
      }
      else
      {
        await showMessage('Отсутствует сертификат в профиле сотрудника. Пожалуйста, выберете сертификат. Сертификат будет сохранен');
        try {
          certResult = await selectFileCerts(cardModel.edsProvider);
        } 
        catch (e) {
          await showMessage(" Предупреждение: Отсутствует возможность выбора сертификата подписи. " +
          "Необходимо проверить наличие КриптоПро ЭЦП Browser plug-in для подписания ЭП на текущем открытом браузере. " + 
          "Обратитесь к администратору системы за помощью. "
          );
          return true;
        }
        if (!certResult) {
          return false;
        }
        if (certResult.cancel || !certResult.cert) {
          return false;
        }
        cert = certResult.cert;

        CertInfoSection.fields.set('EDSCertificateInfo', JSON.stringify(cert), DotNetType.String); 
        const storeRequest = new CardStoreRequest();
        storeRequest.card = cardGetResponse.card;
        //storeRequest.forceTransaction = true;
        const storeResponse = await CardService.instance.store(storeRequest)//, fileContainer.files);
        if (!storeResponse.validationResult.build().isSuccessful) {
          await showNotEmpty(storeResponse.validationResult.build());
          return false;
        }
      }
      /////////////////////////////////
      for (let i = 0; i < cardModel.card.files.length; i++) {
        if (cardModel.card.files[i].isVirtual) {
          continue; // если файл виртуальный - не подписываем
        }
        file = await ODSigningUISignExtension.getFileFromSigningAsync(cardModel, i);
        if (!file) {
          continue;
        }

        for (let control of cardModel.controlsBag) {
          if (control instanceof FileListViewModel) {
            fileControl = control as FileListViewModel;
            break;
          }
        }
        if (!fileControl) {
          await showError('Нет файлового контрола!');
          return false;
        }

        const fileSignatureCount = file.lastVersion.signatures.length;

        await ODFileSigningHelper.signFileAsync(
          cert.serialNumber,
          cardModel.fileContainer,
          file.id,
          new ValidationResultBuilder()
        );

        if (file.lastVersion.signatures.length <= fileSignatureCount) {
          return false;
        }
      }
     /////////////////////////////////////////////////////////
     return true;
    }

    private static async getFileFromSigningAsync(cardModel: ICardModel, i: number): Promise<IFile | null> {
      let file: IFile | undefined;
      
      file = cardModel.fileContainer.files[i];

  
      if (!file) {
        return null;
      }
  
      let validationResult: ValidationResult;
      validationResult = await file.lastVersion.ensureContentDownloaded();
  
      await showNotEmpty(validationResult);
  
      if (!validationResult.isSuccessful) {
        return null;
      }
  
      validationResult = await file.lastVersion.ensureSignaturesLoaded();
  
      await showNotEmpty(validationResult);
  
      if (!validationResult.isSuccessful) {
        return null;
      }
  
      return file;
    }
}



