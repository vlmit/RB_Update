import { ApplicationExtension } from 'tessa';

export class AutoLogoutRedirect extends ApplicationExtension {
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
        //#region Вариант переадрисации

        const url = 'https://172.18.253.63/login';

        const popupWidth = 600;
        const popupHeight = 400;

        const dualScreenLeft = window.screenLeft || window.screenX || 0;
        const dualScreenTop = window.screenTop || window.screenY || 0;

        const screenWidth = window.screen.width;
        const screenHeight = window.screen.height;

        const left = (screenWidth - popupWidth) / 2 + dualScreenLeft;
        const top = (screenHeight - popupHeight) / 2 + dualScreenTop;
        const popupFeatures = `width=${popupWidth},height=${popupHeight},left=${left},top=${top}`;

        const newWindow = window.open(url, '_blank', popupFeatures);
        

        if (!newWindow) return;
        newWindow.onload = function () {
          console.log(newWindow);
          console.log(newWindow.onbeforeunload);
          newWindow.addEventListener('beforeunload', function () {
            console.log(newWindow);
          });

          const selector = '.raised-button';

          const observer = new MutationObserver(() => {
            if (newWindow.document.querySelector<HTMLDivElement>(selector)) {
              const btn = newWindow.document.querySelector<HTMLButtonElement>('.raised-button');

              if (btn) {
                newWindow.addEventListener('keydown', (e) => {
                  if (e.key === 'Enter') {
                    setTimeout(() => {
                      newWindow.stop();
                      newWindow.onbeforeunload = null;
                      newWindow.close();
                    }, 2000);
                  }
                });

                btn.addEventListener('click', () => {
                  setTimeout(() => {
                    newWindow.stop();
                    newWindow.onbeforeunload = null;
                    newWindow.close();
                  }, 2000);
                });
              }
              //newWindow.close();
              observer.disconnect();
            }
          });
          observer.observe(newWindow.document.body, {
            childList: true,
            subtree: true,
          });
        };
        //#endregion
      });
    }
  }
}
