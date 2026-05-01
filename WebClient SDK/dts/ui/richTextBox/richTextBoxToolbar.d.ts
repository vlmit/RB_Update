import { FC } from 'react';
import { BlockToggleHandler, LinkInfo, MarkToggleHandler, RangeStyle, RichTextBoxAttachment } from './common';
import { IColorPalette } from 'ui/colorPicker/';
import { DialogType, UIButton } from 'tessa/ui';
export interface RichTextBoxToolbarProps {
    readOnlyMode?: boolean;
    hasReadOnlyMode?: boolean;
    readOnly?: boolean;
    onChangeReadOnlyMode: (value: boolean) => void;
    onBlockToggle: BlockToggleHandler;
    onStyleToggle: MarkToggleHandler;
    onClearStyle: () => void;
    onUnwrapBlock: () => void;
    foregroundPalette: IColorPalette;
    backgroundPalette: IColorPalette;
    blockPalette: IColorPalette;
    dropDownDirectionUp?: boolean;
    showFileDialog?(dialogType: DialogType): Promise<readonly File[] | File | null>;
    showLinkDialog?(): Promise<LinkInfo | null>;
    onAddFiles(attachments: RichTextBoxAttachment[]): void;
    onAddLink(attachments: RichTextBoxAttachment[]): void;
    onAddImages(attachments: RichTextBoxAttachment[]): void;
    showAttachmentsMenu: boolean;
    selectionStyle: RangeStyle;
    hoverButtons?: readonly UIButton[];
}
export declare const RichTextBoxToolbar: FC<RichTextBoxToolbarProps>;
