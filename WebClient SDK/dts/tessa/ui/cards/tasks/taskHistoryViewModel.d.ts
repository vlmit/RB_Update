/// <reference types="react" />
import { SupportUnloadingViewModel } from 'tessa/ui/cards/supportUnloadingViewModel';
import { IFormViewModel, ICardModel, IBlockViewModel, IFormState } from 'tessa/ui/cards/interfaces';
import { CardTypeFormSealed } from 'tessa/cards/types/cardTypeCommon';
import { IValidationResultBuilder } from 'tessa/platform/validation';
import { TaskHistoryColumnViewModel } from 'tessa/ui/cards/tasks/taskHistoryColumnViewModel';
import { TaskHistoryItemViewModel } from 'tessa/ui/cards/tasks/taskHistoryItemViewModel';
import { EventHandler } from 'tessa/platform';
import { IFilterableGrid, LabelViewModel } from '../controls';
import { MenuAction } from 'tessa/ui/menuAction';
import { IShowViewArgs } from 'tessa/ui/uiHost';
import { TabSelectedContext, TabSelectedEventArgs } from '../tabSelectedEventArgs';
export declare class TaskHistoryViewModel extends SupportUnloadingViewModel implements IFormViewModel, IFilterableGrid {
    constructor(model: ICardModel);
    protected _initialized: boolean;
    private _columns;
    private _rows;
    private _rowModels;
    private _horizontalScroll;
    protected _blocks: ReadonlyArray<IBlockViewModel>;
    protected _tabCaption: string | null;
    protected _blockMargin: string | null;
    protected _headerClass: string;
    protected _contentClass: string;
    protected _className: string;
    protected _isCollapsed: boolean;
    searchText: string;
    private _rowsDisposer;
    private _hideResult;
    private _selectedRowId;
    private _hideOpenViewCommand;
    /**
     * Метод, изменяющий стандартный запрос на открытии представление с историей заданий,
     * или <c>null</c>, если выполняется открытие с параметрами по умолчанию.
     * Полностью изменить команду возможно посредством свойств объекта <see cref="OpenViewCommandClosure"/>.
     */
    modifyOpenViewRequestAction: ((agrs: IShowViewArgs) => IShowViewArgs) | undefined;
    /**
     * Признак того, что команда для открытия представления по ссылке должна быть скрыта. По умолчанию <c>false</c>.
     */
    get hideOpenViewCommand(): boolean;
    set hideOpenViewCommand(value: boolean);
    private setHideOpenViewCommand;
    /**
     * Замыкание для управления командой <see cref="OpenViewCommand"/>.
     * Укажите действие, выполняемое при клике по ссылке,
     */
    get openViewCommandClosure(): ((e: React.SyntheticEvent<Element, Event>) => void) | null;
    set openViewCommandClosure(handler: ((e: React.SyntheticEvent<Element, Event>) => void) | null);
    get columns(): ReadonlyArray<TaskHistoryColumnViewModel>;
    readonly cardModel: ICardModel;
    readonly componentId: guid;
    readonly cardTypeForm: CardTypeFormSealed;
    linkToOpenViewVM: LabelViewModel;
    get blocks(): ReadonlyArray<IBlockViewModel>;
    get name(): string | null;
    get isEmpty(): boolean;
    get horizontalScroll(): boolean;
    set horizontalScroll(value: boolean);
    get tabCaption(): string | null;
    set tabCaption(value: string | null);
    get blockMargin(): string | null;
    set blockMargin(value: string | null);
    get headerClass(): string;
    get contentClass(): string;
    get className(): string;
    set className(value: string);
    get hasFileControl(): boolean;
    get filePreviewIsDisabled(): boolean;
    get isCollapsed(): boolean;
    set isCollapsed(value: boolean);
    get rows(): ReadonlyArray<TaskHistoryItemViewModel>;
    readonly contextMenuGenerators: ((ctx: TaskHistoryItemMenuContext) => void)[];
    get hideResult(): boolean;
    set hideResult(value: boolean);
    get selectedRowId(): guid | null;
    set selectedRowId(value: guid | null);
    get currentItem(): TaskHistoryItemViewModel | null;
    initialize(): void;
    protected initializeCore(): void;
    getState(): IFormState;
    setState(state: IFormState): boolean;
    close(): boolean;
    canSetSearchText(): boolean;
    setSearchText(text: string): void;
    getRowContextMenu: (rowViewModel: TaskHistoryItemViewModel, _columnIndex: number) => ReadonlyArray<MenuAction>;
    changeColumnOrderByTarget(sourceIndex: number, targetIndex: number): void;
    onUnloading(validationResult: IValidationResultBuilder): void;
    readonly closed: EventHandler<() => void>;
    readonly keyDown: EventHandler<(args: TaskHistoryKeyDownEventArgs) => void>;
    tabSelected: EventHandler<(args: TabSelectedEventArgs) => void>;
    tabDeselected: EventHandler<(args: TabSelectedEventArgs) => void>;
    notifyTabSelected(context: TabSelectedContext): Promise<void>;
    notifyTabDeselected(context: TabSelectedContext): Promise<void>;
}
export declare class TaskHistoryViewModelState implements IFormState {
    constructor(form: TaskHistoryViewModel);
    readonly isForceTabMode: boolean;
    readonly isCollapsed: boolean;
    apply(form: TaskHistoryViewModel): boolean;
}
export interface TaskHistoryItemMenuContext {
    readonly historyItem: TaskHistoryItemViewModel;
    readonly history: TaskHistoryViewModel;
    readonly menuActions: MenuAction[];
}
export interface TaskHistoryKeyDownEventArgs {
    control: TaskHistoryViewModel;
    event: React.KeyboardEvent;
}
