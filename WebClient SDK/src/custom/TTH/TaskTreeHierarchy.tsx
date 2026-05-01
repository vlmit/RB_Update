import moment from 'moment';
import { Component,} from 'react';// useState , useEffect} from 'react';
import * as ReactDOM from 'react-dom';
import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
//import { Guid } from 'tessa/platform';
import { ITessaViewResult, RequestParameterBuilder, TessaViewRequest, ViewService } from 'tessa/views';
import { equalsCriteriaOperator } from 'tessa/views/metadata';
import { showLoadingOverlay, showNotEmpty } from 'tessa/ui';
import { ValidationResult } from 'tessa/platform/validation';
import { openCard } from 'tessa/ui/uiHost';
//import ClipLoader from "react-spinners/ClipLoader";

// @ts-ignore
const taskTreeIcon = <svg className="button__icon" id="Layer_1" xmlns="http://www.w3.org/2000/svg" x="0px" y="0px" viewBox="0 0 491.52 491.52" style={{enableBackground: 'new 0 0 491.52 491.52'}}><g><g><path d="M378.88,184.026v307.2h112.64v-307.2H378.88z M471.04,470.747h-71.68v-266.24h71.68V470.747z"/></g></g><g><g><path d="M256,235.227v256h102.4v-256H256z M337.92,470.747h-61.44v-215.04h61.44V470.747z"/></g></g><g><g><path d="M133.12,286.427v204.8h102.4v-204.8H133.12z M215.04,470.747L215.04,470.747H153.6v-163.84h61.44V470.747z"/></g></g><g><g><path d="M0,347.867v143.36h112.64v-143.36H0z M92.16,470.747H20.48v-102.4h71.68V470.747z"/></g></g><g><g><polygon points="336.975,0.293 330.095,19.581 404.601,46.146 56.565,229.831 66.125,247.942 411.508,65.658 385.95,137.341 405.24,144.223 443.07,38.122"/></g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g></svg>;
let InvisibleStutusFlag = true;

type Performer = {
  Description: string;
  IsResponsible: boolean;
  Planned: string;
  Report: string;
  User:  string;
  krState: string,
  IsRedirect: boolean;
  Link: string,
};

type Task = {
  parentTaskID: string | null;
  docID: string;
  authorName: string,
  controlMark: string,
  creationDate: string,
  docSubject: string,
  docDescription: string,
  krState: string,
  performersList: Performer[];
  registratorName: string,
  taskDeadline: string,
  myBranch: number,
  controlStatus: boolean;
};

type PerformerRowProps = {
  performer: Performer;
};

type TaskTreeItemProps = {
  task: Task;
};

type TaskTreeProps = {
  data: [];
  cardId: string;
};

type TaskTreeStates = {
  isTaskTreeHidden: boolean;
  isTaskTreeInvisible: boolean;
};

class PerformerRow extends Component<PerformerRowProps> {
  public render() {
    const {
      User,
      IsResponsible,
      Planned,
      Description,
      Report,
      krState,
      IsRedirect,
      Link,
    } = this.props.performer;

    return <div className="body__row row">
      <div className="row__column">
        {User}
      </div>
      <div className="row__column">
        {IsResponsible ? '⚑' : ''}
      </div>
      <div className="row__column">
        {Planned}
      </div>
      <div className="row__column">
        {Description}
      </div>
      <div className="row__column">
        {Report}
      </div>
      <div className="row__column">
        {krState}
      </div>
      <div className="row__column">
        {IsRedirect ? '➦' : ''}
      </div>
      <div className="row__column"> 
        {Link!='' ? <button onClick={() => this.openLinkInTabPanelById(Link)}>📄</button> : ''}
      </div>
    </div>;
  }
  private async openLinkInTabPanelById(id) {
    // открывается виртуальная карточка по айди
    await showLoadingOverlay(async (splashResolve) => {
      const editor = await openCard({
        cardId: id,
        splashResolve
      });

      if (editor) {
        const workspaceInfo = 'Документ';
        if (editor.workspaceInfo !== workspaceInfo) {
          editor.workspaceInfo = workspaceInfo;
          editor.cardModelInitialized.add(async e => { e.workspaceInfo = workspaceInfo; });
        }
      }
    });
  }
}  

