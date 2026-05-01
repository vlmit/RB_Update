import { ExtensionContainer, ExtensionStage } from 'tessa/extensions';

import { ODCalendarExtension } from './ODCalendarExtension';


ExtensionContainer.instance.registerExtension({
  extension: ODCalendarExtension,
  stage: ExtensionStage.BeforePlatform
});
