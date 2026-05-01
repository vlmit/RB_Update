import { ApplicationExtension, IApplicationExtensionMetadataContext } from 'tessa';
/**
 * Расширение на получение данных о доступных пользователю шаблонов файлов.
 */
export declare class FileTemplateInitializationExtension extends ApplicationExtension {
    afterMetadataReceived(context: IApplicationExtensionMetadataContext): void;
}
