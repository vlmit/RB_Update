/// <reference types="react" />
import { ViewColumnMetadataSealed, ViewReferenceMetadataSealed, ViewMetadataSealed } from 'tessa/views/metadata';
import { SchemeDbType } from 'tessa/platform';
import { SortDirection } from 'tessa/views';
import { MenuAction } from 'tessa/ui/menuAction';
export interface ITableColumnViewModel {
    readonly canGrouping: boolean;
    canSort: boolean;
    header: string;
    readonly columnName: string;
    readonly dataType: SchemeDbType;
    isReferencedColumn: boolean;
    metadata: ViewColumnMetadataSealed | null;
    referenceMetadata: ViewReferenceMetadataSealed | null;
    viewMetadata: ViewMetadataSealed | null;
    sortDirection: SortDirection | null;
    visibility: boolean;
    toolTip: string;
    onClick: (e: React.MouseEvent) => void;
    onDoubleClick: (e: React.MouseEvent) => void;
    onMouseDown: (e: React.MouseEvent) => void;
    sortColumn(addOrInverse?: boolean, descendingByDefault?: boolean): any;
    getContextMenu(): ReadonlyArray<MenuAction>;
}
export interface ITableColumnViewModelCreateOptions {
    metadata: ViewColumnMetadataSealed | null;
    columnName: string;
    dataType: SchemeDbType;
    canSort: boolean;
    toolTip?: string;
    sortAction?: ((column: string, addOrInverse: boolean, descendingByDefault: boolean) => void) | null;
    getContextMenu?: ((column: ITableColumnViewModel) => ReadonlyArray<MenuAction>) | null;
    isReferencedColumn?: boolean;
    header?: string;
    referenceMetadata?: ViewReferenceMetadataSealed;
    viewMetadata?: ViewMetadataSealed;
    visibility?: boolean;
}
export declare class TableColumnViewModel implements ITableColumnViewModel {
    constructor(args: ITableColumnViewModelCreateOptions);
    protected _header: string;
    protected _canSort: boolean;
    protected _sortDirection: SortDirection | null;
    protected _visibility: boolean;
    protected _sortAction: ((column: string, addOrInverse: boolean, descendingByDefault: boolean) => void) | null;
    protected _getContextMenu: ((column: ITableColumnViewModel) => ReadonlyArray<MenuAction>) | null;
    protected _toolTip: string;
    get canGrouping(): boolean;
    get canSort(): boolean;
    set canSort(value: boolean);
    get header(): string;
    set header(value: string);
    readonly columnName: string;
    readonly dataType: SchemeDbType;
    isReferencedColumn: boolean;
    metadata: ViewColumnMetadataSealed | null;
    referenceMetadata: ViewReferenceMetadataSealed | null;
    viewMetadata: ViewMetadataSealed | null;
    get sortDirection(): SortDirection | null;
    set sortDirection(value: SortDirection | null);
    get visibility(): boolean;
    set visibility(value: boolean);
    get toolTip(): string;
    set toolTip(value: string);
    onClick: (e: React.MouseEvent) => void;
    onDoubleClick: (_e: React.MouseEvent) => void;
    onMouseDown: (_e: React.MouseEvent) => void;
    sortColumn(addOrInverse?: boolean, descendingByDefault?: boolean): void;
    getContextMenu(): ReadonlyArray<MenuAction>;
}
