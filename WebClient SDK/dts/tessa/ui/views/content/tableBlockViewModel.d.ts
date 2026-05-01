/// <reference types="react" />
import { MenuAction } from 'tessa/ui/menuAction';
export interface ITableBlockViewModel {
    readonly id: string;
    readonly parentBlockId: string | null;
    isToggled: boolean;
    count: number;
    text: string;
    visibility: boolean;
    toggle(): any;
    onClick: (e: React.MouseEvent) => void;
    onDoubleClick: (e: React.MouseEvent) => void;
    onMouseDown: (e: React.MouseEvent) => void;
    getContextMenu(): ReadonlyArray<MenuAction>;
}
export interface ITableBlockViewModelCreateOptions {
    id: string;
    caption: string;
    parentBlockId?: string | null;
    getContextMenu?: ((column: ITableBlockViewModel) => ReadonlyArray<MenuAction>) | null;
}
export declare class TableBlockViewModel implements ITableBlockViewModel {
    constructor(args: ITableBlockViewModelCreateOptions);
    protected _isToggled: boolean;
    protected _count: number;
    protected _text: string;
    protected _getContextMenu: ((block: ITableBlockViewModel) => ReadonlyArray<MenuAction>) | null;
    protected _visibility: boolean;
    readonly id: string;
    readonly caption: string;
    readonly parentBlockId: string | null;
    get isToggled(): boolean;
    set isToggled(value: boolean);
    get count(): number;
    set count(value: number);
    get text(): string;
    set text(value: string);
    get visibility(): boolean;
    set visibility(value: boolean);
    onClick: (_e: React.MouseEvent) => void;
    onDoubleClick: (_e: React.MouseEvent) => void;
    onMouseDown: (_e: React.MouseEvent) => void;
    toggle(): void;
    getContextMenu(): ReadonlyArray<MenuAction>;
}
