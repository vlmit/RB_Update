// import moment from 'moment';
import {
  CardToolbarAction,
  CardUIExtension,
  ICardUIExtensionContext,
} from 'tessa/ui/cards';
import { getTessaIcon } from 'common/utility';
import { Guid } from 'tessa/platform';
//import { appImpportPFX } from 'importPFX.js';
//import { appImportPFX } from 'c:/tessa/WebClient SDK/src/custom/ui/importPFX.js'
// import { appReady } from './importPFX.js'

// import { ITessaViewResult, RequestParameterBuilder, TessaViewRequest, ViewService } from 'tessa/views';
// import { equalsCriteriaOperator } from 'tessa/views/metadata';
// import { showLoadingOverlay, showNotEmpty } from 'tessa/ui';
// import { ValidationResult } from 'tessa/platform/validation';
// import { openCard } from 'tessa/ui/uiHost';
// import { CardService } from 'tessa/cards/service/cardService';
// import { CardGetRequest } from 'tessa/cards/service';
// import { CardFile } from 'tessa/cards';

// interface Performer {
//   Description: string;
//   IsResponsible: boolean;
//   Planned: string;
//   Report: string;
//   User:  string;
// }

// interface Task {
//   parentTaskID: string | null;
//   docID: string;
//   performersList: Performer[];
// }

export class ODFormImportPFXUIExtension extends CardUIExtension {
  // private _disposer: Function | null = null;

  public initialized(cardContext: ICardUIExtensionContext) {
    // если карточка не "Протокол", то ничего не происходит
    if (!Guid.equals(cardContext.card.typeId, '929ad23c-8a22-09aa-9000-398bf13979b2')
    ) {
      return;
    }

     // добавление кнопки в тулбар
     if (cardContext.toolbar) {
      cardContext.toolbar.removeItemIfExists('TaskTree');
      cardContext.toolbar.addItem(
        new CardToolbarAction({
          name: 'TaskTree',
          caption: 'Импорт PFX (моб. приложение)',
          icon: getTessaIcon('Thin359'),
          command: async () => {
            //const data = await ODFormImportPFXUIExtension.getRequestData(cardContext);
            await ODFormImportPFXUIExtension.getPFX()
            //const data = await ODFormExampleUIExtension.getRequestData(cardContext);
            //await ODFormExampleUIExtension.printImage(data);
            },
        })
      );
    }
  }

  // private static async getRequestData(cardContext: ICardUIExtensionContext) {
  //   // const cardGetRequest = new CardGetRequest();
  //   // cardGetRequest.cardId = cardContext.card.id;
  //   // cardGetRequest.cardTypeId = 'fa9dbdac-8708-41df-bd72-900f69655dfa';
  //   // cardGetRequest.cardTypeName = 'KrPermissions';

  //   // при запросе все расширения CardGetExtension будут вызываны
  //   // const cardGetResponse = await CardService.instance.get(cardGetRequest);
  //   // if (!cardGetResponse.validationResult.isSuccessful) {
  //   //   // если в validationResult есть ошибки, то показываем их
  //   //   await showNotEmpty(cardGetResponse.validationResult.build());
  //   //   return;
  //   // }

  //   const userCard = cardContext.card;

  //   let pfxFile = new CardFile();

  //   let foundFlag = false;

  //   userCard.files.forEach(function(f){
  //     const temp = f.name.split('.');
  //     if (temp[1].toLowerCase().includes('pfx'))
  //     {
  //       pfxFile = f;
  //       foundFlag = true;
  //     }
  //   })

  //   if (!foundFlag)
  //   {
  //     return `not found file pfx`;
  //   }

  //   //let fileUrl = `https://localhost/cardId=${}`;
  //   let tempUrl = `https://sedmo.gov.rb/api/filelink?` +
  //                 `cardId=${userCard.id}` + 
  //                 `&fileId=${pfxFile.card.id}` +
  //                 `&versionId=${pfxFile.versionRowId}` + //check
  //                 `&cardTypeId=${userCard.typeId}` +
  //                 `&cardTypeName=${userCard.typeName}` +
  //                 `&fileName=${pfxFile.name}` +
  //                 `&fileTypeName=${pfxFile.typeName}`;

