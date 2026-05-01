import { ApplicationExtension, IApplicationExtensionMetadataContext } from 'tessa';
import { FileExtension, FileExtensionContext, FileControlExtension, IFileControlExtensionContext, FileVersionExtension, FileVersionExtensionContext } from 'tessa/ui/files';
import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
export declare class DeskiExtension extends ApplicationExtension {
    private _lastMasterKeysUpdate;
    afterMetadataReceived(_context: IApplicationExtensionMetadataContext): void;
}
export declare class DeskiFileExtension extends FileExtension {
    openingMenu(context: FileExtensionContext): void;
}
export declare class DeskiFileControlExtension extends FileControlExtension {
    openingMenu(context: IFileControlExtensionContext): void;
}
export declare class DeskiFileVersionExtension extends FileVersionExtension {
    static showOpenForReadWarningMessage: boolean;
    openingMenu(context: FileVersionExtensionContext): void;
}
export declare class DeskiUIExtension extends CardUIExtension {
    shouldExecute(): boolean;
    initialized(context: ICardUIExtensionContext): void;
    saving(context: ICardUIExtensionContext): Promise<void>;
    finalized(context: ICardUIExtensionContext): Promise<void>;
}
