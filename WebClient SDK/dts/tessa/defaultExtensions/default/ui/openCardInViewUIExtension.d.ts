import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
/**
 * @author Раткевич С.С. Syntellect (C) 2021
 * @description
 * UI расширение, реализующее функционал - открыть карточку из представления.
 */
export declare class OpenCardInViewUIExtension extends CardUIExtension {
    initialized(context: ICardUIExtensionContext): Promise<void>;
    private executeInitializedActionAsync;
    private attachDoubleClickHandlerAsync;
    private doubleClickHandlerAsync;
    private static tryGetCardReferenceMapping;
    private static tryGetCardIDNameMapping;
    private static tryAttachControlInfo;
}
