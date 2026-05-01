import { IStorage } from './storage';
import { IStorageCleanable } from './storageCleanable';
import { IStorageNotificationReciever } from './storageNotificationReciever';
import { TypedField } from 'tessa/platform/typedField';
export interface IStorageObjectStateProvider {
    isChanged(key: string): boolean;
    setChanged(key: string, isChanged: boolean): IStorageObjectStateProvider;
    getAllChanges(): string[];
    hasChanges(): boolean;
    clearChanges(): IStorageObjectStateProvider;
}
export declare class StorageObjectStateProvider implements IStorageObjectStateProvider, IStorageNotificationReciever, IStorageCleanable {
    constructor(storage: IStorage, changedListKey: string);
    protected _storage: IStorage;
    protected _changedListKey: string;
    protected _changedList: TypedField[];
    protected _changedHash: Set<string>;
    isChanged(key: string): boolean;
    setChanged(key: string, isChanged?: boolean): IStorageObjectStateProvider;
    getAllChanges(): string[];
    hasChanges(): boolean;
    clearChanges(): IStorageObjectStateProvider;
    notifyStorageUpdated(): void;
    clean(): void;
}
