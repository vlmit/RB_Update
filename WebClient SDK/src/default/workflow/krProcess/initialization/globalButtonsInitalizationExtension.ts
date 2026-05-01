import { KrGlobalTileContainer } from '../krGlobalTileContainer';
import { ApplicationExtension, IApplicationExtensionMetadataContext } from 'tessa';
import { tryGetFromInfo } from 'tessa/ui';
import { KrTileInfo } from 'tessa/workflow/krProcess';

export class GlobalButtonsInitalizationExtension extends ApplicationExtension {

  public afterMetadataReceived(context: IApplicationExtensionMetadataContext) {
    if (context.mainPartResponse) {
      const tiles = tryGetFromInfo(context.mainPartResponse.info, 'GlobalTilesInfoMark', []);
      const globalTiles: KrTileInfo[] = [];
      if (Array.isArray(tiles)) {
        for (let tile of tiles) {
          globalTiles.push(new KrTileInfo(tile));
        }
      } else {
        const names = Object.getOwnPropertyNames(tiles);
        for (let name of names) {
          globalTiles.push(new KrTileInfo(tiles[name]));
        }
      }

      KrGlobalTileContainer.instance.init(globalTiles);
    }
  }

}