import { FileSignatureState, IFileVersion } from 'tessa/files';
export declare class FileListSignsDialogViewModel {
    constructor(version: IFileVersion);
    readonly version: IFileVersion;
    get signState(): FileSignatureState;
    get signStateText(): string;
}
