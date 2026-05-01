import * as React from 'react';
import { GridUIViewModel, GridUIRowModel, GridUICellModel, GridUIBlockViewModel } from './models';
declare class WideGrid extends React.Component<IWideGridProps, {}> {
    private _previousToolTip;
    private _currentToolTip;
    private _calculatedToolTip;
    containerDOM: HTMLElement | null;
    standartTableDOM: HTMLTableElement | null;
    wideTableDOM: HTMLTableElement | null;
    maxWordLength: number;
    wideTableRef: HTMLTableElement | null;
    timeout: number;
    rqf: any;
    constructor(props: IWideGridProps);
    componentDidMount(): void;
    componentDidUpdate(): void;
    onResize: () => void;
    onResizeInstant: () => void;
    isVisible(): boolean | null;
    calculateWideTableSize(): void;
    calculateWideTableSizeInternal(copyStandartTableDOM: any): void;
    remountAndCalculateWideTableSize(): void;
    resetContentToStandartSize(table?: HTMLTableElement | null): void;
    createHeaderArray(): JSX.Element[];
    createRow(parentKey: string, row: GridUIRowModel, offset: number, columnsCount: number, isTree: boolean): JSX.Element[];
    createRowBlock(block: GridUIBlockViewModel, offset: number, columnsCount: number, isTree: boolean): JSX.Element[];
    createRowsArray(columnsCount: number): JSX.Element[];
    createNoRowBlock(columnsCount: number): JSX.Element[];
    render(): JSX.Element;
    handleCellContextMenu: (cell: GridUICellModel) => (e: any) => void;
    handleRowContextMenu: (row: GridUIRowModel) => (e: any) => void;
    handleColumnContextMenu: (header: any) => (e: any) => void;
    handleRowCheck: (row: any) => (e: any) => void;
    handleKeyDown: (event: React.KeyboardEvent) => void;
    isColumnOverflowed: (columnIndex: number) => boolean;
    focus(opt?: FocusOptions): void;
    private handleHeaderDragStart;
    private handleHeaderDragOver;
    private handleHeaderDrop;
    private handleHeaderDragEnd;
}
export interface IWideGridProps {
    viewModel: GridUIViewModel;
    isShowNoDataText?: string | null;
    containerId: string;
    tabIndex?: number;
    onFocus?: () => void;
    onBlur?: () => void;
    container?: React.RefObject<HTMLElement>;
}
export default WideGrid;
export declare const WideGridRow: (args: {
    row: GridUIRowModel;
    rowIndex: string;
    cells: (JSX.Element | null)[] | null;
    isToggleNeeded?: boolean | undefined;
    cellSelectionMode?: boolean | undefined;
    multiSelectEnabled?: boolean | undefined;
    handleRowCheck: (e: React.ChangeEvent | React.MouseEvent) => void;
    handleContextMenu: (e: React.MouseEvent) => void;
    handleScrollToActiveRow: (row: GridUIRowModel, getRowBoundingClientRect: () => DOMRect | null, container?: React.RefObject<HTMLElement> | undefined) => void;
    container?: React.RefObject<HTMLElement> | undefined;
}) => JSX.Element | null;
export declare const WideGridRowCC: (args: {
    rowIndex: string;
    columnsCount: number;
    childColumns: JSX.Element[] | null;
    row: GridUIRowModel;
    cellSelectionMode?: boolean;
}) => JSX.Element | null;
export declare class WideGridCell extends React.Component<IWideGridCell, {}> {
    private _previousToolTip;
    private _currentToolTip;
    private _calculatedToolTip;
    dropdownRef: HTMLDivElement | null;
    render(): JSX.Element;
}
export interface IWideGridCell {
    columnIndex: number;
    columnClassName?: string;
    content: JSX.Element | null;
    row?: GridUIRowModel;
    cell: GridUICellModel;
    isChildRowsExist?: boolean;
    offset?: number;
    style?: React.CSSProperties;
    parentStyle?: React.CSSProperties;
    cellSelectionMode?: boolean;
    handleContextMenu: (e: React.MouseEvent) => void;
    isToggleNeeded?: boolean;
    isTree: boolean;
    toolTip?: string;
}
