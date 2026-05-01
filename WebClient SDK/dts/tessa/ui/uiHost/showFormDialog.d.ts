import * as React from 'react';
import { CardTypeFormSealed } from 'tessa/cards/types';
import { ICardModel, IFormViewModel } from 'tessa/ui/cards';
import { UIButton } from 'tessa/ui/uiButton';
import { DialogProps } from 'ui/dialog/dialog';
export interface ShowFormDialogOptions {
    backgroundHolder?: boolean;
    hideTopCloseIcon?: boolean;
}
export interface CustomFormDialogProps {
    form: IFormViewModel;
    buttons: UIButton[];
    onClose: (result: unknown) => void;
    dialogOptions?: ShowFormDialogOptions | null;
    dialogComponentProps?: DialogProps | null;
}
export declare class CustomFormDialog extends React.Component<CustomFormDialogProps> {
    render(): JSX.Element;
    private renderForm;
    private renderButtons;
    private handleClose;
}
export declare function showFormDialog(form: CardTypeFormSealed, model: ICardModel, initializeAction?: ((form: IFormViewModel, closeFunc: (result?: unknown) => void) => void) | null, buttons?: UIButton[], dialogOptions?: ShowFormDialogOptions | null, onDialogClosed?: (() => void) | null, dialogComponentProps?: DialogProps | null): Promise<unknown>;
export declare function showDialog<R = unknown>(formModel: IFormViewModel, initializeAction?: ((form: IFormViewModel, closeFunc: (result?: R) => void) => void) | null, buttons?: UIButton[], dialogOptions?: ShowFormDialogOptions | null, onDialogClosed?: (() => void) | null, dialogComponentProps?: DialogProps | null): Promise<R>;
