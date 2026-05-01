import { CardStoreMode } from './cardStoreMode';
import { Card } from './card';
import { CardPermissionFlags } from './cardPermissionFlags';
import { CardTypeExtensionType } from './cardTypeExtensionType';
import { TypeExtensionContext } from './typeExtensionContext';
import { IStorage } from 'tessa/platform/storage';
import { ValidationResult } from 'tessa/platform/validation';
import { CardMetadataSealed } from 'tessa/cards/metadata';
export declare const systemKeyPrefix = ".";
export declare const userKeyPrefix = "__";
export declare const userSettingsOnlyKey: string;
export declare function isSystemKey(key: string): boolean;
export declare function isUserKey(key: string): boolean;
export declare function getStoreMode(version: number): CardStoreMode;
/**
 * Идентификатор типа карточки для настроек лицензии.
 */
export declare const LicenseTypeID = "f9c7b09c-de09-46b5-ba35-e73c83ea52a7";
/**
 * Имя типа карточки для настроек лицензии.
 */
export declare const LicenseTypeName = "License";
/**
 * Отображаемое имя типа карточки для настроек лицензии.
 */
export declare const LicenseTypeCaption = "$CardTypes_TypesNames_License";
/**
 * Идентификатор типа карточки для настроек сервера.
 */
export declare const ServerInstanceTypeID = "7b891314-474f-4a60-8e0d-744dcb075209";
/**
 * Имя типа карточки для настроек сервера.
 */
export declare const ServerInstanceTypeName = "ServerInstance";
/**
 * Отображаемое имя типа карточки для настроек сервера.
 */
export declare const ServerInstanceTypeCaption = "$CardTypes_TypesNames_ServerInstance";
/**
 * Идентификатор типа карточки для шаблона файла.
 */
export declare const FileTemplateTypeID = "b7e1b93e-eeda-49b7-9402-2471d4d14bdf";
/**
 * Имя типа карточки для шаблона файла.
 */
export declare const FileTemplateTypeName = "FileTemplate";
/**
 * Отображаемое имя типа карточки для шаблона файла.
 */
export declare const FileTemplateTypeCaption = "$CardTypes_TypesNames_FileTemplate";
/**
 * Идентификатор типа карточки для удалённой карточки.
 */
export declare const DeletedTypeID = "f5e74fbb-5357-4a6d-adce-4c2607853fdd";
/**
 * Имя типа карточки для удалённой карточки.
 */
export declare const DeletedTypeName = "Deleted";
/**
 * Отображаемое имя типа карточки для удалённой карточки.
 */
export declare const DeletedTypeCaption = "$CardTypes_TypesNames_Deleted";
/**
 * Card type identifier for "ActionHistoryRecord": {ABC13918-AA63-45CA-A3F4-D1FD5673C248}.
 */
export declare const ActionHistoryRecordTypeID = "abc13918-aa63-45ca-a3f4-d1fd5673c248";
/**
 * Card type name for "ActionHistoryRecord".
 */
export declare const ActionHistoryRecordTypeName = "ActionHistoryRecord";
/**
 * Card type identifier for "Error": {FA81208D-2D83-4CB6-A83D-CBA7E3F483A7}.
 */
export declare const ErrorTypeID = "fa81208d-2d83-4cb6-a83d-cba7e3f483a7";
/**
 * Card type name for "Error".
 */
export declare const ErrorTypeName = "Error";
/**
 * Идентификатор типа карточки для настроек синхронизации с Active Directory.
 */
export declare const AdSyncTypeID = "cdaa9e03-9e06-4b4d-9a21-cfc446d2d9d1";
/**
 * Имя типа карточки для настроек синхронизации с Active Directory.
 */
export declare const AdSyncTypeName = "AdSync";
/**
 * Отображаемое имя типа карточки для настроек синхронизации с Active Directory.
 */
export declare const AdSyncTypeCaption = "$CardTypes_TypesNames_ADSync";
/**
 * Идентификатор типа карточки для кэша конвертации файлов.
 */
export declare const FileConverterCacheTypeID = "7609d1d7-9a46-4617-8789-2dff55aa4072";
/**
 * Имя типа карточки для кэша конвертации файлов.
 */
export declare const FileConverterCacheTypeName = "FileConverterCache";
/**
 * Отображаемое имя типа карточки для кэша конвертации файлов.
 */
export declare const FileConverterCacheTypeCaption = "$CardTypes_TypesNames_FileConverterCache";
/**
 * Идентификатор типа карточки шаблона.
 */
export declare const TemplateTypeID = "7ed2fb6d-4ece-458f-9151-0c72995c2d19";
/**
 * Имя типа карточки шаблона.
 */
export declare const TemplateTypeName = "Template";
/**
 * Отображаемое имя типа карточки шаблона.
 */
export declare const TemplateTypeCaption = "$CardTypes_TypesNames_Blocks_Tabs_Template";
/**
 * Идентификатор типа карточки для файла шаблона.
 */
export declare const TemplateFileTypeID = "a259101b-58f7-47b4-959e-dd5e7be1671c";
/**
 * Имя типа карточки для файла шаблона.
 */
export declare const TemplateFileTypeName = "TemplateFile";
/**
 * Отображаемое имя типа карточки для файла шаблона.
 */
export declare const TemplateFileTypeCaption = "$CardTypes_TypesNames_TemplateFile";
/**
 * Все разрешения, доступные для карточки.
 */
export declare const AllowAllCardPermissionFlags: CardPermissionFlags;
/**
 * Запрет всех разрешений, доступных для карточки.
 */
export declare const ProhibitAllCardPermissionFlags: CardPermissionFlags;
/**
 * Все разрешения, доступные для файла.
 */
export declare const AllowAllFilePermissionFlags: CardPermissionFlags;
/**
 * Запрет всех разрешений, доступных для файла.
 */
export declare const ProhibitAllFilePermissionFlags: CardPermissionFlags;
export declare function grantAllPermissions(card: Card, removeOtherPermissions?: boolean, excludeCards?: boolean, excludeFiles?: boolean, excludeTasks?: boolean): void;
export declare function prohibitAllPermissions(card: Card, removeOtherPermissions?: boolean, excludeCards?: boolean, excludeFiles?: boolean, excludeTasks?: boolean): void;
export declare function setAllCardPermissions(card: Card, cardPermissions: CardPermissionFlags, filePermissions: CardPermissionFlags, removeOtherPermissions?: boolean, excludeCards?: boolean, excludeFiles?: boolean, excludeTasks?: boolean): void;
export declare function setAllSingleCardWithFilesPermissions(card: Card, removeOtherPermissions: boolean | undefined, cardPermissions: CardPermissionFlags, filePermissions: CardPermissionFlags, excludeCards?: boolean, excludeFiles?: boolean): void;
export declare function executeExtensions(type: CardTypeExtensionType, card: Card, cardMetadata: CardMetadataSealed, executeAction: (ctx: TypeExtensionContext) => Promise<void> | void, externalContext?: any | null, info?: IStorage | null): Promise<ValidationResult>;
export declare const hasFilesToSave: (card: Card) => boolean;
