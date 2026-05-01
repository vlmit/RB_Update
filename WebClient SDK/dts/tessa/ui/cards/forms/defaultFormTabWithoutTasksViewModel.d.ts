import { DefaultFormMainViewModel } from './defaultFormMainViewModel';
import { ICardModel, IBlockViewModel } from '../interfaces';
import { CardTypeForm } from 'tessa/cards/types';
/**
 * Модель представления для формы карточки по умолчанию со вкладками.
 */
export declare class DefaultFormTabWithoutTasksViewModel extends DefaultFormMainViewModel {
    /**
     * Создаёт экземпляр класса с указанием информации,
     * необходимой для создания формы по умолчанию основной части карточки.
     * @param form Метаинформация по форме.
     * @param model Модель карточки в UI.
     * @param blocks Коллекция моделей представления блоков внутри формы.
     * @param blockInterval Вертикальный интервал между блоками.
     */
    constructor(form: CardTypeForm, model: ICardModel, blocks: IBlockViewModel[], blockInterval: number, filePreviewIsDisabled: boolean, horizontalInterval?: number, gridColumns?: (number | null)[], gridRows?: (number | null)[]);
}
