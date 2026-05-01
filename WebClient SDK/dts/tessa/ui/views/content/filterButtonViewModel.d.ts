import { BaseContentItem } from './baseContentItem';
import { ContentPlaceArea } from './contentPlaceArea';
import { IWorkplaceViewComponent } from '../workplaceViewComponent';
import { Visibility } from 'tessa/platform';
import { IViewComponentBase } from '../viewComponentBase';
export interface IFilterButtonViewModel {
    toolTip: string;
    visibility: Visibility;
    readonly isLoading: boolean;
    openFilter(): any;
}
export declare class FilterButtonViewModel<T extends IViewComponentBase = IWorkplaceViewComponent> extends BaseContentItem<T> implements IFilterButtonViewModel {
    constructor(viewComponent: T, area?: ContentPlaceArea, order?: number);
    protected _toolTip: string;
    protected _visibility: Visibility;
    get toolTip(): string;
    set toolTip(value: string);
    get visibility(): Visibility;
    set visibility(value: Visibility);
    get isLoading(): boolean;
    openFilter(): Promise<void>;
}
