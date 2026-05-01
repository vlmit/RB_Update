import { ExtensionContainer, ExtensionStage } from 'tessa/extensions';

import { ODTestButton } from './CertificateButton';
import { ODOpenUserCard } from './OpenUserCard';
import { ODGetCertinfo } from './GetCertInfoFromProfile';


ExtensionContainer.instance.registerExtension({
  extension: ODTestButton,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: ODOpenUserCard,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: ODGetCertinfo,
  stage: ExtensionStage.AfterPlatform
});
