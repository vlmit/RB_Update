import { ExtensionContainer, ExtensionStage } from 'tessa/extensions';
import { AutoLogout } from './AutoLogout';

ExtensionContainer.instance.registerExtension({
  extension: AutoLogout,
  stage: ExtensionStage.AfterPlatform,
});
