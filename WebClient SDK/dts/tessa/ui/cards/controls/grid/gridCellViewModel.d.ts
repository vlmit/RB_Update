/// <reference types="react" />
import { GridColumnInfo } from './gridColumnInfo';
import { Card, CardRow } from 'tessa/cards';
import { GridCellFormatContext } from './gridCellFormatContext';
export declare class GridCellViewModel {
    constructor(row: CardRow, card: Card, columnInfo: GridColumnInfo);
    private _row;
    private _card;
    private _columnInfo;
    private _style;
    private _gridCellFormatFunc;
    get gridCellFormatFunc(): ((context: GridCellFormatContext) => any) | null;
    set gridCellFormatFunc(value: ((context: GridCellFormatContext) => any) | null);
    private _cacheFunction;
    get value(): any;
    get toolTip(): string;
    get columnInfo(): GridColumnInfo;
    get style(): React.CSSProperties;
    set style(value: React.CSSProperties);
    private getStyle;
    private valueWithCache;
    private getFormattedValue;
}
