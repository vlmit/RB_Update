/// <reference types="react" />
export interface GridHeightProviderProps {
    tableRef: HTMLTableElement | null;
    rowsLength: number;
    render: (height: number) => JSX.Element;
}
export declare const GridHeightProvider: (props: GridHeightProviderProps) => JSX.Element;
