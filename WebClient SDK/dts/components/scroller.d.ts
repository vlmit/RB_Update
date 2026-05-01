import * as React from 'react';
declare class Scroller extends React.Component<IScrollerProps, {}> {
    FTScroller: any;
    componentWillUnmount(): void;
    init(containerRef: any): any;
    render(): JSX.Element;
}
export interface IScrollerProps {
    children?: any;
    scrollbars: boolean;
    scrollingX: boolean;
    scrollingY: boolean;
    hwAccelerationClass?: string;
    bouncing: boolean;
}
export default Scroller;
