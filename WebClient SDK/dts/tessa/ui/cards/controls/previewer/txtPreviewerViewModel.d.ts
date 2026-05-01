import { IFileVersion } from 'tessa/files';
import { IPreviewerViewModel } from './previewerViewModel';
import { HandleFileContentLoadingFuncType } from 'tessa/ui/files';
export declare class TxtPreviewerViewModel implements IPreviewerViewModel {
    static get type(): string;
    readonly type: string;
    readonly showNameHeader: boolean;
    private readonly _getFileContentFunc;
    private _fileVersion;
    private _text;
    constructor(getFileContentFunc: HandleFileContentLoadingFuncType);
    get text(): string;
    get fileVersion(): IFileVersion | null;
    load: (version: IFileVersion) => Promise<void>;
    unload: () => void;
    private onFileLoadHandler;
}
