import { IFileVersion } from 'tessa/files';
import { OnlyOfficeApi } from './onlyOfficeApi';
import { LocalizationManager } from 'tessa/localization';
import { ValidationError, ValidationResult, ValidationResultType } from 'tessa/platform/validation';
import { showNotEmpty } from 'tessa/ui';

/**
 * Представляет собой модель редактора, используемого для чтения и редактирования файлов в отдельной вкладке браузера.
 */
export class OnlyOfficeEditorWindowViewModel {
  public readonly placeholder: string;
  public readonly version: IFileVersion;
  public readonly forEdit: boolean;

  private readonly _cardId: guid;
  private readonly _api: OnlyOfficeApi;
  private _window: Window | null = null;

  public constructor(api: OnlyOfficeApi, version: IFileVersion, forEdit: boolean, cardId: guid) {
    this._api = api;
    this.placeholder = 'onlyoffice-editor-placeholder';
    this.version = version;
    this.forEdit = forEdit;
    this._cardId = cardId;
  }

  public load = async (): Promise<void> => {
    try {
      await this._api.openFile(
        this.version,
        this.openEditorWindow,
        this.unload,
        this.forEdit,
        this._cardId,
        true
      );
    } catch (e) {
      if (e instanceof ValidationError) await showNotEmpty(e.Result);
      else await showNotEmpty(ValidationResult.fromError(e));
    }
  };

  public unload = async (): Promise<void> => {
    if (this._window) {
      this._window.close();
      this._window = null;
    }
  };

  private openEditorWindow = async (id: string): Promise<void> => {
    const config = this._api.createDefaultDocEditorConfig(
      id,
      this.version,
      this.forEdit ? 'edit' : 'view'
    );

    // noinspection JSUnresolvedFunction,JSUnresolvedVariable
    const html = `
<html lang="${LocalizationManager.instance.currentLocaleCode}">
<head>
<title>TESSA DOCUMENT - ${this.version.name}</title>
<script
  id="${OnlyOfficeApi.apiScriptId}"
  onerror="window.checkScript()"
  onload="window.checkScript()"
  src="${this._api.settings.apiScriptUrl}">
</script>
</head>
<body>
<div id="${this.placeholder}"></div>
<script>
const config = ${JSON.stringify(config)};
const editor = new DocsAPI.DocEditor('${this.placeholder}', config);
</script>
</body>
</html>`;

    this._window = window.open();
    // tslint:disable-next-line:triple-equals
    if (!this._window || this._window.closed || typeof this._window.closed == 'undefined') {
      throw new ValidationError(
        ValidationResult.fromText(
          LocalizationManager.instance.localize('$OnlyOffice_Error_NewWindowBlocked'),
          ValidationResultType.Error
        )
      );
    }

    // устанавливаем функцию проверки загрузки скрипта
    let resolveApiAvailableChecking: () => void;
    let rejectApiAvailableChecking: (e: Error) => void;
    const apiAvailableCheckingPromise = new Promise((resolve, reject) => {
      resolveApiAvailableChecking = resolve;
      rejectApiAvailableChecking = reject;
    });

    this._window['checkScript'] = () => {
      try {
        OnlyOfficeApi.throwIfApiScriptIsNotLoaded(this._window!);
        resolveApiAvailableChecking!();
      } catch (e) {
        this._window?.close();
        rejectApiAvailableChecking!(e);
      }
    };

    // загружаем документ
    this._window.document.open();
    this._window.document.write(html);
    this._window.document.close();

    // дожидаемся результатов проверки скрипта
    await apiAvailableCheckingPromise;

    // подписываемся на события закрытия окна и дожидаемся закрытия
    await new Promise(resolve => {
      let closeFired = false;
      const afterWindowClosedFunc = () => {
        if (!closeFired) {
          closeFired = true;
          resolve();
        }
      };

      // навсякий случай подписываемся на оба события
      this._window!.onbeforeunload = afterWindowClosedFunc;
      this._window!.onunload = afterWindowClosedFunc;
    });
  };
}
