import React from 'react';
import { Observer } from 'mobx-react';
import { ITableCellViewModel } from 'tessa/ui/views/content';
import { FileViewModel } from 'tessa/ui/cards/controls';
import { FileListTags } from 'tessa/ui/cards/components/controls';

export interface FileCaptionCellProps {
  cell: ITableCellViewModel;
  file: FileViewModel;
}

export class FileCaptionCell extends React.Component<FileCaptionCellProps> {
  render() {
    const { cell, file } = this.props;
    return (
      <span style={{ display: 'inline-flex' }}>
        <Observer>
          {() =>
            file.isLoading ? (
              <span className="files-control-item-loading">
                <i className="icon expand-icon fa fa-spinner fa-spin" />
              </span>
            ) : (
              <></>
            )
          }
        </Observer>
        {cell.convertedValue}
        <FileListTags
          style={{
            marginLeft: '6px',
            fontSize: '1rem'
          }}
          viewModel={file}
        />
      </span>
    );
  }
}
