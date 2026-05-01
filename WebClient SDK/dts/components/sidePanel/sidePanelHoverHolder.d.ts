import * as React from 'react';
import { ElementSide } from 'common/utility';
declare class SidePanelHoverHolder extends React.Component<ISidePanelHoverHolderProps, {}> {
    isInit: boolean;
    panelRef: any;
    timeoutTimer: any;
    componentWillUnmount(): void;
    shouldComponentUpdate(nextProps: any): boolean;
    init(panelRef: any): void;
    changePanelState(state: any): void;
    onMouseEnter: () => void;
    onMouseLeave: () => void;
    onHolderClick: () => void;
    render(): JSX.Element;
}
export interface ISidePanelHoverHolderProps {
    side: ElementSide.ELEMENT_LEFT_SIDE_MENU | ElementSide.ELEMENT_RIGHT_SIDE_MENU;
    isPanelOpen: boolean;
    openEventType?: number;
    changePanelState: any;
    isOnClickOnly?: boolean;
}
export default SidePanelHoverHolder;
