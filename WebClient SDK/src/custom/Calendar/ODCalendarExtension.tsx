import {  ICardUIExtensionContext } from 'tessa/ui/cards';
//import { Guid } from 'tessa/platform';
import { showMessage } from 'tessa/ui/tessaDialog';
import { tryGetFromInfo } from 'tessa/ui'
import { DateTimeViewModel } from 'tessa/ui/cards/controls';
import moment from 'moment';



/**
 * Данное расширение позволяет добавлять дополнительные параметры и элементы управления в форму логинизации.
 *
 * Результат работы расширения:
 * В форму логинизации добавлен параметр безопасности в виде капчи и тестовая кнопка, при нажатии
 * на которую появляется сообщение в диалоговом окне.
 */
/**
 * Автопредпросмотр первого файла определенной категории
 */
 export class ODCalendarExtension extends DateTimeViewModel {
  public initialized(context: ICardUIExtensionContext) {

const dateControl = tryGetFromInfo<DateTimeViewModel | null>(
      context.model.info,
      'DateView',
      null
    );
    if (dateControl) {
     //return;
     let beginDate: Date = new Date("2023-04-16");
     let maxDate: Date = new Date("2023-04-23");
     let minDate: Date = new Date("2023-04-14");
     
     dateControl.beginDate =moment(beginDate);
     dateControl.maxDate = moment(maxDate) ;
     dateControl.minDate = moment(minDate);
     dateControl.highlightBeginDate = true;
    }

    console.log('Tetsttwetewtwet');
    showMessage('test');

  }
}
