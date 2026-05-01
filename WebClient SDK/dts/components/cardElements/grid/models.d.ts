import { CSSProperties } from 'react';
import { TextAlignProperty } from 'csstype';
import { MenuAction } from 'tessa/ui';
import { ColumnSortDirection } from 'tessa/ui/cards/controls/columnSortDirection';
export declare class GridUIRowModel {
    rowId: string;
    cells: GridUICellModel[];
    private _onClick?;
    private _onDoubleClick?;
    onToggle: (isToggleNeeded?: boolean) => void;
    isToggled: boolean;
    private _isSelected;
    private _isLastSelected;
    private setSelected;
    parentRowId?: string | null;
    parentBlockId: string | undefined;
    isParentToggled: () => boolean;
    getMenuActions?: (vm: GridUIViewModel, columnIndex?: number) => MenuAction[];
    vm: GridUIViewModel;
    /**
     * Необработанные данные с сервера для текущей строки
     *
     * @type {any[]}
     * @memberof GridUIRowModel
     */
    rawData: CellValue[] | undefined;
    menuActions: MenuAction[];
    onClickWrapper: (e: React.MouseEvent) => void;
    onMouseDown?: (e: React.MouseEvent) => void;
    constructor(args: IGridUIRowModelConstructor & IParentToggled & {
        vm: GridUIViewModel;
    });
    getContextMenu(e: React.MouseEvent, columnIndex?: number): MenuAction[];
    setToggle(): void;
    get isSelected(): boolean;
    set isSelected(value: boolean);
    get isLastSelected(): boolean;
    private _style?;
    get style(): CSSProperties;
    getColumnValue: (columnAlias: string) => any;
    getAppearencesSetting: () => never[];
    onClick: (e: React.MouseEvent | React.KeyboardEvent, columnIndex?: number | undefined) => void;
    onDoubleClick: (e: React.MouseEvent | React.KeyboardEvent, columnIndex?: number | undefined) => void;
    toolTip?: string;
}
export interface IGridUIRowModel {
    rowId: string;
    parentRowId?: string | null;
    parentBlockId?: string;
    cells: IGridUICellModel[];
    isToggled?: boolean;
    onClick?: (e: React.MouseEvent, columnIndex?: number) => void;
    onDoubleClick?: (e: React.MouseEvent, columnIndex?: number) => void;
    onMouseDown?: (e: React.MouseEvent) => void;
    getIsSelected: () => boolean;
    getIsLastSelected: () => boolean;
    setSelected: (value: boolean) => void;
    onToggle?: (isToggleNeeded: boolean) => void;
    getMenuActions?: (vm: GridUIViewModel, columnIndex?: number) => MenuAction[];
    style?: React.CSSProperties;
    rawData?: CellValue[];
    menuActions?: MenuAction[];
    toolTip?: string;
}
export interface IUIViewMetadata {
    appearance?: string | null;
}
export interface IGridUIRowModelConstructor extends IGridUIRowModel {
    cells: GridUICellModel[];
}
export interface IGridUICompatibleModal {
    canSelectMultipleItems: boolean;
    rows: ReadonlyArray<{
        rowId: string;
        isSelected: boolean;
    }>;
    selectedRows: ReadonlyArray<{
        rowId: string;
        isSelected: boolean;
    }>;
}
export declare class GridUICellModel {
    content: string | Function;
    className: string | undefined;
    private vmGetter;
    get vm(): GridUIRowModel;
    private _onClick?;
    private _onDoubleClick?;
    private _getIsSelected;
    private _setSelected;
    onClickWrapper: (e: React.MouseEvent) => void;
    _onMouseDown?: (e: React.MouseEvent) => void;
    private _index;
    leftTags?: CellTag[];
    rightTags?: CellTag[];
    constructor(args: IGridUICellModel & {
        vmGetter: () => GridUIRowModel;
        index: number;
    });
    private _style?;
    get style(): CSSProperties;
    getContextMenu(e: React.MouseEvent): MenuAction[];
    get isSelected(): boolean;
    set isSelected(value: boolean);
    onClick: (e: React.MouseEvent | React.KeyboardEvent) => void;
    onDoubleClick: (e: React.MouseEvent | React.KeyboardEvent) => void;
    onMouseDown: (e: React.MouseEvent) => void;
    get index(): number;
    toolTip?: string;
}
export interface IGridUICellModel {
    content: string | Function;
    className?: string;
    style?: React.CSSProperties;
    toolTip?: string;
    leftTags?: CellTag[];
    rightTags?: CellTag[];
    onClick?: (e: React.MouseEvent | React.KeyboardEvent) => void;
    onDoubleClick?: (e: React.MouseEvent | React.KeyboardEvent) => void;
    onMouseDown?: (e: React.MouseEvent) => void;
    getIsSelected?: () => boolean;
    setSelected?: (value: boolean) => void;
}
export declare class GridUIHeaderCellModel {
    alias: string;
    caption: string;
    menuActions: MenuAction[];
    getMenuActions: ((vm: GridUIViewModel) => MenuAction[]) | undefined;
    sortDirection: ColumnSortDirection;
    vm: GridUIViewModel;
    canSort: boolean;
    bold: boolean;
    alignment: TextAlignProperty;
    toolTip?: string;
    index: number;
    _onClick?: (e: React.MouseEvent) => void;
    _onDoubleClick?: (e: React.MouseEvent) => void;
    _onMouseDown?: (e: React.MouseEvent) => void;
    onClickWrapper: (e: React.MouseEvent) => void;
    constructor(args: IGridUIHeaderCellModel & {
        vm: GridUIViewModel;
        index: number;
    });
    getContextMenu(): MenuAction[];
    onClick: (e: React.MouseEvent) => void;
    onDoubleClick: (e: React.MouseEvent) => void;
    onMouseDown: (e: React.MouseEvent) => void;
}
export interface IGridUIHeaderCellModel {
    alias: string;
    caption: string;
    menuActions?: MenuAction[];
    /**
     * Имеет приоритет над массивом
     */
    getMenuActions?: (vm: GridUIViewModel) => MenuAction[];
    sortDirection?: ColumnSortDirection;
    onClick?: (e: React.MouseEvent) => void;
    onDoubleClick?: (e: React.MouseEvent) => void;
    onMouseDown?: (e: React.MouseEvent) => void;
    meta?: IUIColumnMetadata;
    canSort?: boolean;
    bold?: boolean;
    alignment?: TextAlignProperty;
    toolTip?: string;
}
export interface IUIColumnMetadata {
    appearance?: string | null;
}
export declare class GridUIViewModel {
    _rows: GridUIRowModel[];
    _headers: GridUIHeaderCellModel[];
    _blocks?: GridUIBlockViewModel[];
    _horizontalScroll: boolean;
    rawData: IViewData | undefined;
    canSelectMultipleItems: boolean;
    cellSelectionMode: boolean;
    multiSelectEnabled: boolean;
    canReorderHeaders: boolean;
    dragableHeader?: GridUIHeaderCellModel;
    keyDownHandler?: (e: React.KeyboardEvent) => void;
    headerDropHandler?: (args: {
        source: IGridUIHeaderCellModel;
        target: IGridUIHeaderCellModel;
        sourceIndex: number;
        targetIndex: number;
    }) => void;
    selectRowOnContextMenu?: boolean;
    saveTableHeightWhenEmpty?: boolean;
    checkActiveRowScroll: {
        row: GridUIRowModel;
        keyCode: 38 | 40;
    } | null;
    constructor(args: {
        canSelectMultipleItems: boolean;
        cellSelectionMode?: boolean;
        multiSelectEnabled?: boolean;
        canReorderHeaders?: boolean;
        rows: IGridUIRowModel[];
        headers: IGridUIHeaderCellModel[];
        blocks?: IGridUIBlockViewModel[];
        rawData?: IViewData;
        horizontalScroll?: boolean;
        keyDownHandler?: (e: React.KeyboardEvent) => void;
        headerDropHandler?: (args: {
            source: IGridUIHeaderCellModel;
            target: IGridUIHeaderCellModel;
            sourceIndex: number;
            targetIndex: number;
        }) => void;
        selectRowOnContextMenu?: boolean;
        saveTableHeightWhenEmpty?: boolean;
    });
    get horizontalScroll(): boolean;
    set horizontalScroll(value: boolean);
    private isParentToggled;
    get rows(): GridUIRowModel[];
    get headers(): GridUIHeaderCellModel[];
    get blocks(): GridUIBlockViewModel[];
    getColumnIndex: (columnAlias: string) => number;
    getOrderedRows(): GridUIRowModel[];
    handleScrollToActiveRow: (row: GridUIRowModel, getRowBoundingClientRect: () => DOMRect | null, container?: import("react").RefObject<HTMLElement> | undefined) => void;
    handleKeyDown: (event: React.KeyboardEvent, isColumnOverflowed: (columnIndex: number) => boolean, container?: import("react").RefObject<HTMLElement> | undefined) => void;
    private handleScrollExtremePosition;
}
export interface IGridUIBlockViewModel {
    id: string;
    parentBlockId: string | null;
    caption: string;
    count?: number;
    isToggled?: boolean;
    onToggle?: () => void;
    menuActions?: MenuAction[];
    /**
     * Имеет приоритет над массивом
     */
    getMenuActions?: () => MenuAction[];
    onClick?: (e: React.MouseEvent) => void;
    onDoubleClick?: (e: React.MouseEvent) => void;
    onMouseDown?: (e: React.MouseEvent) => void;
}
export declare class GridUIBlockViewModel {
    id: string;
    parentBlockId: string | null;
    caption: string;
    count: number;
    private _isToggled;
    private _onToggle?;
    isParentToggled: () => boolean;
    menuActions: MenuAction[];
    getMenuActions: (() => MenuAction[]) | undefined;
    _onClick?: (e: React.MouseEvent) => void;
    _onDoubleClick?: (e: React.MouseEvent) => void;
    _onMouseDown?: (e: React.MouseEvent) => void;
    onClickWrapper: (e: React.MouseEvent) => void;
    constructor(args: IGridUIBlockViewModel & IParentToggled);
    get isToggled(): boolean;
    set isToggled(value: boolean);
    private setIsOpen;
    getContextMenu(): MenuAction[];
    onClick: (e: React.MouseEvent) => void;
    onDoubleClick: (e: React.MouseEvent) => void;
    onMouseDown: (e: React.MouseEvent) => void;
}
export interface IParentToggled {
    isParentToggled(): boolean;
}
interface IViewData {
    rowcount: number;
    datatypes: string[];
    columns: string[];
    rows: CellValue[][];
}
declare type CellValue = any;
export declare type MouseEventHandlerCtor = {
    onClick?: (e: React.MouseEvent) => void;
    onDoubleClick?: (e: React.MouseEvent) => void;
    onMouseDown?: (e: React.MouseEvent) => void;
};
export declare abstract class MouseEventHandler {
    constructor(args: MouseEventHandlerCtor);
    protected _onClick: (e: React.MouseEvent) => void;
    protected _onDoubleClick: (e: React.MouseEvent) => void;
    protected _onMouseDown: (e: React.MouseEvent) => void;
    protected _onClickWrapper: (e: React.MouseEvent) => void;
    get onClick(): (e: React.MouseEvent) => void;
    set onClick(value: (e: React.MouseEvent) => void);
    get onDoubleClick(): (e: React.MouseEvent) => void;
    set onDoubleClick(value: (e: React.MouseEvent) => void);
    get onMouseDown(): (e: React.MouseEvent) => void;
    set onMouseDown(value: (e: React.MouseEvent) => void);
    get onClickWrapper(): (e: React.MouseEvent) => void;
    protected rebuildClickWrapper(): void;
}
export declare type CellTagCtor = {
    icon: string;
    toolTip?: string;
    visible?: boolean;
} & MouseEventHandlerCtor;
export declare class CellTag extends MouseEventHandler {
    constructor(args: CellTagCtor);
    protected _icon: string;
    protected _toolTip: string;
    protected _visible: boolean;
    get icon(): string;
    set icon(value: string);
    get toolTip(): string;
    set toolTip(value: string);
    get visible(): boolean;
    set visible(value: boolean);
}
export {};
