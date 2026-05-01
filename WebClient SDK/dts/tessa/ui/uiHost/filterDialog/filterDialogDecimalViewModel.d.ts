import { ViewParameterMetadataSealed } from 'tessa/views/metadata';
import { CriteriaValue } from 'tessa/views/metadata/criteriaValue';
import { IFilterDialogEditorViewModel } from './common';
export declare class FilterDialogDecimalViewModel implements IFilterDialogEditorViewModel {
    constructor(meta: ViewParameterMetadataSealed);
    private _isFocused;
    private _value;
    readonly meta: ViewParameterMetadataSealed;
    get isFocused(): boolean;
    set isFocused(value: boolean);
    get value(): string;
    set value(value: string);
    private decimalFormat;
    getValue(): CriteriaValue;
}