  //   return tempUrl;
  //   // let tempUrl = `https://188.170.228.11:44123/api/filelink?` +
  //                 // `cardId=60e40a7a-7e5d-4315-8e7d-410865f920e8` + 
  //                 // `&fileId=bb0b991d-8431-481d-9c14-46c0115ac811` +
  //                 // `&versionId=53acbea0-5248-4610-8904-620577ee3a7e` +
  //                 // `&cardTypeId=929ad23c-8a22-09aa-9000-398bf13979b2` +
  //                 // `&cardTypeName=PersonalRole` +
  //                 // `&fileName=DocGroupsIndexes.xlsx` +
  //                 // `&fileTypeName=File`;

  // }


  // private static async getRequestData111(cardContext: ICardUIExtensionContext) {
  //   // поиск представления "Дерево поручений"
  //   const partnersView = ViewService.instance.getByName('ControlTasks_RB_HierarchyTree');
  //   if (!partnersView) {
  //     return;
  //   }

  //   const request = new TessaViewRequest(partnersView.metadata);

  //   // добавляем параметр фильтрации по имени контрагента (для примера, что имя не равно null)
  //   const paramCardId = new RequestParameterBuilder()
  //     .withMetadata(partnersView.metadata.parameters.get('Parent')!)
  //     .addCriteria(equalsCriteriaOperator(), 'Parent', cardContext.card.id)
  //     .asRequestParameter();
  //   request.values.push(paramCardId);

  //   let result: ITessaViewResult;
  //   try {
  //     // в getData будут добавлены параметры currentUserId и locale
  //     result = await partnersView.getData(request);
  //   } catch (err) {
  //     await showNotEmpty(ValidationResult.fromError(err));
  //     return;
  //   }

  //   const rows = result.rows;
  //   const columns = result.columns.map((column) => column[0].toLowerCase() + column.slice(1));
  //   const tasks = rows.map((row) => {
  //     const obj = {};
  //     for (let i = 0; i < columns.length; i++) {
  //       obj[columns[i]] = row[i];
  //     }
  //     return obj;
  //   });

  //   const tasksWithFormattedPerformersList = tasks.map((task: Task) => ({
  //       ...task,
  //       performersList: JSON.parse(`[${task.performersList}]`)}
  //   ));

  //   const formatTask = (task: Task) => {
  //     const childrenTasks = tasksWithFormattedPerformersList.filter(({ parentTaskID }: Task) => parentTaskID === task.docID);
  //     if (childrenTasks.length === 0) return task;
  //     return {
  //       ...task,
  //       children: childrenTasks.map(formatTask),
  //     };
  //   };

  //   return tasksWithFormattedPerformersList.filter(({ parentTaskID }: Task) => parentTaskID === null).map(formatTask);
  // }

//   OnAppReady() {
//     alert("!!!");
//  }

// private static appImportPFX(string)
// {
// }

//   private static async getPFX()
//   {
//     if (!appReady) {
//       alert('Приложение не активно!');
//       return;
//     }
//     let request = {
//       url: 'https://www.biz-it.ru/mobile/tessa/test.pfx',
//       password: '111111@N',
//       pin: '',
//       infoCallback: 'showInfo',
//       responseCallback: 'importPFXResponse'
//     }; 

//     const requestJSON = JSON.stringify(request);
//     appImportPFX(requestJSON);
//   }



// }
  
