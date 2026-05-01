import { CardSerializableObject } from 'tessa/cards/cardSerializableObject';
export interface CardTypeContentSealed {
    readonly caption: string | null;
    readonly order: number;
    seal<T = CardTypeContentSealed>(): T;
}
/**
 * Базовый объект для CardTypeControl и CardTypeColumn.
 */
export declare abstract class CardTypeContent extends CardSerializableObject {
    constructor();
    /**
     * Отображаемое имя объекта.
     */
    caption: string | null;
    /**
     * Порядок отображения объекта в интерфейсе карточки.
     */
    order: number;
    seal<T = CardTypeContentSealed>(): T;
}
