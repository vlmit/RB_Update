import { TasksFormViewModel } from './tasksFormViewModel';
import { DefaultFormTabWithTaskHistoryViewModel } from './defaultFormTabWithTaskHistoryViewModel';
import { ICardModel, IBlockViewModel } from '../interfaces';
import { CardTypeForm } from 'tessa/cards/types';
import { CardTask } from 'tessa/cards/cardTask';
import { TaskViewModel } from 'tessa/ui/cards/tasks';
import { IValidationResultBuilder } from 'tessa/platform/validation';
/**
 * Форма карточки с заданиями. Хотя бы одно задание должно быть загружено,
 * но заданий может не отображаться в случае, если все существующие задания отложены.
 */
export declare class DefaultFormTabWithTasksViewModel extends DefaultFormTabWithTaskHistoryViewModel {
    /**
     * Создаёт экземпляр класса с указанием информации,
     * необходимой для создания формы по умолчанию основной части карточки.
     * @param form Метаинформация по форме.
     * @param model Модель карточки в UI.
     * @param blocks Коллекция моделей представления блоков внутри формы.
     * @param blockInterval Вертикальный интервал между блоками.
     */
    constructor(form: CardTypeForm, model: ICardModel, blocks: IBlockViewModel[], blockInterval: number, filePreviewIsDisabled: boolean, tasks: CardTask[], horizontalInterval?: number, gridColumns?: (number | null)[], gridRows?: (number | null)[]);
    private _postponedTasksAreVisible;
    private _authorLockedTasksAreVisible;
    readonly tasks: TaskViewModel[];
    readonly tasksTab: TasksFormViewModel;
    get visibileTasks(): ReadonlyArray<TaskViewModel>;
    get hasPostponedTasks(): boolean;
    get hasAuthorLockedTasks(): boolean;
    get postponedTasksAreVisible(): boolean;
    set postponedTasksAreVisible(value: boolean);
    get authorLockedTasksAreVisible(): boolean;
    set authorLockedTasksAreVisible(value: boolean);
    createTaskViewModel(task: CardTask): TaskViewModel;
    onUnloading(validationResult: IValidationResultBuilder): void;
}
