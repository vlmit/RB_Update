export declare class AccessSettings {
    static readonly AllowEdit = 0;
    static readonly DisallowEdit = 1;
    static readonly DisallowRowAdding = 2;
    static readonly DisallowRowDeleting = 3;
    static readonly MaskData = 4;
}
export declare class MandatoryValidationType {
    static readonly Always = 0;
    static readonly OnTaskCompletion = 1;
    static readonly WhenOneFieldFilled = 2;
}
export declare class ControlType {
    static readonly Tab = 0;
    static readonly Block = 1;
    static readonly Control = 2;
}
export declare class FileAccessSettings {
    static readonly FileNotAvailable = 0;
    static readonly ContentNotAvailable = 1;
    static readonly OnlyLastVersion = 2;
    static readonly OnlyLastAndOwnVersions = 3;
    static readonly InfoKey = ".FileAccessSettings";
}
