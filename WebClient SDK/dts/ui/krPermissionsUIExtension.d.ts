import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
export declare class KrPermissionsUIExtension extends CardUIExtension {
    private static _extendedSections;
    private static _extendedControls;
    shouldExecute(context: ICardUIExtensionContext): boolean;
    initialized(context: ICardUIExtensionContext): void;
    private initializeConditions;
    private initializeFlags;
    private initializeExtendedPermissions;
    private extendControls;
    private extendPermissionGrid;
    private extendMandatoryGrid;
    private enableExtendedSettings;
    private disableExtendedSettings;
    private clearExtendedSettings;
    private clearSection;
    private tryUpdateFlag;
}
