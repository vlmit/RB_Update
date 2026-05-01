import * as React from 'react';
import { ValidationResult } from 'tessa/platform/validation';
export interface ValidationResultDialogProps {
    result: ValidationResult;
    caption?: string;
    onClose: () => void;
}
export declare class ValidationResultDialog extends React.Component<ValidationResultDialogProps> {
    constructor(props: ValidationResultDialogProps);
    private _childref;
    render(): JSX.Element;
    private handleClose;
}
