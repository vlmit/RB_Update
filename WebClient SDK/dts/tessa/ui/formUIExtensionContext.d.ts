import { CardTypeFormSealed, CardTypeSealed } from 'tessa/cards/types';
import { ICardModel, IFormViewModel } from 'tessa/ui/cards';
import { UIButton } from 'tessa/ui/uiButton';
import { IValidationResultBuilder } from 'tessa/platform/validation';
import { IStorage } from 'tessa/platform/storage';
import { ShowFormDialogOptions } from 'tessa/ui/uiHost';
export interface IFormUIExtensionContext {
    readonly typeForm: CardTypeFormSealed;
    readonly cardType: CardTypeSealed;
    readonly model: ICardModel;
    dialogOptions?: ShowFormDialogOptions | null;
    form: IFormViewModel;
    readonly buttons: UIButton[];
    cancel: boolean;
    readonly validationResult: IValidationResultBuilder;
    readonly info: IStorage;
    onDialogClosed?: (() => void) | null;
}
export declare class FormUIExtensionContext implements IFormUIExtensionContext {
    readonly model: ICardModel;
    readonly cardType: CardTypeSealed;
    form: IFormViewModel;
    readonly typeForm: CardTypeFormSealed;
    readonly buttons: UIButton[];
    cancel: boolean;
    readonly validationResult: IValidationResultBuilder;
    readonly info: IStorage;
    dialogOptions?: ShowFormDialogOptions | null;
    onDialogClosed?: (() => void) | null;
    constructor(form: IFormViewModel, model: ICardModel, buttons: UIButton[], dialogOptions?: ShowFormDialogOptions | null);
}
