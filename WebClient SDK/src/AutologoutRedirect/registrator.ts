import { ExtensionContainer, ExtensionStage } from 'tessa/extensions';
import { AutoLogoutRedirect } from './AutologoutRedirect';

ExtensionContainer.instance.registerExtension({
  extension: AutoLogoutRedirect,
  stage: ExtensionStage.AfterPlatform,
});
