import { runInAction } from 'mobx';
import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
import { Guid } from 'tessa/platform';
import { AutoCompleteEntryViewModel, AutoCompleteTableViewModel } from 'tessa/ui/cards/controls';
import { TestCardTypeID } from './common';

/**
 * В web-клиенте есть два вида автокомплитов:
 * - Первый используется для десктопного режима - текст набирается прямо в элементе управления и
 *   окно с подсказками появляется рядом с курсором. Также и выпадающий список комбобокса.
 * - Второй используется для мобильных устройств - это отдельный диалог.
 *
 * Достоинством второго является возможность показать в диалоге длинный список подсказок, т.к. в нем есть
 * прокрутка. Вариант с диалогом удобно использовать в десктопе для некоторых автокомплитов, где заранее
 * известно, что будет много вариантов выбора. Т.е. диалог по умолчанию есть только в мобильной версии,
 * но, если нужно, его можно включить и на десктопной версии для выбранных контролов.
 *
 * В этом расширении демонстрируется открытие определённых контролов автокомплита для выбранного типа карточки
 * в режиме диалога.
 *
 * Результат работы расширения:
 * Проверяем, что значения типа карточки и алиаса контрола те, что нам нужны и если так, то добавляем флаг,
 * который указывает автокомплиту работать в режиме диалога.
 */
export class DialogModeAutocompleteUIExtension extends CardUIExtension {
  public initialized(context: ICardUIExtensionContext): void {
    // если карточка не для тестов, то ничего не делаем
    if (!Guid.equals(context.card.typeId, TestCardTypeID)) {
      return;
    }

    // находим контрол с автокомплитом
    const driverNameControl = context.model.controls.get(
      'DriverName2'
    ) as AutoCompleteEntryViewModel;
    if (driverNameControl) {
      // runInAction нужен в том случае если мы пытаемся поменять observable поле,
      // для которого уже созданы observers
      runInAction(() => {
        driverNameControl.alwaysShowInDialog = true;
      });
    }

    // находим контрол с автокомплитом
    const ownersListControl = context.model.controls.get('Owners') as AutoCompleteTableViewModel;
    if (ownersListControl) {
      // runInAction нужен в том случае если мы пытаемся поменять observable поле,
      // для которого уже созданы observers
      runInAction(() => {
        ownersListControl.alwaysShowInDialog = true;
      });
    }
  }
}
