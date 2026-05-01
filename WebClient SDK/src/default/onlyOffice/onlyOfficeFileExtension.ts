import { FileExtension, IFileExtensionContext } from 'tessa/ui/files';
import { MenuAction } from 'tessa/ui';
import { OnlyOfficeApiSingleton } from './onlyOfficeApiSingleton';
import { OnlyOfficeEditorWindowViewModel } from './onlyOfficeEditorWindowViewModel';
import { OnlyOfficeApi } from './onlyOfficeApi';

/**
 * Представляет собой расширение, которое добавляет возможность открытия файла в редакторе OnlyOffice.
 */
export class OnlyOfficeFileExtension extends FileExtension {
  public openingMenu(context: IFileExtensionContext): void {
    const version = context.file.model.lastVersion;
    const api = OnlyOfficeApiSingleton.instance;
    const extSupported = OnlyOfficeApi.isSupportedFormat(version.getExtension());
    const cardId = context.control.model.card.id;

    const openWindowForRead = new MenuAction(
      'DocEditorOpenWindowForRead',
      '$OnlyOffice_OpenWindowForRead',
      'ta icon-thin-006',
      async () => {
        const vm = new OnlyOfficeEditorWindowViewModel(api, version, false, cardId);
        await vm.load();
      },
      [],
      !extSupported
    );

    const openWindowForEdit = new MenuAction(
      'DocEditorOpenWindowForEdit',
      '$OnlyOffice_OpenWindowForEdit',
      'ta icon-thin-003',
      async () => {
        const vm = new OnlyOfficeEditorWindowViewModel(api, version, true, cardId);
        await vm.load();
      },
      [],
      !version.file.permissions.canEdit ||
        !version.file.permissions.canReplace ||
        !extSupported ||
        !!api.openFiles.find(
          of => of.forEdit && of.version.id === version.id && of.cardId === cardId
        )
    );

    const previewIndex = context.actions.findIndex(x => x.name === 'Preview');
    context.actions.splice(previewIndex + 1, 0, openWindowForRead, openWindowForEdit);
  }

  public shouldExecute(): boolean {
    return OnlyOfficeApiSingleton.isAvailable;
  }
}
