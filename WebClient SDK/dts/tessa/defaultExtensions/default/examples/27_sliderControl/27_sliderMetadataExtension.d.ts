import { CardMetadataExtension, ICardMetadataExtensionContext } from 'tessa/cards/extensions';
/**
 * Создает собственные контролы и добавляет их в выбранную карточку.
 *
 * Результат работы расширения:
 * Пример данного расширения создает кастомный слайдер-контрол и добавляет его в блок
 * "Общая информация" тестовой карточки "Автомобиль".
 */
export declare class SliderMetadataExtension extends CardMetadataExtension {
    initializing(context: ICardMetadataExtensionContext): void;
}
