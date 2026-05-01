import * as React from 'react';
import { StageGroup } from './stageGroup';
import { StageType } from './stageType';
import { StageSelectorViewModel } from './stageSelectorViewModel';
export interface StageSelectorDialogProps {
    viewModel: StageSelectorViewModel;
    onClose: (args: {
        cancel: boolean;
        group: StageGroup | null;
        type: StageType | null;
    }) => void;
}
export declare class StageSelectorDialog extends React.Component<StageSelectorDialogProps> {
    constructor(props: StageSelectorDialogProps);
    render(): JSX.Element;
    private getGroupsHeader;
    private getGroupsRows;
    private getTypesHeader;
    private getTypesRows;
    private handleCloseForm;
    private handleCloseFormWithResult;
}
