import {
  TileExtension,
  ITileGlobalExtensionContext,
  Tile,
  TileGroups,
  TileEvaluationEventArgs
} from 'tessa/ui/tiles';
import { UIContext, showMessage } from 'tessa/ui';
import { ITessaView } from 'tessa/views';

/**
 * Данное расширение позволяет добавлять/скрывать/показывать тайл в левой панели,
 * при нажатии на который отображаются данные выделенной строки для выбранного представления.
 * Отображение тайла зависит от алиаса представления и идентификатора узла рабочего места.
 *
 * Результат работы расширения:
 * Добавляет на левую панель тайл, который по нажатию открывает модальное окно
 * с данными выделенной строки представления. Добавленный тайл отображается
 * только для представления **Мои документы**.
 */
export class SimpleViewTileExtension extends TileExtension {
  private static myDocumentsAlias = 'MyDocuments';

  public initializingGlobal(context: ITileGlobalExtensionContext): void {
    // получаем доступ к левой панели тайлов
    const panel = context.workspace.leftPanel;

    // создаем новый тайл
    const tile = new Tile({
      name: 'SimpleViewTile',
      caption: 'SimpleViewTile',
      icon: 'ta icon-thin-002',
      contextSource: panel.contextSource,
      group: TileGroups.Views,
      order: 100,
      command: SimpleViewTileExtension.showViewData,
      evaluating: SimpleViewTileExtension.enableIfMyDocumentsViewAndHasSelectedRow
    });

    // добавляем тайл в левую боковую панель
    panel.tiles.push(tile);
  }

  private static async showViewData(): Promise<void> {
    // пытаемся получить текущий ViewContext
    const viewContext = UIContext.current.viewContext;
    // tslint:disable-next-line:no-any
    let currentRow: ReadonlyMap<string, any>;
    if (!viewContext || !(currentRow = viewContext.selectedRow!)) {
      return;
    }

    const text: string[] = [];
    currentRow.forEach((v, k) => {
      text.push(`${k}: ${v}`);
    });

    await showMessage(text.join('\n'));
  }

  private static enableIfMyDocumentsViewAndHasSelectedRow(e: TileEvaluationEventArgs): void {
    // пытаемся получить текущий ViewContext
    const viewContext = UIContext.current.viewContext;
    let view: ITessaView;
    e.setIsEnabledWithCollapsing(
      e.currentTile,
      !!viewContext &&
      !!(view = viewContext.view!) && // получаем модель текущего представления
        view.metadata.alias === SimpleViewTileExtension.myDocumentsAlias &&
        !!viewContext.selectedRow
    );
  }
}
