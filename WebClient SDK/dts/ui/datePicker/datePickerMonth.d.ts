import * as React from 'react';
import Moment from 'moment';
declare class DatePickerMonth extends React.Component<IDatePickerMonthProps> {
    isWeekInMonth(startOfWeek: Moment.Moment): boolean;
    render(): JSX.Element;
}
export interface IDatePickerMonthProps {
    date: Moment.Moment;
    isHighlightBeginDate?: boolean;
    selectedDate?: Moment.Moment | null;
    beginDate?: Moment.Moment | null;
    maxDate?: Moment.Moment | null;
    minDate?: Moment.Moment | null;
    onDaySelect: (value: Moment.Moment) => void;
}
export default DatePickerMonth;
