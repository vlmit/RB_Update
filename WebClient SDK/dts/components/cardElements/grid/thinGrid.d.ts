import * as React from 'react';
import { TextAlignProperty } from 'csstype';
import { GridUIViewModel, GridUICellModel, GridUIRowModel, GridUIBlockViewModel } from './models';
export default class ThinGrid extends React.Component<IThinGridProps, {}> {
    containerDOM: any;
    createRowsArrayBase(): JSX.Element[];
    createRow(row: GridUIRowModel): JSX.Element[];
    createRowBlock(block: GridUIBlockViewModel, offset: number, isRoot?: boolean): JSX.Element[];
    createRowsArray(): JSX.Element[];
    createNoRowBLock(): JSX.Element[];
    render(): JSX.Element;
    handleCellContextMenu: (cell: any) => (e: any) => void;
    handleColumnContextMenu: (header: any) => (e: any) => void;
    handleRowCheck: (row: any) => (e: any) => void;
    focus(opt?: FocusOptions): void;
}
export interface IThinGridProps {
    viewModel: GridUIViewModel;
    isShowNoDataText?: string | null;
    tabIndex?: number;
    onFocus?: () => void;
    onBlur?: () => void;
}
export declare const ThinGridCell: (args: {
    cell: GridUICellModel;
    row: GridUIRowModel;
    columnIndex: number;
    caption: string;
    bold: boolean;
    alignment: TextAlignProperty;
    isHidden: boolean;
    cellSelectionMode?: boolean | undefined;
    handleColumnContextMenu: (e: React.MouseEvent) => void;
    handleCellContextMenu: (e: React.MouseEvent) => void;
}) => JSX.Element;
export declare const ThinGridRow: (args: {
    rowIndex: string;
    row: GridUIRowModel;
    cells: JSX.Element[] | null;
    hasActiveHiddenCell: boolean;
    cellSelectionMode?: boolean | undefined;
    multiSelectEnabled?: boolean | undefined;
    handleRowCheck: (e: React.ChangeEvent | React.MouseEvent) => void;
}) => JSX.Element | null;
