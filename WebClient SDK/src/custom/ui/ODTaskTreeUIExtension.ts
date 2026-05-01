import { CardUIExtension, ICardUIExtensionContext, CardToolbarAction } from 'tessa/ui/cards';
import { showNotEmpty, showMessage } from 'tessa/ui';
import { getTessaIcon } from 'common/utility';
import { Guid } from 'tessa/platform';
//import { openCard } from 'tessa/ui/uiHost';
//import { LocalizationManager } from 'tessa/localization';
import { RequestParameterBuilder, ViewService,
TessaViewRequest, ITessaViewResult, convertRowsToMap } from 'tessa/views';
import { ValidationResult } from 'tessa/platform/validation';
import { equalsCriteriaOperator } from 'tessa/views/metadata';

/**
 * Добавить глобальную группу\тайл в левую панель. При нажатии:
 * - формируем кастомный реквест на сервер, выполняем его, читаем что-то из reponse, который пришел
 * с сервера (из info, например) и выводим сообщение с этим прочитанным результатом.
 * - формируем реквест на вьюху, добавляем туда для примера какой-то поисковый параметр и результат показываем в сообщении.
 */
 export class ODTaskTreeUIExtension extends CardUIExtension {
  //private _disposer: Function | null = null;

  public initialized(context: ICardUIExtensionContext) {
    // если карточка не Протокол, то ничего не делаем
    if (!Guid.equals(context.card.typeId, '4d9f9590-0131-4d32-9710-5e07c282b5d3')
    ) {
      return;
    }

     // Добавляем тестовые кнопки в тулбар.
     if (context.toolbar) {
      context.toolbar.removeItemIfExists('TestButton');
      context.toolbar.addItem(
        new CardToolbarAction({
          name: 'TestButton',
          caption: 'Test Button',
          icon: getTessaIcon('Thin359'),
          command: () => { ODTaskTreeUIExtension.viewRequestCommand(context)
          }
        })
      );
    }
     
  }

  private static async viewRequestCommand(context: ICardUIExtensionContext) {
    // пытаемся найти представление "Древо поручений"
    const partnersView = ViewService.instance.getByName('ControlTasks_RB_HierarchyTree');
    if (!partnersView) {
      return;
    }

    const request = new TessaViewRequest(partnersView.metadata);

    // добавляем параметр фильтрации по имени контрагента (для примера, что имя не равно null)
    const paramCardId = new RequestParameterBuilder()
      .withMetadata(partnersView.metadata.parameters.get('Parent')!)
      .addCriteria(equalsCriteriaOperator(), 'Parent', context.card.id)
      .asRequestParameter();
    request.values.push(paramCardId);

    let result: ITessaViewResult;
    try {
      // в getData будут добавлены параметры currentUserId и locale
      result = await partnersView.getData(request);
    } catch (err) {
      await showNotEmpty(ValidationResult.fromError(err));
      return;
    }

    // конвертируем строки в Map<string, any>[] для удобства
    const rows = convertRowsToMap(result.columns, result.rows);

    let text: string[] = [];
    rows.forEach(row => {
      const rowText: string[] = [];
      row.forEach((v, k) => {
        rowText.push(`${k}: ${v}`);
      });
      text.push(rowText.join(';'));
    });

    await showMessage(text.join('\n'));
  }
 
 
}
