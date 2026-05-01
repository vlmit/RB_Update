import * as React from 'react';
declare class DropdownItem extends React.Component<IDropdownItemProps, {}> {
    render(): JSX.Element;
}
export interface IDropdownItemProps {
    children?: React.ReactNode;
    className?: string;
    style?: object;
    onClick?: any;
    [key: string]: any;
}
export default DropdownItem;
