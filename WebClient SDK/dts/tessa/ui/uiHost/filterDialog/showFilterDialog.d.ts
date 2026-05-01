import { FilterDialogViewModel } from './filterDialogViewModel';
import { ViewMetadataSealed } from 'tessa/views/metadata';
import { RequestParameter } from 'tessa/views/metadata/requestParameter';
export declare function showFilterDialog(metadata: ViewMetadataSealed, parameters: ReadonlyArray<RequestParameter>, onInitialized?: (ctx: FilterDialogViewModel) => Promise<void>): Promise<RequestParameter[] | null>;
