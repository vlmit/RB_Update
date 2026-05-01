import { IUserSession } from 'common/utility/userSession';
export declare const TessaClientApplicationAlias = "tessaclient";
export declare const TessaProtocol = "tessa";
export declare function getWebLink(action?: string, parameters?: Map<string, string>): string;
export declare function getClientLink(session: IUserSession, action: string, parameters: Map<string, string>): string;
