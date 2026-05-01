import * as React from 'react';
declare class SidePanelScroller extends React.Component<ISidePanelScrollerProps, {}> {
    isInit: boolean;
    FTScroller: any;
    containerRef: any;
    listRef: any;
    scroller: any;
    timeout: any;
    rqf: any;
    componentWillUnmount(): void;
    shouldComponentUpdate(): boolean;
    init(FTScroller: any, containerRef: any, listRef: any): void;
    checkVisibility: () => void;
    show(): void;
    hide(): void;
    onResize: () => void;
    scroll: (up: boolean) => () => void;
    render(): JSX.Element;
}
export interface ISidePanelScrollerProps {
    side: string;
}
export default SidePanelScroller;
