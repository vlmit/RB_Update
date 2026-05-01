import { SliderControlType } from './27_sliderControlType';
import { CardMetadataExtension, ICardMetadataExtensionContext } from 'tessa/cards/extensions';
import { CardTypeEntryControl } from 'tessa/cards/types';
import { TestCardTypeID } from '../common';
/**
 * Создает собственные контролы и добавляет их в выбранную карточку.
 *
 * Результат работы расширения:
 * Пример данного расширения создает кастомный слайдер-контрол и добавляет его в блок
 * "Общая информация" тестовой карточки "Автомобиль".
 */
export class SliderMetadataExtension extends CardMetadataExtension {
  public initializing(context: ICardMetadataExtensionContext) {
    // если карточка не для тестов, то ничего не делаем
    const cardCarType = context.cardMetadata.getCardTypeById(TestCardTypeID);
    if (!cardCarType) {
      return;
    }

    // пытаемся получить блок "Общая информация"
    const mainBlock = cardCarType.blocks.find(block => block.name === 'MainInfo');
    if (!mainBlock) {
      return;
    }

    const maxOrder = Math.max(...mainBlock.controls.map(x => x.order), 0);

    // создаем новый контрол и задаем ему основные параметры
    const sliderType = new CardTypeEntryControl();
    sliderType.type = SliderControlType;
    sliderType.order = maxOrder + 1;
    sliderType.name = 'OurSuperMegaCoolSlider';
    sliderType.caption = 'Slider';
    sliderType.controlSettings = {
      MinValue: 20,
      MaxValue: 150,
      Step: 1
    };
    sliderType.blockSettings = {
      StartAtNewLine: true
    };
    sliderType.sectionId = '509d961f-00cf-4403-a78f-6736841de448';
    sliderType.physicalColumnIdList = ['ef4db447-b0b5-4474-a6b7-6c5c75465355'];

    // добавляем созданный контрол в блок "Общая информация"
    mainBlock.controls.push(sliderType);
  }
}
