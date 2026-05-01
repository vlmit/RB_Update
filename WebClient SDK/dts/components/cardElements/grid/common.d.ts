/// <reference types="react" />
import { CellTag, GridUIBlockViewModel } from './models';
export declare const StyledTr: import("styled-components").StyledComponent<"tr", any, {}, never>;
export declare const handleBlockContextMenu: (block: GridUIBlockViewModel) => (e: any) => void;
export declare function ContentWithTags(props: {
    content: any;
    leftTags?: CellTag[];
    rightTags?: CellTag[];
}): JSX.Element;
