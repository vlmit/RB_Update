import * as React from 'react';
import { IDictionary } from 'common';
declare class FontIcon extends React.Component<IFontIconProps, {}> {
    static defaultProps: {
        lib: string;
    };
    render(): JSX.Element;
}
export interface IFontIconProps {
    className: string;
    lib?: string;
    style?: IDictionary<string>;
    [key: string]: any;
}
export default FontIcon;
