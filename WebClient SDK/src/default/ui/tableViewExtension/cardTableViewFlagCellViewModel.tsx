import React from 'react';
import { computed } from 'mobx';
import styled from 'styled-components';
import { CardTableViewRowData } from './cardTableViewRowData';
import { Card, CardRow, CardPermissionFlags } from 'tessa/cards';
import { GridColumnInfo } from 'tessa/ui/cards/controls/grid/gridColumnInfo';
import { ITableCellViewModelCreateOptions, TableCellViewModel } from 'tessa/ui/views/content';
import { Checkbox } from 'ui';
import { hasFlag } from 'tessa/platform';

const CheckboxContainer = styled.div`
  padding: 8px;
  display: flex;
  justify-content: center;
  align-items: center;

  & .checkbox {
    padding: 0;
    margin: 0 !important;
    width: 1.1em;
    height: 1.1em;
  }

  & .checkbox .indicator {
    position: static;
    top: auto;
    left: auto;
  }
`;

export class CardTableViewFlagCellViewModel extends TableCellViewModel {
  //#region ctor

  constructor(args: ITableCellViewModelCreateOptions) {
    super(args);

    this._rowData = args.row.data as CardTableViewRowData;
    const columnName = args.column.columnName;

    this.card = this._rowData.card;
    this.cardRow = this._rowData.cardRow;
    this.columnInfo = this._rowData.columnInfos.get(columnName)!;

    this._value = '';
    this._convertedValue = '';
    this._toolTip = '';

    this._getContent = this.getCheckBoxContent;
  }

  //#endregion

  //#region fields

  private _rowData: CardTableViewRowData;

  //#endregion

  //#region props

  readonly card: Card;

  readonly cardRow: CardRow;

  readonly columnInfo: GridColumnInfo;

  @computed({ keepAlive: true })
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  public get value(): any {
    return this._rowData.flag;
  }

  @computed({ keepAlive: true })
  public get disabled(): boolean {
    return hasFlag(
      this.card.permissions.resolver.getFieldPermissionsInRow(
        this._rowData.sectionName,
        this._rowData.flagColumnName,
        this.cardRow.rowId
      ),
      CardPermissionFlags.ProhibitModify
    );
  }

  //#endregion

  //#region methods

  private handleContainerClick = (e: React.MouseEvent) => {
    const target = e.target as HTMLElement;
    if (target.classList.contains('checkbox') || target.classList.contains('indicator')) {
      e.stopPropagation();
    }
  };

  private handleCheckboxClick = (e: React.MouseEvent) => {
    e.stopPropagation();
  };

  private getCheckBoxContent = () => {
    return (
      <CheckboxContainer onClick={this.handleContainerClick}>
        <Checkbox
          onClick={this.handleCheckboxClick}
          checked={this.value}
          disabled={this.disabled}
          onChange={() => {
            this._rowData.flag = !this.value;
          }}
        />
      </CheckboxContainer>
    );
  };

  //#endregion
}
