import {
  IViewControlDataProvider,
  ViewControlDataProviderResponse,
  ViewControlDataProviderRequest
} from 'tessa/ui/cards/controls';
import { SortDirection } from 'tessa/views';
import { getMetadataRuntimeTypeFromDbType, CardMetadataRuntimeType } from 'tessa/cards/metadata';
import { compareDates, SchemeDbType } from 'tessa/platform';

export class TaskHistroryViewDataProvider implements IViewControlDataProvider {
  //#region ctor

  constructor(dataProvider: IViewControlDataProvider) {
    this._defaultDataProvider = dataProvider;
  }

  //#endregion

  //#region fields

  private _defaultDataProvider: IViewControlDataProvider;

  private _cachedResponse: ViewControlDataProviderResponse | null = null;

  //#endregion

  //#region methods

  async getDataAsync(
    request: ViewControlDataProviderRequest
  ): Promise<ViewControlDataProviderResponse> {
    let result = this._cachedResponse;
    if (!result) {
      result = await this._defaultDataProvider.getDataAsync(request);
      this._cachedResponse = result;
    }

    if (request.sortingColumns.length === 1) {
      const columnName = request.sortingColumns[0].alias;
      const sortDirection = request.sortingColumns[0].sortDirection;
      const ascending = sortDirection === SortDirection.Ascending;
      const column = result.columns.find(x => x[0] === columnName);
      const isDateTimeColumn = column
        ? getMetadataRuntimeTypeFromDbType(column[1]) === CardMetadataRuntimeType.DateTime
        : false;
      const isTimeSpan = column ? column[1] === SchemeDbType.Time : false;
      result.rows.sort((x, y) => {
        const right = x[columnName] ?? '';
        const left = y[columnName] ?? '';

        let r: number;
        if (isTimeSpan) {
          r = right.localeCompare(left);
        } else if (isDateTimeColumn) {
          r = compareDates(right, left, (a, b) => a - b);
        } else if (typeof right === 'string') {
          r = right.localeCompare(left);
        } else {
          r = right - left;
        }

        return ascending ? r : -r;
      });
    }

    return result;
  }

  //#endregion
}
