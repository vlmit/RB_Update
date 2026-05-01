import { computed } from 'mobx';
import { CardTableViewRowData } from './cardTableViewRowData';
import { Card, CardRow } from 'tessa/cards';
import { GridColumnInfo } from 'tessa/ui/cards/controls/grid/gridColumnInfo';
import { ITableCellViewModelCreateOptions, TableCellViewModel } from 'tessa/ui/views/content';

export class CardTableViewCellViewModel extends TableCellViewModel {
  //#region ctor

  constructor(args: ITableCellViewModelCreateOptions) {
    super(args);

    const rowData = args.row.data as CardTableViewRowData;
    const columnName = args.column.columnName;

    this.card = rowData.card;
    this.cardRow = rowData.cardRow;
    this.columnInfo = rowData.columnInfos.get(columnName)!;

    this._value = '';
    this._convertedValue = '';
    this._toolTip = '';
  }

  //#endregion

  //#region fields

  //#endregion

  //#region props

  readonly card: Card;

  readonly cardRow: CardRow;

  readonly columnInfo: GridColumnInfo;

  @computed({ keepAlive: true })
  // tslint:disable-next-line:no-any
  public get value(): any {
    let { value } = this.columnInfo.getValue(this.cardRow, this.card);
    return value;
  }

  @computed({ keepAlive: true })
  // tslint:disable-next-line:no-any
  public get convertedValue(): any {
    let { formattedValue } = this.columnInfo.getValue(this.cardRow, this.card);
    return this.columnInfo.clipValue(formattedValue);
  }

  @computed({ keepAlive: true })
  public get toolTip(): string {
    const { value } = this.columnInfo.getValue(this.cardRow, this.card);
    const clip = this.columnInfo.isClipValueNeed(value);
    return clip ? value : '';
  }

  //#endregion

  //#region methods

  //#endregion
}
