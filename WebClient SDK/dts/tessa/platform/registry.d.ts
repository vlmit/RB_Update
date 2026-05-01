export interface IRegistry<T extends IRegistryItem> {
    register(item: T): any;
    get(id: guid): T | undefined;
    tryGet(id: guid): T | null;
    getAll(): T[];
    isDefined(id: guid): boolean;
    isDefined(item: T): boolean;
}
export interface IRegistryItem {
    id: guid;
}
export declare abstract class Registry<T extends IRegistryItem> implements IRegistry<T> {
    private items;
    register(item: T): void;
    get(id: guid): T | undefined;
    tryGet(id: guid): T | null;
    getAll(): T[];
    isDefined(id: guid): boolean;
    isDefined(item: T): boolean;
}
