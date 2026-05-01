import { FormTypeBase } from './formTypeBase';
import { ICardModel, IFormViewModel } from '../interfaces';
import { CardTypeControl, CardTypeForm } from 'tessa/cards/types';
import { FormCreationOptions } from 'tessa/ui';
/**
 * Тип формы, используемой в автоматическом UI карточки по умолчанию.
 */
export declare class DefaultFormType extends FormTypeBase {
    static get formClass(): string;
    protected createFormCore(form: CardTypeForm, parentControl: CardTypeControl | null, model: ICardModel, formCreationOptions?: FormCreationOptions): IFormViewModel;
}
