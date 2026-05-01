import { FileListVersionsDialogViewModel } from './fileListVersionsDialogViewModel';
import { FileCategory, FileType, CertData, IFileVersion } from 'tessa/files';
import { IFileControl } from 'tessa/ui/files';
import { ICAdESProvider } from 'tessa/cards';
import { FileViewModel } from './fileViewModel';
export declare function addFiles(control: IFileControl, contents: ReadonlyArray<File>, fileTypes: FileType[]): Promise<void>;
export declare function showFileListVersionsDialog(viewModel: FileListVersionsDialogViewModel): Promise<any>;
export interface SelectCategoryResult {
    cancel: boolean;
    category: FileCategory | null;
}
export declare function selectFileCategory(control: IFileControl, autoSelectWhenFiltering: boolean): Promise<SelectCategoryResult>;
export interface SelectFileTypeResult {
    cancel: boolean;
    type: FileType | null;
}
export declare function selectFileType(control: IFileControl, fileTypes: FileType[], category: FileCategory): Promise<SelectFileTypeResult>;
export interface RenameResult {
    cancel: boolean;
    name: string;
}
export declare function renameFile(name: string): Promise<RenameResult>;
export declare function showFileSigns(version: IFileVersion): Promise<void>;
export declare function checkSigns(edsProvider: ICAdESProvider, version: IFileVersion): Promise<void>;
export interface SelectFileCertResult {
    cancel: boolean;
    cert: CertData | null;
    comment: string | null;
}
export declare function selectFileCerts(edsProvider: ICAdESProvider, noCommentDialog?: boolean): Promise<SelectFileCertResult>;
export declare function setSign(edsProvider: ICAdESProvider, version: IFileVersion, cert: CertData): Promise<import("../../../../cards").ISignedData>;
export declare function getFileNameAndExtension(name: string): {
    name: string;
    ext: string;
};
export declare function getSelectedFilesPreviewMessage(files: ReadonlyArray<FileViewModel>): {
    main: string;
    additional: string;
};
