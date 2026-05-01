import * as React from 'react';
import { FileListVersionsDialogViewModel } from 'tessa/ui/cards/controls';
import { IFileVersion } from 'tessa/files';
export interface FileListVersionsDialogProps {
    viewModel: FileListVersionsDialogViewModel;
    onClose: () => void;
}
export interface FileListVersionsDialogState {
    sourceSelectedVersion: IFileVersion | null;
    selectedVersions: IFileVersion[];
}
export declare class FileListVersionsDialog extends React.Component<FileListVersionsDialogProps, FileListVersionsDialogState> {
    constructor(props: FileListVersionsDialogProps);
    private _dropDownRef;
    render(): JSX.Element;
    private renderDropDown;
    private getGridVM;
    private handleDropDownRef;
    private handleClickDropDown;
    private handleCloseDropDown;
    private handleCloseForm;
}
