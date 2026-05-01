import { ErrorInfo } from 'react';
import { CardModelFlags } from './cardModelFlags';
import { CardModelTableInfo } from './cardModelTableInfo';
import { RowEditingType, CardRowFormContext } from './cardRowFormContext';
import { PreviewManager } from './previewManager';
import { CardSavingRequest } from './cardSavingRequest';
import { CardEditorOpeningContext } from './cardEditorOpeningContext';
import { CardEditorCreationContext } from './cardEditorCreationContext';
import { CardEditorOperationType } from './cardEditorOperationType';
import { ICardToolbarViewModel } from './cardToolbarViewModel';
import { UIButton } from '../uiButton';
import { Card, CardRow, CardTask, ICAdESProvider } from 'tessa/cards';
import { CardMetadataSealed, CardMetadataBinder } from 'tessa/cards/metadata';
import type { CardTypeSealed, CardTypeFormSealed, CardTypeBlockSealed, CardTypeControlSealed } from 'tessa/cards/types';
import { Visibility } from 'tessa/platform/visibility';
import { IStorage } from 'tessa/platform/storage';
import { ISupportUnloading } from 'tessa/ui/supportUnloading';
import { IUIContext } from 'tessa/ui/uiContext';
import { FileContainer } from 'tessa/files';
import { ValidationResult, ValidationResultBuilder } from 'tessa/platform/validation';
import { TaskViewModel, TaskHistoryViewModel } from 'tessa/ui/cards/tasks';
import { CardNewRequest, CardNewResponse, CardGetRequest, CardGetResponse, CardStoreRequest, CardStoreResponse, CardDeleteRequest, CardDeleteResponse } from 'tessa/cards/service';
import { EventHandler, HorizontalAlignment, VerticalAlignment, Margin } from 'tessa/platform';
import { PreviewFormViewModel } from 'tessa/ui/cards/forms';
import { MediaStyle } from 'ui/mediaStyle';
import { TabSelectedContext, TabSelectedEventArgs } from './tabSelectedEventArgs';
import { UIErrorPresenterViewModel } from './uiErrorPresenterViewModel';
export interface ICardEditorModel {
    readonly context: IUIContext;
    cardModel: ICardModel | null;
    readonly info: IStorage;
    readonly lastData: ICardEditorData;
    currentOperationType: CardEditorOperationType | null;
    lastOperationType: CardEditorOperationType | null;
    lastOperationCompleted: Date | null;
    lastOperationDuration: number | null;
    lastOperationRequestDuration: number | null;
    operationStatusText: string | null;
    operationInProgress: boolean;
    operationProgress: number | null;
    readonly isClosed: boolean;
    closePending: boolean;
    workspaceName: string;
    workspaceInfo: string;
    localizedWorkspaceName: string;
    localizedWorkspaceInfo: string;
    isUpdatedServer: boolean;
    withUIExtensions: boolean;
    dialogName: string | null;
    toolbar: ICardToolbarViewModel;
    bottomToolbar: ICardToolbarViewModel;
    readonly bottomDialogButtons: UIButton[];
    statusBarIsVisible: boolean;
    readonly cardModelInitialized: EventHandler<(args: CardModelInitializingEventArgs) => Promise<void>>;
    readonly closing: EventHandler<(args: CardEditorModelClosingArgs) => void>;
    readonly closed: EventHandler<(args: CardEditorModelClosedArgs) => void>;
    readonly uiErrorOccurred: EventHandler<(args: CardEditorModelUIErrorOccurredArgs) => void>;
    notifyUIErrorOccurred: (error: Error, errorInfo: ErrorInfo, close: () => void) => UIErrorPresenterViewModel | undefined;
    close(force?: boolean): Promise<boolean>;
    createCardModel(card: Card, sectionRows: Map<string, CardRow>): Promise<ICardModel>;
    createFormWithExtensions(model: ICardModel, otherContext?: IUIContext | null): Promise<boolean>;
    initializeCardModel(model: ICardModel, info?: IStorage | null): Promise<void>;
    createAndInitializeModel(args: {
        card: Card;
        sectionRows?: Map<string, CardRow>;
        info?: IStorage;
        flags?: CardModelFlags;
        contextExecutor?: (action: (context: IUIContext) => void) => void;
        savingFunc?: (request?: CardSavingRequest) => Promise<ValidationResult>;
        otherContext?: IUIContext;
        initializingModelAction?: (model: ICardModel) => void;
        initializedModelAction?: (model: ICardModel) => void;
    }): Promise<ICardModel>;
    changeWorkspace(name: string, info?: string): any;
    setOperationInProgress<T = void>(action: () => Promise<T>): Promise<T>;
    createCard(args: {
        cardId?: guid;
        cardTypeId?: guid;
        cardTypeName?: string;
        context?: IUIContext;
        info?: IStorage;
        cardModifierAction?: (context: CardEditorCreationContext) => void;
        cardModelModifierAction?: (context: CardEditorCreationContext) => void;
    }): Promise<boolean>;
    openCard(args: {
        cardId?: guid;
        cardTypeId?: guid;
        cardTypeName?: string;
        context?: IUIContext;
        info?: IStorage;
        cardModifierAction?: (context: CardEditorOpeningContext) => void;
        cardModelModifierAction?: (context: CardEditorOpeningContext) => void;
    }): Promise<boolean>;
    refreshCard(context?: IUIContext, info?: IStorage): Promise<boolean>;
    saveCard(context?: IUIContext, info?: IStorage, request?: CardSavingRequest, resultClosure?: {
        validationResult?: ValidationResult;
    }): Promise<boolean>;
    deleteCard(args: {
        context?: IUIContext;
        info?: IStorage;
        withoutBackupOnly?: boolean;
    }): Promise<boolean>;
    getTemplatedCard(args: {
        cardId?: guid;
        cardTypeId?: guid;
        cardTypeName?: string;
        context?: IUIContext;
        info?: IStorage;
    }): Promise<Card | null>;
    getContext(): ICardEditorModelContext;
    setModelContext(modelContext: ICardEditorModelContext): any;
    notifyContextInitialized(): ValidationResult;
}
export interface ICardEditorModelContext {
}
export interface ICardEditorData {
    newRequest: CardNewRequest | null;
    newResponse: CardNewResponse | null;
    getRequest: CardGetRequest | null;
    getResponse: CardGetResponse | null;
    storeRequest: CardStoreRequest | null;
    storeResponse: CardStoreResponse | null;
    deleteRequest: CardDeleteRequest | null;
    deleteResponse: CardDeleteResponse | null;
    templateGetRequest: CardGetRequest | null;
    templateGetResponse: CardGetResponse | null;
}
export declare class CardModelInitializingEventArgs {
    constructor(cardModel: ICardModel, digest: string, workspaceName: string, workspaceInfo: string);
    readonly cardModel: ICardModel;
    readonly digest: string;
    workspaceName: string;
    workspaceInfo: string;
}
export interface CardEditorModelClosingArgs {
    cardEditor: ICardEditorModel;
    cancel: boolean;
}
export interface CardEditorModelClosedArgs {
    cardEditor: ICardEditorModel;
}
export interface CardEditorModelUIErrorOccurredArgs {
    readonly cardEditorModel: ICardEditorModel;
    readonly uiErrorPresenterViewModel: UIErrorPresenterViewModel;
    readonly result: ValidationResultBuilder;
    readonly error: Error;
    readonly errorInfo: ErrorInfo;
}
export interface ICardModel {
    readonly componentId: guid;
    readonly card: Card;
    readonly sectionRows: ReadonlyMap<string, CardRow>;
    readonly cardMetadata: CardMetadataSealed;
    readonly generalMetadata: CardMetadataSealed;
    readonly binder: CardMetadataBinder;
    readonly cardType: CardTypeSealed;
    readonly forms: ReadonlyArray<IFormViewModel>;
    readonly formsBag: ReadonlyArray<IFormViewModel>;
    readonly blocks: ReadonlyMap<string, IBlockViewModel>;
    readonly blocksBag: ReadonlyArray<IBlockViewModel>;
    readonly controls: ReadonlyMap<string, IControlViewModel>;
    readonly controlsBag: ReadonlyArray<IControlViewModel>;
    parentModel: ICardModel | null;
    cardTask: CardTask | null;
    mainForm: IFormViewModel | null;
    stateIsInitialized: boolean;
    contextIsInitialized: boolean;
    table: CardModelTableInfo | null;
    tableStack: ReadonlyArray<CardModelTableInfo>;
    hasActiveValidation: boolean;
    flags: CardModelFlags;
    readonly info: any;
    currentRow: ICardModel | null;
    readonly rowFormContext: CardRowFormContext | null;
    fileContainer: FileContainer;
    readonly previewManager: PreviewManager;
    readonly edsProvider: ICAdESProvider;
    readonly inSpecialMode: boolean;
    digest: string;
    readonly lastRequest: CardSavingRequest | null;
    closingRequest: CardSavingRequest | null;
    readonly controlCreationOverrides: Array<CardControlCreationOverride>;
    readonly controlInitializers: Array<CardControlInitializer>;
    readonly blockInitializers: Array<CardBlockInitializer>;
    readonly formInitializers: Array<CardFormInitializer>;
    readonly taskInitializers: Array<CardTaskInitializer>;
    setActiveValidation(validation: boolean): any;
    close(force?: boolean): any;
    commitChanges(info?: Storage): Promise<ValidationResult>;
    createEmptyRow(sectionName: string): CardRow;
    createForRow(sectionName: string, rowIndex: number): ICardModel;
    createRowFormContext(editingType: RowEditingType, initFunc: (context: CardRowFormContext) => Promise<boolean>, closeFunc: (context: CardRowFormContext) => Promise<boolean>): Promise<CardRowFormContext>;
    setContextExecutor(executor: (action: (context: IUIContext) => void) => void): any;
    executeInContext(action: (context: IUIContext) => void): any;
    setSavingFunc(action: (request?: CardSavingRequest) => Promise<ValidationResult>): any;
    saveAsync(request?: CardSavingRequest): Promise<ValidationResult>;
    hasChanges(ignoreForceChanges?: boolean): Promise<boolean>;
    tryGetPreviewTab(): PreviewFormViewModel | null;
    tryGetTasks(): TaskViewModel[] | null;
    tryGetTaskHistory(): TaskHistoryViewModel | null;
    clearBags(): any;
}
export interface ICardCommitChangesContext {
    readonly model: ICardModel;
    readonly validationResult: ValidationResultBuilder;
    readonly info: IStorage;
}
export interface IFormViewModel extends ISupportUnloading, ISupportTabNotifications {
    readonly cardModel: ICardModel;
    readonly componentId: guid;
    readonly cardTypeForm: CardTypeFormSealed;
    readonly name: string | null;
    tabCaption: string | null;
    readonly blocks: ReadonlyArray<IBlockViewModel>;
    blockMargin: string | null;
    readonly headerClass: string;
    readonly contentClass: string;
    className: string;
    readonly isEmpty: boolean;
    readonly hasFileControl: boolean;
    filePreviewIsDisabled: boolean;
    isCollapsed: boolean;
    initialize(): any;
    getState(): IFormState;
    setState(state: IFormState): boolean;
    close(): boolean;
    readonly closed: EventHandler<() => void>;
}
export interface ITabbedFormViewModel extends IFormViewModel {
    tabsAreCollapsed: boolean;
    tabs: ReadonlyArray<IFormViewModel>;
    selectedTab: IFormViewModel;
}
export interface IFormState {
}
export interface IBlockViewModel extends ISupportUnloading, ISupportTabNotifications {
    readonly componentId: guid;
    readonly cardTypeBlock: CardTypeBlockSealed;
    readonly name: string | null;
    readonly form: IFormViewModel;
    caption: string;
    captionVisibility: Visibility;
    blockVisibility: Visibility;
    blockMargin: string | null;
    readonly controls: ReadonlyArray<IControlViewModel>;
    controlMargin: string | null;
    readonly isEmpty: boolean;
    leftCaptions: boolean;
    horizontalInterval: number;
    verticalInterval: number;
    columnIndex?: number;
    rowIndex?: number;
    columnSpan?: number;
    rowSpan?: number;
    collapsed: boolean;
    doNotCollapseWithTopBlock: boolean;
    initialize(): any;
    setForm(form: IFormViewModel): any;
    getConnectedBlocks(): IBlockViewModel[];
    stretchVertically: boolean;
}
export interface IControlViewModel extends ISupportUnloading, ISupportTabNotifications {
    readonly componentId: guid;
    readonly cardTypeControl: CardTypeControlSealed;
    readonly name: string | null;
    readonly block: IBlockViewModel;
    caption: string;
    tooltip: string;
    captionVisibility: Visibility;
    captionStyle: MediaStyle | null;
    controlVisibility: Visibility;
    controlStyle: MediaStyle | null;
    margin: Margin | null;
    minWidth: string | null;
    maxWidth: string | null;
    columnSpan: number;
    emptyColumnsToTheLeft: number;
    horizontalAlignment: HorizontalAlignment;
    verticalAlignment: VerticalAlignment;
    startAtNewLine: boolean;
    readonly isEmpty: boolean;
    isRequired: boolean;
    requiredText: string;
    isReadOnly: boolean;
    isSpanned: boolean;
    isFocused: boolean;
    hasActiveValidation: boolean;
    validationFunc: ((control: IControlViewModel) => string | null) | null;
    readonly error: string | null;
    readonly hasEmptyValue: boolean;
    initialize(): any;
    commitChanges(context: ICardCommitChangesContext): any;
    focus(opt?: FocusOptions): any;
    bindReactComponentRef(ref: React.RefObject<any>): any;
    unbindReactComponentRef(): any;
    setBlock(block: IBlockViewModel): any;
    getState(): IControlState | null;
    setState(state: IControlState): boolean;
    notifyUpdateValidation(): any;
}
export interface IControlState {
}
interface ISupportTabNotifications {
    tabSelected: EventHandler<(args: TabSelectedEventArgs) => void>;
    tabDeselected: EventHandler<(args: TabSelectedEventArgs) => void>;
    notifyTabSelected(context: TabSelectedContext): Promise<void>;
    notifyTabDeselected(context: TabSelectedContext): Promise<void>;
}
export interface ControlKeyDownEventArgs {
    control: IControlViewModel;
    event: React.KeyboardEvent;
}
export interface CardControlCreationOverride {
    (control: CardTypeControlSealed, block: CardTypeBlockSealed, form: CardTypeFormSealed, parentControl: CardTypeControlSealed | null, model: ICardModel): IControlViewModel | null;
}
export interface CardControlInitializer {
    (control: IControlViewModel, model: ICardModel): void;
}
export interface CardBlockInitializer {
    (block: IBlockViewModel, model: ICardModel): void;
}
export interface CardFormInitializer {
    (form: IFormViewModel, model: ICardModel): void;
}
export interface CardTaskInitializer {
    (model: ICardModel): void;
}
export {};
