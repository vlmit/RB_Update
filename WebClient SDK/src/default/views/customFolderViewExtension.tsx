import * as React from 'react';
import { observer } from 'mobx-react';
import classNames from 'classnames';
import { ApplicationExtension } from 'tessa';
import { TreeItemExtension } from 'tessa/ui/views/extensions';
import { ITreeItem, FolderTreeItem } from 'tessa/ui/views/workplaces/tree';
import { IContentProvider, ViewComponentRegistry, IViewContext } from 'tessa/ui/views';
import { LocalizationManager } from 'tessa/localization';
// import { ITessaViewResult, RequestParameterBuilder, TessaViewRequest, ViewService, convertRowsToMap } from 'tessa/views';
// import { isNotNullCriteriaOperator } from 'tessa/views/metadata';
// import { showMessage, showNotEmpty } from 'tessa/ui';
// import { ValidationResult } from 'tessa/platform/validation';

export class CustomFolderViewExtension extends TreeItemExtension {

  public getExtensionName(): string {
    return 'Tessa.Extensions.Default.Client.Views.CustomFolderViewExtension';
  }

  public initialize(model: ITreeItem) {
    

    model.switchExpandOnSingleClick = false;
    if (model instanceof FolderTreeItem) {
      model.hasContent = true;
    }
    model.contentProviderFactory = () => new CustomFolderContentProvider(model);
  }

  // private static async viewRequestCommand(): Promise<void> {
  //   // пытаемся найти представление "Контрагенты"
  //   const partnersView = ViewService.instance.getByName('Partners');
  //   if (!partnersView) {
  //     return;
  //   }

  //   const request = new TessaViewRequest(partnersView.metadata);

  //   // добавляем параметр фильтрации по имени контрагента (для примера, что имя не равно null)
  //   const nameParam = new RequestParameterBuilder()
  //     .withMetadata(partnersView.metadata.parameters.get('Name')!)
  //     .addCriteria(isNotNullCriteriaOperator())
  //     .asRequestParameter();
  //   request.values.push(nameParam);

  //   let result: ITessaViewResult;
  //   try {
  //     // в getData будут добавлены параметры currentUserId и locale
  //     result = await partnersView.getData(request);
  //   } catch (err) {
  //     await showNotEmpty(ValidationResult.fromError(err));
  //     return;
  //   }

  //   // конвертируем строки в Map<string, any>[] для удобства
  //   const rows = convertRowsToMap(result.columns, result.rows);

  //   const text: string[] = [];
  //   rows.forEach(row => {
  //     const rowText: string[] = [];
  //     row.forEach((v, k) => {
  //       rowText.push(`${k}: ${v}`);
  //     });
  //     text.push(rowText.join(';'));
  //   });

  //   await showMessage(text.join('\n'));
  // }



}








export class CustomFolderInitializeExtension extends ApplicationExtension {

  public initialize() {
    ViewComponentRegistry.instance.register(CustomFolderViewModel, () => CustomViewContentComponent);
  }

}

class CustomFolderContentProvider implements IContentProvider<CustomFolderViewModel> {

  constructor(tree: ITreeItem) {
    this.viewModel = new CustomFolderViewModel(tree);
  }

  public readonly viewModel: CustomFolderViewModel;

  public readonly viewContext: IViewContext | null = null;

  public readonly components: ReadonlyMap<guid, IViewContext> = new Map();

  public async refresh(): Promise<void> {
  }

  public dispose() {
  }

}

class CustomFolderViewModel {

  constructor(tree: ITreeItem) {
    this.tree = tree;
  }

  public readonly tree: ITreeItem;

}

interface CustomFolderComponentProps {
  // tslint:disable-next-line:no-any
  viewModel: CustomFolderViewModel;
}

@observer
class CustomViewContentComponent extends React.Component<CustomFolderComponentProps> {

  public render() {
    const { viewModel } = this.props;
    let icon = viewModel.tree.isExpanded
      ? viewModel.tree.expandedIcon
      : viewModel.tree.icon;
    if (!icon) {
      icon = 'icon-thin-101';
    }
    return (
      <div style={{
        height: '90vh',
        fontSize: '80px',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center'
      }}>
        <div>
          <i className={classNames('icon ta', icon)} />
          <div style={{
            display: 'inline-block'
          }}>
            {LocalizationManager.instance.localize(viewModel.tree.text)}
          </div>
        </div>
      </div>
    );
  }

}