class TaskTreeItem extends Component<TaskTreeItemProps> {
  private formatDate(date) {
    return moment(date).utc().format('DD.MM.YYYY');
  }

  private async openTaskInTabPanelById(id) {
    // открывается виртуальная карточка по айди
    await showLoadingOverlay(async (splashResolve) => {
      const editor = await openCard({
        cardId: id,
        splashResolve
      });

      if (editor) {
        const workspaceInfo = 'Поручение';
        if (editor.workspaceInfo !== workspaceInfo) {
          editor.workspaceInfo = workspaceInfo;
          editor.cardModelInitialized.add(async e => { e.workspaceInfo = workspaceInfo; });
        }
      }
    });
  }

  public render() {
    const {
      docID,
      authorName,
      controlMark,
      creationDate,
      docSubject,
      docDescription,
      krState,
      performersList,
      registratorName,
      taskDeadline,
      myBranch,
      controlStatus
    } = this.props.task;

    return (
      <div
        
        onDoubleClick={() => this.openTaskInTabPanelById(docID)}
        className={myBranch === 1 ? "mytask-tree__item item" : "task-tree__item item"}
         
      >
        <div className="item__header header">
          <div className="header__title title">
            <i className="title__icon">
              {'>'}
            </i>
            <p className="title__text">{docSubject}</p>
          </div>
          <div className="secondheader__title title">
            <p className="secondtitle__text">{docDescription}</p>
          </div>
          <div className="header__labels labels header__labels_bottom">
            <span className="labels__label label">{this.formatDate(creationDate)}</span>
            <span className="labels__label label">Автор: {authorName}</span>
            <span className={controlStatus === true ? "labels__controlON_label" : "labels__controlOFF_label"}>{controlMark}</span>
            <span className="labels__label label">Контролер: {registratorName || '-'}</span>
          </div>
          <div className="header__labels labels header__labels_right">
            <span className="labels__label label">Состояние: {krState}</span>
            <span className="labels__label label">Срок/Факт: {this.formatDate(taskDeadline)}/</span>
          </div>
        </div>
        <div className="item__body body">
          <div className="body__table table">
            <div className="table__header header">
              <div className="header__column">Кому назначено</div>
              <div className="header__column">Ответственный</div>
              <div className="header__column">Срок/Факт</div>
              <div className="header__column">Описание</div>
              <div className="header__column">Отчет об исполнении</div>
              <div className="header__column">Состояние</div>
              <div className="header__column"> </div>
              <div className="header__column"> </div>
            </div>
            <div className="table__body body">
              {
              performersList.length
                ? performersList.map((performer) => //,index) =>   
              //  {
                 // if(index != 0)
                 // {
                    <PerformerRow performer={performer}/> 
                //  }
                 // else
                 // {
                 //    'sadfsadfasdfasdf' 
                 // }                       
              //  }         
              )
                : 'Исполнители не назначены'
              }
            </div>
          </div>
        </div>
      </div>
    );
  }
}

class TaskTree extends Component<TaskTreeProps, TaskTreeStates> {
  constructor(props) {
    super(props);
    this.state = {
      isTaskTreeHidden: false,
      isTaskTreeInvisible: false
    };
  }



  private handleToggleTaskTree = () => {
    const taskTreeItems = document.querySelectorAll(`.cardContainer:not(.hidden) .TaskTreeHierarchyTabContent-${this.props.cardId} #ReactTaskTreeHierarchyTabContent-${this.props.cardId} .task-tree__item`);
      if (this.state.isTaskTreeHidden) {
        for (let taskTreeItem of taskTreeItems) {
          taskTreeItem.classList.remove('item_performers-hidden');
        }
      } else {
        for (let taskTreeItem of taskTreeItems) {
          taskTreeItem.classList.add('item_performers-hidden');
        }
      }
    this.setState({ isTaskTreeHidden: !this.state.isTaskTreeHidden });
  }

