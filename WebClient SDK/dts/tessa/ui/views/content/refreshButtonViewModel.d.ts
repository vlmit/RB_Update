import { BaseContentItem } from './baseContentItem';
import { ContentPlaceArea } from './contentPlaceArea';
import { IWorkplaceViewComponent } from '../workplaceViewComponent';
import { IViewComponentBase } from '../viewComponentBase';
export interface IRefreshButtonViewModel {
    toolTip: string;
    readonly isLoading: boolean;
    refresh(): any;
}
export declare class RefreshButtonViewModel<T extends IViewComponentBase = IWorkplaceViewComponent> extends BaseContentItem<T> implements IRefreshButtonViewModel {
    constructor(viewComponent: T, area?: ContentPlaceArea, order?: number);
    private _toolTip;
    get toolTip(): string;
    set toolTip(value: string);
    get isLoading(): boolean;
    refresh(): void;
}
