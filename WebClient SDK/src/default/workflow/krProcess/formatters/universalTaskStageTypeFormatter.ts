import {
  KrStageTypeFormatter,
  IKrStageTypeFormatterContext,
  StageTypeHandlerDescriptor,
  universalTaskDescriptor
} from 'tessa/workflow/krProcess';
import { plainColumnName } from 'tessa/workflow';

export class UniversalTaskStageTypeFormatter extends KrStageTypeFormatter {
  private _kindCaption = plainColumnName('KrUniversalTaskSettingsVirtual', 'KindCaption');
  private _digest = plainColumnName('KrUniversalTaskSettingsVirtual', 'Digest');

  public descriptors(): StageTypeHandlerDescriptor[] {
    return [universalTaskDescriptor];
  }

  public format(context: IKrStageTypeFormatterContext) {
    super.format(context);

    const settings = context.stageRow.getStorage();
    const buidler = { text: '' };

    this.appendString(buidler, settings, this._kindCaption, '', true);
    this.appendString(buidler, settings, this._digest, '', true, false, 30);

    context.displaySettings = buidler.text;
  }
}
