import { ICardEditorModel } from './interfaces';
export declare function onWorkspaceClosing(editor: ICardEditorModel, e: {
    cancel: boolean;
}, activateAction?: () => Promise<void>): Promise<boolean>;
