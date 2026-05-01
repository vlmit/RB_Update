import { IApplicationExtensionContext, IApplicationExtensionMetadataContext, IApplicationExtensionRouteContext } from './applicationExtensionContext';
import { IExtension } from './extensions';
export interface IApplicationExtension extends IExtension {
    initialize(context: IApplicationExtensionContext): any;
    afterMetadataReceived(context: IApplicationExtensionContext): any;
    routeResolve(context: IApplicationExtensionRouteContext): any;
    finalize(context: IApplicationExtensionContext): any;
}
export declare class ApplicationExtension implements IApplicationExtension {
    static readonly type = "ApplicationExtension";
    shouldExecute(_context: any): boolean;
    initialize(_context: IApplicationExtensionContext): void;
    afterMetadataReceived(_context: IApplicationExtensionMetadataContext): void;
    routeResolve(_context: IApplicationExtensionRouteContext): void;
    finalize(_context: IApplicationExtensionContext): void;
}
