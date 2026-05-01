import React from 'react';
import { ApplicationExtension } from 'tessa';
import { showViewModelDialog } from 'tessa/ui';
import {
  DefaultLoginForm,
  ILoginExtensionContext,
  LoginExtension,
  // LoginButtons,
  // LoginFields,
  // LoginForm,
  // LoginLogo,
  // LoginMessage,
} from 'tessa/ui/login';
import { LoginFormViewModel } from 'tessa/ui/login';
import { Dialog, DialogContainer, DialogContent } from 'ui';

export class AutoLogoutDialog extends ApplicationExtension {
  private readonly selector: string = `.dialog-wrapper .error-modal-view .dialog-container .dialog-content .messages .messages_content .messages_content_message span`;

  initialize(): void {
    const observer = new MutationObserver(() => {
      if (
        document.querySelector<HTMLElement>(this.selector) &&
        document
          .querySelector<HTMLElement>(this.selector)!
          //.textContent!.startsWith('Сессия была неактивна длительное время')
          .textContent!.startsWith('Сессия не найдена')
      ) {
        this.addReloadButton();
        //observer.disconnect();
      }
    });

    observer.observe(document.body, {
      childList: true,
      subtree: true,
    });
  }

  public addReloadButton() {
    const mess = document.querySelector<HTMLElement>(this.selector);
    if (!mess) return;
    let okBtns = mess.closest('.messages')?.querySelectorAll<HTMLButtonElement>('.raised-button');
    if (!okBtns) return;
    const btn: HTMLButtonElement = okBtns[1];

    if (btn) {
      btn.addEventListener('click', async () => {
        //#region Вариант диалог
        await this.showLoginDialog()
        //#endregion

        //#region Вариант переадрисации

        // const url = 'https://172.18.253.63/login';

        // const popupWidth = 600;
        // const popupHeight = 400;

        // const dualScreenLeft = window.screenLeft || window.screenX || 0;
        // const dualScreenTop = window.screenTop || window.screenY || 0;

        // const screenWidth = window.screen.width;
        // const screenHeight = window.screen.height;

        // const left = (screenWidth - popupWidth) / 2 + dualScreenLeft;
        // const top = (screenHeight - popupHeight) / 2 + dualScreenTop;
        // const popupFeatures = `width=${popupWidth},height=${popupHeight},left=${left},top=${top}`;

        // const newWindow = window.open(url, '_blank', popupFeatures);

        // if (!newWindow) return;
        // newWindow.onload = function () {
        //   console.log(newWindow);
        //   console.log(newWindow.onbeforeunload);
        //   newWindow.addEventListener('beforeunload', function () {
        //     console.log(newWindow);
        //   });

        //   //const selector = '.tab-panel';
        //   const selector = '.raised-button';

        //   const observer = new MutationObserver(() => {
        //     if (newWindow.document.querySelector<HTMLDivElement>(selector)) {
        //       const btn = newWindow.document.querySelector<HTMLButtonElement>('.raised-button');

        //       if (btn) {
        //         newWindow.addEventListener('keydown', (e) => {
        //           if (e.key === 'Enter') {
        //             setTimeout(() => {
        //               newWindow.close();
        //             }, 2000);
        //           }
        //         });

        //         btn.addEventListener('click', () => {
        //           setTimeout(() => {
        //             newWindow.close();
        //           }, 2000);
        //         });
        //       }
        //       //newWindow.close();
        //       observer.disconnect();
        //     }
        //   });
        //   observer.observe(newWindow.document.body, {
        //     childList: true,
        //     subtree: true,
        //   });
        // };

        //#endregion


      });
    }
  }
  public async showLoginDialog() {
    const viewModel = new CustomLoginDialogViewModel();
    // вызываем диалоговое окно
    await showViewModelDialog(viewModel, CustomLoginDialog);
  }
}

interface CustomLoginDialogProps {
  viewModel: CustomLoginDialogViewModel;
  onClose: () => void;
}

export class LogExtension extends LoginExtension {
  // initializing(context: ILoginExtensionContext) {
  //   LogExtension.setContext(context)
  // }
  public static ctx
  static setContext(context: ILoginExtensionContext) {
    this.ctx = context
  }
  static getContext(): ILoginExtensionContext {
    return this.ctx
  }

  initialized(context: ILoginExtensionContext): void {
    LogExtension.setContext(context)
  }
}


class CustomLoginDialog extends React.Component<CustomLoginDialogProps> {

  public render() {

    const viewTest = LogExtension.getContext()
    if (!viewTest) return
    viewTest.loginFormViewModelFactory = () => new CustomLoginFormViewModel()

    // viewTest.loginFormFactory = (viewModel) => (
    //   <CustomLoginForm viewModel={viewModel as CustomLoginFormViewModel}></CustomLoginForm>
    // )
    //viewTest.viewModel!.redirectTo = 'https://172.18.253.63/view'
    viewTest.viewModel!.passwordField.value = ''
    // const btn = viewTest.viewModel?.buttons[0]
    // if (btn) btn.onClick = () => { Application.login("KazakovVV", "12345") }
    console.log(viewTest.viewModel)
    return (<>
      <Dialog
        isOpened={true}
        noPortal={true}
        isAutoSize={false}
        style={{
          width: '500px',
          height: '500px',
          minHeight: 'auto',
          minWidth: 'auto',
          borderRadius: '4px',
          outline: 'none',
          padding: '0.5rem',
          border: '1px solid #ccc',
          background: '#fff'
        }}
      >
        <DialogContainer>
          <DialogContent>
            {/* <div >тест</div>
            <button onClick={() => {
              Application.login('KazakovVV', "12345")
            }} ></button> */}
            <CustomLoginForm viewModel={viewTest.viewModel as CustomLoginFormViewModel}></CustomLoginForm>
          </DialogContent>
        </DialogContainer>
      </Dialog>
    </>
    );
  }
}

class CustomLoginDialogViewModel { }

class CustomLoginFormViewModel extends LoginFormViewModel {

}

interface CustomLoginFormProps {
  viewModel: CustomLoginFormViewModel;
}
const CustomLoginForm = (props: CustomLoginFormProps) => {
  const { viewModel } = props;

  return (


    // <LoginForm viewModel={viewModel}>
    //   <div>ntcn2</div>
    //   <LoginLogo viewModel={viewModel} />
    //   <LoginFields viewModel={viewModel} />
    //   <LoginMessage viewModel={viewModel} />
    //   <LoginButtons viewModel={viewModel} />
    // </LoginForm>
    <DefaultLoginForm viewModel={viewModel}></DefaultLoginForm>


  );
};
