import { ViewMetadataSealed } from 'tessa/views/metadata';
import { SortingColumn, SortDirection } from 'tessa/views';
export interface IViewSorting {
    readonly columns: ReadonlyArray<SortingColumn>;
    sortColumn(alias: string, shiftDown: boolean, descendingByDefault: boolean): SortDirection | null;
    clear(): any;
}
export declare class ViewSorting implements IViewSorting {
    constructor(viewMetadata: ViewMetadataSealed | null, sortingColumns?: SortingColumn[] | null);
    private _viewMetadata;
    private _sortingColumns;
    private _atom;
    get columns(): ReadonlyArray<SortingColumn>;
    sortColumn(alias: string, shiftDown: boolean, descendingByDefault: boolean): SortDirection | null;
    clear(): void;
    private setDefaultSorting;
    private static getNextDirection;
}
