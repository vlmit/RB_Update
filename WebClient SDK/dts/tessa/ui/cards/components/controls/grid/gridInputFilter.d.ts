import * as React from 'react';
import { IFilterableGrid } from 'tessa/ui/cards/controls/grid';
import { Visibility } from 'tessa/platform';
export declare class GridInputFilter extends React.Component<IGridInputFilter, {}> {
    searchRef: HTMLInputElement | null;
    timeout: number;
    render(): JSX.Element;
    private onInputFilterChange;
    filterItems: () => void;
}
export interface IGridInputFilter {
    filterableGrid: IFilterableGrid;
    visibility: Visibility;
}
