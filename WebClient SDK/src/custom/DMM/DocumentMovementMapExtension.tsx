import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
import React, { ChangeEvent, Component } from 'react';
import * as ReactDOM from 'react-dom';
import moment from 'moment/moment';
import { LocalizationManager } from 'tessa/localization';

type TaskObject = {
  optionCaption: string;
  optionName: string;
  completed: string;
  created: string;
  authorName: string;
  roleName: string;
  completedByName: string;
  planned: string;
  result: string;
};

type Task = {
  TypeCaption: {
    $value: string,
    $type: string,
  };
  TypeName: {
    $value: string,
    $type: string,
  };
};

type Cycle = {
  cyclesItems: Task[];
  Caption: {
    $value: string,
    $type: string,
  };
  GroupRowID: {
    $value: string,
    $type: string,
  };
  cycles: Task[];
};

type TaskCardProps = {
  task: Task;
};

type TaskCardStates = {
  isCommentExpanded: boolean;
};

type TaskCycleProps = {
  cycle: Cycle;
  selectedOption: string;
  handleHoverCycle: (event: React.MouseEvent<HTMLDivElement, MouseEvent>, closestSelector: string) => void;
  handleLeaveCycle: (event: React.MouseEvent<HTMLDivElement, MouseEvent>) => void;
  checkpointIndex: number;
  isCheckpointActive: boolean;
};

type DocumentMovementMapProps = {
  rootCycle: Cycle[];
  cyclesItems: Task[];
};

type DocumentMovementMapStates = {
  selectedOption: string;
  activeCheckpointIndex: number | null;
};

type TaskSelectOption = {
  value: string;
  label: string;
};

type TaskSelectProps = {
  selectedOption: string;
  handleSelectChange: (event: ChangeEvent<HTMLSelectElement>) => void;
  options: TaskSelectOption[];
};

type CycleCheckpointProps = {
  checkpointIndex: number;
  isCheckpointActive: boolean;
  isCheckpointLast: boolean;
  handleHoverCycle: (event: React.MouseEvent<HTMLDivElement, MouseEvent>, closestSelector: string) => void;
  handleLeaveCycle: (event: React.MouseEvent<HTMLDivElement, MouseEvent>) => void;
};

class CycleCheckpoint extends Component<CycleCheckpointProps> {
  render() {
    const {
      isCheckpointActive,
      checkpointIndex,
      isCheckpointLast,
      handleHoverCycle,
      handleLeaveCycle,
    } = this.props;

    return (
      <div
        className="cycle-checkpoint"
        onMouseOver={(e) => handleHoverCycle(e, ".cycle-checkpoint")}
        onMouseLeave={handleLeaveCycle}
        data-active-checkpoint-index={checkpointIndex}
      >
        <div className={`cycle-checkpoint__point point ${isCheckpointActive ? "point_active" : ""}`}>
          <span>{checkpointIndex + 1}</span>
          <div className="point__line"></div>
        </div>
        {isCheckpointLast
          ? null
          : <div className="cycle-checkpoint__line"></div>
        }
      </div>
    );
  }
}

class TaskSelect extends Component<TaskSelectProps> {
  render() {
    return (
      <select
        className="task-select"
        value={this.props.selectedOption}
        onChange={this.props.handleSelectChange}
        >
          {this.props.options.map(({value, label}) =>
            <option value={value}>{label}</option>)
          }
      </select>
    );
  }
}

class TaskCycle extends Component<TaskCycleProps> {
  render() {
    const {
      cycle,
      selectedOption,
      checkpointIndex,
      isCheckpointActive,
      handleHoverCycle,
      handleLeaveCycle,
    } = this.props;

    const filteredCycleTasks = cycle.cyclesItems
      .filter((task) => {
        const taskOption = task.TypeCaption.$value;
        return selectedOption === 'ShowAll' || taskOption === selectedOption;
      });

    const title = LocalizationManager.instance.format(cycle.Caption.$value, 'GroupTypeCaption');
    const type = LocalizationManager.instance.format(
        cycle.cyclesItems[cycle.cyclesItems.length - 1].TypeCaption.$value,
        cycle.cyclesItems[cycle.cyclesItems.length - 1].TypeName.$value,
      );

    return <div
      className={`cycle ${isCheckpointActive ? 'cycle_active' : ''}`}
      onMouseOver={(e) => handleHoverCycle(e, ".cycle")}
      onMouseLeave={handleLeaveCycle}
      data-active-checkpoint-index={checkpointIndex}
    >
      <div className="cycle__header header">
        <div className="header__item item">
          <div className="item__title">{title}</div>
          <div className="item__text">{type}</div>
        </div>
        <div className="item__type">{type}</div>
      </div>
      <div className="cycle__grid grid">
        {filteredCycleTasks.length
          ? filteredCycleTasks.map((item) => <TaskCard task={item}/>)
          : 'Нет заданий'
        }
      </div>
    </div>;
  }
}

