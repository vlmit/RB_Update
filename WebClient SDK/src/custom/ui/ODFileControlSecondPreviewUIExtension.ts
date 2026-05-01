import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
//import { Guid } from 'tessa/platform';
import { FileListViewModel } from 'tessa/ui/cards/controls';
//import { showMessage } from 'tessa/ui/tessaDialog';
import { tryGetFromInfo } from 'tessa/ui';

/**
 * Автопредпросмотр первого файла определенной категории
 */
export class ODFileControlSecondPreviewUIExtension extends CardUIExtension {
  public initialized(context: ICardUIExtensionContext) {

    const imageFileControl = tryGetFromInfo<FileListViewModel | null>(
      context.model.info,
      'FilesViewDevice',
      null
    );
    if (!imageFileControl) {
      return;
    }

    // показываем файлы только с категорией Документ
    //for (let i = imageFileControl.files.length - 1; i > -1; i--) {
    //  const file = imageFileControl.files[i];
    //  if (!(file.model.category && file.model.category.caption === 'Документ')) {
   //     imageFileControl.removeFile(file);
   //   }
   // }

    const preview = imageFileControl.manager;
    if (preview) {
    for (let i = imageFileControl.files.length - 1; i > -1; i--) {
      const file = imageFileControl.files[i];
      if ((file.model.category && file.model.category.caption === 'Документ')) {
        preview.showPreview(file.model);
        break;
      }
    }
    }

  }
}
