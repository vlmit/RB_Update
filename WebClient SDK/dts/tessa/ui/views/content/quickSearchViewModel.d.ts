/// <reference types="react" />
import { BaseContentItem } from './baseContentItem';
import { ContentPlaceArea } from './contentPlaceArea';
import { IWorkplaceViewComponent } from '../workplaceViewComponent';
import { IViewComponentBase } from '../viewComponentBase';
export interface IQuickSearchViewModel {
    quickSearchEnabled: boolean;
    searchText: string;
    placeholder: string;
    readonly isLoading: boolean;
    dispose: () => void;
    bindReactComponentRef: (ref: React.RefObject<any>) => void;
    unbindReactComponentRef: () => void;
    search: () => void;
    focusControlWhenDataWasLoaded: boolean;
}
export declare class QuickSearchViewModel<T extends IViewComponentBase = IWorkplaceViewComponent> extends BaseContentItem<T> implements IQuickSearchViewModel {
    constructor(viewComponent: T, area?: ContentPlaceArea, order?: number);
    protected _searchText: string;
    protected _placeholder: string;
    protected _reactComponentRef: React.RefObject<any> | null;
    get quickSearchEnabled(): boolean;
    set quickSearchEnabled(value: boolean);
    get searchText(): string;
    set searchText(value: string);
    get placeholder(): string;
    set placeholder(value: string);
    get isLoading(): boolean;
    focusControlWhenDataWasLoaded: boolean;
    dispose(): void;
    bindReactComponentRef(ref: React.RefObject<any>): void;
    unbindReactComponentRef(): void;
    focus(opt?: FocusOptions): void;
    search(): void;
}