  private handleToggleMyTaskTree = () => {
    //const taskTreeItems = document.querySelectorAll<HTMLElement>('.cardContainer:not(.hidden) .header__title .title__icon');
    //alert(InvisibleStutusFlag);
    //if(InvisibleStutusFlag == true)
    //{
    //  this.setState({ isTaskTreeInvisible: !this.state.isTaskTreeInvisible });
    //}   
    const taskTreeItems = document.querySelectorAll(`.cardContainer:not(.invisible) .TaskTreeHierarchyTabContent-${this.props.cardId} #ReactTaskTreeHierarchyTabContent-${this.props.cardId} .task-tree__item`);
      if (this.state.isTaskTreeInvisible || InvisibleStutusFlag == true) {
        for (let taskTreeItem of taskTreeItems) {
          taskTreeItem.classList.remove('item_performers-invisible');  
        }
        this.setState({ isTaskTreeInvisible: false});
      } else { 
            for (let taskTreeItem of taskTreeItems) {
              taskTreeItem.classList.add('item_performers-invisible');
              this.setState({ isTaskTreeInvisible: true});
          }  
      }
     // this.setState({ isTaskTreeInvisible: !this.state.isTaskTreeInvisible });

    InvisibleStutusFlag = false;
    //alert(InvisibleStutusFlag);  
  }

  private getNestedTaskTreeItems(task, className, padding = 'unset') {

    if (!task.children) {
      return <div className={className} style={{ paddingLeft: padding }}>
        <TaskTreeItem task={task}/>
      </div>;
    }
    
    return <div className={className} style={{ paddingLeft: padding }}>
      <TaskTreeItem task={task}/>
      {task.children.map((item) => this.getNestedTaskTreeItems(item, 'child', '30px'))}
    </div>;
  }

  

  public render() {
    const {
      data,
    } = this.props;

    

    return (
      <div className="task-tree">
          {
          data.length ?
            <><div className="btn-group">

              <label className="toggle">
              <input className="toggle-checkbox" onClick={this.handleToggleTaskTree} type="checkbox"/>
              <div className="toggle-switch"></div>
              <span className="toggle-label">Только поручения 1-ого уровня  </span>
              </label>
              

              <label className="toggle">
              <input className="toggle-checkbox" onClick={this.handleToggleMyTaskTree} type="checkbox"/>
              <div className="toggle-switch"></div>
              <span className="toggle-label">Показать все ветви</span>
              </label>

              </div>
              <div 
                className={
                this.state.isTaskTreeHidden
                ? "task-tree__items-container task-tree__items-container_hidden"
                : "task-tree__items-container"
              }>
                {data.map((item) => this.getNestedTaskTreeItems(item, 'root'))}
              </div>
            </>
            : 'Нет данных для отображения'
          }
      </div>
  );
  }
}

export class TaskTreeHierarchy extends CardUIExtension {
  public contextInitialized(cardContext: ICardUIExtensionContext) {
    // если карточка не "Протокол", то ничего не происходит
   // if (!Guid.equals(cardContext.card.typeId, '4d9f9590-0131-4d32-9710-5e07c282b5d3')) {
   //   return;
   // }
    const cardId = cardContext.card.id;
    const tabForm = cardContext.model.forms.find(x => x.name === 'ControlTasks_RB');
    const tabFormClassName = `TaskTreeHierarchyTabContent-${cardId}`;
    if (tabForm) {
      tabForm.className = tabFormClassName;
    }
    const domContainerId = `ReactTaskTreeHierarchyTabContent-${cardId}`;

    const renderTaskTreeHierarchyTabContent = async (tabContent) => {
      await showLoadingOverlay(async (splashResolve) => {
      tabContent.innerHTML = `<div id="${domContainerId}"></div>`;
      const domContainer = document.getElementById(domContainerId);
      // get task tree data
      const data: [] = await TaskTreeHierarchy.getRequestData(cardContext) as [];
      
      /*const [loading , setloading] = useState(false);
      useEffect(() => 
      {
        setloading(true);
      },[])
      domContainer.
          ?
          <ClipLoader
            size={150}
            color={"F37A24"}
            loading={this.loading}
          />
          :*/
  

      ReactDOM.render(<TaskTree data={data} cardId={cardId}/>, domContainer);
      // add event listeners to expand/hide performers list buttons
      const taskTreeItemExpandPerformersButtons = document.querySelectorAll<HTMLElement>('.cardContainer:not(.hidden) .header__title .title__icon');
      for (let taskTreeItemExpandPerformersButton of taskTreeItemExpandPerformersButtons) {
        const item = taskTreeItemExpandPerformersButton.closest('.task-tree__item');
        const myitem = taskTreeItemExpandPerformersButton.closest('.mytask-tree__item');
        if(item !== null)
        {
          taskTreeItemExpandPerformersButton.addEventListener('click', function() {item!.classList.toggle('item_performers-hidden'); });
        }
        if(myitem !== null)
        {
          taskTreeItemExpandPerformersButton.addEventListener('click', function() {item!.classList.toggle('item_performers-hidden'); });
        }
        //ReactDOM.render(<TaskTree data={data} cardId={cardId}/>, domContainer);
        //taskTreeItemExpandPerformersButton.addEventListener('click', ReactDOM.render(<TaskTree data={data} cardId={cardId}/>, domContainer) );
        
      }
      InvisibleStutusFlag = true;
      const taskTreeItems = document.querySelectorAll(`.cardContainer:not(.invisible) .task-tree__item`);
      for (let taskTreeItem of taskTreeItems) {
        taskTreeItem.classList.add('item_performers-invisible');
      }
      splashResolve});     
    };


    const startWatcher = setInterval(async function () {
      const tabContent = document.querySelector(`.cardContainer:not(.hidden) .${tabFormClassName}`);
      if (typeof tabContent !== 'undefined' && tabContent !== null && tabContent.innerHTML) {
        clearInterval(startWatcher);
        await renderTaskTreeHierarchyTabContent(tabContent);
      }
    }, 50);

  }

