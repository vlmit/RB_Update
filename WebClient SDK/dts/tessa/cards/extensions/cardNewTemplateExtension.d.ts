import { ICardNewTemplateExtensionContext } from './cardNewTemplateExtensionContext';
import { IExtension } from 'tessa/extensions';
export interface ICardNewTemplateExtension extends IExtension {
    beforeRequest(context: ICardNewTemplateExtensionContext): any;
    afterRequest(context: ICardNewTemplateExtensionContext): any;
}
export declare class CardNewTemplateExtension implements ICardNewTemplateExtension {
    static readonly type = "CardNewTemplateExtension";
    shouldExecute(_context: ICardNewTemplateExtensionContext): boolean;
    beforeRequest(_context: ICardNewTemplateExtensionContext): void;
    afterRequest(_context: ICardNewTemplateExtensionContext): void;
}
