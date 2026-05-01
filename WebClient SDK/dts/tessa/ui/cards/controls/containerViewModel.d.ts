import { CardTypeTabControl, CardTypeControl } from 'tessa/cards/types';
import { ValidationResultBuilder } from 'tessa/platform/validation';
import { TabSelectedContext } from '../tabSelectedEventArgs';
import { ControlViewModelBase } from './controlViewModelBase';
import { ICardModel, IFormViewModel } from '../interfaces';
export declare class ContainerViewModel extends ControlViewModelBase {
    constructor(control: CardTypeTabControl, parentControl: CardTypeControl | null, model: ICardModel);
    private _form;
    get form(): IFormViewModel;
    set form(value: IFormViewModel);
    get isEmpty(): boolean;
    onUnloading(validationResult: ValidationResultBuilder): void;
    notifyTabSelected(context: TabSelectedContext): Promise<void>;
    notifyTabDeselected(context: TabSelectedContext): Promise<void>;
}
