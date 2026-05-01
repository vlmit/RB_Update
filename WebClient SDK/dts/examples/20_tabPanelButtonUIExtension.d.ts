import { TabPanelUIExtension, ITabPanelUIExtensionContext } from 'tessa/ui';
/**
 * Добавляет дополнительную кнопку в панель с вкладками. При нажатии на кнопку открывает виртуальную карточку.
 *
 * Результат работы расширения:
 * Пример данного расширения добавляет дополнительную кнопку в панель с вкладками,
 * при нажатии на которую открывается виртуальная карточка "Мои замещения".
 */
export declare class CustomTabPanelButtonUIExtension extends TabPanelUIExtension {
    initialize(context: ITabPanelUIExtensionContext): void;
}
