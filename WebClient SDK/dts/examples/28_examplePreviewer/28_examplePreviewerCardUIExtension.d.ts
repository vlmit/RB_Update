import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
/**
 * Позволяет создать собственное окно предпросмотра и использовать его для определенного
 * типа данных в выбранной карточке.
 *
 * Результат работы расширения:
 * В тестовой карточке "Автомобиль" создает кастомное окно предпросмотра и отображает его
 * для типа данных с расширением ".txt" в области предпросмотра вкладки "Карточка" и для контролов
 * "Предпросмотр" вкладки "Сравнение файлов".
 */
export declare class ExamplePreviewerCardUIExtension extends CardUIExtension {
    initializing(context: ICardUIExtensionContext): void;
    private static setExamplePreviewerFactory;
}
