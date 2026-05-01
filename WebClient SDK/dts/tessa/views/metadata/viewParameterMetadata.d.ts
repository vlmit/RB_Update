import { AutoCompleteInfo, AutoCompleteInfoSealed } from './autoCompleteInfo';
import { DropDownInfo, DropDownInfoSealed } from './dropDownInfo';
import { CriteriaOperator } from './criteriaOperators';
import { SchemeType } from 'tessa/scheme';
export interface IViewParameterMetadata {
    alias: string;
    allowedOperands: string[] | null;
    autoCompleteInfo: AutoCompleteInfo | null;
    caption: string | null;
    treatValueAsUtc: boolean;
    disallowedOperands: string[] | null;
    dropDownInfo: DropDownInfo | null;
    hidden: boolean;
    hideAutoCompleteButton: boolean;
    multiple: boolean;
    refSection: string | null;
    sourceViews: string[] | null;
    schemeType: SchemeType | null;
    seal<T = ViewParameterMetadataSealed>(): T;
    getDefaultCriteria(): CriteriaOperator;
}
export interface ViewParameterMetadataSealed {
    readonly alias: string;
    readonly allowedOperands: ReadonlyArray<string> | null;
    readonly autoCompleteInfo: AutoCompleteInfoSealed | null;
    readonly caption: string | null;
    readonly treatValueAsUtc: boolean;
    readonly disallowedOperands: ReadonlyArray<string> | null;
    readonly dropDownInfo: DropDownInfoSealed | null;
    readonly hidden: boolean;
    readonly hideAutoCompleteButton: boolean;
    readonly multiple: boolean;
    readonly refSection: string | null;
    readonly sourceViews: ReadonlyArray<string> | null;
    readonly schemeType: SchemeType | null;
    seal<T = ViewParameterMetadataSealed>(): T;
    getDefaultCriteria(): CriteriaOperator;
}
export declare class ViewParameterMetadata implements IViewParameterMetadata {
    constructor();
    alias: string;
    allowedOperands: string[] | null;
    autoCompleteInfo: AutoCompleteInfo | null;
    caption: string | null;
    treatValueAsUtc: boolean;
    disallowedOperands: string[] | null;
    dropDownInfo: DropDownInfo | null;
    hidden: boolean;
    hideAutoCompleteButton: boolean;
    multiple: boolean;
    refSection: string | null;
    sourceViews: string[] | null;
    schemeType: SchemeType | null;
    seal<T = ViewParameterMetadataSealed>(): T;
    getDefaultCriteria(): CriteriaOperator;
}
