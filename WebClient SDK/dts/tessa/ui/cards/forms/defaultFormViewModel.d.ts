import { FormViewModelBase, FormViewModelBaseState } from './formViewModelBase';
import { ICardModel, IFormState, IBlockViewModel, IControlViewModel, IControlState } from '../interfaces';
import { CardTypeForm } from 'tessa/cards/types';
/**
 * Базовый класс для модели представления для формы по умолчанию.
 */
export declare abstract class DefaultFormViewModel extends FormViewModelBase {
    constructor(form: CardTypeForm, model: ICardModel, blocks: IBlockViewModel[], blockInterval: number, filePreviewIsDisabled: boolean, horizontalInterval?: number, gridColumns?: (number | null)[], gridRows?: (number | null)[]);
    protected _controls: ReadonlyMap<string, IControlViewModel>;
    /**
     * Контейнер для именованных элементов управления, содержащихся в текущей форме
     * или в её дочерних формах.
     */
    get controls(): ReadonlyMap<string, IControlViewModel>;
    get filePreviewIsDisabledInternal(): boolean;
    getState(): IFormState;
    get stretchVertically(): boolean;
}
/**
 * Состояние формы DefaultFormViewModel.
 * Наследники класса DefaultFormViewModel могут унаследовать класс DefaultFormViewModelState,
 * добавив в него сохраняемую информацию.
 */
export declare class DefaultFormViewModelState extends FormViewModelBaseState implements IFormState {
    constructor(form: DefaultFormViewModel);
    readonly controlStatesByName: Map<string, IControlState>;
    apply(form: DefaultFormViewModel): boolean;
}
