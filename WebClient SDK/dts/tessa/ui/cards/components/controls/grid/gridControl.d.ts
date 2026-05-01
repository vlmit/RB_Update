import * as React from 'react';
import { ControlProps } from '../controlProps';
import { IControlViewModel } from 'tessa/ui/cards/interfaces';
export declare class GridControl extends React.Component<ControlProps<IControlViewModel>> {
    private _mainRef;
    constructor(props: ControlProps<IControlViewModel>);
    componentDidMount(): void;
    componentWillUnmount(): void;
    componentDidUpdate(prevProps: ControlProps<IControlViewModel>): void;
    _currentSearch: string | undefined;
    setCurrentPage: (value: number | undefined) => void;
    _currentPage: number | undefined;
    get isEnabled(): boolean;
    get rowsPerPage(): number;
    get pages(): number;
    get currentPage(): number;
    movePage: (isForward: boolean) => void;
    setPage: (currentPage: number | undefined) => void;
    render(): JSX.Element | null;
    focus(opt?: FocusOptions): void;
    private gridHeaders;
    private gridRows;
    private handleKeyDown;
    private handleFocus;
    private handleBlur;
    private getHighlightedContent;
    private handleHeaderDrop;
}
