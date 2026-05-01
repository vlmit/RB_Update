import { ContentPlaceArea, ContentPlaceOrder, FilterTextViewModel } from 'tessa/ui/views/content';
import { IWorkplaceViewComponent } from 'tessa/ui/views';

/**
 * Модель представления, позволяющая переопределить действие, выполняемое при нажатии на кнопку открытия диалога с параметрами фильтрации представления отображаемую при применённом фильтре.
 */
export class CustomFilterTextViewModel extends FilterTextViewModel {
  //#region ctor

  constructor(
    openFilterCommand: (viewComponent: IWorkplaceViewComponent) => Promise<void>,
    viewComponent: IWorkplaceViewComponent,
    area: ContentPlaceArea = ContentPlaceArea.ContextPanel,
    order: number = ContentPlaceOrder.BeforeAll
  ) {
    super(viewComponent, area, order);
    this._openFilterCommand = openFilterCommand;
  }

  //#endregion

  //#region fields

  private _openFilterCommand: (viewComponent: IWorkplaceViewComponent) => Promise<void>;

  //#endregion

  //#region base overrides

  async openFilter() {
    await this._openFilterCommand(this.viewComponent);
  }

  //#endregion
}
