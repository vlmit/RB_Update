import * as React from 'react';
declare class Wrapper extends React.Component<WrapperProps> {
    shouldComponentUpdate(nextProps: WrapperProps): boolean;
    componentDidUpdate(_prevProps: WrapperProps): void;
    render(): JSX.Element;
}
export interface WrapperProps {
    match: {
        params: {
            id?: string;
            name?: string;
        };
    };
    savedScrollPosition?: number;
}
export default Wrapper;
