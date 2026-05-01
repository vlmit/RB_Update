import * as React from 'react';
import { ThemeProps } from 'styled-components';
import { IPagingViewModel } from '../../content';
export interface PagingViewProps {
    viewModel: IPagingViewModel;
}
declare class PagingViewInternal extends React.Component<PagingViewProps & ThemeProps<any>> {
    render(): JSX.Element;
    private handleOptionalPagingClick;
    private handleCurrentPageChanged;
}
export declare const PagingView: React.ForwardRefExoticComponent<Pick<PagingViewProps & ThemeProps<any> & React.RefAttributes<PagingViewInternal>, "viewModel" | "ref" | "key"> & {
    theme?: any;
}>;
export {};
