import { FileControlExtension, IFileControlExtensionContext, IFileExtensionContextBase } from 'tessa/ui/files';
import { MenuAction } from 'tessa/ui';
import { IFile } from 'tessa/files';
/**
 * Расширение, инициализирующее меню файлового контрола значениями по умолчанию.
 */
export declare class PlatformFileControlExtension extends FileControlExtension {
    initializing(context: IFileControlExtensionContext): void;
    openingMenu(context: IFileControlExtensionContext): void;
    static getFileTemplateActions(context: IFileExtensionContextBase, fileToReplace?: IFile | null): MenuAction[];
    private getSortingActions;
    private getGroupingActions;
    private getFilteringActions;
    private static tryGetDocType;
    private static createFileTemplateCardAction;
}
