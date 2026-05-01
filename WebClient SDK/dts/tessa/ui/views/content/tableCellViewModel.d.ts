/// <reference types="react" />
import { ITableRowViewModel } from './tableRowViewModel';
import { ITableColumnViewModel } from './tableColumnViewModel';
import { TableTagViewModel } from './tableTagViewModel';
export interface ITableCellViewModel {
    readonly row: ITableRowViewModel;
    readonly column: ITableColumnViewModel;
    readonly value: any;
    readonly maxLength: number;
    readonly convertedValue: any;
    readonly isSelected: boolean;
    readonly toolTip: string;
    style: React.CSSProperties;
    getContent: () => any;
    onClick: (e: React.MouseEvent | React.KeyboardEvent) => void;
    onDoubleClick: (e: React.MouseEvent | React.KeyboardEvent) => void;
    onMouseDown: (e: React.MouseEvent) => void;
    readonly leftTags: Array<TableTagViewModel>;
    readonly rightTags: Array<TableTagViewModel>;
    initialize(): any;
    dispose(): any;
    selectCell(isSelected?: boolean): any;
}
export interface ITableCellViewModelCreateOptions {
    row: ITableRowViewModel;
    column: ITableColumnViewModel;
    value: any;
    appearance?: string | null;
}
export declare class TableCellViewModel implements ITableCellViewModel {
    constructor(args: ITableCellViewModelCreateOptions);
    protected _value: any;
    protected _convertedValue: any;
    protected _toolTip: string;
    protected _style: React.CSSProperties;
    protected _leftTags: Array<TableTagViewModel>;
    protected _rightTags: Array<TableTagViewModel>;
    protected _getContent: () => any;
    readonly row: ITableRowViewModel;
    readonly column: ITableColumnViewModel;
    readonly maxLength: number;
    get value(): any;
    get convertedValue(): any;
    get isSelected(): boolean;
    get toolTip(): string;
    get style(): React.CSSProperties;
    set style(value: React.CSSProperties);
    get getContent(): () => any;
    set getContent(value: () => any);
    onClick: (_e: React.MouseEvent | React.KeyboardEvent) => void;
    onDoubleClick: (_e: React.MouseEvent | React.KeyboardEvent) => any;
    onMouseDown: (_e: React.MouseEvent) => void;
    get leftTags(): Array<TableTagViewModel>;
    get rightTags(): Array<TableTagViewModel>;
    initialize(): void;
    dispose(): void;
    protected convertValue(sourceValue: any, column: ITableColumnViewModel): any;
    protected sliceValue(value: any): any;
    selectCell(isSelected?: boolean): void;
}
