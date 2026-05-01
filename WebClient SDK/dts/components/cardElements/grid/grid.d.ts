import * as React from 'react';
import { GridUIViewModel } from './models';
import WideGrid from './wideGrid';
import ThinGrid from './thinGrid';
export default class Grid extends React.Component<IGridProps, IGridState> {
    elementQueryRef: HTMLElement | null;
    rqf: number | null;
    timeout: number;
    containerId: string;
    openedChildRows: number[];
    containerDOM: HTMLElement | null;
    wideGridRef: WideGrid | null;
    thinGridRef: ThinGrid | null;
    stopNextResize: boolean;
    constructor(props: IGridProps);
    componentDidMount(): void;
    generateContainerUniqueId(): string;
    handleResize: () => void;
    handleBreakpointResize: (layout: string) => void;
    focus(opt?: FocusOptions): void;
    render(): JSX.Element;
}
export interface IGridProps {
    viewModel: GridUIViewModel;
    isShowNoDataText?: string | null;
    wideVisibleSize?: number;
    tabIndex?: number;
    onFocus?: () => void;
    onBlur?: () => void;
}
export interface IGridState {
    layout: string;
}
