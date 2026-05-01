import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';

/**
 * Расширение для карточек, открывающихся в диалоге.
 *
 * Если карточка открывается в диалоговом окне,
 * предпросмотр файлов будет выводиться так же в диалоговом окне.
 */
export class CardDialogPreviewUIExtension extends CardUIExtension {
  initialized(context: ICardUIExtensionContext): void {
    if (!!context.dialogName) {
      context.model.previewManager.previewInDialog = true;
    }
  }
}
