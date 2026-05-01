import React from 'react';
import { observer } from 'mobx-react';
import { TaskHistoryViewSearchViewModel } from './taskHistoryViewSearchViewModel';
import { GridInputFilter } from 'tessa/ui/cards/components/controls';

export interface TaskHistoryViewSearchProps {
  viewModel: TaskHistoryViewSearchViewModel;
}

@observer
export class TaskHistoryViewSearch extends React.Component<TaskHistoryViewSearchProps> {
  render() {
    const { viewModel } = this.props;
    return <GridInputFilter filterableGrid={viewModel} visibility={viewModel.visibility} />;
  }
}
