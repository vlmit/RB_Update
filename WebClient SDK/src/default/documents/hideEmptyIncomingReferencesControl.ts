import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
import { DefaultFormMainViewModel } from 'tessa/ui/cards/forms';
import { AutoCompleteTableViewModel } from 'tessa/ui/cards/controls';
import { Visibility } from 'tessa/platform';

export class HideEmptyIncomingReferencesControl extends CardUIExtension {
  public initialized(context: ICardUIExtensionContext) {
    const model = context.model;
    const view = model.mainForm as DefaultFormMainViewModel;

    if (!view || !(view instanceof DefaultFormMainViewModel)) {
      return;
    }

    const block = view.blocks.find(x => x.name === 'RefsBlock');
    if (!block) {
      return;
    }

    const control = block.controls.find(x => x.name === 'IncomingRefsControl');
    const autoComplete = control as AutoCompleteTableViewModel;

    if (
      autoComplete &&
      autoComplete instanceof AutoCompleteTableViewModel &&
      autoComplete.items.length === 0
    ) {
      autoComplete.controlVisibility = Visibility.Collapsed;
    }
  }
}