  private static async getPFX()
  {
    //appImportPFX();
    const width = window.innerWidth;
    const height = window.innerHeight;
    const options = `toolbar=no,location=no,directories=no,menubar=no,scrollbars=yes,width=${width},height=${height}`;
    const printWindow = window.open('', '_self', options);
    if (!printWindow) {
      return;
    }

    // const template = `
    // <!DOCTYPE html>
    // <html lang="ru">
    // <head>
    
    // </head>
    // <body>
    //   <br>
    //   <br>
    //   <br>
    //   <br>
    //   <br>
    //   <br>
    //   <br>
    //   <br><br>
    //   <br><br>
    //   <br>
    //   <p>${data}</p>
      
    
    // </body>

    // </html>`

    const template = `
      <!DOCTYPE html>
      <html lang="ru">
      <head>
      <script>
      function importPFX() {
          let request = {
        url: 'https://www.biz-it.ru/mobile/tessa/test.pfx',
        password: '111111@N',
        pin: '',
        infoCallback: 'showInfo',
        responseCallback: 'importPFXResponse'
          };
          appImportPFX(JSON.stringify(request));
      }
      </script>
      </head>
      <body>
      <br>
	<br>
	<br>
	<br>
	<br>
	<br>
	<br>
	<br><br>
	<br><br>
	<br>
	
 <p><input type="button" value="Импорт" onclick="importPFX()"></p>
      </body>

      </html>
      `;

    printWindow.document.open();
    printWindow.document.write(template);
    printWindow.focus();
    //printWindow!.close();
  }
}

  // private static async printImage(data) {
  //   const width = window.innerWidth;
  //   const height = window.innerHeight;
  //   const options = `toolbar=no,location=no,directories=no,menubar=no,scrollbars=yes,width=${width},height=${height}`;
  //   const printWindow = window.open('', 'template', options);
  //   if (!printWindow) {
  //     return;
  //   }

  //   function toggleClass(item, className) {
  //     item.classList.toggle(className);
  //   }

  //   function formatDate(date) {
  //     return moment(date).utc().format('DD.MM.YYYY');
  //   }

  //   function createTaskTreeItemPerformersList(performersList) {
  //     return performersList.map((performer) => `<div class="body__row row">
  //           <div class="row__column">
  //             ${performer.User}
  //           </div>
  //           <div class="row__column">
  //             ${performer.IsResponsible ? '⚑' : ''}
  //           </div>
  //           <div class="row__column">
  //             ${performer.Planned}
  //           </div>
  //           <div class="row__column">
  //             ${performer.Description}
  //           </div>
  //           <div class="row__column">
  //             ${performer.Report}
  //           </div>
  //         </div>
  //     `).join('');
  //   }

//     function createTaskTreeItem(task) {
//       return `<div data-task-id="${task.docID}" class="task-tree__item item">
//     <div class="item__header header">
//       <div class="header__title title">
//         <i class="title__icon"> > </i>
//         <p class="title__text">${task.docSubject}</p>
//       </div>
//       <div class="header__labels labels header__labels_bottom">
//         <span class="labels__label label">${formatDate(task.creationDate)}</span>
//         <span class="labels__label label">Автор: ${task.authorName}</span>
//         <span class="labels__label label">Контролер: ${task.registratorName || '-'}</span>
//       </div>
//       <div class="header__labels labels header__labels_right">
//         <span class="labels__label label">Состояние: ${task.krState}</span>
//         <span class="labels__label label">Срок/Факт: ${formatDate(task.taskDeadline)}/</span>
//       </div>
//     </div>
//     <div class="item__body body">
//       <div class="body__table table">
//         <div class="table__header header">
//           <div class="header__column">Кому назначено</div>
//           <div class="header__column">Ответственный</div>
//           <div class="header__column">Срок/Факт</div>
//           <div class="header__column">Описание</div>
//           <div class="header__column">Отчет об исполнении</div>
//         </div>
//         <div class="table__body body">
//         ${createTaskTreeItemPerformersList(task.performersList)}
//         </div>
//       </div>
//     </div>
//   </div>`;
//     }

//     function getNestedTaskTreeItems(task, className, padding = 'unset') {
//       if (!task.children) {
//         return `<div class="${className}" style="padding-left: ${padding}">${createTaskTreeItem(task)}</div>`;
//       }
//       return `<div class="${className}" style="padding-left: ${padding}">${createTaskTreeItem(task)} ${task.children.map((item) => getNestedTaskTreeItems(item, 'child', '30px')).join('')}</div>`;
//     }

//     const nestedTasks = data.map((item) => getNestedTaskTreeItems(item, 'root')).join('');

