import { reaction } from 'mobx';
import { TreeItemExtension } from 'tessa/ui/views/extensions';
import { ITreeItem, isTreeItemVisibleInPath } from 'tessa/ui/views/workplaces/tree';
import { IStorage } from 'tessa/platform/storage';
import { tryGetFromSettings } from 'tessa/ui';
import { IWorkplaceViewModel, IWorkplaceViewComponent } from 'tessa/ui/views';

export class AutomaticNodeRefreshExtension extends TreeItemExtension {

  private _settings: AutomaticNodeRefreshSettings;

  // tslint:disable-next-line:no-any
  private _timer: any | null;

  private _refreshPending: boolean = false;

  private _treeItem: ITreeItem;

  private _disposes: Function[] = [];

  public getExtensionName(): string {
    return 'Tessa.Extensions.Default.Client.Workplaces.AutomaticNodeRefreshExtension';
  }

  public initialized(model: ITreeItem) {
    this._treeItem = model;
    this._settings = new AutomaticNodeRefreshSettings(this.settingsStorage);
    this.subscribeToEvents(model);
    reaction(
      () => model.parent,
      (parent) => {
        this.unsubscribeFromEvents();
        if (!parent) {
          this.stopTimer();
        } else {
          this.subscribeToEvents(model);
          this.startTimer();
        }
      }
    );
    if (model.workplace && model.workplace.isActive) {
      // вкладка с рабочим местом активна на момент запуска приложения
      this.startTimer();
    }
  }

  private subscribeToEvents(treeItem: ITreeItem) {
    this._disposes.push(reaction(
      () => treeItem.workplace.isActive,
      (isActive) => {
        if (isActive) {
          if (this._refreshPending) {
            this.updateByTimer(true);
          }
          this.startTimer();
        }
      }
    ));

    this._disposes.push(reaction(
      () => treeItem.isLoading,
      (isLoading) => {
        if (!isLoading && !this._timer) {
          this.stopTimer();
          this.startTimer();
          this._refreshPending = false;
        }
      }
    ));

    let currentNode = treeItem.parent;
    while (currentNode) {
      this._disposes.push(reaction(
        () => currentNode ? currentNode.isExpanded : false,
        (isExpanded) => {
          if (isExpanded && isTreeItemVisibleInPath(treeItem)) {
            if (this._refreshPending) {
              this.updateByTimer(false);
            }
            this.startTimer();
          }
        }
      ));
      currentNode = currentNode.parent;
    }
  }

  private unsubscribeFromEvents() {
    for (let dispose of this._disposes) {
      dispose();
    }
    this._disposes.length = 0;
  }

  private startTimer() {
    if (!this._timer) {
      this._timer = setInterval(() => this.updateByTimer(false), this._settings.refreshInterval * 1000);
    }
  }

  private stopTimer() {
    if (this._timer) {
      clearInterval(this._timer);
      this._timer = null;
    }
  }

  private async updateByTimer(skipUpdateTable: boolean) {
    // Если задача не успела отработать или узел находится в процессе обновления,
    // то просто выходим из задачи
    if (this._treeItem.isLoading
      || (Date.now() - this._treeItem.lastUpdateTime) < (this._settings.refreshInterval * 1000)
    ) {
      return;
    }

    if (!this._treeItem.workplace.isActive
      || !isTreeItemVisibleInPath(this._treeItem)
    ) {
      this._refreshPending = true;
      this.stopTimer();
      return;
    }

    await this._treeItem.refreshNode();
    if (this._treeItem.hasSelection()) {
      this.refreshTableContent(skipUpdateTable);
    }
  }

  private refreshTableContent(skipUpdateTable: boolean) {
    this._refreshPending = false;
    if (this._settings.withContentDataRefreshing && !skipUpdateTable) {
      this.refreshContent(this._treeItem.workplace);
    }
  }

  private refreshContent(workplaceViewModel: IWorkplaceViewModel) {
    if (!workplaceViewModel) {
      return;
    }

    // обновляем содержимое (таблицы)
    const viewContext = workplaceViewModel.context.viewContext;
    if (viewContext) {
      // получаем верхнюю вью (от которой зависят остальные)
      let rootContext = viewContext;
      while (rootContext.parentContext) {
        rootContext = rootContext.parentContext;
      }
      const viewComponent = rootContext as IWorkplaceViewComponent;

      // tslint:disable-next-line:triple-equals
      if (viewComponent.currentPage == undefined || viewComponent.currentPage === 1) {
        // либо вью не поддерживает пейджинг, либо страница и так первая, либо это какой-то кастом
        // если кастом, то надеемся, что он поддерживает RefreshCommand
        rootContext.refreshView();
      }
      else {
        // Refresh будет автоматом при изменении номера страницы на первую
        viewComponent.currentPage = 1;
      }
    }
  }

}

class AutomaticNodeRefreshSettings {

  constructor(storage: IStorage) {
    this.refreshInterval = tryGetFromSettings(storage, 'RefreshInterval', 300);
    this.withContentDataRefreshing = tryGetFromSettings(storage, 'WithContentDataRefreshing', true);
  }

  public refreshInterval: number;

  public withContentDataRefreshing: boolean;

}