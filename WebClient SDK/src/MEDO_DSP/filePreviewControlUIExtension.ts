import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
import { FileListViewModel } from 'tessa/ui/cards/controls';
import { tryGetFromInfo } from 'tessa/ui';

/**
 * В выбранном файловом контроле показываем только определенные файлы.
 * Разрешаем добавлять файлы только с определенным расширением.
 */
export class filePreviewControlUIExtension extends CardUIExtension {

  public initialized(context: ICardUIExtensionContext) {
      // пытаемся получить контрол "Без изображений (всё, кроме категории "Image")"
    const AllFilesControl = tryGetFromInfo<FileListViewModel | null>(
      context.model.info,
      'FilesPreviewMedo',
      null
    );

    if (!AllFilesControl) {
      return;
    }

    for (let i = AllFilesControl.files.length - 1; i > -1; i--) {
      const file = AllFilesControl.files[i];
      if (!(file.model.category && file.model.category.caption === 'Предпросмотр')) {
        AllFilesControl.removeFile(file);
      }
    }

    const preview = AllFilesControl.manager;
    
    if (preview) {
      for (let i = AllFilesControl.files.length - 1; i > -1; i--) {
        const file = AllFilesControl.files[i];
        if ((file.model.category && file.model.category.caption === 'Предпросмотр')) {
          preview.showPreview(file.model);
          break;
        }
      }
    }
  }
}