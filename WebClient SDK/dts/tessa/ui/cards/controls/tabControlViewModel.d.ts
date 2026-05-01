import { ControlViewModelBase } from './controlViewModelBase';
import { CardTypeTabControl, CardTypeControl } from 'tessa/cards/types';
import { ICardModel, IFormViewModel, IControlState } from '../interfaces';
import { ValidationResultBuilder } from 'tessa/platform/validation';
/**
 * Модель представления для элемента управления вкладками.
 */
export declare class TabControlViewModel extends ControlViewModelBase {
    constructor(control: CardTypeTabControl, parentControl: CardTypeControl | null, model: ICardModel);
    private _tabs;
    private _selectedTab;
    cardModel: ICardModel;
    get tabs(): ReadonlyArray<IFormViewModel>;
    get selectedTab(): IFormViewModel | null;
    set selectedTab(value: IFormViewModel | null);
    get isEmpty(): boolean;
    get visibleTabs(): ReadonlyArray<IFormViewModel>;
    addTab(tab: IFormViewModel): void;
    removeTab(tab: IFormViewModel): boolean;
    getState(): IControlState;
    setState(state: IControlState): boolean;
    onUnloading(validationResult: ValidationResultBuilder): void;
}
/**
 * Состояние контрола с табами.
 */
export declare class TabControlState implements IControlState {
    constructor(control: TabControlViewModel);
    readonly selectedTabIndex: number;
    apply(control: TabControlViewModel): boolean;
}
