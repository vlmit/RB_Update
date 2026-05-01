import { IExtension } from 'tessa/extensions';
import { MenuAction } from 'tessa/ui/menuAction';
import { FileContainer, IFileVersion, IFile } from 'tessa/files';
import { FileViewModel, FileSortingDirection, FileListCategoryFilter, FileListTypeFilter, FileControlCancelEventArgs, FileControlEventArgs, FileListGroupSorting, FileListVersionsDialogViewModel, ReadonlyFileGroupViewModel } from 'tessa/ui/cards/controls';
import { FileGrouping } from 'tessa/ui/cards/controls/fileList/grouping';
import { FileSorting } from 'tessa/ui/cards/controls/fileList/sorting';
import { FileFiltering } from 'tessa/ui/cards/controls/fileList/filtering';
import { IControlState, ICardModel, IControlViewModel } from 'tessa/ui/cards';
import { EventHandler, Result } from 'tessa/platform';
import { IStorage } from 'tessa/platform/storage';
import { IPreviewerViewModel } from 'tessa/ui/cards/controls/previewer/previewerViewModel';
import { PreviewFileDialogOpts, PreviewFileDialogViewModel } from 'tessa/ui/tessaDialog';
export interface IFileControl extends IControlViewModel {
    readonly model: ICardModel;
    fileContainer: FileContainer;
    readonly files: ReadonlyArray<FileViewModel>;
    readonly filteredFiles: ReadonlyArray<FileViewModel>;
    readonly groupings: ReadonlyArray<FileGrouping>;
    readonly sortings: ReadonlyArray<FileSorting>;
    readonly actions: ReadonlyArray<MenuAction>;
    readonly fileActions: ReadonlyArray<MenuAction>;
    readonly versionActions: ReadonlyArray<MenuAction>;
    readonly groups: ReadonlyMap<string, ReadonlyFileGroupViewModel> | null;
    readonly isFileExists: boolean;
    selectedSorting: FileSorting | null;
    selectedSortDirection: FileSortingDirection;
    selectedGrouping: FileGrouping | null;
    groupsExpanded: boolean;
    selectedFiltering: FileFiltering | null;
    isCategoriesEnabled: boolean;
    isManualCategoriesCreationDisabled: boolean;
    isNullCategoryCreationDisabled: boolean;
    isPreservingCategoriesOrder: boolean;
    isIgnoreExistingCategories: boolean;
    multiSelectionMode: boolean;
    categoryFilter: FileListCategoryFilter | null;
    typeFilter: FileListTypeFilter | null;
    groupSorting: FileListGroupSorting | null;
    readonly cryptoProPluginEnabled: boolean;
    getState(): IControlState;
    setState(state: IControlState): boolean;
    handleDropFiles(contents: ReadonlyArray<File>): Promise<void>;
    readonly containerFileAdding: EventHandler<(args: FileControlCancelEventArgs) => void>;
    readonly containerFileAdded: EventHandler<(args: FileControlEventArgs) => void>;
    readonly containerFileRemoving: EventHandler<(args: FileControlCancelEventArgs) => void>;
    readonly containerFileRemoved: EventHandler<(args: FileControlEventArgs) => void>;
    manager: IFileControlManager | null;
    info: IStorage;
    addFile(file: IFile): FileViewModel;
    removeFile(file: IFile | FileViewModel): any;
}
export interface IFileControlExtension extends IExtension {
    initializing(context: IFileControlExtensionContext): any;
    openingMenu(context: IFileControlExtensionContext): any;
}
export interface IFileExtension extends IExtension {
    openingMenu(context: IFileExtensionContext): any;
}
export interface IFileVersionExtension extends IExtension {
    openingMenu(context: IFileVersionExtensionContext): any;
}
export interface IFileExtensionContextBase {
    readonly control: IFileControl;
    readonly actions: MenuAction[];
    readonly info: IStorage;
}
export interface IFileControlExtensionContext extends IFileExtensionContextBase {
    readonly groupings: FileGrouping[];
    readonly sortings: FileSorting[];
}
export interface IFileExtensionContext extends IFileExtensionContextBase {
    readonly file: FileViewModel;
    readonly files: ReadonlyArray<FileViewModel>;
}
export interface IFileVersionExtensionContext extends IFileExtensionContextBase {
    readonly file: FileViewModel;
    readonly version: IFileVersion;
    readonly versions: ReadonlyArray<IFileVersion>;
    readonly dialog: FileListVersionsDialogViewModel;
}
export declare type HandleFileContentLoadingFuncType = (version: IFileVersion, callback: (result: Result<File>) => Promise<void>, handleParams?: {
    showLoadingMessage?: boolean;
    showErrorMessage?: boolean;
}) => Promise<void>;
/**
 * Предоставляет возможность управления предпросмотром файлов.
 */
