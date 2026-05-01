import { ControlViewModelBase } from './controlViewModelBase';
import { CardTypeControl } from 'tessa/cards/types';
import { IFilePreviewControl, IFileControl, IFileControlManager } from 'tessa/ui/files';
/**
 * Элемент управления предпросмотром в карточке.
 */
export declare class FilePreviewViewModel extends ControlViewModelBase implements IFilePreviewControl {
    constructor(control: CardTypeControl, minHeight?: number, maxHeight?: number);
    private _minHeight?;
    /**
     * Минимальная высота элемента управления. Укажите <c>double.NaN</c>, чтобы не ограничивать размер.
     */
    get minHeight(): number | undefined;
    set minHeight(value: number | undefined);
    private _maxHeight?;
    /**
     * Максимальная высота элемента управления. Укажите <c>double.NaN</c>, чтобы не ограничивать размер.
     */
    get maxHeight(): number | undefined;
    set maxHeight(value: number | undefined);
    /**
     * Объект, управляющий доступностью предпросмотра.
     */
    fileControlManager: IFileControlManager;
    attach(fileControl: IFileControl): void;
}
