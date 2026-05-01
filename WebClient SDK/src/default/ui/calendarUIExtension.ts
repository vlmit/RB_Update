import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
import { ButtonViewModel } from 'tessa/ui/cards/controls';
import { hasFlag, createTypedField, DotNetType } from 'tessa/platform';
import { CardPermissionFlags } from 'tessa/cards';
import { showNotEmpty, UIContext, showMessage } from 'tessa/ui';
import { OperationService, OperationTypes } from 'tessa/platform/operations';
import { BusinessCalendarService, RebuildOperationGuidKey } from 'tessa/businessCalendar';

export class CalendarUIExtension extends CardUIExtension {
  public initialized(context: ICardUIExtensionContext) {
    CalendarUIExtension.attachCommandToButton(
      context,
      'ValidateCalendar',
      CalendarUIExtension.validateCalendarButtonAction
    );
    CalendarUIExtension.attachCommandToButton(
      context,
      'RebuildCalendar',
      CalendarUIExtension.rebuildCalendarButtonAction
    );
  }

  private static attachCommandToButton(
    context: ICardUIExtensionContext,
    buttonAlias: string,
    action: Function
  ) {
    const control = context.model.controls.get(buttonAlias);
    if (!control) {
      return;
    }

    const button = control as ButtonViewModel;
    if (
      !hasFlag(
        context.model.card.permissions.resolver.getCardPermissions(),
        CardPermissionFlags.AllowModify
      )
    ) {
      button.isReadOnly = true;
      return;
    }

    // tslint:disable-next-line:no-any
    button.onClick = action as any;
  }

  private static async validateCalendarButtonAction() {
    const result = await BusinessCalendarService.validateCalendar();
    await showNotEmpty(result, '$UI_BusinessCalendar_ValidatedTitle');
  }

  private static async rebuildCalendarButtonAction() {
    const context = UIContext.current;
    const editor = context.cardEditor;

    if (editor && !editor.operationInProgress) {
      await editor.setOperationInProgress(async () => {
        const operationId = await OperationService.instance.create({
          typeId: OperationTypes.CalendarRebuild
        });

        const result = await editor.saveCard(context, {
          [RebuildOperationGuidKey]: createTypedField(operationId, DotNetType.Guid)
        });

        if (!result) {
          return;
        }

        let isActive = true;
        while (isActive) {
          await new Promise(resolve => setTimeout(resolve, 1000));
          isActive = await OperationService.instance.isAlive(operationId);
        }
      });

      await showMessage('$UI_BusinessCalendar_CalendarIsRebuiltNotification');
    }
  }
}
