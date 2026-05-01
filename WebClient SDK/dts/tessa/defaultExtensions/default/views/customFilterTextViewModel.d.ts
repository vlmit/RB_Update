import { ContentPlaceArea, FilterTextViewModel } from 'tessa/ui/views/content';
import { IWorkplaceViewComponent } from 'tessa/ui/views';
/**
 * Модель представления, позволяющая переопределить действие, выполняемое при нажатии на кнопку открытия диалога с параметрами фильтрации представления отображаемую при применённом фильтре.
 */
export declare class CustomFilterTextViewModel extends FilterTextViewModel {
    constructor(openFilterCommand: (viewComponent: IWorkplaceViewComponent) => Promise<void>, viewComponent: IWorkplaceViewComponent, area?: ContentPlaceArea, order?: number);
    private _openFilterCommand;
    openFilter(): Promise<void>;
}
