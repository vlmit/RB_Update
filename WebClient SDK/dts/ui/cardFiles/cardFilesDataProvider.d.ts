import { IViewControlDataProvider, FileListViewModel, ViewControlDataProviderResponse, ViewControlDataProviderRequest, FileViewModel } from 'tessa/ui/cards/controls';
import { RequestParameter, ViewMetadataSealed } from 'tessa/views/metadata';
import { IStorage } from 'tessa/platform/storage';
export declare class CardFilesDataProvider implements IViewControlDataProvider {
    readonly viewMetadata: ViewMetadataSealed;
    readonly fileControl: FileListViewModel;
    constructor(viewMetadata: ViewMetadataSealed, fileControl: FileListViewModel);
    getDataAsync(request: ViewControlDataProviderRequest): Promise<ViewControlDataProviderResponse>;
    private addFieldDescriptions;
    private populateDataRowsAsync;
    static buildParametersCollectionFromRequest(request: ViewControlDataProviderRequest): RequestParameter[];
    static mapFileToRow(file: FileViewModel, fileControl: FileListViewModel): IStorage;
}
