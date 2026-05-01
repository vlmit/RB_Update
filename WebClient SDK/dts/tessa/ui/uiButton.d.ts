import * as React from 'react';
import { IUIContext } from './uiContext';
import { Visibility } from 'tessa/platform';
export declare class UIButton {
    constructor(caption: string | null, buttonAction?: ((btn: UIButton, e?: React.MouseEvent) => void) | null, icon?: string | null, isEnabled?: boolean | (() => boolean) | null, visibility?: Visibility | (() => Visibility) | null, className?: string | (() => string | null) | null, style?: React.CSSProperties | (() => React.CSSProperties | null) | null, contextExecutor?: ((action: (context: IUIContext) => void) => void) | null, child?: UIButton[] | null, name?: string | null, tooltip?: string | null);
    private _caption;
    private _captionPosition;
    private _icon;
    private _title;
    private _buttonAction;
    private _isEnabled;
    private _visibility;
    private _className;
    private _style;
    private _dropDownClassName;
    private _closeRequest;
    private _isIsEnabledFunc;
    private _isVisibilityFunc;
    private _isClassNameFunc;
    private _isStyleFunc;
    private _isDropDownClassNameFunc;
    private _reactComponent;
    private _contextExecutor?;
    private _asyncGuard;
    private _child;
    readonly name: string;
    get caption(): string | null;
    set caption(value: string | null);
    get captionPosition(): 'after' | 'before' | null;
    set captionPosition(value: 'after' | 'before' | null);
    get icon(): string | null;
    set icon(value: string | null);
    get isEnabled(): boolean;
    get visibility(): Visibility;
    get className(): string | null;
    get style(): React.CSSProperties | null;
    get dropDownClassName(): string | null;
    get reactComponent(): ((props: UIButtonComponentProps) => JSX.Element) | null;
    get child(): UIButton[];
    get tooltip(): string | null;
    set tooltip(value: string | null);
    private handleButtonAction;
    onClick: (e?: React.MouseEvent<Element, MouseEvent> | undefined) => void;
    setCloseRequest(request: (result?: any) => void): void;
    close(result?: any): void;
    setIsEnabled(isEnabled?: boolean | (() => boolean) | null): void;
    setVisibility(visibility?: Visibility | (() => Visibility) | null): void;
    setClassName(className?: string | (() => string | null) | null): void;
    setStyle(style?: React.CSSProperties | (() => React.CSSProperties | null) | null): void;
    setDropDownClassName(className?: string | (() => string | null) | null): void;
    setReactComponent(component: (props: UIButtonComponentProps) => JSX.Element): void;
    setContextExecutor(contextExecutor: ((action: (context: IUIContext) => void) => void) | null, overwrite?: boolean): void;
    static create(args: {
        caption?: string | null;
        buttonAction?: (btn: UIButton, e?: React.MouseEvent) => void;
        icon?: string | null;
        isEnabled?: boolean | (() => boolean) | null;
        visibility?: Visibility | (() => Visibility) | null;
        className?: string | (() => string | null) | null;
        style?: React.CSSProperties | (() => React.CSSProperties | null) | null;
        contextExecutor?: ((action: (context: IUIContext) => void) => void) | null;
        child?: UIButton[] | null;
        name?: string | null;
        tooltip?: string | null;
    }): UIButton;
}
export interface UIButtonComponentProps {
    viewModel: UIButton;
    className?: string;
    style?: React.CSSProperties;
    dropDownClassName?: string;
    dropDownOpenPosition?: 'left' | 'right';
    dropDownOpenDirection?: 'left' | 'right';
    dropDownOpenUp?: boolean;
}
export interface UIButtonComponentState {
    isDropDownOpen: boolean;
}
export declare class UIButtonComponent extends React.Component<UIButtonComponentProps, UIButtonComponentState> {
    constructor(props: UIButtonComponentProps);
    private _dropDownRef;
    render(): JSX.Element;
    private getActions;
    private handleButtonClick;
    private handleDropDownRef;
    private handleOpenDropDown;
    private handleCloseDropDown;
}
