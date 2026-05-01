import { TileExtension, ITileLocalExtensionContext } from 'tessa/ui/tiles';
/**
 * Пример расширения, которое скрывает дефолтный тайл для определенного типа карточки.
 *
 * Результат работы расширения:
 * Скрывает дефолтный тайл "Сохранить" из левой панели для тестовой карточки.
 */
export declare class HideTileExtension extends TileExtension {
    initializingLocal(context: ITileLocalExtensionContext): void;
}
