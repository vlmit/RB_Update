import React from 'react';
import { AvalonTextBoxFontType } from 'tessa/cards/avalonTextBoxFontType';
import { SyntaxHighlighting } from 'tessa/cards/syntaxHighlighting';
import { TextBoxMode } from './enhancedTextarea';
export interface TextFieldProps {
    children?: any;
    id?: string;
    type?: string;
    className?: string;
    value?: any;
    defaultValue?: any;
    disabled?: boolean;
    multiLine?: boolean;
    formatOptions?: object | null;
    rows?: number;
    rowsMax?: number;
    onChange?: (e: React.SyntheticEvent<HTMLInputElement>, validatedValue: string) => void;
    onFocus?: any;
    onBlur?: any;
    onValidate?: (rawValue: string) => string;
    onKeyDown?: any;
    placeholder?: string;
    style?: React.CSSProperties;
    divContainer?: boolean;
    textBoxMode?: TextBoxMode;
    useStateValue?: boolean;
    avalonFontType?: AvalonTextBoxFontType;
    avalonShowLineNumbers?: boolean;
    avalonSyntaxType?: SyntaxHighlighting;
    [key: string]: any;
}
export interface TextFieldState {
    stateValue: string | null;
    isFocused: boolean;
}
declare class TextField extends React.PureComponent<TextFieldProps, TextFieldState> {
    input: any;
    static defaultProps: {
        type: string;
        rows: number;
        rowsMax: number;
        formatOptions: null;
        divContainer: boolean;
    };
    constructor(props: any);
    componentDidUpdate(prevProps: TextFieldProps): void;
    static convertValue(value: any): any;
    getDOMNode(): any;
    focus(opt?: FocusOptions): void;
    blur(): void;
    select(): void;
    getValue(): any;
    handleInputChange: (event: any) => void;
    handleInputFocus: (event: any) => void;
    handleInputBlur: (event: any) => void;
    handleKeyDown: (event: any) => void;
    renderSingleInput(other: any, inputProps: any): JSX.Element;
    render(): any;
}
export default TextField;
