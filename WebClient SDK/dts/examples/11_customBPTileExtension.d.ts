import { TileExtension, ITileGlobalExtensionContext } from 'tessa/ui/tiles';
/**
 * Добавляет тайл запуска бизнес-процесса в левую боковую панель для определенного типа карточек.
 *
 * Результат работы расширения:
 * Для карточки "Автомобиль" добавляет на левую панель тайл "Запустить кастомный БП".
 * При нажатии на тайл в "Info" карточки добавляется ключ `.startProcess` со значением `TestProcess`.
 * После, карточка сохраняется и обновляется.
 */
export declare class CustomBPTileExtension extends TileExtension {
    initializingGlobal(context: ITileGlobalExtensionContext): void;
    private static startCustomBP;
    private static enableIfCard;
}
