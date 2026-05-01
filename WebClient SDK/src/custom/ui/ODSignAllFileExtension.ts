import { FileExtension, IFileExtensionContext } from 'tessa/ui/files';
//import { Card } from 'tessa/cards';
import { UIContext, MenuAction, showLoadingOverlay } from 'tessa/ui';
//import { ICardEditorModel } from 'tessa/ui/cards';
//import { CardRequest, CardService } from 'tessa/cards/service';
//import { FileViewModel } from 'tessa/ui/cards/controls';
//import { DotNetType, createTypedField } from 'tessa/platform';
//import { CardUIExtension, ICardUIExtensionContext, CardToolbarAction } from 'tessa/ui/cards';
//import { getTessaIcon } from 'common/utility';
import { showNotEmpty,showMessage, } from 'tessa/ui/tessaDialog';
import { CertData } from 'tessa/files';
//import { FileContainer } from 'tessa/files';
import { selectFileCerts,SelectFileCertResult } from 'tessa/ui/cards/controls';
//import { CardSavingRequest, CardSavingMode } from 'tessa/ui/cards';
import { CardGetRequest, CardService, CardStoreRequest } from 'tessa/cards/service';
import { userSession } from 'common/utility';
import { openCard } from 'tessa/ui/uiHost';
import {  ValidationResultBuilder } from 'tessa/platform/validation';
import { DotNetType } from 'tessa/platform';
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

