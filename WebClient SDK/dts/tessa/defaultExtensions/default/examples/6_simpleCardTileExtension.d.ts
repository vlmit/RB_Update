import { TileExtension, ITileGlobalExtensionContext } from 'tessa/ui/tiles';
/**
 * Добавлять/cкрывать/показывать тайл в левой панели для выбранных карточек в зависимости:
 * - От типа карточки.
 * - От данных карточки.
 *
 * Результат работы расширения:
 * Добавляет на левую панель тайл, который по нажатию открывает модальное окно.
 * Добавленный тайл отображается только в карточке типа "Автомобиль", и при условии,
 * что значение контрола "Пробег, км" составляет "100".
 */
export declare class SimpleCardTileExtension extends TileExtension {
    initializingGlobal(context: ITileGlobalExtensionContext): void;
    private static showMessageBoxCommand;
    private static enableIfCardWithSubject;
}
