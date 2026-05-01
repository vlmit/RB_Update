import { ApplicationExtension, IApplicationExtensionMetadataContext, MetadataStorage } from 'tessa';
import { IStorage } from 'tessa/platform/storage';
import { tryGetFromInfo } from 'tessa/ui';
import { TileExtension, ITileGlobalExtensionContext, Tile, TileGroups } from 'tessa/ui/tiles';

const AdditionalTilesKey = 'AdditionalTiles';

/**
 * Позволяет получать и использовать данные из _MetadataStorage.info_, добавленные
 * в мету через ServerInitializationExtension.
 *
 * Результат работы расширения:
 * - На клиенте достает данные, содержащие информацию о тайлах, добавленную через
 *   "ServerInitializationExtension" (по ключу из меты и добавляет в _MetadataStorage.info_).
 * - После инициализации приложения обращается к _MetadataStorage.info_, получает
 *   информацию о тайлах и добавляет их в левую панель.
 */
export class AdditionalMetaInitializationExtension extends ApplicationExtension {
  public afterMetadataReceived(context: IApplicationExtensionMetadataContext): void {
    if (context.mainPartResponse) {
      // проверяем наличие info в контексте
      const info = context.mainPartResponse.tryGetInfo();
      if (!info) {
        return;
      }

      // получаем информацию о тайлах, добавленную в мету через ServerInitializationExtension
      const additionalTiles = tryGetFromInfo<IStorage[] | null>(info, AdditionalTilesKey, null);
      if (!additionalTiles) {
        return;
      }

      // если информация о тайлах отсутствует в MetadataStorage, то добавляем ее
      if (!MetadataStorage.instance.info.has(AdditionalTilesKey)) {
        MetadataStorage.instance.info.set(AdditionalTilesKey, additionalTiles);
      }
    }
  }
}

export class AdditionalMetaTileExtension extends TileExtension {
  public initializingGlobal(context: ITileGlobalExtensionContext): void {
    // достаем информацию о тайлах из MetadataStorage
    const additionalTiles: IStorage[] | null = MetadataStorage.instance.info.get(
      AdditionalTilesKey
    );
    if (!additionalTiles) {
      return;
    }

    // получаем доступ к левой боковой панели
    const leftPanel = context.workspace.leftPanel;

    for (let tileInfo of additionalTiles) {
      // создаем тайлы из полученной иноформации и добавляем их в левую панель
      leftPanel.tiles.push(
        new Tile({
          name: tileInfo.name,
          caption: tileInfo.caption,
          icon: tileInfo.info,
          contextSource: leftPanel.contextSource,
          group: TileGroups.Cards,
          order: 10
        })
      );
    }
  }
}
