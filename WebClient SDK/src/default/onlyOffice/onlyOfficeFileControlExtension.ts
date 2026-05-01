import { FileControlExtension, IFileControlExtensionContext } from 'tessa/ui/files';
import { MenuAction, SeparatorMenuAction } from 'tessa/ui';
import { LocalizationManager } from 'tessa/localization';
import { OnlyOfficeApiSingleton } from './onlyOfficeApiSingleton';

/**
 * Представляет собой расширение, которое добавляет возможность создания файлов по шаблону.
 */
export class OnlyOfficeFileControlExtension extends FileControlExtension {
  public openingMenu(context: IFileControlExtensionContext): void {
    const container = context.control.fileContainer;
    const newFileNameCaption = LocalizationManager.instance.localize(
      '$OnlyOffice_NewDocumentCaption'
    );

    const createWordFileAction = new MenuAction(
      'CreateEmptyWordFile',
      'Word',
      'ta icon-thin-069',
      async () => {
        await OnlyOfficeApiSingleton.instance.createTemplateFile(
          container,
          'empty.docx',
          newFileNameCaption + '.docx'
        );
      }
    );

    const createExcelFileAction = new MenuAction(
      'CreateEmptyExcelFile',
      'Excel',
      'ta icon-thin-085',
      async () => {
        await OnlyOfficeApiSingleton.instance.createTemplateFile(
          container,
          'empty.xlsx',
          newFileNameCaption + '.xlsx'
        );
      }
    );

    const createPowerPointFileAction = new MenuAction(
      'CreateEmptyExcelFile',
      'PowerPoint',
      'ta icon-thin-073',
      async () => {
        await OnlyOfficeApiSingleton.instance.createTemplateFile(
          container,
          'empty.pptx',
          newFileNameCaption + '.pptx'
        );
      }
    );

    const createEmptyFileGroup = new MenuAction(
      'CreateEmptyFileGroup',
      '$OnlyOffice_CreateEmptyFile',
      'ta icon-thin-080',
      null,
      [createWordFileAction, createExcelFileAction, createPowerPointFileAction],
      !context.control.fileContainer.permissions.canAdd
    );

    // ищем пункт "Загрузить", после него ищем ближайший разделитель, и перед ним вставляем группу "Создать файл"
    let uploadActionIndex = context.actions.findIndex(a => a.name === 'Upload');
    if (uploadActionIndex !== -1) {
      const separatorIndex = context.actions
        .slice(uploadActionIndex)
        .findIndex(a => a instanceof SeparatorMenuAction);
      if (separatorIndex !== -1) {
        uploadActionIndex = uploadActionIndex + separatorIndex - 1;
      }
    }

    const insertToIndex = uploadActionIndex !== -1 ? uploadActionIndex + 1 : context.actions.length;
    context.actions.splice(insertToIndex, 0, createEmptyFileGroup);
  }

  public shouldExecute(): boolean {
    return OnlyOfficeApiSingleton.isAvailable;
  }
}
