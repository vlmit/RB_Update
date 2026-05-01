import { CardUIExtension, ICardUIExtensionContext } from 'tessa/ui/cards';
import { Guid, DotNetType} from 'tessa/platform';


/**
 * В зависимости от значения флажка на форме карточки менять другие поля карточки:
 * - в текстовое поле
 * - добавлять строчку в коллекционную секцию
 *
 * Когда жмакают галку "Базовый цвет", то:
 * - если галка установлена, то значение поля "Цвет" меняется на "Это базовый цвет"
 * - если галка установлена, то в секцию "Исполнители" добавляется новый сотрудник "Admin"
 * - если галка не установлена, то занчение поля "Цвет" меняется на "Это другой, не базовый цвет"
 */
export class ODApprovalStagesChangeFieldUIExtension extends CardUIExtension {

  public initialized(context: ICardUIExtensionContext) {
    // если карточка не Вхд Исхд Внутр ОРД Протоколов, то ничего не делаем 
    if (!Guid.equals(context.card.typeId, 'ed6d5ee1-9075-4ce0-bdae-76afe87544f2') 
    && !Guid.equals(context.card.typeId, 'de076ddc-70aa-4a4e-abbc-53b87567dc06') 
    && !Guid.equals(context.card.typeId, 'a0881723-a22a-499c-a764-b1796cd36cc5')
    && !Guid.equals(context.card.typeId, '486c9839-3e07-40be-bebc-2cfd6a222397')
    && !Guid.equals(context.card.typeId, '4d9f9590-0131-4d32-9710-5e07c282b5d3') 
    && !Guid.equals(context.card.typeId, '21bca3fc-f75f-413b-b5c8-49538cbfc761')
    ) {
      return;
    }

    // находим секции
    const approvalStages = context.card.sections.tryGet('ApprovalStages');
   // const performers = context.card.sections.tryGet('Performers');
    if (!approvalStages
      //|| !performers
    ) {
      return;
    }

    // подписываемся на изменения полей в секции
    approvalStages.fields.fieldChanged.add(e => {
      // если было изменено другое поле, то никак не реагируем
      if (e.fieldName !== 'TimeLimit' && e.fieldName !== 'Planned' && e.fieldName !== 'StageTypeID') {
        return;
      }      
      //const flagsForApproval = context.model.blocks.get('forApprovalFlags');

      if (e.fieldName == 'TimeLimit') {
        // ставим значения поля в секции
        approvalStages.fields.set('Planned', null, DotNetType.Double);
      }
      if (e.fieldName == 'Planned') {
        // ставим значения поля в секции
        approvalStages.fields.set('TimeLimit', null, DotNetType.DateTime);
      }
    /*  if (e.fieldName == 'StageTypeID' && e.fieldValue == 1) {
        // скрываем контролы
        if (!flagsForApproval) return;
        flagsForApproval.blockVisibility = Visibility.Hidden;
      }
      if (e.fieldName == 'StageTypeID' && e.fieldValue == 1) {
        // делаем контролы видимыми
        if (!flagsForApproval) return;
        flagsForApproval.blockVisibility = Visibility.Visible;
      }*/
    });
  }

}