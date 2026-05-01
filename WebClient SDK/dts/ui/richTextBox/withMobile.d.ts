import { BaseEditor } from 'slate';
export interface MobileEditor extends BaseEditor {
    isMobile: boolean;
    isAndroid: boolean;
}
export declare function withMobile<T extends BaseEditor>(e: T): T & MobileEditor;
