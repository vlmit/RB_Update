import * as React from 'react';
import { ControlProps } from './controlProps';
import { IControlViewModel } from 'tessa/ui/cards/interfaces';
export declare class TaskInfo extends React.Component<ControlProps<IControlViewModel>> {
    render(): JSX.Element;
    private renderDisplayRoleName;
    private renderTaskTag;
    private displayRoleNameClickHandler;
    private authorClickHandler;
    private creatorClickHandler;
}
