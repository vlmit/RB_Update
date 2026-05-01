import React from 'react';
import { PopoverInternalProps } from './popoverInternal';
declare class Popover extends React.Component<PopoverProps, PopoverState> {
    private _el;
    private _unmounted;
    constructor(props: PopoverProps);
    componentDidMount(): void;
    componentWillUnmount(): void;
    render(): React.ReactPortal;
}
export interface PopoverState {
    mounted: boolean;
}
export interface PopoverProps extends PopoverInternalProps {
    isOpened?: boolean;
    instaUnmount?: boolean;
    cssTransitionEnabled?: boolean;
}
export default Popover;
