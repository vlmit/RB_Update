import type { History } from 'history';
import { MainInitializationResponse, CardInitializationResponse } from 'tessa/platform/initialization';
export interface IApplicationExtensionContext {
}
export declare class ApplicationExtensionContext implements IApplicationExtensionContext {
}
export interface IApplicationExtensionMetadataContext {
    mainPartResponse: MainInitializationResponse | null;
    cardPartResponse: CardInitializationResponse | null;
    readonly autoLogin: boolean;
}
export declare class ApplicationExtensionMetadataContext implements IApplicationExtensionMetadataContext {
    constructor(autoLogin?: boolean);
    mainPartResponse: MainInitializationResponse | null;
    cardPartResponse: CardInitializationResponse | null;
    readonly autoLogin: boolean;
}
export interface IApplicationExtensionRouteContext {
    readonly history: History;
    path: string;
    resolved: boolean;
}
