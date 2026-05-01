import {
  TabPanelUIExtension,
  ITabPanelUIExtensionContext,
  TabPanelButton,
  showLoadingOverlay
} from 'tessa/ui';
import { openCard } from 'tessa/ui/uiHost';

/**
 * Добавляет дополнительную кнопку в панель с вкладками. При нажатии на кнопку открывает виртуальную карточку.
 *
 * Результат работы расширения:
 * Пример данного расширения добавляет дополнительную кнопку в панель с вкладками,
 * при нажатии на которую открывается виртуальная карточка "Мои замещения".
 */
export class CustomTabPanelButtonUIExtension extends TabPanelUIExtension {
  public initialize(context: ITabPanelUIExtensionContext): void {
    // выводится слева и на мобилах выводтся в контекстном меню
    context.buttons.push(
      TabPanelButton.create({
        name: 'RoleDeputiesManagement',
        caption: '$UI_Tiles_RoleDeputiesManagement',
        icon: 'icon-thin-285',
        buttonAction: async () => {
          // открываем виртуальную карточку "Мои замещения" для текущего пользователя
          await showLoadingOverlay(async splashResolve => {
            const editor = await openCard({
              cardTypeId: 'cb931209-2ad9-4370-bb3c-3172e61937ba', // RoleDeputiesManagementTypeID
              splashResolve
            });

            if (editor) {
              const workspaceInfo = '$UI_Tiles_Settings';
              if (editor.workspaceInfo !== workspaceInfo) {
                editor.workspaceInfo = workspaceInfo;
                editor.cardModelInitialized.add(async e => {
                  e.workspaceInfo = workspaceInfo;
                });
              }
            }
          });
        }
      })
    );
  }
}
