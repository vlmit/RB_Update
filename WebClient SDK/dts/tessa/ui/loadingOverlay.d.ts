export declare class LoadingOverlay {
    private constructor();
    private static _instance;
    static get instance(): LoadingOverlay;
    private _stack;
    show<T = void>(action: (resolve: (value?: T | PromiseLike<T>) => void) => Promise<T>, text?: string): Promise<T>;
    private close;
}
export declare function showLoadingOverlay<T = void>(action: (resolve: (value?: T | PromiseLike<T>) => void) => Promise<T>, text?: string): Promise<T>;
