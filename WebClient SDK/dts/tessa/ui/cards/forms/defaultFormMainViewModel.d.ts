import { DefaultFormViewModel, DefaultFormViewModelState } from './defaultFormViewModel';
import { PreviewFormViewModel } from './previewFormViewModel';
import { ICardModel, IFormViewModel, IBlockViewModel, IFormState, ITabbedFormViewModel } from '../interfaces';
import { CardTypeForm } from 'tessa/cards/types';
import { IValidationResultBuilder } from 'tessa/platform/validation';
import { MenuAction } from 'tessa/ui/menuAction';
/**
 * Модель представления для формы по умолчанию основной части карточки.
 */
export declare abstract class DefaultFormMainViewModel extends DefaultFormViewModel implements ITabbedFormViewModel {
    constructor(form: CardTypeForm, model: ICardModel, blocks: IBlockViewModel[], blockInterval: number, filePreviewIsDisabled: boolean, horizontalInterval?: number, gridColumns?: (number | null)[], gridRows?: (number | null)[]);
    protected _tabs: IFormViewModel[];
    protected _selectedTab: IFormViewModel | null;
    protected _openedTabs: Set<string>;
    protected _menuActions: MenuAction[];
    get openedTabs(): ReadonlySet<string>;
    /**
     * Вкладки карточки.
     */
    get tabs(): ReadonlyArray<IFormViewModel>;
    /**
     * Видимые вкладки карточки.
     */
    get visibleTabs(): ReadonlyArray<IFormViewModel>;
    /**
     * Текущая выбранная вкладка.
     */
    get selectedTab(): IFormViewModel;
    set selectedTab(value: IFormViewModel);
    readonly previewTab: PreviewFormViewModel;
    get previewHidden(): boolean;
    protected _tabInitializing: boolean;
    protected initializeCore(): void;
    restoreSelectedTab(): void;
    private findAvailableSelectedTab;
    getState(): IFormState;
    get menuActions(): MenuAction[];
    set menuActions(value: MenuAction[]);
    getContextMenu(): MenuAction[];
    tabsAreCollapsed: boolean;
    onUnloading(validationResult: IValidationResultBuilder): void;
}
/**
 * Состояние формы DefaultFormMainViewModel.
 * Наследники класса DefaultFormMainViewModel могут унаследовать
 * класс DefaultFormMainViewModelState, добавив в него сохраняемую информацию.
 */
export declare class DefaultFormMainViewModelState extends DefaultFormViewModelState {
    constructor(form: DefaultFormMainViewModel);
    readonly tabsAreCollapsed: boolean;
    readonly selectedTabIndex: number;
    readonly formStatesByName: Map<string, IFormState>;
    apply(form: DefaultFormMainViewModel): boolean;
}
