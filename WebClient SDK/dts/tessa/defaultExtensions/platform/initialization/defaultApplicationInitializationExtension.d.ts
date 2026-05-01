import { ApplicationExtension, IApplicationExtensionMetadataContext } from 'tessa';
import { MainInitializationResponse } from 'tessa/platform/initialization';
export declare class DefaultApplicationInitializationExtension extends ApplicationExtension {
    initialize(): void;
    afterMetadataReceived(context: IApplicationExtensionMetadataContext): void;
    private initLocalization;
    private initCommonMeta;
    private initSingletons;
    private initExistentSingletons;
    private initKrTypesCache;
    private initViews;
    private initSearchQueries;
    private initWorkplaces;
    private initTiles;
    private initCards;
    private initTheme;
    private initWallpaper;
    private initUserSettings;
    initPaletteSettingsManager(response: MainInitializationResponse): void;
    private initLicense;
}
