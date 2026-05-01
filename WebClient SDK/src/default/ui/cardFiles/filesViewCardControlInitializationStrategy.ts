import { FilesViewMetadata } from './filesViewMetadata';
import { CardFilesDataProvider } from './cardFilesDataProvider';
import {
  ViewControlInitializationStrategy,
  ViewControlInitializationContext,
  FileListViewModel
} from 'tessa/ui/cards/controls';
import { tryGetFromInfo } from 'tessa/ui';

export class FilesViewCardControlInitializationStrategy extends ViewControlInitializationStrategy {
  initializeMetadata(context: ViewControlInitializationContext) {
    context.controlViewModel.viewMetadata = FilesViewMetadata.create();
  }

  initializeDataProvider(context: ViewControlInitializationContext) {
    const fileControl = tryGetFromInfo<FileListViewModel | null>(
      context.model.info,
      context.controlViewModel.name ?? '',
      null
    );
    if (fileControl) {
      context.controlViewModel.dataProvider = new CardFilesDataProvider(
        context.controlViewModel.viewMetadata!,
        fileControl
      );
    }
  }
}
