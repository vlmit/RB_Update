import { CardGetExtension, ICardGetExtensionContext } from 'tessa/cards/extensions';
/**
 * Если загружается та же карточка, которая содержится в текущем контексте,
 * то из карточки в контексте все задания с флагом CardTaskFlags.UnlockedForAuthor
 * переносятся в запрос CardGetRequest.AuthorTaskRowIDList.
 */
export declare class FillAuthorTaskRowIdListGetExtension extends CardGetExtension {
    beforeRequest(context: ICardGetExtensionContext): void;
}