export class ODSignAllFileExtension extends FileExtension {
  public async openingMenu(context: IFileExtensionContext) {
    //let uiContext = UIContext.current;
    //let editor = uiContext.cardEditor;
    //let card: Card = context.control.model.card;
    // Добавляем пункт меню для получения файлов
    
    context.actions.push(
        new MenuAction(
          'SignAll',
          'Подписать все выбранные файлы',
          "ta icon-thin-236",
          async () => {
            const editor = UIContext.current.cardEditor;
            if (!editor
            ) {
              return;
            }
            const fCont = context.files
            
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
      
             //var cert: CertData | undefined;
            
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
              var cert: CertData;   
              cert = {subjectName: cert2.subjectName,//'Андрей',
                    issuerName: cert2.issuerName,//'Officedoc',
                    validFrom: cert2.validFrom,//'2022-01-01',
                    validTo: cert2.validTo,//'2025-01-01',
                    company: cert2.company,///'Office-Doc',
                    serialNumber: cert2.serialNumber,//'01D8EF236759ECB0000001D00FBB0013', //cert2.certificateStr,//
                    certificateStr: cert2.certificateStr,//'MIIFmDCCBUegAwIBAgIQAdjvI2dZ7LAAAAHQD7sAEzAIBgYqhQMCAgMwgfIxIzAhBgNVBAwMGtCQ0LTQvNC40L3QuNGB0YLRgNCw0YLQvtGAMUQwQgYDVQQLDDvQo9C00L7RgdGC0L7QstC10YDRj9GO0YnQuNC5INC4INC60LvRjtGH0LXQstC+0Lkg0YbQtdC90YLRgDFSMFAGA1UECgxJ0JDQtNC8LiDQn9GA0LXQt9C40LTQtdC90YLQsCDQuCDQn9GA0LDQstC40YLQtdC70YzRgdGC0LLQsCDQkdGD0YDRj9GC0LjQuDExMC8GA1UEAwwo0JDQtNC80LjQvdC40YHRgtGA0LDRgtC+0YAg0YHQtdGC0LggNDAyNzAeFw0yMjExMDMwMTI3MDBaFw0yNzExMDMwMTI3MDBaMIIBzTFSMFAGA1UECQxJ0L/RgC4g0JvQuNCz0L7QstGB0LrQuNC5LCDQtC4gMTE0LCDQu9C40YLQtdGAINCRLCDQv9C+0LzQtdGJ0LXQvdC40LUgNyDQnTEmMCQGA1UEBwwd0KHQsNC90LrRgi3Qn9C10YLQtdGA0LHRg9GA0LMxHTAbBgNVBCoMFNCV0LLQs9C10L3RjNC10LLQuNGHMRUwEwYDVQQEDAzQkNC90LTRgNC10LkxgYAwfgYDVQQMDHfQl9Cw0LzQtdGB0YLQuNGC0LXQu9GMINCz0LXQvdC10YDQsNC70YzQvdC+0LPQviDQtNC40YDQtdC60YLQvtGA0LAg0L/QviDRgNCw0LfRgNCw0LHQvtGC0LrQtSDQuCDQstC90LXQtNGA0LXQvdC40Y4g0JjQoTFqMGgGA1UECgxh0J7QsdGJ0LXRgdGC0LLQviDRgSDQvtCz0YDQsNC90LjRh9C10L3QvdC+0Lkg0L7RgtCy0LXRgtGB0YLQstC10L3QvdC+0YHRgtGM0Y4gwqvQntGE0LjRgS3QlNC+0LrCuzEVMBMGBSqFA2QEEgo3ODQyNTEyMDczMRMwEQYDVQQDDArQn9GP0YLQvtCyMGYwHwYIKoUDBwEBAQEwEwYHKoUDAgIkAAYIKoUDBwEBAgIDQwAEQMkyz/9zoFcagqkmKpQ46ARVDuDxgn57kiir4V17+PYv4Qw2mIoEirwJ0FitaMxMox4/Nln91TEUciZICtHmGPuBCQAwRkJCMDAxM4IJADBGQkIwNjQwo4IBvjCCAbowggEwBgNVHSMEggEnMIIBI4AUZCVjtzl9elzbg4K6/bp2arR/Za+hgfikgfUwgfIxIzAhBgNVBAwMGtCQ0LTQvNC40L3QuNGB0YLRgNCw0YLQvtGAMUQwQgYDVQQLDDvQo9C00L7RgdGC0L7QstC10YDRj9GO0YnQuNC5INC4INC60LvRjtGH0LXQstC+0Lkg0YbQtdC90YLRgDFSMFAGA1UECgxJ0JDQtNC8LiDQn9GA0LXQt9C40LTQtdC90YLQsCDQuCDQn9GA0LDQstC40YLQtdC70YzRgdGC0LLQsCDQkdGD0YDRj9GC0LjQuDExMC8GA1UEAwwo0JDQtNC80LjQvdC40YHRgtGA0LDRgtC+0YAg0YHQtdGC0LggNDAyN4IQAdhl2e/bYLAAAAGXD7sAEzALBgNVHQ8EBAMCA/gwHQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMEMAwGA1UdEwEB/wQCMAAwKwYDVR0QBCQwIoAPMjAyMjExMDMwMTI3MDBagQ8yMDI0MDIwMzAxMjcwMFowHQYDVR0OBBYEFFaUL0PI0DdJKf5a46ArUszjgeSoMAgGBiqFAwICAwNBAFH8Kv+aRbPM0pFc5kOpNzWW7oLqYNzhqjs2CPp4PiUl//OCjPBRAFxWVWsGxOg7D80gnzI3LEAJJfN0LwSUvuQ='
                    thumbprint: cert2.thumbprint//'2D8A96AFC2154CB48AA02AFB7C3E60566B799177'
                  };
                  
                  await showLoadingOverlay(async (splashResolve) => {
                    fCont.forEach(async (f) => {
                      const validationResult = await f.model.lastVersion.ensureContentDownloaded();
                      if (!validationResult.isSuccessful) {
                        await showNotEmpty(validationResult);
                        return;
                      }

                      const validationResultSign =
                        await f.model.lastVersion.ensureSignaturesLoaded();
                      if (!validationResultSign.isSuccessful) {
                        await showNotEmpty(validationResultSign);
                        return;
                      }

                      const version = f.model.lastVersion;

                      if (!version) {
                        return;
                      }

                      await ODFileSigningHelper.signFileAsync(
                        cert.serialNumber,
                        context.control.model.fileContainer,
                        f.id,
                        new ValidationResultBuilder()
                      );
                    });

                    splashResolve;
                  });
            }
            else
            {
              await showMessage('Отсутствует сертификат в профиле сотрудника. Пожалуйста, выберете сертификат. Сертификат будет сохранен');
              try {
                certResult = await selectFileCerts(context.control.model.edsProvider);
              } 
              catch (e) {
                await showMessage(" Предупреждение: Отсутствует возможность выбора сертификата подписи. " +
                "Необходимо проверить наличие КриптоПро ЭЦП Browser plug-in для подписания ЭП на текущем открытом браузере. " + 
                "Обратитесь к администратору системы за помощью. "
                );
                return false;
              }
              if (!certResult) {
                return false;
              }
              if (certResult.cancel || !certResult.cert) {
                return false;
              }
              var cert: CertData;
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
              
              await showLoadingOverlay(async (splashResolve) => {
                fCont.forEach(async (f) => {
                  const validationResult = await f.model.lastVersion.ensureContentDownloaded();
                  if (!validationResult.isSuccessful) {
                    await showNotEmpty(validationResult);
                    return;
                  }

                  const validationResultSign = await f.model.lastVersion.ensureSignaturesLoaded();
                  if (!validationResultSign.isSuccessful) {
                    await showNotEmpty(validationResultSign);
                    return;
                  }

                  const version = f.model.lastVersion;

                  if (!version) {
                    return;
                  }

                  await ODFileSigningHelper.signFileAsync(
                    cert.serialNumber,
                    context.control.model.fileContainer,
                    f.id,
                    new ValidationResultBuilder()
                  );
                });

                splashResolve;
              });
            }
            
            //const result = await  selectFileCerts(context.control.model.edsProvider);
            //if (result.cancel || !result.cert) {
            //  alert ('Not found certs')
            //  return;
            //}
            //const cert = result.cert;

            
            
           // await editor.saveCard(
           //   editor.context,
           //   undefined,
           //   new CardSavingRequest(CardSavingMode.RefreshOnSuccess))
            
           alert ('Файлы подписаны. Пожалуйста, сохраните карточку.');

            //await editor.saveCard(editor.context);
            
            /* if (!foundFlag)
            {
              return `Не найдено прикрепленных файлов`;
            } 
            */
          },
          null,
          false
      ));
  }
}
