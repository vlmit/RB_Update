import { IFormViewModel } from './interfaces';
/**
 * Контекст метода по уведомлению форм, блоков и контролов при переключении вкладок.
 */
export declare class TabSelectedContext {
    readonly selectedTab: IFormViewModel | null;
    readonly sender: object | null;
    /**
     * Создаёт экземпляр класса с указанием значений его свойств.
     * @param {IFormViewModel} selectedTab Вкладка, на которую было выполнено переключение.
     * Это может быть как вкладка основной формы, так и вкладка контрола "Вкладки".
     * @param {object} sender Объект, инициировавший событие по переключению вкладок. Может быть равен null.
     */
    constructor(selectedTab: IFormViewModel | null, sender: object | null);
}
/**
 * Контекст метода по уведомлению форм, блоков и контролов при переключении вкладок.
 */
export interface TabSelectedEventArgs {
    readonly sender: object | null;
    readonly context: TabSelectedContext;
}
