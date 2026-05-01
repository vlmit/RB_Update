import { ExtensionContainer, ExtensionStage } from 'tessa/extensions';

import { CalendarUIExtension } from './calendarUIExtension';
import { OutgoingPartnerUIExtension } from './outgoingPartnerUIExtension';
import { CarUIExtension } from './carUIExtension';
import { KrDocStateUIExtension } from './krDocStateUIExtension';
import { CreateAndSelectToolbarUIExtension } from './createAndSelectToolbarUIExtension';
import { KrVirtualFilesUIExtension } from './krVirtualFilesUIExtension';
import { KrPermissionsUIExtension } from './krPermissionsUIExtension';
import { KrExtendedPermissionsUIExtension } from './krExtendedPermissionsUIExtension';
import KrRoutesInWorkflowEngineUIExtension from './workflowEngine/krRoutesInWorkflowEngineUIExtension';
import { KrCheckStateTileManagerUIExtension } from './workflowEngine/krCheckStateTileManagerUIExtension';
import { KrGetCycleFileInfoUIExtension } from './krGetCycleFileInfoUIExtension';
import { InitializeFilesViewUIExtension } from './cardFiles/initializeFilesViewUIExtension';
import { FilesViewApplicationExtension } from './cardFiles/filesViewApplicationExtension';
import { MakeViewTaskHistoryUIExtension } from './taskHistory/makeViewTaskHistoryUIExtension';
import { TaskHistroryViewApplicationExtension } from './taskHistory/taskHistroryViewApplicationExtension';
import { MakeViewTableControlUIExtension } from './tableViewExtension/makeViewTableControlUIExtension';
import { CardTableViewControlApplicationExtension } from './tableViewExtension/cardTableViewControlApplicationExtension';
import { OpenCardInViewUIExtension } from './openCardInViewUIExtension';
import { CardDialogPreviewUIExtension } from './cardDialogPreviewUIExtension';
import { UIErrorPresenterButtonsUIExtension } from './uiErrorPresenterButtonsUIExtension';

ExtensionContainer.instance.registerExtension({
  extension: KrGetCycleFileInfoUIExtension,
  stage: ExtensionStage.BeforePlatform
});
ExtensionContainer.instance.registerExtension({
  extension: KrCheckStateTileManagerUIExtension,
  stage: ExtensionStage.Platform
});
ExtensionContainer.instance.registerExtension({
  extension: CalendarUIExtension,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: OutgoingPartnerUIExtension,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: CarUIExtension,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: KrDocStateUIExtension,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: CreateAndSelectToolbarUIExtension,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: KrVirtualFilesUIExtension,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: KrPermissionsUIExtension,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: KrExtendedPermissionsUIExtension,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: KrRoutesInWorkflowEngineUIExtension,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: InitializeFilesViewUIExtension,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: FilesViewApplicationExtension,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: MakeViewTaskHistoryUIExtension,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: TaskHistroryViewApplicationExtension,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: MakeViewTableControlUIExtension,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: CardTableViewControlApplicationExtension,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: OpenCardInViewUIExtension,
  stage: ExtensionStage.AfterPlatform
});
ExtensionContainer.instance.registerExtension({
  extension: CardDialogPreviewUIExtension,
  stage: ExtensionStage.Platform
});
ExtensionContainer.instance.registerExtension({
  extension: UIErrorPresenterButtonsUIExtension,
  stage: ExtensionStage.Platform,
  singleton: true
});
