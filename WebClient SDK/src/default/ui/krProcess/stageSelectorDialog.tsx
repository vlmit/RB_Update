import * as React from 'react';
import { observer } from 'mobx-react';
import styled from 'styled-components';
import { StageGroup } from './stageGroup';
import { StageType } from './stageType';
import { StageSelectorViewModel } from './stageSelectorViewModel';
import { LocalizationManager } from 'tessa/localization';
import {
  Dialog,
  DialogContainer,
  DialogContent,
  DialogHeader,
  DialogFooter,
  RaisedButton
} from 'ui';
import { Grid, GridUIViewModel } from 'components/cardElements';

const StageDialog = styled(Dialog)`
  background: ${props => props.theme.card.cardSelectorBackground};
`;

export interface StageSelectorDialogProps {
  viewModel: StageSelectorViewModel;
  onClose: (
    args: {
      cancel: boolean;
      group: StageGroup | null;
      type: StageType | null;
    }
  ) => void;
}

@observer
// tslint:disable-next-line:max-line-length
export class StageSelectorDialog extends React.Component<StageSelectorDialogProps> {
  //#region ctor

  constructor(props: StageSelectorDialogProps) {
    super(props);
  }

  //#endregion

  //#region react

  public render() {
    // const { viewModel } = this.props;

    const groupsGridVM = new GridUIViewModel({
      canSelectMultipleItems: false,
      rows: this.getGroupsRows(),
      headers: this.getGroupsHeader()
    });

    const typesGridVM = new GridUIViewModel({
      canSelectMultipleItems: false,
      rows: this.getTypesRows(),
      headers: this.getTypesHeader()
    });

    return (
      <StageDialog
        isOpened={true}
        noPortal={true}
        isAutoSize={true}
        onCloseRequest={this.handleCloseForm}
        className="kr-stages-modal"
      >
        <DialogContainer>
          <DialogHeader>
            {LocalizationManager.instance.localize('$UI_Cards_SelectGroupAndType')}
          </DialogHeader>
          <DialogContent>
            <Grid viewModel={groupsGridVM} wideVisibleSize={1} />
            <Grid
              viewModel={typesGridVM}
              wideVisibleSize={1}
              isShowNoDataText={LocalizationManager.instance.localize(
                '$UI_Error_NoAvailableStages'
              )}
            />
          </DialogContent>
          <DialogFooter className='default-footer'>
            <RaisedButton
              icon="icon-thin-254"
              onClick={this.handleCloseFormWithResult}
              label={LocalizationManager.instance.localize('$UI_Common_OK')}
            />
            <RaisedButton
              icon="icon-thin-253"
              onClick={this.handleCloseForm}
              label={LocalizationManager.instance.localize('$UI_Common_Cancel')}
            />
          </DialogFooter>
        </DialogContainer>
      </StageDialog>
    );
  }

  //#endregion

  //#region renders

  //#endregion

  //#region methods

  private getGroupsHeader = () => {
    return [
      {
        alias: 'name',
        caption: LocalizationManager.instance.localize('$Views_KrStageTemplates_StageGroup')
      }
    ];
  }

  private getGroupsRows = () => {
    const { viewModel } = this.props;

    if (viewModel.groups.length === 0) {
      return [];
    }

    return viewModel.groups.map((group, index) => ({
      rowId: group.id,
      // order: group.order,
      cells: [{ content: LocalizationManager.instance.localize(group.name) }],
      getIsSelected: () => {
        return viewModel.selectedGroupIndex === index;
      },
      getIsLastSelected: () => {
        return viewModel.selectedGroupIndex === index;
      },
      setSelected: () => {
        const { viewModel } = this.props;
        if (viewModel.selectedGroupIndex === index) {
          return;
        }

        viewModel.setSelectedGroupIndex(index);
        viewModel.updateType();
      }
    }));
  }

  private getTypesHeader = () => {
    return [
      {
        alias: 'name',
        caption: LocalizationManager.instance.localize('$Views_KrStageTemplates_Types')
      }
    ];
  }

  private getTypesRows = () => {
    const { viewModel } = this.props;

    if (viewModel.types.length === 0) {
      return [];
    }

    return viewModel.types.map((type, index) => ({
      rowId: type.id,
      // order: i,
      cells: [{ content: LocalizationManager.instance.localize(type.caption) }],
      getIsSelected: () => {
        return viewModel.selectedTypeIndex === index;
      },
      getIsLastSelected: () => {
        return viewModel.selectedTypeIndex === index;
      },
      setSelected: () => {
        viewModel.setSelectedTypeIndex(index);
      }
    }));
  }

  //#endregion

  //#region handlers

  private handleCloseForm = () => {
    this.props.onClose({
      cancel: true,
      group: null,
      type: null
    });
  };

  private handleCloseFormWithResult = () => {
    const { viewModel } = this.props;

    this.props.onClose({
      cancel: false,
      group: viewModel.selectedGroup,
      type: viewModel.selectedType
    });
  };

  //#endregion
}
