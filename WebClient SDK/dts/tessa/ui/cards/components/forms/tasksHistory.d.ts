import * as React from 'react';
import { DefaultFormTabWithTaskHistoryViewModel } from 'tessa/ui/cards/forms/defaultFormTabWithTaskHistoryViewModel';
import { IGridUIBlockViewModel } from 'components/cardElements';
import { ColumnSortDirection } from '../../controls/columnSortDirection';
export declare class TasksHistory extends React.Component<TasksHistoryProps> {
    constructor(props: TasksHistoryProps);
    rootBlock: IGridUIBlockViewModel;
    wfResolutionRootBlock: IGridUIBlockViewModel;
    otherRootBlock: IGridUIBlockViewModel;
    _sortingColumn: number | undefined;
    _sortDirection: ColumnSortDirection;
    render(): JSX.Element;
    private get headers();
    setSortingColumn(value: number, descendingByDefault?: boolean): void;
    setSortDirection(value: ColumnSortDirection): void;
    private getNewSortDirection;
    private tagClickHandler;
    private get rows();
    private get blocks();
    private get rawRows();
    private getSortedRows;
    private searchFilterFunc;
    private getHighlightedContent;
    private handleKeyDown;
    private handleHeaderDrop;
}
export interface TasksHistoryProps {
    viewModel: DefaultFormTabWithTaskHistoryViewModel;
}
