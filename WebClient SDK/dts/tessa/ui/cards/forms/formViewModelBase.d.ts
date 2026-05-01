import { ICardModel, IFormViewModel, IFormState, IBlockViewModel } from '../interfaces';
import { SupportUnloadingViewModel } from '../supportUnloadingViewModel';
import { CardTypeForm, CardTypeFormSealed } from 'tessa/cards/types';
import { IValidationResultBuilder } from 'tessa/platform/validation';
import { EventHandler } from 'tessa/platform';
import { TabSelectedContext, TabSelectedEventArgs } from '../tabSelectedEventArgs';
/**
 * Базовый класс для моделей представления формы в автоматическом UI карточки.
 */
export declare abstract class FormViewModelBase extends SupportUnloadingViewModel implements IFormViewModel {
    constructor(form: CardTypeForm, model: ICardModel);
    protected _initialized: boolean;
    protected _blocks: ReadonlyArray<IBlockViewModel>;
    protected _tabCaption: string | null;
    protected _blockMargin: string | null;
    protected _blockInterval: number | null;
    protected _horizontalInterval: number | undefined;
    protected _filePreviewIsDisabled: boolean;
    protected _gridColumns: (number | null)[] | undefined;
    protected _gridRows: (number | null)[] | undefined;
    protected _headerClass: string;
    protected _contentClass: string;
    protected _className: string;
    protected _hasFileControl: boolean;
    protected _isCollapsed: boolean;
    readonly cardModel: ICardModel;
    readonly componentId: guid;
    /**
     * Информация о типе отображаемой формы.
     */
    readonly cardTypeForm: CardTypeFormSealed;
    /**
     * Упорядоченная коллекция блоков на форме, доступная только для чтения.
     */
    get blocks(): ReadonlyArray<IBlockViewModel>;
    /**
     * Имя формы, по которому она доступна в коллекции, или null, если это основная форма
     * типа карточки или другая форма, не имеющая имени.
     */
    get name(): string | null;
    /**
     * Признак того, что форма не содержит отображаемых данных.
     */
    get isEmpty(): boolean;
    /**
     * Заголовок вкладки или null, если форма не является вкладкой или заголовок не задан.
     */
    get tabCaption(): string | null;
    set tabCaption(value: string | null);
    /**
     * Отступ между блоками внутри формы в виде значения для css-параметра margin или padding.
     */
    get blockMargin(): string | null;
    set blockMargin(value: string | null);
    /**
     * Отступ между блоками внутри формы.
     */
    get blockInterval(): number | null;
    set blockInterval(value: number | null);
    /**
     * Отступ горизонтальный между блоками внутри формы.
     */
    get horizontalInterval(): number | undefined;
    set horizontalInterval(value: number | undefined);
    /**
     * Не показывать предпросмотр
     */
    get filePreviewIsDisabled(): boolean;
    set filePreviewIsDisabled(value: boolean);
    protected get filePreviewIsDisabledInternal(): boolean;
    /**
     * Описание колонок блоков
     */
    get gridColumns(): (number | null)[] | undefined;
    set gridColumns(value: (number | null)[] | undefined);
    /**
     * Описание строк блоков
     */
    get gridRows(): (number | null)[] | undefined;
    set gridRows(value: (number | null)[] | undefined);
    get headerClass(): string;
    get contentClass(): string;
    get className(): string;
    set className(value: string);
    get hasFileControl(): boolean;
    get isCollapsed(): boolean;
    set isCollapsed(value: boolean);
    initialize(): void;
    protected initializeCore(): void;
    getState(): IFormState;
    setState(state: IFormState): boolean;
    close(): boolean;
    onUnloading(validationResult: IValidationResultBuilder): void;
    readonly closed: EventHandler<() => void>;
    tabSelected: EventHandler<(args: TabSelectedEventArgs) => void>;
    tabDeselected: EventHandler<(args: TabSelectedEventArgs) => void>;
    notifyTabSelected(context: TabSelectedContext): Promise<void>;
    notifyTabDeselected(context: TabSelectedContext): Promise<void>;
}
export declare class FormViewModelBaseState implements IFormState {
    constructor(form: FormViewModelBase);
    readonly isCollapsed: boolean;
    apply(form: FormViewModelBase): boolean;
}
