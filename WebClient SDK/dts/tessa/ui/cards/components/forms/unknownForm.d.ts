import * as React from 'react';
import { IFormViewModel } from 'tessa/ui/cards/interfaces';
export interface UnknownFormProps {
    viewModel: IFormViewModel;
}
export declare class UnknownForm extends React.Component<UnknownFormProps> {
    render(): JSX.Element;
}
