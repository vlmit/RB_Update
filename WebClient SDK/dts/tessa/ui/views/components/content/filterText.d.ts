import * as React from 'react';
import { IFilterTextViewModel } from '../../content';
export interface FilterTextProps {
    viewModel: IFilterTextViewModel;
}
export declare class FilterText extends React.Component<FilterTextProps> {
    private _buttonWidthInRem;
    render(): JSX.Element | null;
    private renderParamsAsText;
    private renderItemName;
    private renderItemValue;
    private getContentForParameter;
    private renderButtons;
    private handleOpenFilter;
    private handleClearFilter;
}
