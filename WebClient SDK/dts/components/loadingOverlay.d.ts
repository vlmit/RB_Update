import React, { FC } from 'react';
export interface LoadingOverlayProps {
    timeout?: number;
    skipDialogsPush?: boolean;
    progress?: number | null;
    progressDelay?: number;
    text?: string | null;
}
export interface LoadingOverlayState {
    isHidden: boolean;
    hideProgress: boolean;
}
export declare class LoadingOverlay extends React.Component<LoadingOverlayProps, LoadingOverlayState> {
    private _zIndex;
    private _timeoutId;
    private _progressDelayId;
    private _overlayRef;
    constructor(props: LoadingOverlayProps);
    componentDidMount(): void;
    componentWillUnmount(): void;
    render(): JSX.Element;
    handleClick: (e: React.MouseEvent) => void;
}
export declare class LoadingOverlayWithPortal extends React.Component<LoadingOverlayProps> {
    private _el;
    constructor(props: LoadingOverlayProps);
    componentDidMount(): void;
    componentWillUnmount(): void;
    render(): JSX.Element;
}
interface ILoaderUIProps {
    wrapperClass?: string;
}
export declare const LoaderUI: FC<ILoaderUIProps>;
export default LoadingOverlay;
