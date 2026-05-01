import { TileExtension, ITileGlobalExtensionContext } from 'tessa/ui/tiles';
/**
 * Данное расширение позволяет добавлять/скрывать/показывать тайл в левой панели,
 * при нажатии на который отображаются данные выделенной строки для выбранного представления.
 * Отображение тайла зависит от алиаса представления и идентификатора узла рабочего места.
 *
 * Результат работы расширения:
 * Добавляет на левую панель тайл, который по нажатию открывает модальное окно
 * с данными выделенной строки представления. Добавленный тайл отображается
 * только для представления **Мои документы**.
 */
export declare class SimpleViewTileExtension extends TileExtension {
    private static myDocumentsAlias;
    initializingGlobal(context: ITileGlobalExtensionContext): void;
    private static showViewData;
    private static enableIfMyDocumentsViewAndHasSelectedRow;
}
