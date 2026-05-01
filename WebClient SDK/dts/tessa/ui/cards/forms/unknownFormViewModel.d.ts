import { FormViewModelBase } from './formViewModelBase';
import { CardTypeForm } from 'tessa/cards/types';
import { ICardModel } from '..';
/**
 * Модель представления для формы по умолчанию основной части карточки.
 */
export declare class UnknownFormViewModel extends FormViewModelBase {
    constructor(form: CardTypeForm, model: ICardModel);
}
