import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
import { Guid, DotNetType } from 'tessa/platform';
import { CardRowState, CardRow } from 'tessa/cards';
import { ButtonViewModel } from 'tessa/ui/cards/controls';
//import {
//  tryGetFromInfo
//} from 'tessa/ui';
//import { autorun } from 'mobx';

/**
 * Описание логики для кнопки "Добавить исполнителя":
 * - заполнение строк таблицы осполнителя через доп. поля над таблицей исполнителей
 * , что позволяет не открывать каждый раз диалоговое окно новой строки таблицы, а вводить исполнителей непосредственно из карточки
 *
 * Когда нажимают на кнопку "Добавить исполнителей", то:
 * - добавляется новая строка в таблицу "Исполнители" по данным трех реквизитов и формы сверху
 * - после добавления строки реквизиты очищаются для добавления новой задачи
 * 
 * Описание логики для подстановки текста поручений из шаблона:
 * - заполнение строк таблицы осполнителя через доп. поля над таблицей исполнителей
 * , что позволяет не открывать каждый раз диалоговое окно новой строки таблицы, а вводить исполнителей непосредственно из карточки
 *
 */
export class ODControlTaskCustomButtonUIExtension extends CardUIExtension {
  //private _disposes: Array<(() => void) | null> = [];

  public initialized(context: ICardUIExtensionContext) {
    // если карточка не Поручения, то ничего не делаем
    if (!Guid.equals(context.card.typeId, '01ad498e-5f8e-417a-bd28-942739dbf342')
    && !Guid.equals(context.card.typeId, '0d4db642-7b3e-4988-92e9-2fd3752a488c')) {
      return;
    }

    // находим секции
    const performers = context.card.sections.tryGet('Performers');
    const taskDecisions = context.card.sections.tryGet('ControlTaskDecisions');
    const taskPerformers = context.card.sections.tryGet('ControlTaskPerformers');
    const documentCommonInfo = context.card.sections.tryGet('DocumentCommonInfo');
    const button = context.model.controls.get('AddTaskBtn') as ButtonViewModel;
    if (!button) {
      return;
    }

    if (!performers
      || !documentCommonInfo
      || !taskDecisions
      || !taskPerformers
    ) {
      return;
    }

    button.onClick = async () => {
      if(performers.rows.length > 0){
        
        const performerRows = performers.rows;
        performerRows.forEach(row => {
          const commentField = documentCommonInfo.fields.getField('Comment');
          const plannedField = documentCommonInfo.fields.getField('PlanDate');

          const newRow = new CardRow();
          newRow.rowId = Guid.newGuid();  
          newRow.set('Question', commentField?.$value, DotNetType.String);
          newRow.set('Planned', plannedField?.$value, DotNetType.DateTime);
          let order = taskDecisions.rows.length;
          newRow.set('Order', order, DotNetType.Int32);
          newRow.state = CardRowState.Inserted;
          taskDecisions.rows.push(newRow);
          // переносим исполнителей
          
        
          
            const userID = row.getField('UserID');
            const userName = row.getField('UserName');
            const newPerformerRow = new CardRow();
            newPerformerRow.rowId = Guid.newGuid();  
            newPerformerRow.set('ParentRowID', newRow.rowId, DotNetType.Guid);
            newPerformerRow.set('UserID', userID?.$value, DotNetType.Guid);
            newPerformerRow.set('UserName', userName?.$value, DotNetType.String);
            newPerformerRow.state = CardRowState.Inserted;
            taskPerformers.rows.push(newPerformerRow);
          });

          for (let i = performers.rows.length - 1; i >= 0; i--) {
            const row = performers.rows[i];
              if (row.state !== CardRowState.Inserted) {
              row.state = CardRowState.Deleted;
              } else {
                performers.rows.remove(row);
              }  
          }
          
          // чистим поле
          documentCommonInfo.fields.set('Comment', null, DotNetType.String);
          documentCommonInfo.fields.set('PlanDate', null, DotNetType.DateTime);
      }
    };

        // подписываемся на изменения полей в секции
        documentCommonInfo.fields.fieldChanged.add(e => {
          // если было изменено другое поле, то никак не реагируем
          if (e.fieldName !== 'TaskTextTemplateName' && e.fieldName !== 'TaskTextDescriptionName') {
            return;
          }
    
        const text = e.fieldValue;
          // ставим значения поля в секции

          if (e.fieldValue && e.fieldName == 'TaskTextTemplateName') {
          documentCommonInfo.fields.set('Subject', text, DotNetType.String);
            // чистим поле
          documentCommonInfo.fields.set('TaskTextTemplateID', null, DotNetType.Guid);
          documentCommonInfo.fields.set('TaskTextTemplateName', null, DotNetType.String);
          }
          if (e.fieldValue && e.fieldName == 'TaskTextDescriptionName') {
          documentCommonInfo.fields.set('Comment', text, DotNetType.String);
            // чистим поле
          documentCommonInfo.fields.set('TaskTextDescriptionID', null, DotNetType.Guid);
          documentCommonInfo.fields.set('TaskTextDescriptionName', null, DotNetType.String);
          }
        });

        /*const cardModel = context.model;
        const imageFileControl = tryGetFromInfo<FileListViewModel | null>(
          cardModel.info,
          'PerformerFilesControl',
          null
        );
    
        // показываем файлы только с категорией Image
        if (imageFileControl) {
          for (let i = imageFileControl.files.length - 1; i > -1; i--) {
            const file = imageFileControl.files[i];
            if (!(file.model.category && file.model.category.caption === 'Image')) {
              imageFileControl.removeFile(file);
            }
          }
    
          this._disposes.push(
            imageFileControl.containerFileAdding.addWithDispose(e => {
           ///   if (!(e.file.category && e.file.category.caption === 'Image')) {
           //     e.cancel = true;
          //    }
              if(taskPerformers.rows.length > 0 && e.file.category != null){
              for (let i = taskPerformers.rows.length - 1; i >= 0; i--) {
                const row = taskPerformers.rows[i];
                const userName = row.getField('UserName');
                const parentRow = row.getField('ParentRowID');
                if(e.file.category.caption === userName?.$value)
                {
                  for (let i = taskDecisions.rows.length - 1; i >= 0; i--) {
                    const rowDec = taskDecisions.rows[i];
                    const rowDecID = rowDec.getField('ID');
                    const rowDecReport = rowDec.getField('Report');
                    let text: string = rowDecReport?.$value;
                    if(rowDecID?.$value === parentRow?.$value)
                    {
                      text += 'Приложен файл: ' + e.file.name;
                      rowDec.set('Report', text, DotNetType.String);
                    }
                  }
                }        
              }
            }
            })
          );
        }*/

  }
  

}