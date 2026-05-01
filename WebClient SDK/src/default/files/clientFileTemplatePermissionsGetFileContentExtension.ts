import {
  CardGetFileContentExtension,
  ICardGetFileContentExtensionContext
} from 'tessa/cards/extensions';
import { UIContext } from 'tessa/ui';
import { KrToken } from 'tessa/workflow';
import { ICardModel } from 'tessa/ui/cards';
import { systemKeyPrefix } from 'tessa/cards';

export class ClientFileTemplatePermissionsGetFileContentExtension extends CardGetFileContentExtension {
  public beforeRequest(context: ICardGetFileContentExtensionContext): void {
    const editor = UIContext.current.cardEditor;
    let model: ICardModel;
    let token: KrToken;

    if (
      editor &&
      (model = editor.cardModel!) &&
      (token = KrToken.tryGet(model.card.info)!) &&
      context.request.versionRowId === '36a03878-57b6-2263-2e3a-9ae659032132' && // ReplacePlaceholdersVersionRowID
      model.card.id === context.request.info[systemKeyPrefix + 'currentCardID']
    ) {
      token.setInfo(context.request.info);
    }
  }
}

