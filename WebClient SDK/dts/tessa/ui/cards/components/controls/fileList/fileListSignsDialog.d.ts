import * as React from 'react';
import { FileListSignsDialogViewModel } from 'tessa/ui/cards/controls';
export interface FileListSignsDialogProps {
    viewModel: FileListSignsDialogViewModel;
    onClose: () => void;
}
export declare class FileListSignsDialog extends React.Component<FileListSignsDialogProps> {
    render(): JSX.Element;
    private getHeaders;
    private getRows;
    private getCheckClass;
    private getEventText;
    private handleCloseForm;
}