  private static async getRequestData(cardContext: ICardUIExtensionContext) {
    // поиск представления "Дерево поручений"
    const partnersView = ViewService.instance.getByName('ControlTasks_RB_HierarchyExtension');
    if (!partnersView) {
      return;
    }

    const request = new TessaViewRequest(partnersView.metadata);
    // добавляем параметр фильтрации по имени контрагента (для примера, что имя не равно null)
    if(cardContext.card.typeId != '01ad498e-5f8e-417a-bd28-942739dbf342'
    && cardContext.card.typeId != '0d4db642-7b3e-4988-92e9-2fd3752a488c')
    {
      const paramCardId = new RequestParameterBuilder()
      .withMetadata(partnersView.metadata.parameters.get('Parent')!)
      .addCriteria(equalsCriteriaOperator(), 'Parent', cardContext.card.id)
      .asRequestParameter();
      request.values.push(paramCardId);
    }
    else{
      const paramTaskCardId = new RequestParameterBuilder()
      .withMetadata(partnersView.metadata.parameters.get('ParentTask')!)
      .addCriteria(equalsCriteriaOperator(), 'ParentTask', cardContext.card.id)
      .asRequestParameter();
      const paramParentCardId = new RequestParameterBuilder()
      .withMetadata(partnersView.metadata.parameters.get('Parent')!)
      .addCriteria(equalsCriteriaOperator(), 'Parent', cardContext.card.id)
      .asRequestParameter();
      request.values.push(paramTaskCardId);
      request.values.push(paramParentCardId);
    }
    

    let result: ITessaViewResult;
    try {
      // в getData будут добавлены параметры currentUserId и locale
      result = await partnersView.getData(request);
    } catch (err) {
      await showNotEmpty(ValidationResult.fromError(err));
      return;
    }

    const rows = result.rows;
    const columns = result.columns.map((column) => column[0].toLowerCase() + column.slice(1));
    const tasks = rows.map((row) => {
      const obj = {};
      for (let i = 0; i < columns.length; i++) {
        obj[columns[i]] = row[i];
      }
      return obj;
    });

    const tasksWithFormattedPerformersList = tasks.map((task: Task) => ({
        ...task,
        performersList: task.performersList ? JSON.parse(`[${task.performersList}]`.replace(/[\r\n]/g, '')) : [],
    }));

    

    const formatTask = (task: Task) => {
      const childrenTasks = tasksWithFormattedPerformersList.filter(({ parentTaskID }: Task) => parentTaskID === task.docID);
      
      if (childrenTasks.length === 0) return task;
      return {
        ...task,
        children: childrenTasks.map(formatTask),
      };
    };

    return tasksWithFormattedPerformersList.filter(({ parentTaskID }: Task) => parentTaskID === null).map(formatTask);
  }
}