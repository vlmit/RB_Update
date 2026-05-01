import { ApplicationExtension, IApplicationExtensionMetadataContext } from 'tessa';
export declare class WorkplacesExtension extends ApplicationExtension {
    afterMetadataReceived(context: IApplicationExtensionMetadataContext): Promise<void>;
    private getFilteredWorkplacesMetadata;
    private getUserDefaultOpeningWorkplaces;
    private getWorkplaces;
}
