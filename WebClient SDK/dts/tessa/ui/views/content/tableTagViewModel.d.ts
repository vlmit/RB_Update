import { CellTag, CellTagCtor } from 'components/cardElements/grid/models';
import { Command, CommandFunc } from 'tessa/platform';
export declare class TableTagViewModel extends CellTag {
    constructor(args: {
        handler?: CommandFunc;
    } & CellTagCtor);
    protected _command: Command;
    get command(): Command;
}
