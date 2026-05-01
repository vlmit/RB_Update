import { ICardModel, IFormViewModel } from './interfaces';
import { CardTypeControl, CardTypeForm } from 'tessa/cards/types';
import { FormCreationOptions } from 'tessa/ui';
export interface IFormType {
    createForm(form: CardTypeForm, parentControl: CardTypeControl | null, model: ICardModel, formCreationOptions?: FormCreationOptions): IFormViewModel;
}
export interface IFormTypeRegistry {
    register(key: string, factory: () => IFormType): any;
    get(form: CardTypeForm): IFormType;
    getAll(): IFormType[];
    createCardForm(model: ICardModel): IFormViewModel;
}
export declare class FormTypeRegistry implements IFormTypeRegistry {
    private constructor();
    private static _instance;
    static get instance(): IFormTypeRegistry;
    private registry;
    register(key: string, factory: () => IFormType): void;
    get(form: CardTypeForm): IFormType;
    getAll(): IFormType[];
    createCardForm(model: ICardModel): IFormViewModel;
}
