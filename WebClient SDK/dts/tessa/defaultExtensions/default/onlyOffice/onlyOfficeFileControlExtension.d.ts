import { FileControlExtension, IFileControlExtensionContext } from 'tessa/ui/files';
/**
 * Представляет собой расширение, которое добавляет возможность создания файлов по шаблону.
 */
export declare class OnlyOfficeFileControlExtension extends FileControlExtension {
    openingMenu(context: IFileControlExtensionContext): void;
    shouldExecute(): boolean;
}
