import { CardExtensionContext, ICardExtensionContext } from './cardExtensionContext';
import { CardCopyRequest, CardCreateFromTemplateRequest, CardNewResponse } from 'tessa/cards/service';
import { CardTypeSealed } from 'tessa/cards/types';
import { CardMetadataSealed } from 'tessa/cards/metadata';
import { IUserSession } from 'common/utility/userSession';
export interface ICardNewTemplateExtensionContext extends ICardExtensionContext {
    readonly request: CardCopyRequest | CardCreateFromTemplateRequest;
    response: CardNewResponse | null;
    readonly isCopyRequest: boolean;
}
export declare class CardNewTemplateExtensionContext extends CardExtensionContext implements ICardNewTemplateExtensionContext {
    constructor(request: CardCopyRequest | CardCreateFromTemplateRequest, isCopyRequest: boolean, cardType: CardTypeSealed | null, cardTypeName: string | null, cardMetadata: CardMetadataSealed, session: IUserSession);
    readonly request: CardCopyRequest | CardCreateFromTemplateRequest;
    response: CardNewResponse | null;
    readonly isCopyRequest: boolean;
    tryGetCardCopyRequest(): CardCopyRequest | null;
    tryGetCardCreateFromTemplateRequest(): CardCreateFromTemplateRequest | null;
}
