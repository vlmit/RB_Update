import { TileExtension, ITileGlobalExtensionContext, ITileLocalExtensionContext, ITilePanelExtensionContext } from 'tessa/ui/tiles';
export declare class MySettingsTileExtension extends TileExtension {
    initializingGlobal(context: ITileGlobalExtensionContext): void;
    initializingLocal(context: ITileLocalExtensionContext): void;
    openingLocal(context: ITilePanelExtensionContext): void;
}
