import Moment from 'moment';
import { IFilterDialogEditorViewModel } from './common';
import { ViewParameterMetadataSealed } from 'tessa/views/metadata';
import { CriteriaValue } from 'tessa/views/metadata/criteriaValue';
export declare class FilterDialogDateTimeViewModel implements IFilterDialogEditorViewModel {
    constructor(meta: ViewParameterMetadataSealed);
    private _date;
    private _formatedDate;
    private _isTimeSpanRegExp;
    readonly meta: ViewParameterMetadataSealed;
    readonly dateEnabled: boolean;
    readonly timeEnabled: boolean;
    readonly treatValueAsUtc: boolean;
    get selectedDate(): Moment.Moment | null;
    get selectedDayString(): string | null;
    setDate(date: string | Moment.Moment | null): void;
    private getMoment;
    getValue(): CriteriaValue;
    setDefaultValue(secondControl: boolean): void;
}
