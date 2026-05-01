import { CardTypeForm } from 'tessa/cards/types';
import { DefaultFormViewModel } from './defaultFormViewModel';
import { ICardModel, IBlockViewModel } from '../interfaces';
/**
 * Модель представления для формы по умолчанию, которая создаётся
 * для редактирования строки коллекционной, древовидной секции или отображения таска.
 */
export declare class DefaultFormSimpleViewModel extends DefaultFormViewModel {
    /**
     * Создаёт экземпляр класса с указанием метаинформации по форме, модели карточки
     * и списка моделей представления блоков внутри формы.
     * @param form Метаинформация по форме.
     * @param model Модель карточки в UI.
     * @param blocks Коллекция моделей представления блоков внутри формы.
     * @param blockInterval Вертикальный интервал между блоками.
     */
    constructor(form: CardTypeForm, model: ICardModel, blocks: IBlockViewModel[], blockInterval: number, filePreviewIsDisabled: boolean, horizontalInterval?: number, gridColumns?: (number | null)[], gridRows?: (number | null)[]);
    private _modelInitializing;
    protected initializeCore(): Promise<void>;
}
