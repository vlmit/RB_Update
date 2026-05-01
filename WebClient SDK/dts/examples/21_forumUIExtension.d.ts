import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
/**
 * Данное расширение для выбранной карточки добавляет элементы управления:
 * - В верхней панели текущего обсуждения.
 * - В контекстное меню текущего обсуждения.
 *
 * Результат работы расширения:
 * Данное расширение добавляет в верхней панели, а также в контекстном меню текущего обсуждения
 * соответствующие тестовые кнопку и пункт контекстного меню для карточки "Автомобиль".
 */
export declare class ForumUIExtension extends CardUIExtension {
    initialized(context: ICardUIExtensionContext): void;
    private tryGetForumControl;
}
