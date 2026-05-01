import { ExtensionContainer, ExtensionStage } from 'tessa/extensions';

import { DocumentMovementMapExtension } from './DocumentMovementMapExtension';
import './DocumentMovementMapExtension.css';

ExtensionContainer.instance.registerExtension({
  extension: DocumentMovementMapExtension,
  stage: ExtensionStage.AfterPlatform
});
