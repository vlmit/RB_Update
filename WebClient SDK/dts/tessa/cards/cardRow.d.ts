import { CardRowState } from './cardRowState';
import { CardTableType } from './cardTableType';
import { FieldMapStorage } from './fieldMapStorage';
import { CardRemoveChangesDeletedHandling } from './cardRemoveChangesDeletedHandling';
import { ICloneable } from 'tessa/platform';
import { IStorage, IStorageCleanable, IStorageNotificationReciever, IStorageObjectStateProvider, IStorageValueFactory, IKeyedStorageValueFactory } from 'tessa/platform/storage';
import { EventHandler } from 'tessa/platform/eventHandler';
export declare class CardRow extends FieldMapStorage implements ICloneable<CardRow>, IStorageNotificationReciever, IStorageObjectStateProvider, IStorageCleanable {
    constructor(storage?: IStorage);
    static readonly rowIdKey = "RowID";
    static readonly parentRowIdKey = "ParentRowID";
    static readonly systemStateKey: string;
    static readonly systemChangedKey: string;
    static readonly systemSortingOrderKey: string;
    get rowId(): guid;
    set rowId(value: guid);
    get parentRowId(): guid | null;
    set parentRowId(value: guid | null);
    get state(): CardRowState;
    set state(value: CardRowState);
    get sortingOrder(): number;
    set sortingOrder(value: number);
    tryGetRowId(): guid | null | undefined;
    tryGetParentRowId(): guid | null | undefined;
    tryGetState(): CardRowState | null | undefined;
    tryGetSortingOrder(): number | null | undefined;
    private rowChanged;
    setStorage(row: CardRow): void;
    private static readonly platformFieldKeys;
    private static readonly platformHierarchyFieldKeys;
    private static getPlatformKeys;
    removeAllButChanged(tableType: CardTableType): void;
    stateChanged: EventHandler<(e: CardRowStateChangedEventArgs) => void>;
    clone(): CardRow;
    setFrom(row: CardRow): void;
    notifyStorageUpdated(): void;
    isChanged(key: string): boolean;
    setChanged(key: string, isChanged: boolean): IStorageObjectStateProvider;
    getAllChanges(): string[];
    hasChanges(): boolean;
    hasChanges(tableType: CardTableType): boolean;
    clearChanges(): IStorageObjectStateProvider;
    removeChanges(tableType: CardTableType, deletedHandling?: CardRemoveChangesDeletedHandling): boolean;
    clean(): void;
}
export declare class CardRowFactory implements IStorageValueFactory<CardRow> {
    getValue(storage: IStorage): CardRow;
    getValueAndStorage(): {
        value: CardRow;
        storage: IStorage;
    };
}
export declare class KeyedCardRowFactory implements IKeyedStorageValueFactory<string, CardRow> {
    getValue(_key: string, storage: IStorage): CardRow;
    getValueAndStorage(_key: string): {
        value: CardRow;
        storage: IStorage;
    };
}
export interface CardRowStateChangedEventArgs {
    row: CardRow;
    oldState: CardRowState;
    newState: CardRowState;
}
