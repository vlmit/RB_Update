import { ICardValidator } from './cardValidator';
import { Registry, IRegistry } from 'tessa/platform/registry';
export declare class CardValidatorRegistry extends Registry<ICardValidator> {
    private constructor();
    private static _instance;
    static get instance(): IRegistry<ICardValidator>;
}
