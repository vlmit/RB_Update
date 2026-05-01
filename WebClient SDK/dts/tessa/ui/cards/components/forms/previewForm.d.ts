import * as React from 'react';
import { DefaultFormMainViewModel } from 'tessa/ui/cards/forms';
export interface PreviewFormProps {
    viewModel: DefaultFormMainViewModel;
    innerRef?: React.RefObject<HTMLElement>;
    innerStyle?: React.CSSProperties;
}
export declare class PreviewForm extends React.Component<PreviewFormProps> {
    render(): JSX.Element | null;
}
