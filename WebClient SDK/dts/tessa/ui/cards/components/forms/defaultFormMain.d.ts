import * as React from 'react';
import { IFormViewModel } from 'tessa/ui/cards/interfaces';
export interface DefaultFormMainProps {
    viewModel: IFormViewModel;
}
export interface DefaultFormMainState {
    isDropDownOpen: boolean;
}
export declare class DefaultFormMain extends React.Component<DefaultFormMainProps, DefaultFormMainState> {
    constructor(props: DefaultFormMainProps);
    private _dropDownRef;
    private _tabScrollInViewDispose;
    private _mouseClicked;
    private _treeWidth;
    private _bodyRef;
    private _previewRef;
    private _resizerRef;
    componentDidMount(): void;
    componentWillUnmount(): void;
    render(): JSX.Element;
    private handleContextMenu;
    private isTabSelected;
    private isPreviewTabSelected;
    private renderTabContent;
    private renderDropDown;
    private renderPreviewButtons;
    private handleCloseInplacePreview;
    private handleSwitchToPreviewTab;
    private handleChangeSides;
    private handleEqualizeSidesWidth;
    private createTabClickHandler;
    private handleDropDownRef;
    private handleSwitchDropDown;
    private handleCloseDropDown;
    private handleMouseUp;
    private handleMouseDown;
    private handleMouseMove;
}
