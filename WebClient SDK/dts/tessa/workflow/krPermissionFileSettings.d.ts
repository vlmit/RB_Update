import { StorageObject, IStorage } from 'tessa/platform/storage';
export declare class KrPermissionFileSettings extends StorageObject {
    constructor(storage?: IStorage);
    static readonly fileIdKey: string;
    static readonly accessSettingKey: string;
    get id(): guid;
    set id(value: guid);
    get acessSetting(): number;
    set acessSetting(value: number);
}
