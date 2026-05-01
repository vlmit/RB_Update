import { ExtensionContainer, ExtensionStage } from 'tessa/extensions';
import { ODIncomingStamp } from './ODIncomingStampFileExtension';
import { ODOutgoingStamp } from './ODOutgoingStampFileExtension';

ExtensionContainer.instance.registerExtension({
  extension: ODIncomingStamp,
  stage: ExtensionStage.BeforePlatform
});

ExtensionContainer.instance.registerExtension({
  extension: ODOutgoingStamp,
  stage: ExtensionStage.BeforePlatform
});