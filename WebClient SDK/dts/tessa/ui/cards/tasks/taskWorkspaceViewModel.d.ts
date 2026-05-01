import { TaskWorkspaceState } from './taskWorkspaceState';
import { TaskAction } from './taskAction';
import { TaskTagViewModel } from './taskTagViewModel';
import { TaskLinkViewModel } from './taskLinkViewModel';
import { IFormViewModel, ICardModel } from '../interfaces';
import { TaskInfoViewModel } from '../controls';
export declare class TaskWorkspaceViewModel {
    constructor(content: any, state: TaskWorkspaceState, actions: TaskAction[], additionalActions: TaskAction[], model: ICardModel);
    private _model;
    private _additionalContent;
    private _tag;
    readonly content: any;
    get additionalContent(): TaskLinkViewModel | null;
    get tag(): TaskTagViewModel | null;
    get form(): IFormViewModel | null;
    readonly state: TaskWorkspaceState;
    readonly actions: TaskAction[];
    readonly moreAction: TaskAction;
    readonly additionalActions: TaskAction[];
    tryGetTaskInfo(): TaskInfoViewModel | null;
    setTag(icon: string, command?: Function | null): void;
    setLink(text: string, command?: Function | null): void;
}
