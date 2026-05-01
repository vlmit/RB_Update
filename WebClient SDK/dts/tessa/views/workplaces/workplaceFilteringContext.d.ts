import { IWorkplaceFilteringRule } from './workplaceFilteringRule';
import { RequestParameter } from '../metadata';
export declare class WorkplaceFilteringContext {
    readonly rules: IWorkplaceFilteringRule[];
    readonly refSection: string;
    readonly parameters: RequestParameter[];
    constructor(rules: IWorkplaceFilteringRule[], refSection: string, parameters: RequestParameter[]);
}
