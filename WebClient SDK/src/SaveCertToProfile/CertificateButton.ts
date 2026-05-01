import { CardUIExtension, ICardUIExtensionContext, CardToolbarAction } from 'tessa/ui/cards';
import { getTessaIcon } from 'common/utility';
import { UIContext } from 'tessa/ui';
import { Guid, DotNetType } from 'tessa/platform';
// import { IFile, createFakeFile } from 'tessa/files';
//import { CertData } from 'tessa/files';
import { selectFileCerts } from 'tessa/ui/cards/controls';
//import { FileContainer } from 'tessa/files';
//import { userSession } from 'common/utility';



/*
 * Добавить кнопку в верхем меню табов "Обновить сертификат" для web-клиента - показывает окно выбора сертификата, сохраняет выбранный сертификат в профиль.
 */
 export class ODTestButton extends CardUIExtension {
  //private _disposer: Function | null = null;

  public initialized(context: ICardUIExtensionContext) {
    // если это не карточка сотрудника, то ничего не делаем
    if (!Guid.equals(context.card.typeId, '929ad23c-8a22-09aa-9000-398bf13979b2')) {
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
            //var cert: CertData;
            /* cert = {subjectName: 'Андрей',
              issuerName: 'Officedoc',
              validFrom: '2022-01-01',
              validTo: '2025-01-01',
              company: 'Office-Doc',
              serialNumber: '01D8EF236759ECB0000001D00FBB0013',
              certificateStr: 'MIIFmDCCBUegAwIBAgIQAdjvI2dZ7LAAAAHQD7sAEzAIBgYqhQMCAgMwgfIxIzAhBgNVBAwMGtCQ0LTQvNC40L3QuNGB0YLRgNCw0YLQvtGAMUQwQgYDVQQLDDvQo9C00L7RgdGC0L7QstC10YDRj9GO0YnQuNC5INC4INC60LvRjtGH0LXQstC+0Lkg0YbQtdC90YLRgDFSMFAGA1UECgxJ0JDQtNC8LiDQn9GA0LXQt9C40LTQtdC90YLQsCDQuCDQn9GA0LDQstC40YLQtdC70YzRgdGC0LLQsCDQkdGD0YDRj9GC0LjQuDExMC8GA1UEAwwo0JDQtNC80LjQvdC40YHRgtGA0LDRgtC+0YAg0YHQtdGC0LggNDAyNzAeFw0yMjExMDMwMTI3MDBaFw0yNzExMDMwMTI3MDBaMIIBzTFSMFAGA1UECQxJ0L/RgC4g0JvQuNCz0L7QstGB0LrQuNC5LCDQtC4gMTE0LCDQu9C40YLQtdGAINCRLCDQv9C+0LzQtdGJ0LXQvdC40LUgNyDQnTEmMCQGA1UEBwwd0KHQsNC90LrRgi3Qn9C10YLQtdGA0LHRg9GA0LMxHTAbBgNVBCoMFNCV0LLQs9C10L3RjNC10LLQuNGHMRUwEwYDVQQEDAzQkNC90LTRgNC10LkxgYAwfgYDVQQMDHfQl9Cw0LzQtdGB0YLQuNGC0LXQu9GMINCz0LXQvdC10YDQsNC70YzQvdC+0LPQviDQtNC40YDQtdC60YLQvtGA0LAg0L/QviDRgNCw0LfRgNCw0LHQvtGC0LrQtSDQuCDQstC90LXQtNGA0LXQvdC40Y4g0JjQoTFqMGgGA1UECgxh0J7QsdGJ0LXRgdGC0LLQviDRgSDQvtCz0YDQsNC90LjRh9C10L3QvdC+0Lkg0L7RgtCy0LXRgtGB0YLQstC10L3QvdC+0YHRgtGM0Y4gwqvQntGE0LjRgS3QlNC+0LrCuzEVMBMGBSqFA2QEEgo3ODQyNTEyMDczMRMwEQYDVQQDDArQn9GP0YLQvtCyMGYwHwYIKoUDBwEBAQEwEwYHKoUDAgIkAAYIKoUDBwEBAgIDQwAEQMkyz/9zoFcagqkmKpQ46ARVDuDxgn57kiir4V17+PYv4Qw2mIoEirwJ0FitaMxMox4/Nln91TEUciZICtHmGPuBCQAwRkJCMDAxM4IJADBGQkIwNjQwo4IBvjCCAbowggEwBgNVHSMEggEnMIIBI4AUZCVjtzl9elzbg4K6/bp2arR/Za+hgfikgfUwgfIxIzAhBgNVBAwMGtCQ0LTQvNC40L3QuNGB0YLRgNCw0YLQvtGAMUQwQgYDVQQLDDvQo9C00L7RgdGC0L7QstC10YDRj9GO0YnQuNC5INC4INC60LvRjtGH0LXQstC+0Lkg0YbQtdC90YLRgDFSMFAGA1UECgxJ0JDQtNC8LiDQn9GA0LXQt9C40LTQtdC90YLQsCDQuCDQn9GA0LDQstC40YLQtdC70YzRgdGC0LLQsCDQkdGD0YDRj9GC0LjQuDExMC8GA1UEAwwo0JDQtNC80LjQvdC40YHRgtGA0LDRgtC+0YAg0YHQtdGC0LggNDAyN4IQAdhl2e/bYLAAAAGXD7sAEzALBgNVHQ8EBAMCA/gwHQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMEMAwGA1UdEwEB/wQCMAAwKwYDVR0QBCQwIoAPMjAyMjExMDMwMTI3MDBagQ8yMDI0MDIwMzAxMjcwMFowHQYDVR0OBBYEFFaUL0PI0DdJKf5a46ArUszjgeSoMAgGBiqFAwICAwNBAFH8Kv+aRbPM0pFc5kOpNzWW7oLqYNzhqjs2CPp4PiUl//OCjPBRAFxWVWsGxOg7D80gnzI3LEAJJfN0LwSUvuQ='
            }; 
            */

            const result = await  selectFileCerts(context.model.edsProvider);
            if (result.cancel || !result.cert) {
              alert ('Not found certs')
              return;
            }
            const cert = result.cert;
            alert(JSON.stringify(cert));            
            
            const CertInfo = context.card.sections.tryGet('PersonalRoles');
            if (!CertInfo ) {
              return;
            }
            CertInfo.fields.set('EDSCertificateInfo', JSON.stringify(cert), DotNetType.String);
          }
        })
      );
    }
     
  }
 
 
}
