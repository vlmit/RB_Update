import * as React from 'react';
import { observable, computed, action } from 'mobx';
import { observer } from 'mobx-react';
import { CardToolbarAction, CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
//import { ButtonViewModel } from 'tessa/ui/cards/controls';
import { Guid } from 'tessa/platform';
import { Dialog } from 'ui';
//import { LocalizationManager } from 'tessa/localization';
import { showViewModelDialog } from 'tessa/ui';
import { getTessaIcon } from 'common/utility';

/**
 * Показываем кастомное диалоговое окно
 */
export class showImportPFXDialogUIExtension extends CardUIExtension {

  public initialized(context: ICardUIExtensionContext) {
    // если карточка не для тестов, то ничего не делаем
    if (!Guid.equals(context.card.typeId, '929ad23c-8a22-09aa-9000-398bf13979b2')) {
      return;
    }

    // добавление кнопки в тулбар
    if (context.toolbar) {
      context.toolbar.removeItemIfExists('TaskTree');
      context.toolbar.addItem(
        new CardToolbarAction({
          name: 'TaskTree',
          caption: 'Импорт PFX (моб. приложение)',
          icon: getTessaIcon('Thin359'),
          command: async () => {
            //const data = await ODFormImportPFXUIExtension.getRequestData(cardContext);
            await showImportPFXDialogUIExtension.showCustomDialog()
            //const data = await ODFormExampleUIExtension.getRequestData(cardContext);
            //await ODFormExampleUIExtension.printImage(data);
            },
        })
      );
    }




    // const button = context.model.controls.get('CustomDialogBtn') as ButtonViewModel;
    // if (!button) {
    //   return;
    // }

    // // будем открывать кастомный диалог
    // button.onClick = showImportPFXDialogUIExtension.showCustomDialog;
  }

  private static async showCustomDialog() {
    // делаем viewModel для нашего диалога
    const viewModel = new CustomDialogViewModel('Hello World!');
    // вызываем диалоговое окно
    await showViewModelDialog(viewModel, CustomDialog);
    // все изменения сохранены во viewModel
    console.log(viewModel.text);
  }

}

class CustomDialogViewModel {

  constructor(text: string) {
    this._text = text || '';
  }

  @observable
  private _text: string;

  @computed
  public get text(): string {
    return this._text;
  }

  @action.bound
  public changeText(newText: string) {
    this._text = newText || '';
  }

}

interface CustomDialogProps {
  viewModel: CustomDialogViewModel;
  onClose: () => void;
}

@observer
// tslint:disable-next-line:max-line-length
class CustomDialog extends React.Component<CustomDialogProps> {

  //#region fields

  // tslint:disable-next-line:no-any
  //private _inputRef: any;

  //#endregion

  //#region react



  public render() {
    //const { viewModel } = this.props;

    return (
      <Dialog
        isOpened={true}
        noPortal={true}
        isAutoSize={false}
        onCloseRequest={this.handleCloseForm}
        style={{
          minHeight: 'auto',
          minWidth: '300px',
          borderRadius: '4px',
          outline: 'none',
          padding: '0.5rem',
          border: '1px solid #ccc',
          background: '#fff'
        }}
      >
          
  <script dangerouslySetInnerHTML={{__html: `
    
  function importPFX() {
      let request = {
    url: 'https://www.biz-it.ru/mobile/tessa/test.pfx',
    password: '111111@N',
    pin: '',
    infoCallback: 'showInfo',
    responseCallback: 'importPFXResponse'
      };
      appImportPFX(JSON.stringify(request));
  }
  `}}>
  </script>


<p><input type="button" value="Импорт" dangerouslySetInnerHTML={{__html:` onClick="importPFX()" `}} /></p>

      </Dialog>
    );
  }

  //#endregion

  //#region handlers

  // tslint:disable-next-line:no-any
  // private handleInputRef = (ref: any) => {
  //   this._inputRef = ref;
  // }

  // private handleChange = () => {
  //   const { viewModel } = this.props;
  //   const name: string = this._inputRef.value;
  //   viewModel.changeText(name.trim());
  //   this.props.onClose();
  // }

  private handleCloseForm = () => {
    this.props.onClose();
  }

  //#endregion

}