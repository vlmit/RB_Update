import { IViewSorting } from 'tessa/ui/views';
import { SortingColumn, SortDirection } from 'tessa/views';
export declare class TableViewSorting implements IViewSorting {
    constructor();
    private _sortingColumns;
    private _atom;
    get columns(): ReadonlyArray<SortingColumn>;
    sortColumn(alias: string, _shiftDown: boolean, descendingByDefault: boolean): SortDirection | null;
    clear(): void;
}
