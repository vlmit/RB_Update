import { ControlViewModelBase } from './controlViewModelBase';
import { ICardModel, ControlKeyDownEventArgs } from '../interfaces';
import { CardTypeEntryControl } from 'tessa/cards/types';
import { EventHandler } from 'tessa/platform';
/**
 * Модель представления для элемента управления, выполняющего ввод вещественных чисел.
 */
export declare class DoubleBoxViewModel extends ControlViewModelBase {
    constructor(control: CardTypeEntryControl, model: ICardModel);
    private _fields;
    private _fieldName;
    /**
     * Минимальное допустимое значение введённого числа.
     */
    readonly minValue: number;
    /**
     * Максимальное допустимое значение введённого числа.
     */
    readonly maxValue: number;
    /**
     * Текстовое представление введённого числа.
     */
    get text(): string;
    set text(value: string);
    get error(): string | null;
    get hasEmptyValue(): boolean;
    getInternalValidation(value: string): string | null;
    readonly keyDown: EventHandler<(args: ControlKeyDownEventArgs) => void>;
}
