import { CardToolbarItem, CardToolbarItemCommand } from './cardToolbarItem';
import { MediaStyle } from 'ui';
export declare class CardToolbarAction extends CardToolbarItem {
    constructor(args: {
        name: string;
        caption: string;
        icon: string;
        toolTip?: string;
        order?: number;
        style?: MediaStyle;
        command: CardToolbarItemCommand;
    });
}
