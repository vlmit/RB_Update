import * as React from 'react';
import { SyntaxHighlighting } from '../../tessa/cards/syntaxHighlighting';
import { AvalonTextBoxFontType } from '../../tessa/cards/avalonTextBoxFontType';
import 'highlight.js/styles/vs.css';
declare class EnhancedTextarea extends React.PureComponent<EnhancedTextareaProps, EnhancedTextareaState> {
    detector: any;
    cachedElement: Element | Text;
    static defaultProps: {
        type: string;
        rows: number;
        rowsMax: number;
    };
    inputAreaRef: React.RefObject<HTMLTextAreaElement>;
    outputAreaRef: React.RefObject<HTMLElement>;
    numbersAreaRef: React.RefObject<HTMLDivElement>;
    inputDivRef: React.RefObject<HTMLDivElement>;
    constructor(props: any);
    componentDidMount(): void;
    resizeTimout: number;
    handleResize: () => void;
    setHighlightOutput: () => void;
    componentDidUpdate(prevProps: EnhancedTextareaProps): void;
    getTopNode: (node: any) => any;
    inputAreaOnChangeHandler: () => void;
    outputAreaClickHandler: () => void;
    outputTimeout: number;
    outputScrolingHandler: () => void;
    inputScrollingHandler: () => void;
    refreshHighlighting: () => void;
    componentWillUnmount(): void;
    getDOMNode(): Element | Text | null;
    syncHeightWithShadow(newValue?: any): void;
    handleChange: (event: any) => void;
    render(): JSX.Element;
}
export interface EnhancedTextareaProps {
    id?: string;
    type?: string;
    style?: object;
    className?: string;
    value?: any;
    defaultValue?: any;
    disabled?: boolean;
    rows?: number;
    rowsMax?: number;
    onChange?: any;
    onFocus?: any;
    onBlur?: any;
    syncOnResize?: boolean;
    textBoxMode?: TextBoxMode;
    avalonFontType?: AvalonTextBoxFontType;
    avalonShowLineNumbers?: boolean;
    avalonSyntaxType?: SyntaxHighlighting;
}
export interface EnhancedTextareaState {
    rows: number;
}
export default EnhancedTextarea;
export declare enum TextBoxMode {
    /**
     * Стандартный TextBox.
     */
    Default = 0,
    /**
     * Редактор AvalonEdit.
     */
    Avalon = 1,
    /**
     * Режим ввода пароля с использованием PasswordBox.
     */
    Password = 2
}
