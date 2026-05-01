import * as React from 'react';
import Moment from 'moment';
declare class DatePickerWeek extends React.Component<IDatePickerWeekProps> {
    render(): JSX.Element;
}
export interface IDatePickerWeekProps {
    date: Moment.Moment;
    isHighlightBeginDate?: boolean;
    beginDate?: Moment.Moment | null;
    maxDate?: Moment.Moment | null;
    minDate?: Moment.Moment | null;
    month: number;
    selectedDate?: Moment.Moment | null;
    onDaySelect: (day: Moment.Moment) => void;
}
export default DatePickerWeek;
