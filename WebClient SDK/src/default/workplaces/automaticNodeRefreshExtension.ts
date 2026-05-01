import { reaction } from 'mobx';
import { TreeItemExtension } from 'tessa/ui/views/extensions';
import { ITreeItem, isTreeItemVisibleInPath } from 'tessa/ui/views/workplaces/tree';
import { IStorage } from 'tessa/platform/storage';
import { tryGetFromSettings } from 'tessa/ui';
import { IWorkplaceViewModel, IWorkplaceViewComponent } from 'tessa/ui/views';
import { ITessaViewResult, RequestParameterBuilder, TessaViewRequest, ViewService } from 'tessa/views';
//import { isNotNullCriteriaOperator } from 'tessa/views/metadata';
import { ValidationResult } from 'tessa/platform/validation';
import { isTrueCriteriaOperator } from 'tessa/views/metadata';

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

  public async initialized(model: ITreeItem) {

    if (model.text === '$Workplaces_User_MyTasks') {
      const count = await AutomaticNodeRefreshExtension.viewRequestCommand('MyTasks');
      if (count > 0) {
        model.text = 'Мои задания' + ' ' + count;
      }
    }
    else if(model.text === 'В работе') {
      const count = await AutomaticNodeRefreshExtension.viewInWorkTasks('MyTasks');
      if (count > 0) {
        model.text = model.text + ' ' + count;
      }
    }

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

  private static async viewRequestCommand(viewName: string): Promise<number> {
    // пытаемся найти представление "Контрагенты"
    const partnersView = ViewService.instance.getByName(viewName);
    if (!partnersView) {
      console.log('1');
      return 0;
    }

    console.log(partnersView);

    const request = new TessaViewRequest(partnersView.metadata);

    

    let result: ITessaViewResult;
    try {
      // в getData будут добавлены параметры currentUserId и locale
      result = await partnersView.getData(request);
    } catch (err) {
      //await showNotEmpty(ValidationResult.fromError(err));
      console.log(ValidationResult.fromError(err));
      return 0;
    }

    console.log(result.rows.length);

    return result.rows.length
  }

  private static async viewInWorkTasks(viewName: string): Promise<number> {
    // пытаемся найти представление "Контрагенты"
    const partnersView = ViewService.instance.getByName(viewName);
    if (!partnersView) {
      console.log('1');
      return 0;
    }

    console.log(partnersView);

    const request = new TessaViewRequest(partnersView.metadata);

    const nameParam = new RequestParameterBuilder()
    .withMetadata(partnersView.metadata.parameters.get('InWork')!)
    .addCriteria(isTrueCriteriaOperator())
    .asRequestParameter();
  request.values.push(nameParam);

    let result: ITessaViewResult;
    try {
      // в getData будут добавлены параметры currentUserId и locale
      result = await partnersView.getData(request);
    } catch (err) {
      //await showNotEmpty(ValidationResult.fromError(err));
      console.log(ValidationResult.fromError(err));
      return 0;
    }

    console.log(result.rows.length);

    return result.rows.length
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