import {
  IViewControlDataProvider,
  FileListViewModel,
  ViewControlDataProviderResponse,
  ViewControlDataProviderRequest,
  FileViewModel
} from 'tessa/ui/cards/controls';
import { RequestParameter, ViewMetadataSealed, RequestCriteria } from 'tessa/views/metadata';
import { ValidationResultBuilder } from 'tessa/platform/validation';
import { SchemeDbType, formatSize } from 'tessa/platform';
import { IStorage } from 'tessa/platform/storage';
import { LocalizationManager } from 'tessa/localization';
import { SortingColumn, SortDirection } from 'tessa/views';

export class CardFilesDataProvider implements IViewControlDataProvider {
  constructor(readonly viewMetadata: ViewMetadataSealed, readonly fileControl: FileListViewModel) {}

  async getDataAsync(
    request: ViewControlDataProviderRequest
  ): Promise<ViewControlDataProviderResponse> {
    const result = new ViewControlDataProviderResponse(new ValidationResultBuilder());
    this.addFieldDescriptions(result);
    this.populateDataRowsAsync(request, result);
    return result;
  }

  private addFieldDescriptions(result: ViewControlDataProviderResponse) {
    result.columns.push(['GroupCaption', SchemeDbType.String]);
    result.columns.push(['Caption', SchemeDbType.String]);
    result.columns.push(['CategoryCaption', SchemeDbType.String]);
    result.columns.push(['Size', SchemeDbType.String]);
    result.columns.push(['SizeAbsolute', SchemeDbType.Int64]);
  }

  private populateDataRowsAsync(
    request: ViewControlDataProviderRequest,
    result: ViewControlDataProviderResponse
  ) {
    const requestParameters: RequestParameter[] = CardFilesDataProvider.buildParametersCollectionFromRequest(
      request
    );

    // если фильтры сбросили через кнопку в представлении, то нужно сбросить фильтрацию в файловом контроле.
    if (
      this.fileControl.selectedFiltering &&
      !requestParameters.some(x => x.name === 'FilterParameter')
    ) {
      this.fileControl.selectedFiltering = null;
    }

    let filteredFiles = this.fileControl.filteredFiles;
    filteredFiles = filteredFiles.filter(new FileFilter(request).filter);

    const rows: Array<IStorage> = [];
    for (let file of filteredFiles) {
      rows.push(CardFilesDataProvider.mapFileToRow(file, this.fileControl));
    }

    rows.sort(new FilesSorter(this.viewMetadata, request).sort);
    result.rows.push(...rows);
  }

  static buildParametersCollectionFromRequest(
    request: ViewControlDataProviderRequest
  ): RequestParameter[] {
    const parametersCollection: RequestParameter[] = [];
    for (let action of request.parametersActions) {
      action(parametersCollection);
    }
    return parametersCollection;
  }

  static mapFileToRow(file: FileViewModel, fileControl: FileListViewModel): IStorage {
    const size =
      formatSize(file.model.size, 1000) +
      LocalizationManager.instance.localize('$Format_Unit_Kilobytes');

    let groupCaption = '';
    if (fileControl.groups) {
      for (let group of fileControl.groups.values()) {
        if (group.files.find(x => x === file)) {
          groupCaption = group.caption;
          break;
        }
      }
    }

    return {
      FileViewModel: file,
      GroupCaption: groupCaption,
      CategoryCaption:
        file.model.category?.caption ??
        LocalizationManager.instance.localize('$UI_Cards_FileNoCategory'),
      Caption: file.caption,
      SizeAbsolute: file.model.size,
      Size: size
    };
  }
}

class FileFilter {
  constructor(request: ViewControlDataProviderRequest) {
    this._request = request;
    this.buildFilter();
  }

  private readonly _filter: Array<Array<(item: FileViewModel) => boolean>> = [];

  private readonly _request: ViewControlDataProviderRequest;

  filter = (item: FileViewModel): boolean => {
    return this._filter
      .map(block => block.reduce((current, func) => current || func(item), false))
      .reduce((filterResult, blockResult) => filterResult && blockResult, true);
  };

  private buildFilter() {
    this._filter.length = 0;
    this._filter.push([() => true]);
    const requestParameters = CardFilesDataProvider.buildParametersCollectionFromRequest(
      this._request
    );
    for (let parameter of requestParameters) {
      if (parameter.name === 'Caption') {
        const filterBlock: Array<(item: FileViewModel) => boolean> = [];
        for (let criteria of parameter.criteriaValues) {
          this.appendCriteriaToCaptionFilterFunc(criteria, filterBlock);
        }
        this._filter.push(filterBlock);
      }
    }
  }

  private appendCriteriaToCaptionFilterFunc(
    criteria: RequestCriteria,
    filterBlock: Array<(item: FileViewModel) => boolean>
  ) {
    const valueAsString = (criteria.values[0].value as string)?.toLocaleUpperCase() ?? '';
    switch (criteria.criteriaName) {
      case 'Contains':
        filterBlock.push(x => x.caption.toLocaleUpperCase().includes(valueAsString));
        break;
      case 'Equality':
        filterBlock.push(x => x.caption.toLocaleUpperCase() === valueAsString);
        break;
      case 'StartWith':
        filterBlock.push(x => x.caption.toLocaleUpperCase().startsWith(valueAsString));
        break;
      case 'EndWith':
        filterBlock.push(x => x.caption.toLocaleUpperCase().endsWith(valueAsString));
        break;
    }
  }
}

class FilesSorter {
  constructor(viewMetadata: ViewMetadataSealed, request: ViewControlDataProviderRequest) {
    for (let column of request.sortingColumns) {
      const sortingColumn = new SortingColumn(
        viewMetadata.columns.get(column.alias!)?.sortBy ?? '',
        column.sortDirection
      );
      this._sortingColumns.push(sortingColumn);
    }
  }

  private _sortingColumns: Array<SortingColumn> = [];

  sort = (lhv: IStorage, rhv: IStorage): number => {
    if (this._sortingColumns.length === 0) {
      return 0;
    }

    const lhvFileVM: FileViewModel = lhv['FileViewModel'];
    const rhvFileVM: FileViewModel = rhv['FileViewModel'];
    // оргининал всегда "выше" чем копия файла независимо от направления сортировки
    if (lhvFileVM.model.origin === rhvFileVM.model) {
      return 1;
    }

    if (rhvFileVM.model.origin === lhvFileVM.model) {
      return -1;
    }

    let comparsion = -0;
    const sortingColumn = this._sortingColumns[0];
    const lvalue = lhv[sortingColumn.alias] ?? '';
    const rvalue = rhv[sortingColumn.alias] ?? '';
    if (typeof lvalue === 'string') {
      comparsion = lvalue.localeCompare(rvalue);
    } else {
      comparsion = lvalue - rvalue;
    }
    return sortingColumn.sortDirection === SortDirection.Ascending ? comparsion : -comparsion;
  };
}