//     const taskTreeIcon = '<svg class="button__icon" id="Layer_1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" x="0px" y="0px" viewBox="0 0 491.52 491.52" style="enable-background:new 0 0 491.52 491.52;" xml:space="preserve"><g><g><path d="M378.88,184.026v307.2h112.64v-307.2H378.88z M471.04,470.747h-71.68v-266.24h71.68V470.747z"/></g></g><g><g><path d="M256,235.227v256h102.4v-256H256z M337.92,470.747h-61.44v-215.04h61.44V470.747z"/></g></g><g><g><path d="M133.12,286.427v204.8h102.4v-204.8H133.12z M215.04,470.747L215.04,470.747H153.6v-163.84h61.44V470.747z"/></g></g><g><g><path d="M0,347.867v143.36h112.64v-143.36H0z M92.16,470.747H20.48v-102.4h71.68V470.747z"/></g></g><g><g><polygon points="336.975,0.293 330.095,19.581 404.601,46.146 56.565,229.831 66.125,247.942 411.508,65.658 385.95,137.341 405.24,144.223 443.07,38.122"/></g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g></svg>';

//     const template = `
// <!DOCTYPE html>
// <html lang="ru">
// <head>
//   <meta name="viewport" content="width=device-width, initial-scale=1" charset="UTF-8">
//   <title>Дерево поручений</title>
// </head>
// <body>
// <div class="task-tree">
//   <button id="task-tree__button" class="task-tree__button button">
//     ${taskTreeIcon}
//     <div class="button__title">Показать/скрыть всё дерево</div>
//   </button>
//   <div id="task-tree__items-container" class="task-tree__items-container">
//     ${nestedTasks}
//   </div>
// </div>
// </body>
// </html>
// `;
//     printWindow.document.open();
//     printWindow.document.write(template);
//     printWindow.focus();

//     // on double-click a task will open in the tab panel
//     const taskTreeItems = printWindow!.document.querySelectorAll<HTMLElement>('.task-tree__item');
//     async function openTaskInTabPanel(taskId) {
//       // открывается виртуальная карточка по айди
//       await showLoadingOverlay(async (splashResolve) => {
//         const editor = await openCard({
//           cardId: taskId,
//           splashResolve
//         });

//         if (editor) {
//           const workspaceInfo = '$UI_Tiles_Settings';
//           if (editor.workspaceInfo !== workspaceInfo) {
//             editor.workspaceInfo = workspaceInfo;
//             editor.cardModelInitialized.add(async e => { e.workspaceInfo = workspaceInfo; });
//           }
//         }
//       });
      // printWindow!.close();
  //   }

  //   for (let taskTreeItem of taskTreeItems) {
  //     taskTreeItem.addEventListener('dblclick', function() {openTaskInTabPanel(taskTreeItem.dataset.taskId); });
  //   }

  //   // hide or expand performers list
  //   const taskTreeItemExpandButtons = printWindow!.document.querySelectorAll<HTMLElement>('.header__title .title__icon');
  //   for (let taskTreeItemExpandButton of taskTreeItemExpandButtons) {
  //     const item = taskTreeItemExpandButton.closest('.task-tree__item');
  //     taskTreeItemExpandButton.addEventListener('click', function() {toggleClass(item, 'item_performers-hidden'); });
  //   }

  //   // hide or expand task tree
  //   const taskTreeItemsContainer = printWindow!.document.getElementById('task-tree__items-container');
  //   const taskTreeButton = printWindow.document.getElementById('task-tree__button');
  //   function expandOrHideTaskTree() {
  //     if (taskTreeItemsContainer!.classList.contains('task-tree__items-container_hidden')) {
  //       for (let taskTreeItem of taskTreeItems) {
  //         taskTreeItem.classList.remove('item_performers-hidden');
  //       }
  //     } else {
  //       for (let taskTreeItem of taskTreeItems) {
  //         taskTreeItem.classList.add('item_performers-hidden');
  //       }
  //     }
  //     toggleClass(taskTreeItemsContainer, 'task-tree__items-container_hidden');
  //   }
  //   taskTreeButton!.addEventListener('click', function() {expandOrHideTaskTree(); });
  // }


// const launcherIcon64 =
//  'data:image/png;base64,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';


