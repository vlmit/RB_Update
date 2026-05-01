import { CardUIExtension, ICardUIExtensionContext, CardDefaultDialog, CardToolbarAction } from 'tessa/ui/cards';
import { Card } from 'tessa/cards';
import { tryGetFromInfo, showConfirmWithCancel } from 'tessa/ui';
import { TypedField, DotNetType, getTypedFieldValue } from 'tessa/platform';
import { openMarkedCard } from 'tessa/ui/tiles';

export class KrEditModeToolbarUIExtension extends CardUIExtension {

  public initialized(context: ICardUIExtensionContext) {
    if (context.dialogName !== CardDefaultDialog) {
      return;
    }

    if (KrEditModeToolbarUIExtension.tileIsVisible(context.card, 'KrEditMode')) {
      context.toolbar.addItemIfNotExists(new CardToolbarAction({
        name: 'KrEditMode',
        caption: '$KrTiles_EditMode',
        icon: 'ta icon-thin-002',
        order: 11,
        toolTip: '$KrTiles_EditModeTooltip',
        command: this.openForEditing
      }),
      {
        name: 'Alt+E', key: 'KeyE', modifiers: {alt: true}
      });
    } else {
      context.toolbar.removeItemIfExists('KrEditMode');
    }
  }

  private openForEditing = async () => {
    await openMarkedCard(
      'kr_calculate_permissions',
      null, // Не требуем подтверждения действия, если не было изменений
      () => showConfirmWithCancel('$KrTiles_EditModeConfirmation')
    );
  }

  private static tileIsVisible(card: Card, tileName: string): boolean {
    const info = card.tryGetInfo();
    if (!info) {
      return false;
    }

    const tiles = tryGetFromInfo<TypedField<DotNetType.String, string>[] | null>(info, '.tiles', null);
    if (!tiles) {
      return false;
    }

    for (let tile of tiles) {
      const tileValue = getTypedFieldValue(tile) as string;
      if (tileValue.toLocaleLowerCase() === tileName.toLocaleLowerCase()) {
        return true;
      }
    }

    return false;
  }

}