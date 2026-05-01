import { TileExtension, ITileGlobalExtensionContext, ITileLocalExtensionContext } from 'tessa/ui/tiles';
/**
 * Расширение на формирование тайлов для формирования файлов по шаблонам.
 */
export declare class CreateFileTemplateTileExtension extends TileExtension {
    initializingGlobal(context: ITileGlobalExtensionContext): void;
    initializingLocal(context: ITileLocalExtensionContext): void;
    private static evaluateOnCardUpdate;
    private static evaluateOnViewUpdate;
    private static addTemplateTiles;
    private static tryGetDocType;
    private static setCardFileTemplatesVisibility;
    private static createFileTemplateCardAction;
    private static createFileTemplateViewAction;
}
