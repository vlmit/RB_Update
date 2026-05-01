import { IFilterDialogEditorViewModel } from './common';
import { ViewParameterMetadataSealed } from 'tessa/views/metadata';
import { CriteriaValue } from 'tessa/views/metadata/criteriaValue';
export declare class FilterDialogTextViewModel implements IFilterDialogEditorViewModel {
    constructor(meta: ViewParameterMetadataSealed);
    private _value;
    readonly meta: ViewParameterMetadataSealed;
    get value(): string;
    set value(value: string);
    getValue(): CriteriaValue;
}
