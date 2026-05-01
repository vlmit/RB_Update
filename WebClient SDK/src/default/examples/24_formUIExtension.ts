import { FormUIExtension } from 'tessa/ui';
import { IFormUIExtensionContext, showMessage, UIButton } from 'tessa/ui';

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
export class ExampleFormUIExtension extends FormUIExtension {
  public initialized(context: IFormUIExtensionContext): void {
    console.info(`ExampleFormUIExtension.initialized started for ${context.form.name}`);

    // можем задать настроки диалога
    context.dialogOptions = { hideTopCloseIcon: true };

    // есть возможность получить контрол через алиас
    // const someControl = context.model.controls.get('ALIAS') as ButtonViewModel;

    // добавляем новую кнопку с желаемым функционалом в форму
    context.buttons.push(
      new UIButton('TEST BTN FROM ExampleFormUIExtension', async () => {
        await showMessage('TestButton was clicked!', 'Test', { OKButtonText: 'OK!' });
      })
    );

    // добавляем функционал на закрытие диалогового окна
    context.onDialogClosed = async () => {
      await showMessage('Dialog was closed!', 'Test', { OKButtonText: 'OK!' });
    };
  }

  public shouldExecute(_context: IFormUIExtensionContext): boolean {
    // расширение применимо для всех форм диалога
    return true;
  }
}
