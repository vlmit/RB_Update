import { ExtensionContainer, ExtensionStage } from 'tessa/extensions';
import { FileControlUIExtension } from './FileControlExtension';

ExtensionContainer.instance.registerExtension({
  extension: FileControlUIExtension,
  stage: ExtensionStage.AfterPlatform
});