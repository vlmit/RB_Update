import { FilesViewControlTableGridViewModel } from './filesViewControlTableGridViewModel';
import {
  StandardViewControlContentItemsFactory,
  ViewControlInitializationContext,
  BaseViewControlItem
} from 'tessa/ui/cards/controls';

export class FilesViewControlContentItemsFactory extends StandardViewControlContentItemsFactory {
  createTableContent(context: ViewControlInitializationContext): BaseViewControlItem | null {
    const controlViewModel = context.controlViewModel;
    return new FilesViewControlTableGridViewModel(controlViewModel);
  }
}
