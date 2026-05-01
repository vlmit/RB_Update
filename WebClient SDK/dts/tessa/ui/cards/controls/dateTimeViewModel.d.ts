import Moment from 'moment';
import { ControlViewModelBase } from './controlViewModelBase';
import { ICardModel, ControlKeyDownEventArgs } from '../interfaces';
import { CardTypeEntryControl } from 'tessa/cards/types';
import { EventHandler } from 'tessa/platform';
/**
 * Элемент управления "Дата и время" в карточке.
 */
export declare class DateTimeViewModel extends ControlViewModelBase {
    constructor(control: CardTypeEntryControl, model: ICardModel);
    private _fields;
    private _fieldName;
    /**
     * Признак того, что элемент управления будет отображать или редактировать дату.
     */
    readonly dateEnabled: boolean;
    /**
     * Признак того, что элемент управления будет отображать или редактировать время.
     */
    readonly timeEnabled: boolean;
    /**
     * Признак того, что часовой пояс сотрудника игнорируется и дата всегда указывается как Utc.
     * Если контрол редактирует только дату, то часовой пояс всегда игнорируется,
     * независимо от этой настройки.
     */
    readonly ignoreTimezone: boolean;
    /**
     * Признак того, что разрешены пустые значения
     */
    readonly allowNulls: boolean;
    /**
     * Текущие дата/время, отображаемые элементом управления,
     * или null, если дата/время не выбраны.
     */
    get selectedDate(): Moment.Moment | null;
    set selectedDate(value: Moment.Moment | null);
    get selectedDayString(): string | null;
    get error(): string | null;
    get hasEmptyValue(): boolean;
    /**
      устанавливает минимальную границу даты, дни ДО указанного значения в свойстве будут недоступны.
      По умолчанию null - минимальный диапазон не задан, доступны все дни.
    */
    minDate: Moment.Moment | null;
    /**
      устанавливает максимальную границу даты, дни ПОСЛЕ указанного значения в свойстве будут недоступны.
      По умолчанию null - максимальный диапазон не задан, доступны все дни.
    */
    maxDate: Moment.Moment | null;
    /**
      дата, на которой открывыается календарь. По умолчанию null: текущий день.
    */
    beginDate: Moment.Moment | null;
    /**
      добавляет подсветку для дня в календаре, указанного в beginDate. По дефолту отключен.
    */
    highlightBeginDate: boolean;
    readonly keyDown: EventHandler<(args: ControlKeyDownEventArgs) => void>;
}
