import { ApplicationExtension, IApplicationExtensionMetadataContext } from 'tessa';
import { TileExtension, ITileGlobalExtensionContext } from 'tessa/ui/tiles';
/**
 * Позволяет получать и использовать данные из _MetadataStorage.info_, добавленные
 * в мету через ServerInitializationExtension.
 *
 * Результат работы расширения:
 * - На клиенте достает данные, содержащие информацию о тайлах, добавленную через
 *   "ServerInitializationExtension" (по ключу из меты и добавляет в _MetadataStorage.info_).
 * - После инициализации приложения обращается к _MetadataStorage.info_, получает
 *   информацию о тайлах и добавляет их в левую панель.
 */
export declare class AdditionalMetaInitializationExtension extends ApplicationExtension {
    afterMetadataReceived(context: IApplicationExtensionMetadataContext): void;
}
export declare class AdditionalMetaTileExtension extends TileExtension {
    initializingGlobal(context: ITileGlobalExtensionContext): void;
}
