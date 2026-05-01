import { ApplicationExtension, IApplicationExtensionMetadataContext } from 'tessa';
import { tryGetFromInfo } from 'tessa/ui';
import { ClientConditionTypesProvider, IConditionType } from 'tessa/platform/conditions';

export class ConditionTypesClientInitializationExtension extends ApplicationExtension {
  public afterMetadataReceived(_context: IApplicationExtensionMetadataContext): void {
    const conditionTypes = tryGetFromInfo<IConditionType[]>(
      _context.mainPartResponse?.info,
      '.ConditionTypes'
    );

    if (!!conditionTypes) {
      ClientConditionTypesProvider.instance.initialize(conditionTypes);
    }
  }
}
