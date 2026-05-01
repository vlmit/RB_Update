import * as React from 'react';
import { IFormViewModel } from 'tessa/ui/cards/interfaces';
export interface DefaultFormTabsProps {
    viewModel: IFormViewModel;
}
export interface DefaultFormTabsState {
    isDropDownOpen: boolean;
}
export declare class DefaultFormTabs extends React.Component<DefaultFormTabsProps, DefaultFormTabsState> {
    constructor(props: DefaultFormTabsProps);
    private _tabControlRef;
    private _dropDownRef;
    render(): JSX.Element | null;
    private renderTabHeaders;
    private renderDropDown;
    private renderTabContents;
    private scrollIntoView;
    private createTabClickHandler;
    private handleTabControlRef;
    private handleDropDownRef;
    private handleOpenDropDown;
    private handleCloseDropDown;
}
