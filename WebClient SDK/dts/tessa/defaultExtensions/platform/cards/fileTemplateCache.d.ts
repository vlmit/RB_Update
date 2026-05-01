import { FileTemplate } from './fileTemplate';
import { IStorage } from 'tessa/platform/storage';
export declare class FileTemplateCache {
    private constructor();
    private static _instance;
    static get instance(): FileTemplateCache;
    private _templates;
    get templates(): ReadonlyArray<FileTemplate>;
    getFromStorage(storage: IStorage): void;
}
