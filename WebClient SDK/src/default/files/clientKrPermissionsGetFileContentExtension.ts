import {
  CardGetFileContentExtension,
  ICardGetFileContentExtensionContext
} from 'tessa/cards/extensions';
import { UIContext } from 'tessa/ui';
import { ICardModel } from 'tessa/ui/cards';
import { KrToken } from 'tessa/workflow';

export class ClientKrPermissionsGetFileContentExtension extends CardGetFileContentExtension {
  public beforeRequest(context: ICardGetFileContentExtensionContext) {
    const editor = UIContext.current.cardEditor;
    let model: ICardModel;
    let token: KrToken;

    if (
      editor &&
      (model = editor.cardModel!) &&
      (token = KrToken.tryGet(model.card.info)!) &&
      context.request.versionRowId !== '36a03878-57b6-2263-2e3a-9ae659032132' && // ReplacePlaceholdersVersionRowID
      model.card.id === context.request.cardId
    ) {
      token.setInfo(context.request.info);
    }
  }
}