class TaskCard extends Component<TaskCardProps, TaskCardStates> {
  constructor(props) {
    super(props);
    this.state = {
      isCommentExpanded: false,
    };
  }

  private toggleCommentExpansion = () => {
    this.setState({
      isCommentExpanded: !this.state.isCommentExpanded,
    });
  }

  private formatDate(date) {
    return moment(date).utc().format('DD.MM.YYYY');
  }

  private getDuration(created, completed) {
    moment.locale('ru');
    const start = moment(created);
    const end = moment(completed);
    const duration = moment.duration(end.diff(start));
    const humanizedDuration = duration.humanize(true); // with suffix
    return humanizedDuration
      .replace('через ', '')
      .replace(' назад', '');
  }

  public render() {
    const task = this.props.task;

    let taskObjectValues = {} as TaskObject;
    Object.entries(task).forEach(([key, value]) => {
      taskObjectValues[key[0].toLowerCase() + key.slice(1)] = value ? value.$value : null;
    });

    const {
      optionCaption,
      optionName,
      completed,
      created,
      authorName,
      roleName,
      completedByName,
      planned,
      result,
    } = taskObjectValues;

    const option = LocalizationManager.instance.format(optionCaption, optionName); // optionCaption === "Вариант завершения"
    const comment = LocalizationManager.instance.format(result, 'Result');

    return <div className="task-card">
      <div className="task-card__header header">
        {option
          ? <div className="task-card__status status">{option}</div>
          : <div className="task-card__status status status_in-progress">Без результата</div>
        }
        {completed
          ? <div className="task-card__execution-time">Исполнено за {this.getDuration(created, completed)}</div>
          : null
        }
      </div>
      <div className="task-card__body body">
        <div className="body__description description">
          <div className="description__supervisor">{roleName}</div>
          <div className="description__label label title">{authorName}</div>
          <div className="description__label label">
            <span className="label__title title">Исполнено: </span>
            <span className="label__text text">{completedByName ? completedByName : '-'}</span>
          </div>
          <div className="description__label label">
            <span className="label__title title">План/Факт: </span>
            <span className="label__text text">
              {this.formatDate(planned)}/{completed ? this.formatDate(completed) : '-'}
            </span>
          </div>
        </div>
        <div className={`body__comment comment ${this.state.isCommentExpanded ? "comment_expanded" : ""}`}>
          {comment
            ? <i
              className={`comment__icon icon ${this.state.isCommentExpanded ? "icon_expanded" : ""}`}
              onClick={this.toggleCommentExpansion}
            >
              {'>'}
            </i>
            : null
          }
          <div className="comment__text">
            {comment}
          </div>
        </div>
      </div>
    </div>;
  }
}

class DocumentMovementMap extends Component<DocumentMovementMapProps, DocumentMovementMapStates> {
  constructor(props) {
    super(props);
    this.state = {
      selectedOption: 'ShowAll',
      activeCheckpointIndex: null,
    };
  }

  private handleSelectChange = ({ target }) => {
    this.setState({
      selectedOption: target.value,
    });
  }

  private handleHoverCycle = (e, closestSelector) => {
    if (e.target) {
      const closestEl = e.target.closest(closestSelector);
      this.setState({
        activeCheckpointIndex: Number(closestEl.getAttribute('data-active-checkpoint-index')),
      });
    }
  }

  private handleLeaveCycle = () => {
    this.setState({
      activeCheckpointIndex: null,
    });
  }

