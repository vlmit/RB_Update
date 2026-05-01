import { ExtensionContainer, ExtensionStage } from 'tessa/extensions';
import { HideControlNPAUIExtension } from './HideControlNPAUIExtension';
import { HideTableButton } from './HideTableButton';

ExtensionContainer.instance.registerExtension({
  extension: HideControlNPAUIExtension,
  stage: ExtensionStage.AfterPlatform,
});

ExtensionContainer.instance.registerExtension({
  extension: HideTableButton,
  stage: ExtensionStage.AfterPlatform,
});
