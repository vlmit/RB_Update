import React from 'react';
import { CardTableViewControlViewModel } from './cardTableViewControlViewModel';
import { UIButton, UIButtonComponent } from 'tessa/ui';
import { BaseViewControlItem, ViewControlPagingViewModel } from 'tessa/ui/cards/controls';
import { GridInputFilter } from 'tessa/ui/cards/components/controls';
import { Visibility } from 'tessa/platform';
import { PagingView } from 'tessa/ui/views/components';

export class CardTableViewPanelViewModel extends BaseViewControlItem {
  constructor(viewComponent: CardTableViewControlViewModel) {
    super(viewComponent);

    this.paging = new ViewControlPagingViewModel(viewComponent);
  }

  leftButtons: UIButton[] = [];

  rightButtons: UIButton[] = [];

  paging: ViewControlPagingViewModel;
}

export interface CardTableViewPanelProps {
  viewModel: CardTableViewPanelViewModel;
}

export class CardTableViewPanel extends React.Component<CardTableViewPanelProps> {
  render() {
    const { viewModel } = this.props;

    const leftButtons = viewModel.leftButtons.map((x, xi) => {
      return <UIButtonComponent key={`lb_${xi}`} viewModel={x} />;
    });

    const rightButtons = viewModel.rightButtons.map((x, xi) => {
      return <UIButtonComponent key={`rb_${xi}`} viewModel={x} />;
    });

    return (
      <div className="tableControlButtons">
        <div className="left">
          {leftButtons}
          {/* @ts-ignore после копирования SDK Pick почему-то требует наличия ref,key */}
          <PagingView viewModel={viewModel.paging} />
          <GridInputFilter
            filterableGrid={viewModel.viewComponent as CardTableViewControlViewModel}
            visibility={Visibility.Visible}
          />
        </div>
        <div className="right">{rightButtons}</div>
      </div>
    );
  }
}
