import { ExtensionContainer, ExtensionStage } from 'tessa/extensions';

import { TaskTreeHierarchy } from './TaskTreeHierarchy';
import './TaskTreeHierarchy.css';

ExtensionContainer.instance.registerExtension({
  extension: TaskTreeHierarchy,
  stage: ExtensionStage.AfterPlatform
});
