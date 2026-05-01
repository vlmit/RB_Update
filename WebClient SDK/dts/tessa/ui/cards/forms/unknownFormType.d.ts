import { FormTypeBase } from './formTypeBase';
import { ICardModel, IFormViewModel } from '../interfaces';
import { CardTypeForm, CardTypeControl } from 'tessa/cards/types';
/**
 * Тип формы, используемой в автоматическом UI карточки по умолчанию.
 */
export declare class UnknownFormType extends FormTypeBase {
    caption: string;
    constructor(key: string);
    static get formClass(): string;
    protected createFormCore(form: CardTypeForm, _parentControl: CardTypeControl | null, model: ICardModel): IFormViewModel;
}
