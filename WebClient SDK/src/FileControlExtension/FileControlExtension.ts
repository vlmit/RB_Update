import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
import { FileListViewModel } from 'tessa/ui/cards/controls';
import { showMessage } from 'tessa/ui/tessaDialog';
import { tryGetFromInfo } from 'tessa/ui';

/**
 * Пример расширения для файлового контрола. Расширение фильтрует файлы по категории,
 * которые выводятся в файловом контроле, а также запрещает добавление файла с определенным расширением
 * для определенного типа карточки.
 *
 * Результат работы расширения:
 * В карточке типа "Автомобиль" выполняет фильтрацию в файловом
 * контроле "Без изображений (всё, кроме категории "Image")" по названию категории: отображает файлы
 * только с категорией "Text". Запрещает добавление файлов с разрешением, отличным от ".txt",
 * для всех файловых контролов выбранной карточки.
 */
export class FileControlUIExtension extends CardUIExtension {
  public initialized(context: ICardUIExtensionContext): void 
  {
    // пытаемся получить контрол "Без изображений (всё, кроме категории "Image")"
    const AllFilesControl = tryGetFromInfo<FileListViewModel | null>
    (
      context.model.info,
      'FilesView',
      null
    );

    if (!AllFilesControl)
    {
      return;
    }
    

    // если добавляется файл с расширением отличным от 'txt', то прерываем добавление
    context.fileContainer.containerFileChanging.add(async e => 
    {
      const file = e.added;
      if (file && file.getExtension() !== 'doc' && file.getExtension() !== 'docx' && file.getExtension() !== 'txt' && file.getExtension() !== 'pdf'
      && file.getExtension() !== 'xlsx' && file.getExtension() !== 'pptx' && file.getExtension() !== 'tiff' && file.getExtension() !== 'tif'
      && file.getExtension() !== 'xml' && file.getExtension() !== 'zip' && file.getExtension() !== 'rar' && file.getExtension() !== 'png'
      && file.getExtension() !== 'jpeg' && file.getExtension() !== 'jpg' && file.getExtension() !== 'html'
      && file.getExtension() !== 'sig' && file.getExtension() !== 'sign'&& file.getExtension() !== 'sgn'
      && file.getExtension() !== '7z') 
      {
        await showMessage(`Недопустимое расширение вложенного файла.`);
        e.cancel = true;
      }
    });
  }
}
