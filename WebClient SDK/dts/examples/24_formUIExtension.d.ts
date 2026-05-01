import { FormUIExtension } from 'tessa/ui';
import { IFormUIExtensionContext } from 'tessa/ui';
/**
 * Позволяет добавлять дополнительные элементы управления с желаемой функциональностью
 * в выбранную форму диалога, а также изменять ее настройки.
 *
 * Результат работы расширения:
 * В форму диалога (например, диалоговое окно загрузки "Deski") добавляет тестовую кнопку, при нажатии
 * на которую отображается сообщение в диалоговом окне о нажатии кнопки. При закрытии формы диалога,
 * появляется диалоговое окно с сообщением о закрытии формы. Также, изменяет настройки рассматриваемой
 * формы диалога: удаляет кнопку закрытия из правого вехнего угла.
 */
export declare class ExampleFormUIExtension extends FormUIExtension {
    initialized(context: IFormUIExtensionContext): void;
    shouldExecute(_context: IFormUIExtensionContext): boolean;
}
