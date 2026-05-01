import { IFileVersion } from 'tessa/files';
import { OnlyOfficeApi } from './onlyOfficeApi';
/**
 * Представляет собой модель редактора, используемого для чтения и редактирования файлов в отдельной вкладке браузера.
 */
export declare class OnlyOfficeEditorWindowViewModel {
    readonly placeholder: string;
    readonly version: IFileVersion;
    readonly forEdit: boolean;
    private readonly _cardId;
    private readonly _api;
    private _window;
    constructor(api: OnlyOfficeApi, version: IFileVersion, forEdit: boolean, cardId: guid);
    load: () => Promise<void>;
    unload: () => Promise<void>;
    private openEditorWindow;
}
