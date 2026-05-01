import { ValidationResult } from 'tessa/platform/validation';
import { FileContainer, IFile, IFileVersion } from 'tessa/files';
import { OnlyOfficeEditorConfig } from './onlyOfficeEditorConfig';
import { OnlyOfficeSettings } from './onlyOfficeSettings';
import { OnlyOfficeOpenFileInfo } from './onlyOfficeOpenFileInfo';
/**
 * Предоставляет API редактора документов OnlyOffice и его хранилища.
 */
export declare class OnlyOfficeApi {
    static readonly apiScriptId = "onlyOfficeApiScript";
    readonly settings: OnlyOfficeSettings;
    private _openFiles;
    constructor(settings: OnlyOfficeSettings);
    /**
     * Открывает указанный файл, выполняя кэширование, а затем, после закрытия редактора, сохранение изменений и удаление из кэша.
     * @param version Версия файла.
     * @param openEditorAsyncCallback
     * Ассинхронная функция обратного вызова, в которой необходимо открыть редактор.
     * Ассинхронная функция должна завершаться после закрытия редактора.
     * @param forceCloseEditorCallback
     * Функция, которая вызывается, когда необходимо незамедлительно закрыть редактор
     * и закончить выполнение функции работы с редактором.
     * @param forEdit Признак того, что необходимо отследить изменения в файле, выполнив сохранение.
     * @param cardId Идентификатор карточки. Может быть null. Может служить дополнительным признаком для отслеживания открытых файлов в карточке.
     * @param loadingOverlay Признак того, что необходимо показать оверлей загрузки.
     * @throws ValidationError Ошибка, возникающая в случае неудачной работы с файлом.
     */
    openFile(version: IFileVersion, openEditorAsyncCallback: (id: guid) => Promise<void>, forceCloseEditorCallback: () => void, forEdit: boolean, cardId: guid | null, loadingOverlay: boolean): Promise<void>;
    /**
     * Добавляет скрипт редактора в указанный документ, если его не имеется.
     */
    ensureApiScriptAdded(d: Document): void;
    /**
     * Создаёт новый файл в указанном контейнере, с помощью указанного шаблона.
     * @param container Контейнер файлов.
     * @param templateName Полное имя шаблона.
     * @param nameAfterCreation Новое имя после создания.
     */
    createTemplateFile(container: FileContainer, templateName: string, nameAfterCreation: string): Promise<{
        file: IFile | null;
        validation: ValidationResult;
    }>;
    /**
     * Создаёт редактор документов в указанном элементе и с указанным конфигом.
     */
    createDocEditorFrame(placeholder: string, config: OnlyOfficeEditorConfig): Record<string, unknown>;
    /**
     * Создаёт стандартный конфиг для указанной версии файла.
     */
    createDefaultDocEditorConfig(id: guid, version: IFileVersion, mode: 'preview' | 'view' | 'edit'): OnlyOfficeEditorConfig;
    /**
     * Получает список открытых файлов в редакторе.
     */
    get openFiles(): ReadonlyArray<OnlyOfficeOpenFileInfo>;
    /**
     * Возвращает признак того, что указанный формат поддерживается редактором.
     */
    static isSupportedFormat(ext: string): boolean;
    /**
     * Выбрасывает исключение, если указанное расширение не поддерживается.
     * @throws ValidationResult
     */
    static throwIfFormatUnsupported(ext: string): void;
    /**
     * Выбрасывает исключение, если в указанном окне не содержится загруженного API-скрипта.
     * @throws ValidationResult
     */
    static throwIfApiScriptIsNotLoaded(w: Window): void;
    private static get basePath();
    private static getUrlWithParams;
    private static getDocumentTemplate;
    private static getCallbackUrl;
    private static getFileForDocumentServerUrl;
    private static cache;
    private static checkFinalFile;
    private static getFinalFile;
    /**
     * @throws ValidationError
     */
    private static waitUntilEditorInfoPresentAndGetFinalFile;
    private static delete;
    private static deleteSynchronously;
}
