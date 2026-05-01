import { ApplicationExtension } from 'tessa';

export class AutoLogout extends ApplicationExtension {
  private readonly selector: string = `.dialog-wrapper .error-modal-view .dialog-container .dialog-content .messages .messages_content .messages_content_message span`;

  initialize(): void {
    const observer = new MutationObserver(() => {
      if (
        document.querySelector<HTMLElement>(this.selector) &&
        document
          .querySelector<HTMLElement>(this.selector)!
          .textContent!.startsWith('Сессия не найдена. Возможно, она закрыта')
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

  private addReloadButton() {
    const mess = document.querySelector<HTMLElement>(this.selector);
    if (!mess) return;
    let okBtns = mess.closest('.messages')?.querySelectorAll<HTMLButtonElement>('.raised-button');
    if (!okBtns) return;
    const btn: HTMLButtonElement = okBtns[1];

    if (btn) {
      btn.addEventListener('click', () => {
        const url = 'https://172.18.253.63/login';
        window.open(url, '_blank');
      });
    }
  }
}
