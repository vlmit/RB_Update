import { getTessaIcon } from "common/utility";
import { CardRequest, CardService } from "tessa/cards/service";
import { DotNetType, createTypedField } from "tessa/platform";
import { UIContext, showLoadingOverlay, showMessage, tryGetFromInfo } from "tessa/ui";
import { CardToolbarAction, CardUIExtension, ICardUIExtensionContext } from "tessa/ui/cards";

export class GetSignaturesBtn extends CardUIExtension {
    
    public initialized(context: ICardUIExtensionContext) {
        if (context.toolbar) {
            context.toolbar.removeItemIfExists('GetSignaturesBtn');
            context.toolbar.addItem(
                new CardToolbarAction({
                    name: 'GetSignaturesBtn',
                    caption: 'Сохранить ЭП',
                    icon: getTessaIcon('icon-thin-005'),
                    command: async _ => {
                        const editor = UIContext.current.cardEditor;
                        if (!editor) {
                            return;
                        }
                        await editor.saveCard();
                        
                        showLoadingOverlay(async (splashResolve) => {
                            let getSignaturesRequest = new CardRequest();
                            getSignaturesRequest.requestType = "73f8c4a9-af21-48c3-b63f-2d6476c94e11";
                            getSignaturesRequest.info["cardID"] = createTypedField(context.card.id, DotNetType.Guid);
                            
                            let getSignaturesResponse = await CardService.instance.request(getSignaturesRequest);
                            if (!getSignaturesResponse.validationResult.isSuccessful) {
                                showMessage('Ошибка получения ЭП файлов');
                                return;
                            }
                            
                            let zipName = tryGetFromInfo<string>(getSignaturesResponse.info, 'FileName', undefined);
                            let zipB64 = tryGetFromInfo<string>(getSignaturesResponse.info, 'Content', undefined);
                            
                            if (!zipName || !zipB64) {
                                showMessage('Ошибка получения ЭП файлов');
                                return;
                            }
                            
                            downloadBase64File(zipB64, zipName);
                            
                            splashResolve;
                        });
                    }
                })
            );
        }
    }
}

function downloadBase64File(contentBase64, fileName) {
    const linkSource = `data:archive/zip;base64,${contentBase64}`;
    const downloadLink = document.createElement('a');
    document.body.appendChild(downloadLink);
    
    downloadLink.href = linkSource;
    downloadLink.target = '_self';
    downloadLink.download = fileName;
    downloadLink.click(); 
}