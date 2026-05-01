/**
 * Выполняет запрос к серверу и отправляет на логин, если токен неправильный или закончилась сессия.
 */
export declare const fetchWithLogoutCheck: (path: any, options?: RequestInit) => Promise<Response>;
/**
 * Выполняет запрос к серверу и отправляет на логин, если токен неправильный или закончилась сессия,
 * и выполняет стандартную обработку.
 */
export declare const fetchWithLogoutCheckAndDefaultThen: (path: any, options?: RequestInit | undefined) => Promise<any>;
/**
 * Выполняет стандартную обработку ответа от сервера.
 */
export declare const processDefaultThen: (promise: Promise<Response>, getResult?: ((reponse: Response) => Promise<any>) | undefined) => Promise<any>;
