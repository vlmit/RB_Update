import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
export declare class CarUIExtension extends CardUIExtension {
    private _disposes;
    private _syncPageEvents;
    initialized(context: ICardUIExtensionContext): void;
    finalized(): void;
    saving(context: ICardUIExtensionContext): void;
    private get1CfileAsync;
    private setPageSyncEvents;
    private clearSyncPagesEvents;
}
