import React from 'react';
import { MessageBoxButtons, MessageBoxIcon, MessageBoxResult, MessageBoxOptions } from 'tessa/ui/tessaDialog';
declare class MessageBox extends React.Component<MessageBoxProps, {}> {
    closeRequest: (result: any) => void;
    getMessageResultOnClosing(result?: boolean): MessageBoxResult.None | MessageBoxResult.OK | MessageBoxResult.Yes;
    renderHeader(): JSX.Element | null;
    renderContent(): JSX.Element;
    handleCloseRequest: (arg: MessageBoxResult) => () => void;
    renderFooter(): JSX.Element | null;
    render(): JSX.Element;
    handleKeyDown: (event: React.KeyboardEvent) => void;
}
export interface MessageBoxProps {
    text?: string | null;
    noPortal?: boolean;
    caption?: string;
    buttons?: MessageBoxButtons;
    icon?: MessageBoxIcon;
    iconColor?: string;
    options?: MessageBoxOptions;
    onClose: (value: MessageBoxResult) => void;
    l: Function;
}
export { MessageBox };
