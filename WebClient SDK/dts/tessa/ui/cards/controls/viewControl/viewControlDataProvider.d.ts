import type { SortingColumn } from 'tessa/views';
import { ITessaView } from 'tessa/views';
import { IStorage } from 'tessa/platform/storage';
import { IValidationResultBuilder } from 'tessa/platform/validation';
import type { RequestParameter } from 'tessa/views/metadata';
import type { SchemeDbType } from 'tessa/platform';
export declare class ViewControlDataProviderRequest {
    sortingColumns: SortingColumn[];
    selectedMasterRowData: IStorage | null;
    selectedMasterColumnName: string | null;
    parametersActions: Array<(parameters: RequestParameter[]) => void>;
    validationResult: IValidationResultBuilder;
}
export declare class ViewControlDataProviderResponse {
    validationResult: IValidationResultBuilder;
    constructor(validationResult: IValidationResultBuilder);
    calculatedRowCount: number;
    columns: Array<[string, SchemeDbType]>;
    rows: Array<IStorage>;
}
export interface IViewControlDataProvider {
    getDataAsync(request: ViewControlDataProviderRequest): Promise<ViewControlDataProviderResponse>;
}
export declare class ViewControlDataProvider implements IViewControlDataProvider {
    readonly view: ITessaView;
    constructor(view: ITessaView);
    getDataAsync(request: ViewControlDataProviderRequest): Promise<ViewControlDataProviderResponse>;
    private createRequest;
    private createParametersForRequest;
    private fillColumns;
    private fillRows;
}
