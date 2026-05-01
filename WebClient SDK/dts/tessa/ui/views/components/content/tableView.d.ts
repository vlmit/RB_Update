import * as React from 'react';
import { TableGridViewModelBase } from '../../content';
import { IViewComponentBase } from '../../viewComponentBase';
import { IWorkplaceViewComponent } from '../../workplaceViewComponent';
export interface TableViewProps<T extends IViewComponentBase> {
    viewModel: TableGridViewModelBase<T>;
}
export declare class TableView<T extends IViewComponentBase = IWorkplaceViewComponent> extends React.Component<TableViewProps<T>> {
    private _gridUIVM;
    render(): JSX.Element;
    private renderRowCounter;
    private getHeaders;
    private getRows;
    private getBlocks;
    private convertFromSortDirection;
    private handleHeaderDrop;
    private handleKeyDown;
}
