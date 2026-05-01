import { UserLoginType, UserAccessLevel, SessionLicenseType } from 'common/cards';
import { TypedField } from 'tessa/platform/typedField';
import { DotNetType } from 'tessa/platform';
import { IStorage } from 'tessa/platform/storage';
export interface IUserSession {
    /**
     * Пользователь админ
     * @type {boolean}
     */
    isAdmin: boolean;
    AccessLevel: UserAccessLevel;
    ApplicatonID: string;
    Created: Date | undefined;
    Culture: any;
    Expires: Date | undefined;
    HostIP: string;
    HostName: string;
    InstanceName: string;
    LicenseType: SessionLicenseType;
    LoginType: UserLoginType;
    ServerCode: string;
    SessionID: string;
    Signature: string;
    UICulture: any;
    UserID: string;
    UserLogin: string;
    UserName: string;
    UtcOffsetMinutes: number;
    TimeZoneUtcOffset: number;
    init(storage: IStorage): boolean;
    serializeToXml(): string;
}
export interface IToken {
    AccessLevel: TypedField<DotNetType.Int32, UserAccessLevel> | null;
    ApplicatonID: TypedField<DotNetType.Guid, guid> | null;
    Created: TypedField<DotNetType.DateTime, Date>;
    Culture: TypedField<DotNetType.String, any>;
    Expires: TypedField<DotNetType.DateTime, Date>;
    HostIP: TypedField<DotNetType.String, string>;
    HostName: TypedField<DotNetType.String, string>;
    InstanceName: TypedField<DotNetType.String, string>;
    LicenseType: TypedField<DotNetType.Int32, SessionLicenseType>;
    LoginType: TypedField<DotNetType.Int32, UserLoginType>;
    ServerCode: TypedField<DotNetType.String, string>;
    SessionID: TypedField<DotNetType.Guid, guid>;
    Signature: TypedField<DotNetType.String, string>;
    UICulture: TypedField<DotNetType.String, any>;
    UserID: TypedField<DotNetType.Guid, guid>;
    UserLogin: TypedField<DotNetType.String, string>;
    UserName: TypedField<DotNetType.String, string>;
    UtcOffsetMinutes: TypedField<DotNetType.Double, number>;
    TimeZoneUtcOffset: TypedField<DotNetType.Double, number>;
}
declare let userSession: IUserSession;
export default userSession;
