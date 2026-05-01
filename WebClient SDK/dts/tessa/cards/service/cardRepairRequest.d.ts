import { CardInfoStorageObject } from 'tessa/cards/cardInfoStorageObject';
import { IStorage } from 'tessa/platform/storage';
import { Card } from 'tessa/cards/card';
import { CardNewMode } from 'tessa/cards/cardNewMode';
export declare class CardRepairRequest extends CardInfoStorageObject {
    constructor(storage?: IStorage);
    static readonly newModeKey: string;
    static readonly notifyFieldsUpdatedKey: string;
    static readonly cardKey: string;
    static readonly cardJsonKey: string;
    static readonly convertFromJsonBeforeRequestKey: string;
    static readonly convertFromJsonAfterRequestKey: string;
    get newMode(): CardNewMode;
    set newMode(value: CardNewMode);
    get notifyFieldsUpdated(): boolean;
    set notifyFieldsUpdated(value: boolean);
    get card(): Card;
    set card(value: Card);
    get cardJson(): string;
    set cardJson(value: string);
    get convertFromJsonBeforeRequest(): boolean;
    set convertFromJsonBeforeRequest(value: boolean);
    get convertFromJsonAfterRequest(): boolean;
    set convertFromJsonAfterRequest(value: boolean);
    tryGetCard(): Card | null | undefined;
}
