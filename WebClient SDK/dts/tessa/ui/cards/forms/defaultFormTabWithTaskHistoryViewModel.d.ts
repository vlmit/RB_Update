import { DefaultFormMainViewModel } from './defaultFormMainViewModel';
import { ICardModel, IBlockViewModel } from '../interfaces';
import { CardTypeForm } from 'tessa/cards/types';
import { TaskHistoryViewModel } from 'tessa/ui/cards/tasks';
/**
 * Форма карточки с историей заданий.
 */
export declare class DefaultFormTabWithTaskHistoryViewModel extends DefaultFormMainViewModel {
    /**
     * Создаёт экземпляр класса с указанием информации,
     * необходимой для создания формы по умолчанию основной части карточки.
     * @param form Метаинформация по форме.
     * @param model Модель карточки в UI.
     * @param blocks Коллекция моделей представления блоков внутри формы.
     * @param blockInterval Вертикальный интервал между блоками.
     */
    constructor(form: CardTypeForm, model: ICardModel, blocks: IBlockViewModel[], blockInterval: number, filePreviewIsDisabled: boolean, horizontalInterval?: number, gridColumns?: (number | null)[], gridRows?: (number | null)[]);
    readonly taskHistory: TaskHistoryViewModel;
}
