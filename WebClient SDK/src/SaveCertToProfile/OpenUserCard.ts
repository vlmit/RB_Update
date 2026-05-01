import { TileExtension, ITileGlobalExtensionContext, Tile, TileGroups } from 'tessa/ui/tiles';
import { showLoadingOverlay } from 'tessa/ui';
import { openCard } from 'tessa/ui/uiHost';
import { userSession } from 'common/utility';
// import { Guid } from 'tessa/platform';


/**
 * Добавляем тайл в левой панели для представления (не в карточке), по нажатию на который открываем карточку текущего пользователя
 */
export class ODOpenUserCard extends TileExtension {

  public initializingGlobal(context: ITileGlobalExtensionContext) {
    const panel = context.workspace.leftPanel;
    const tile = new Tile({
      name: 'OpenUserCard',
      caption: 'Мои сведения',
      icon: 'ta icon-thin-193',
      contextSource: panel.contextSource,
      group: TileGroups.Cards,
      order: 100,
      command: ODOpenUserCard.cardRequestCommand
    });
    panel.tiles.push(tile);
  }

  private static async cardRequestCommand() {
    await showLoadingOverlay(async (splashResolve) => {
      const editor = await openCard({
        cardTypeId: '929ad23c-8a22-09aa-9000-398bf13979b2', // PersonalRole
        cardId: userSession.UserID,
        splashResolve
      });

      if (editor) {
        const workspaceInfo = '$UI_Tiles_Settings';
        if (editor.workspaceInfo !== workspaceInfo) {
          editor.workspaceInfo = workspaceInfo;
          editor.cardModelInitialized.add(async e => { e.workspaceInfo = workspaceInfo; });
        }
      }
    });
  }
}