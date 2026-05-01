import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
/**
 * В этом расширении демонстрируется подписка на изменение элементов
 * табличной секции для выбранного типа карточки.
 *
 * Результат работы расширения:
 * В тестовой карточке "Автомобиль" при добавлении или удалении строки
 * из таблицы "Список Акций" в поле "Цвет" добавляется текст с соответствующим
 * идентификатором строки и наименованием действия (добавления или удаления соответственно).
 */
export declare class TableSectionChangedUIExtension extends CardUIExtension {
    private _listener;
    initialized(context: ICardUIExtensionContext): void;
    finalized(): void;
}
