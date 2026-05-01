import React from 'react';
import { UIButton } from 'tessa/ui/uiButton';
import { BaseViewControlItem } from './baseViewControlItem';
import type { ViewControlViewModel } from '../viewControlViewModel';
export declare class ViewControlButtonPanelViewModel extends BaseViewControlItem {
    constructor(viewComponent: ViewControlViewModel);
    leftButtons: UIButton[];
    rightButtons: UIButton[];
}
export interface ViewControlButtonPanelProps {
    viewModel: ViewControlButtonPanelViewModel;
}
export declare class ViewControlButtonPanel extends React.Component<ViewControlButtonPanelProps> {
    render(): JSX.Element;
}
