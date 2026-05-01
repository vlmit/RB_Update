import { ICardEditorModel } from './interfaces';
import { CardToolbarItem } from './cardToolbarItem';
import { IUIContext } from '../uiContext';
import { CardToolbarHotkeyStorage } from './cardToolbarHotkeyStorage';
import { KeyboardModifiers } from '../tiles';
export interface ICardToolbarViewModel {
    readonly componentId: string;
    readonly context: IUIContext;
    rightToLeft: boolean;
    readonly items: ReadonlyArray<CardToolbarItem>;
    readonly hotkeys: CardToolbarHotkeyStorage;
    backgroundColor: string | null;
    addItem(item: CardToolbarItem): any;
    removeItem(item: CardToolbarItem): any;
    clearItems(): any;
    addItemIfNotExists(item: CardToolbarItem, hotkey?: {
        name: string;
        key: string;
        modifiers?: KeyboardModifiers;
    }): any;
    removeItemIfExists(name: string): any;
    dispose(): any;
}
export declare class CardToolbarViewModel implements ICardToolbarViewModel {
    constructor(cardEditor: ICardEditorModel, isBottom?: boolean);
    private _componentId;
    private _cardEditor;
    private _items;
    private _backgroundColor;
    get componentId(): string;
    get context(): IUIContext;
    rightToLeft: boolean;
    get items(): ReadonlyArray<CardToolbarItem>;
    readonly hotkeys: CardToolbarHotkeyStorage;
    readonly isBottom: boolean;
    get backgroundColor(): string | null;
    set backgroundColor(value: string | null);
    addItem(item: CardToolbarItem): void;
    removeItem(item: CardToolbarItem): void;
    clearItems(): void;
    addItemIfNotExists(item: CardToolbarItem, hotkey?: {
        name: string;
        key: string;
        modifiers?: KeyboardModifiers;
    }): void;
    removeItemIfExists(name: string): void;
    dispose(): void;
}
