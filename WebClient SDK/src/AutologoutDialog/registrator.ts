import { ExtensionContainer, ExtensionStage } from 'tessa/extensions';
import {  AutoLogoutDialog, LogExtension } from './AutoLogout';

ExtensionContainer.instance.registerExtension({
  extension: AutoLogoutDialog,
  stage: ExtensionStage.AfterPlatform,
});
ExtensionContainer.instance.registerExtension({
  extension: LogExtension,
  stage: ExtensionStage.AfterPlatform,
});
