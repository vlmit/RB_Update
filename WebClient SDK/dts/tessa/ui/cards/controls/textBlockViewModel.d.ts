import { TextBlockViewModelBase } from './textBlockViewModelBase';
import { ICardModel } from '../interfaces';
import { CardTypeEntryControl } from 'tessa/cards/types';
import { MediaStyle } from 'ui/mediaStyle';
/**
 * Модель представления для элемента управления, выполняющего ввод текстовых строк.
 */
export declare class TextBlockViewModel extends TextBlockViewModelBase {
    constructor(control: CardTypeEntryControl, model: ICardModel);
    private _fields;
    private _fieldNames;
    private readonly _defaultFieldName;
    private _displayFormat;
    /**
     * Текстовое представление введённой строки.
     */
    get text(): string;
    set text(value: string);
    /**
     * Формат поля. Значение null или пустая строка определяют
     * отсутствие дополнительного форматирования.
     */
    get displayFormat(): string | null;
    set displayFormat(value: string | null);
    private getFormattedText;
    getControlStyle(): MediaStyle | null;
}
