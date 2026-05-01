import { IFile } from './file';
import { IFileVersion } from './fileVersion';
import { IValidationResultBuilder, ValidationResult } from 'tessa/platform/validation';
import { Card } from 'tessa/cards';
import { RichTextBoxAttachmentFile } from 'ui/richTextBox';
import { ForumItemViewModel } from 'tessa/ui/cards/controls/forums';
import { ICardModel } from 'tessa/ui/cards';
export interface FileRelation {
    hasData: boolean;
    fileIdIndexRelation: {
        [key: string]: number;
    };
    fileIdVersionIdRelation: {
        [key: string]: string;
    };
    files: ReadonlyArray<File>;
}
export declare function createFileRelation(card: Card, files: ReadonlyArray<IFile>): FileRelation;
export declare function checkCanDownloadFilesAndShowMessages(operationFiles: IFile[] | IFileVersion[]): Promise<boolean>;
export declare function checkCanDownloadFile(file: IFile | IFileVersion): ValidationResult;
/**
 * В IE и мобильном сафари в FileAPI не доступа к конструктору File.
 * Используем этот грязный хак.
 */
export declare function createFakeFile(blob: Blob, name?: string, lastModifiedDate?: Date): File;
export declare function readFileContentAsBase64(data: File): Promise<string | null>;
/**
 * Задаёт контент указанных файлов в соответствующие CardFile.Info.
 *
 * @param {Card} dialogCard Карточка диалога.
 * @param {Readonly<IFile[]>} files Коллекция файлов.
 * @param {IValidationResultBuilder} validationResult Объект, выполняющий построение результатов валидации.
 */
export declare function setFileContentToInfo(dialogCard: Card, files: Readonly<IFile[]>, validationResult: IValidationResultBuilder): Promise<void>;
/**
 * Deski, U can't touch this dum do-do-dum do-dum do-dum U can't touch this
 */
export declare const DeskiIgnoreGetContentExtensionKey: string;
export declare const base64ToArrayBuffer: (base64: string) => ArrayBuffer;
export declare const arrayBufferToBase64: (buffer: ArrayBuffer | number[]) => string;
/**
 * Ключ для активации выполнения логики расширения PreviewAttachmentGetFileContentExtension
 */
export declare const PreviewAttachmentGetContentExtensionKey = ".PreviewAttachmentsRequestParams";
export declare const createFileFromAttachment: (attachment: RichTextBoxAttachmentFile | ForumItemViewModel, cardModel: ICardModel, satelliteId?: string | undefined, cardTypeId?: string | undefined) => Promise<IFile | null>;
