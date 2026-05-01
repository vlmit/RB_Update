export interface IForumSanitizer {
    sanitizeMessage(html: string): string;
}
export declare class ForumSanitizer implements IForumSanitizer {
    static current: IForumSanitizer;
    private static _instance;
    static get instance(): ForumSanitizer;
    sanitizeMessage(html: string): string;
}
