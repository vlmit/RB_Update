import { createAtom, IAtom } from 'mobx';
import { IViewSorting } from 'tessa/ui/views';
import { SortingColumn, SortDirection } from 'tessa/views';

export class TableViewSorting implements IViewSorting {
  //#region ctor

  constructor() {
    this._atom = createAtom('ViewSorting');
  }

  //#endregion

  //#region fields

  private _sortingColumns: SortingColumn[] = [];

  private _atom: IAtom;

  //#endregion

  //#region props

  public get columns(): ReadonlyArray<SortingColumn> {
    this._atom.reportObserved();
    return this._sortingColumns;
  }

  //#endregion

  //#region methods

  public sortColumn(
    alias: string,
    _shiftDown: boolean,
    descendingByDefault: boolean
  ): SortDirection | null {
    let column = this.columns.find(x => x.alias === alias);
    if (!column) {
      this._sortingColumns.length = 0;
      column = new SortingColumn(
        alias,
        descendingByDefault ? SortDirection.Descending : SortDirection.Ascending
      );
      this._sortingColumns.push(column);
      this._atom.reportChanged();
    } else {
      column.sortDirection =
        column.sortDirection === SortDirection.Ascending
          ? SortDirection.Descending
          : SortDirection.Ascending;
      this._atom.reportChanged();
    }
    return column.sortDirection;
  }

  public clear() {
    this._sortingColumns.length = 0;
    this._atom.reportChanged();
  }

  //#endregion
}
