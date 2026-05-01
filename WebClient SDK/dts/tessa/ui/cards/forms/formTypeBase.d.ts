import { IFormType } from '../formTypeRegistry';
import { ICardModel, IFormViewModel } from '../interfaces';
import { CardTypeForm, CardTypeControl } from 'tessa/cards/types';
import { FormCreationOptions } from 'tessa/ui';
/**
 * Базовый класс для типа формы, используемой в автоматическом UI карточки по умолчанию.
 */
export declare abstract class FormTypeBase implements IFormType {
    protected abstract createFormCore(form: CardTypeForm, parentControl: CardTypeControl | null, model: ICardModel, formCreationOptions: FormCreationOptions): IFormViewModel;
    createForm(form: CardTypeForm, parentControl: CardTypeControl | null, model: ICardModel, formCreationOptions?: FormCreationOptions, skipInitialization?: boolean): IFormViewModel;
}
