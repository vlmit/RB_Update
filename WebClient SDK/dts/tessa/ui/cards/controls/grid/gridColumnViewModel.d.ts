import { TextAlignProperty } from 'csstype';
export declare class GridColumnViewModel {
    constructor(header: string, alignment?: TextAlignProperty);
    readonly header: string;
    readonly alignment: TextAlignProperty;
}
