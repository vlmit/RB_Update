import * as React from 'react';
import { ElementSide } from 'common/utility';
declare class SidePanelDragHolder extends React.Component<ISidePanelDragHolderProps, {}> {
    isInit: boolean;
    holderWidth: number;
    panelRef: HTMLElement | null;
    panelWidth: number;
    startX: number | null;
    lastX: number | null;
    offsetX: number | null;
    openAmountValue: any;
    isDragging: boolean;
    openDirection: 'right' | 'left';
    closeDirection: 'right' | 'left';
    dragHolderGesture: any;
    releaseHolderGesture: any;
    dragPanelGesture: any;
    releasePanelGesture: any;
    scrollerRef: any;
    dragHolderRef: any;
    constructor(props: ISidePanelDragHolderProps);
    componentWillUnmount(): void;
    shouldComponentUpdate(nextProps: ISidePanelDragHolderProps): boolean;
    enableOrDisableIfNeeded(isOnClickOnly: boolean, panelRef: HTMLElement | null, scrollerRef: HTMLElement): void;
    init(panelRef: HTMLElement | null, scrollerRef: HTMLElement): void;
    release(): void;
    openPercentage(percentage: number): void;
    openAmount(amount: number): void;
    snapToRest(event: any): void;
    getOpenRatio(): number;
    getOffset(): number;
    getOpenAmount(): any;
    changePanelState(state: any, withTimeout?: boolean): void;
    onDrag: (event: any) => void;
    onEndDrag: (event: any) => void;
    handleClick: (event: any) => void;
    render(): JSX.Element | null;
}
export interface ISidePanelDragHolderProps {
    side: ElementSide.ELEMENT_LEFT_SIDE_MENU | ElementSide.ELEMENT_RIGHT_SIDE_MENU;
    isPanelOpen: boolean;
    changePanelState: any;
    isOnClickOnly?: boolean;
}
export default SidePanelDragHolder;