  public render() {
    const rootCycle = this.props.rootCycle;
    const cyclesItems = this.props.cyclesItems;

    const optionsFromCyclesItems = cyclesItems.map((task) => ({
      value: task.TypeCaption.$value,
      label: LocalizationManager.instance.format(task.TypeCaption.$value, task.TypeName.$value),
    }));

    // remove duplicates in the options
    const filteredOptionsFromCyclesItems = optionsFromCyclesItems.reduce((acc: { value: any, label: string }[], current) => {
      const x = acc.find(({ value }) => value === current.value);
      if (!x) {
        return acc.concat([current]);
      }
      return acc;
    }, []);

    const optionsForTaskSelect = [
      { label: 'Показать все', value: 'ShowAll' },
        ...filteredOptionsFromCyclesItems,
    ];

    const renderRootCycle = (item) => {
      const childCycles = item.cycles;
      if (childCycles.length === 0) return null;
      return childCycles.map((childCycle, index) => <>
        <CycleCheckpoint
          checkpointIndex={index}
          isCheckpointActive={index === this.state.activeCheckpointIndex}
          isCheckpointLast={index + 1 === childCycles.length}
          handleHoverCycle={this.handleHoverCycle}
          handleLeaveCycle={this.handleLeaveCycle}
        />
        <TaskCycle
          checkpointIndex={index}
          isCheckpointActive={index === this.state.activeCheckpointIndex}
          cycle={childCycle}
          selectedOption={this.state.selectedOption}
          handleHoverCycle={this.handleHoverCycle}
          handleLeaveCycle={this.handleLeaveCycle}
        />
      </>);
    };

    return <div className="document-movement-map">
        <div className="document-movement-map__header header">
          <div className="header__filter filter">
            <span>Фильтрация по статусам </span>
            <TaskSelect
              selectedOption={this.state.selectedOption}
              handleSelectChange={this.handleSelectChange}
              options={optionsForTaskSelect}
            />
          </div>
        </div>
        <div className="document-movement-map__content content">
          {rootCycle
            ? renderRootCycle(rootCycle)
            : "Нет данных для отображения"
          }
        </div>
    </div>;
  }
}

export class DocumentMovementMapExtension extends CardUIExtension {
  public contextInitialized(cardContext: ICardUIExtensionContext) {
    const tabForm = cardContext.model.forms.find(x => x.name === 'DocumentMovementMap');
    if (!tabForm) {
      return;
    }
    const cardId = cardContext.card.id;
    const tabFormClassName = `DocumentMovementMapTabContent-${cardId}`;
    tabForm.className = tabFormClassName;
    const domContainerId = `ReactDocumentMovementMapTabContent-${cardId}`;

    const renderDocumentMovementMapTabContent = (tabContent) => {
      tabContent.innerHTML = `<div id="${domContainerId}"></div>`;
      const cycles = cardContext.card.taskHistoryGroups.getStorage();
      const cyclesItems = cardContext.card.taskHistory.getStorage();
      const cyclesWithItems = cycles.map((group) => ({
          ...group,
          // @ts-ignore
          cyclesItems: cyclesItems.filter(({GroupRowID}) => GroupRowID.$value === group.RowID.$value),
        })
      );
      const formatParentCycle = (cycle) => {
        // @ts-ignore
        const childCycles = cyclesWithItems.filter(({ ParentRowID }) => (ParentRowID ? ParentRowID.$value : null) === cycle.RowID.$value);
        if (childCycles.length === 0) return cycle;
        return {
          ...cycle,
          cycles: childCycles,
        };
      };
      // @ts-ignore
      const rootCycle = formatParentCycle(cyclesWithItems.filter(({ ParentRowID }) => (ParentRowID ? ParentRowID.$value : null) === null)[0]);
      const domContainer = document.getElementById(domContainerId);
      // @ts-ignore
      ReactDOM.render(<DocumentMovementMap cyclesItems={cyclesItems} rootCycle={rootCycle}/>, domContainer);
    };

    const startWatcher = setInterval( function () {
      const tabContent = document.querySelector(`.cardContainer:not(.hidden) .${tabFormClassName}`);
      if (typeof tabContent !== 'undefined' && tabContent !== null && tabContent.innerHTML) {
        clearInterval(startWatcher);
        renderDocumentMovementMapTabContent(tabContent);
      }
    }, 50);
  }
}