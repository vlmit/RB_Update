import { ExtensionContainer, ExtensionStage } from 'tessa/extensions';

import { MEDOPreviewBtn } from './PreviewButton';
import { filePreviewControlUIExtension } from './filePreviewControlUIExtension';



ExtensionContainer.instance.registerExtension({
  extension: MEDOPreviewBtn,
  stage: ExtensionStage.AfterPlatform
});

ExtensionContainer.instance.registerExtension({
  extension: filePreviewControlUIExtension,
  stage: ExtensionStage.BeforePlatform
});

