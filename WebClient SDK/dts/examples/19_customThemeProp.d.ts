import { ApplicationExtension, IApplicationExtensionMetadataContext } from 'tessa';
import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
/**
 * Добавляет дополнительное свойство в темы в расширении "ApplicationExtension" с последующим
 * использованием для выбранного типа карточки.
 *
 * Результат работы расширения:
 * Данное расширение добавляет дополнительное свойство "MyCustomBackgroundProp" в настройки текущей темы,
 * которое, в свою очередь, устанавливается в качестве фона для заголовка контрола "Марка автомобиля"
 * тестовой карточки.
 */
export declare class CustomThemePropApplicationExtension extends ApplicationExtension {
    afterMetadataReceived(_context: IApplicationExtensionMetadataContext): void;
}
export declare class CustomThemePropUIExtension extends CardUIExtension {
    initialized(context: ICardUIExtensionContext): void;
}