export interface IFileControlManager {
    /**
     * Модель представления области предпросмотра.
     */
    readonly previewToolViewModel: IPreviewerViewModel | null;
    /**
     * Свойство отвечает за предпросмотр файла в диалоговом окне.
     */
    previewInDialog: boolean;
    /**
     * Фабрика, на основе которой происходит определение модели представления области предпросмотра.
     * Может быть переопределена.
     * Если тип текущей модели previewToolViewModel совпадает с типом, созданным фабрикой, создание новой модели происходить не будет.
     * @param version Версия файла.
     * @returns Объект, представляющий тип вью-модели и функцию создания вью-модели.
     * Или null, если отображение указанного файла невозможно.
     */
    previewToolFactory: (version: IFileVersion) => {
        type: string;
        createViewModelFunc: () => IPreviewerViewModel;
    } | null;
    /**
     * Отображает указанный файл в предпросмотре.
     */
    showPreview: (file: IFile, previewFileDialogOpts?: PreviewFileDialogOpts) => Promise<PreviewFileDialogViewModel | void>;
    showDefaultPreview: (file: IFile) => void;
    showPreviewInDialog: (file: IFile, previewFileDialogOpts?: PreviewFileDialogOpts) => Promise<PreviewFileDialogViewModel | void>;
    /**
     * Признак того, что предпросмотр через конвертацию в PDF включен.
     */
    readonly previewPdfEnabled: boolean;
    /**
     * Выполняет загрузку указанного файла, предварительно определяя необходимость конвертации в PDF-формат.
     * @param version Версия файла.
     * @param callback Функция обратного вызова, вызываемая при окончании загрузки.
     */
    handleFileContentLoading: HandleFileContentLoadingFuncType;
    reset: () => void;
    /**
     * Сбрасывает текущий файл предпросмотра.
     */
    resetPreview: () => void;
    /**
     * Сбрасывает сообщение и файл предпросмотра, если указанный файл является текущим.
     */
    resetIfInPreview(file: IFile): any;
    /**
     * Сбрасывает сообщение и файл предпросмотра, если указанный идентификатор версии файла является текущим.
     */
    resetIfInPreview(versionId: guid): any;
    /**
     * Устанавливает указанный угол поворота страницы для файла.
     */
    setFilePageAngle(fileVersionId: guid, pageIndex: number, angle: number): any;
    /**
     * Получает угол поворота указанной страницы для указанного файла.
     */
    getFilePageAngle(fileVersionId: guid, pageIndex: number): number | null;
    /**
     * Текстовая информация, выводимая в области предпросмотра.
     * Имеет приоритет над моделью previewToolViewModel, поэтому сообщение будет выводиться всегда, если отлично от null.
     */
    message: {
        main: string;
        additional?: string | null;
    } | null;
    /**
     * Признак того, что в данный момент производится загрузка файла.
     */
    readonly inProgress: boolean;
}
/**
 * Элемент управления для области предпросмотра.
 */
export interface IFilePreviewControl {
    attach: (fileControl: IFileControl) => void;
}
export declare enum ScaleOption {
    auto = 0,
    custom = 2,
    _50 = 50,
    _100 = 100,
    _200 = 200,
    _400 = 400,
    _800 = 800
}
