import * as React from 'react';
import Moment from 'moment';
import { MediaStyle } from '../mediaStyle';
declare class DatePicker extends React.Component<DatePickerProps, DatePickerState> {
    constructor(props: DatePickerProps);
    private _inputRef;
    private _calendarButtonRef;
    private _datePickerRef;
    static defaultProps: {
        dateEnabled: boolean;
        timeEnabled: boolean;
    };
    static tryGetDate(date?: string | null): Moment.Moment | null;
    static validateDate(date?: Moment.Moment | null): Moment.Moment | null;
    getDateFormat(date?: Moment.Moment | null): string | undefined;
    getDateFromText(value?: string): Moment.Moment | null;
    updateDate(date: string | null, fromPopover?: boolean): void;
    getInputFormatOptions(): string | undefined;
    focus(opt?: FocusOptions): void;
    handleCalendarButtonClick: (event: React.SyntheticEvent) => void;
    handleClearButtonClick: (event: React.SyntheticEvent) => void;
    handleCalendarDateSelect: (value: Moment.Moment) => void;
    handleInputFocus: (event: React.SyntheticEvent) => void;
    handleInputBlur: (event: React.SyntheticEvent) => void;
    handleInputValidate: (value: string) => string;
    handleKeyDown: (event: React.KeyboardEvent) => void;
    renderCalendar(): JSX.Element | null;
    render(): JSX.Element;
}
export interface DatePickerProps {
    date: string | null;
    onDateChange: (date: string | null, fromPopover?: boolean) => void;
    isHighlightBeginDate?: boolean;
    beginDate?: Moment.Moment | null;
    minDate?: Moment.Moment | null;
    maxDate?: Moment.Moment | null;
    disabled?: boolean;
    className?: string;
    dateEnabled?: boolean;
    timeEnabled?: boolean;
    hideClearButton?: boolean;
    mediaStyle?: MediaStyle | null;
    style?: React.CSSProperties;
    title?: string;
    isInvalid?: boolean;
    onChange?: (event: React.SyntheticEvent) => void;
    onFocus?: (event: React.SyntheticEvent) => void;
    onBlur?: (event: React.SyntheticEvent) => void;
    onKeyDown?: (event: React.SyntheticEvent) => void;
}
export interface DatePickerState {
    isOpened: boolean;
}
export default DatePicker;
