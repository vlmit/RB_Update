using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MimeTypes;
using Tessa.Extensions.Default.Server.OnlyOffice;
using Tessa.Json;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.IO;
using Tessa.Platform.Runtime;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using Tessa.Web;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;
using ISession = Tessa.Platform.Runtime.ISession;

namespace Tessa.Extensions.Default.Server.Web.Controllers
{
    /// <summary>
    /// Предоставляет средства интеграции с сервером документов OnlyOffice
    /// для просмотра и редактирования документов офисных форматов.
    /// </summary>
    [Route("api/v1/onlyoffice"), AllowAnonymous, ApiController]
    [ProducesErrorResponseType(typeof(PlainValidationResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public sealed class OnlyOfficeController :
        Controller
    {
        #region Constructors

        public OnlyOfficeController(
            IWebHostEnvironment environment,
            IHttpClientFactory httpClientFactory,
            ISession session,
            IDbScope dbScope,
            IOnlyOfficeSettingsProvider settingsProvider,
            IOnlyOfficeFileCache fileCache,
            IOnlyOfficeFileCacheInfoStrategy fileCacheInfoStrategy)
        {
            this.environment = environment;
            this.httpClientFactory = httpClientFactory;
            this.session = session;
            this.dbScope = dbScope;
            this.settingsProvider = settingsProvider;
            this.fileCache = fileCache;
            this.fileCacheInfoStrategy = fileCacheInfoStrategy;
        }

        #endregion

        #region Fields

        private readonly IWebHostEnvironment environment;

        private readonly IHttpClientFactory httpClientFactory;

        private readonly ISession session;

        private readonly IDbScope dbScope;

        private readonly IOnlyOfficeSettingsProvider settingsProvider;

        private readonly IOnlyOfficeFileCache fileCache;

        private readonly IOnlyOfficeFileCacheInfoStrategy fileCacheInfoStrategy;

        #endregion

        #region Private Methods

        /// <summary>
        /// Создаёт объект <see cref="HttpClient"/> для обращения к веб-сервису OnlyOffice.
        /// Используйте конструкцию <c>using</c>, чтобы закрыть соединение с сервисом.
        /// </summary>
        /// <param name="settings">Настройки OnlyOffice, полученные из карточки настроек.</param>
        /// <returns>Созданный объект.</returns>
        private HttpClient CreateOnlyOfficeHttpClient(IOnlyOfficeSettings settings)
        {
            HttpClient httpClient = null;

            try
            {
                httpClient = this.httpClientFactory.CreateClient();

                httpClient.Timeout = settings.LoadTimeout;
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

                HttpClient result = httpClient;
                httpClient = null;

                return result;
            }
            finally
            {
                httpClient?.Dispose();
            }
        }

        /// <summary>
        /// Получает поток изменного файла в редакторе документов.
        /// </summary>
        /// <param name="info">Информация о файле.</param>
        /// <param name="convertToSourceExtension">Признак того, что следует произвести конвертацию в исходный формат файла.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Поток файла.</returns>
        private async Task<(Stream stream, string fileName)> GetModifiedFileFromOnlyOfficeAsync(
            IOnlyOfficeFileCacheInfo info,
            bool convertToSourceExtension,
            CancellationToken cancellationToken = default)
        {
            if (info.ModifiedFileUrl is null)
            {
                throw new InvalidOperationException("Modified file doesn't exist.");
            }

            IOnlyOfficeSettings settings = await this.settingsProvider.GetSettingsAsync(cancellationToken);
            using HttpClient httpClient = this.CreateOnlyOfficeHttpClient(settings);

            if (convertToSourceExtension)
            {
                string targetExtension = FileExtensionWithoutDot(info.SourceFileName);

                Stream convertedFileStream = await ConvertFileAsync(
                    httpClient,
                    settings.ConverterUrl,
                    info.ModifiedFileUrl,
                    targetExtension,
                    cancellationToken);

                return (convertedFileStream, info.SourceFileName);
            }

            string rawFileName = Path.GetFileNameWithoutExtension(info.SourceFileName) + "." +
                ExtractFileExtensionFromUrl(info.ModifiedFileUrl);

            Stream rawStream = await GetFileStreamByUrlAsync(httpClient, info.ModifiedFileUrl, cancellationToken);
            return (rawStream, rawFileName);
        }

        /// <summary>
        /// Конвертирует указанный файл в необходимый формат, используя сервер документов.
        /// </summary>
        /// <param name="httpClient">Объект, посредством которого выполняется вызов OnlyOffice.</param>
        /// <param name="converterUrl">Ссылка на эндпоинт конвертера.</param>
        /// <param name="fileUrl">Путь к исходному файлу.</param>
        /// <param name="toExt">Расширение, в которое необходимо конвертировать.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Возвращает поток сконвертированного файла.</returns>
        private static async Task<Stream> ConvertFileAsync(
            HttpClient httpClient,
            string converterUrl,
            string fileUrl,
            string toExt,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(converterUrl))
            {
                throw new ArgumentException("ConverterUrl is not specified", nameof(converterUrl));
            }

            var body = new Dictionary<string, string>
            {
                { "filetype", ExtractFileExtensionFromUrl(fileUrl) },
                { "outputtype", toExt },
                { "key", Guid.NewGuid().ToString("N")[..20] },
                { "url", fileUrl }
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, MediaTypeNames.Application.Json);

            HttpResponseMessage convertFileResponse = await httpClient.PostAsync(converterUrl, content, cancellationToken);

            var convertFileData = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                await convertFileResponse.Content.ReadAsStringAsync(cancellationToken));

            string convertedFileUrl = convertFileData.Get<string>("fileUrl");
            return await GetFileStreamByUrlAsync(httpClient, convertedFileUrl, cancellationToken);
        }

        /// <summary>
        /// Получает поток файла по указанному пути.
        /// </summary>
        /// <param name="httpClient">Объект, посредством которого выполняется вызов OnlyOffice.</param>
        /// <param name="fileUrl">URL к файлу.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Контент файла в виде потока.</returns>
        private static async Task<Stream> GetFileStreamByUrlAsync(
            HttpClient httpClient,
            string fileUrl,
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await httpClient.GetAsync(fileUrl, cancellationToken);
            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        /// <summary>
        /// Получает MIME-тип для указанного расширения.
        /// </summary>
        private static string GetContentType(string fileExt) =>
            string.IsNullOrWhiteSpace(fileExt)
                ? MediaTypeNames.Application.Octet
                : MimeTypeMap.GetMimeType(fileExt);

        /// <summary>
        /// Извлекает из URL-пути к файлу расширение файла без точки.
        /// </summary>
        private static string ExtractFileExtensionFromUrl(string fileUrl) =>
            FileExtensionWithoutDot(fileUrl[..fileUrl.IndexOf('?', StringComparison.Ordinal)]);

        /// <summary>
        /// Извлекает из полного имени файла расширение файла без точки.
        /// </summary>
        private static string FileExtensionWithoutDot(string fileName) =>
            Path.GetExtension(fileName)[1..];

        #endregion

        #region Controller Methods

        /// <summary>
        /// Возвращает содержимое документа-шаблона в папке <c>wwwroot/templates</c> по указанному имени.
        /// Обычно это шаблоны пустых документов: <c>empty.docx, empty.xlsx, empty.pptx</c>.
        /// Возвращает код ошибки <c>404</c>, если файл не найден.
        /// </summary>
        /// <param name="name">Имя файла шаблона.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Содержимое файла шаблона.</returns>
        // GET api/v1/onlyoffice/templates/{name}
        [HttpGet("templates/{name}"), SessionMethod]
        [Produces(MediaTypeNames.Application.Octet, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTemplate(
            [FromRoute] string name,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNullOrWhiteSpace(name, nameof(name));

            if (name.Contains('/', StringComparison.Ordinal)
                || name.Contains('\\', StringComparison.Ordinal))
            {
                // относительные пути недопустимы
                return this.Forbid();
            }

            string path = Path.Combine(this.environment.WebRootPath, "templates", name);
            if (!System.IO.File.Exists(path))
            {
                return this.NotFound();
            }

            FileStream stream = null;

            try
            {
                stream = FileHelper.OpenRead(path);
                var result = new FileStreamResult(stream, MediaTypeNames.Application.Octet);

                stream = null;
                return result;
            }
            finally
            {
                if (stream is not null)
                {
                    await stream.DisposeAsync();
                }
            }
        }


        /// <summary>
        /// Возвращает состояние указанного файла, который был открыт на редактирование.
        /// Если свойство <c>hasChangesAfterClose</c> в возвращаемом значении отлично от <c>null</c>,
        /// то посредством запроса к <c>files/{id}</c> можно получить актуальное содержимое файла;
        /// в противном случае используйте запрос к <c>files/{id}/editor</c> для актуального содержимого в процессе редактирования.
        /// </summary>
        /// <param name="id">Идентификатор редактируемого файла.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>
        /// <para>JSON-объект вида <c>{ hasChangesAfterClose: false | true | null }</c>.</para>
        /// <para><c>null</c> - редактор пока не закрыт.</para>
        /// <para><c>true</c> - редактор был закрыт, а в файле, который можно получить через <see cref="GetFinalFile"/>, имеются изменения.</para>
        /// <para><c>false</c> - редактор был закрыт, но в файле нет никаких изменений.</para>
        /// </returns>
        // GET api/v1/onlyoffice/files/{id}/state
        [HttpGet("files/{id:guid}/state"), SessionMethod]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<JsonResult> GetFileState(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            IOnlyOfficeFileCacheInfo? fileInfo = await this.fileCacheInfoStrategy.TryGetInfoAsync(id, cancellationToken);
            if (fileInfo is null)
            {
                throw new InvalidOperationException("Specified file isn't found.");
            }

            if (!fileInfo.EditorWasOpen)
            {
                throw new InvalidOperationException("Editor wasn't opened yet. Can't resolve content.");
            }

            return this.Json(new
            {
                hasChangesAfterClose = fileInfo.HasChangesAfterClose
            });
        }

        /// <summary>
        /// Возвращает содержимое редактируемого файла, используемое для сервера документов OnlyOffice.
        /// </summary>
        /// <param name="id">Идентификатор редактируемого файла.</param>
        /// <param name="token">Токен прав доступа для текущей сессии.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Содержимое редактируемого файла.</returns>
        /// <exception cref="InvalidOperationException">Файл не найден.</exception>
        // GET api/v1/onlyoffice/files/{id}/editor/?token=...
        [HttpGet("files/{id:guid}/editor"), SessionMethod]
        [Produces(MediaTypeNames.Application.Octet, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<FileStreamResult> GetEditableFile(
            [FromRoute] Guid id,
            [FromQuery, SessionToken] string token,
            CancellationToken cancellationToken = default)
        {
            IDbScopeInstance dbScopeInstance = null;
            Stream stream = null;

            // если у нас уже есть отредактированный файл этим пользователем, возвращаем его
            try
            {
                dbScopeInstance = this.dbScope.Create();

                IOnlyOfficeFileCacheInfo? fileInfo = await this.fileCacheInfoStrategy.TryGetInfoAsync(id, cancellationToken);
                if (fileInfo is null)
                {
                    throw new InvalidOperationException("Specified file isn't found.");
                }

                string contentType;
                if (fileInfo.ModifiedFileUrl is not null)
                {
                    await dbScopeInstance.DisposeAsync();
                    dbScopeInstance = null;

                    (Stream modifiedFileStream, string fileName) =
                        await this.GetModifiedFileFromOnlyOfficeAsync(fileInfo, false, cancellationToken);

                    stream = modifiedFileStream;
                    contentType = GetContentType(Path.GetExtension(fileName));
                }
                else
                {
                    // это обращение к fileCache будет на том же соединении dbScope, что и вышележащее
                    (ValidationResult result, Func<CancellationToken, ValueTask<Stream>>? getContentStreamFunc, long size) =
                        await this.fileCache.GetContentAsync(id, cancellationToken);

                    if (getContentStreamFunc is null || !result.IsSuccessful)
                    {
                        throw new ValidationException(result);
                    }

                    stream = await getContentStreamFunc(cancellationToken);
                    contentType = GetContentType(Path.GetExtension(fileInfo.SourceFileName));

                    this.Response.ContentLength = size;
                }

                var streamResult = new FileStreamResult(stream, contentType);

                stream = null;
                return streamResult;
            }
            finally
            {
                if (stream is not null)
                {
                    await stream.DisposeAsync();
                }

                if (dbScopeInstance is not null)
                {
                    await dbScopeInstance.DisposeAsync();
                }
            }
        }


        /// <summary>
        /// Возвращает содержимое файла, доступное после завершения редактирования на сервере документов OnlyOffice.
        /// </summary>
        /// <param name="id">Идентификатор редактируемого файла.</param>
        /// <param name="originalFormat">Признак того, что следует произвести конвертацию в исходный формат файла.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Содержимое редактируемого файла.</returns>
        /// <exception cref="InvalidOperationException">Файл не найден.</exception>
        // GET api/v1/onlyoffice/files/{id}/?original-format=false
        [HttpGet("files/{id:guid}"), SessionMethod]
        [Produces(MediaTypeNames.Application.Octet, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<FileStreamResult> GetFinalFile(
            [FromRoute] Guid id,
            [FromQuery(Name = "original-format")] bool originalFormat = false,
            CancellationToken cancellationToken = default)
        {
            IOnlyOfficeFileCacheInfo? fileInfo = await this.fileCacheInfoStrategy.TryGetInfoAsync(id, cancellationToken);
            if (fileInfo is null)
            {
                throw new InvalidOperationException("Specified file isn't found.");
            }

            string contentType = GetContentType(Path.GetExtension(fileInfo.SourceFileName));
            (Stream stream, string fileName) =
                await this.GetModifiedFileFromOnlyOfficeAsync(fileInfo, originalFormat, cancellationToken);

            try
            {
                // здесь исключение маловероятно, но всё же
                this.Response.Headers.Add(
                    "Content-Disposition",
                    "attachment; filename*=UTF-8''" + HttpUtility.UrlPathEncode(fileName)
                        .Replace(",", "%2C", StringComparison.Ordinal));

                var result = new FileStreamResult(stream, contentType);

                stream = null;
                return result;
            }
            finally
            {
                if (stream is not null)
                {
                    await stream.DisposeAsync();
                }
            }
        }


        /// <summary>
        /// Добавляет содержимое файла в кэш, используемый для сервера документов OnlyOffice.
        /// Содержимое файла должно находиться в теле запроса. Пустое тело соответствует пустому файлу.
        /// Связывается с версией файла в карточке в параметре <c>versionId</c>.
        /// Вызовите удаление документа запросом <c>DELETE files/{id}</c> по завершению работы с ним, в т.ч. при закрытии вкладки или приложения.
        /// </summary>
        /// <param name="id">
        /// Идентификатор редактируемого файла, по которому его содержимое будет доступно в методах этого API.
        /// Укажите здесь уникальный идентификатор для каждого вызова, связанного с открытием версии файла на редактирование.
        /// </param>
        /// <param name="versionId">Идентификатор версии файла в карточке.</param>
        /// <param name="name">Имя файла в карточке, включая его расширение.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Результат запроса.</returns>
        // POST api/v1/onlyoffice/files/{id}/?versionId=...&name=...
        [HttpPost("files/{id:guid}/create"), DisableRequestSizeLimit, SessionMethod]
        [Consumes(MediaTypeNames.Application.Octet), Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CreateFile(
            [FromRoute] Guid id,
            [FromQuery] Guid versionId,
            [FromQuery] string name,
            CancellationToken cancellationToken = default)
        {
            // берем поток напрямую из запроса, а не параметром имеющим [FromBody],
            // т.к. биндинг не позволяет слать пустое тело, коим может являться пустой текстовый файл.
            Stream contentStream = this.Request.Body;

            try
            {
                ValidationResult result = await this.fileCache.CreateAsync(id, versionId, name, contentStream, cancellationToken);
                if (!result.IsSuccessful)
                {
                    throw new ValidationException(result);
                }
            }
            finally
            {
                // чтобы не упасть с ошибкой из-за непрочитанного тела запроса,
                // если в процессе создания файла в кэше возникло исключение
                await contentStream.DrainAsync(cancellationToken);
            }

            return this.NoContent();
        }


        /// <summary>
        /// Обрабатывает обратный вызов от сервера документов по запросам, связанным с сохранением файла и закрытием документа.
        /// В свойстве <c>error</c> результата запроса содержится числовой код ошибки.
        /// </summary>
        /// <param name="id">Идентификатор редактируемого файла.</param>
        /// <param name="token">Токен прав доступа для текущей сессии.</param>
        /// <param name="data">Объект с параметрами обратного вызова, определяемыми сервером документов OnlyOffice.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Результат запроса. В свойстве <c>error</c> содержится числовой код ошибки.</returns>
        // POST api/v1/onlyoffice/files/{id}/callback/?token=...
        [HttpPost("files/{id:guid}/callback"), SessionMethod]
        [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<JsonResult> Callback(
            [FromRoute] Guid id,
            [FromQuery, SessionToken] string token,
            [FromBody] IDictionary<string, object> data,
            CancellationToken cancellationToken = default)
        {
            var status = data.Get<long>("status");
            switch (status)
            {
                // received every user connection to or disconnection from document co-editing
                case 1:
                    await this.fileCacheInfoStrategy.UpdateInfoOnEditorOpenedAsync(id, cancellationToken);
                    break;

                // document is ready for saving
                case 2:
                    var newFileUrlAfterClose = data.Get<string>("url");
                    await this.fileCacheInfoStrategy.UpdateInfoAsync(id, newFileUrlAfterClose, true, cancellationToken);
                    break;

                // document is being edited, but the current document state is saved
                case 6:
                    var newFileUrlAfterForceSave = data.Get<string>("url");
                    await this.fileCacheInfoStrategy.UpdateInfoAsync(id, newFileUrlAfterForceSave, null, cancellationToken);
                    break;

                // document saving error has occurred
                case 3:
                    // do nothing
                    break;

                // document is closed with no changes
                case 4:
                    await this.fileCacheInfoStrategy.UpdateInfoAsync(id, null, false, cancellationToken);
                    break;

                // error has occurred while force saving the document
                case 7:
                    // do nothing
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }

            return this.Json(new { error = 0 });
        }


        /// <summary>
        /// Удаляет файл из кэша, используемого для взаимодействия с сервером документов OnlyOffice.
        /// </summary>
        /// <param name="id">Идентификатор редактируемого файла.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Результат запроса.</returns>
        // DELETE api/v1/onlyoffice/files/{id}
        [HttpDelete("files/{id:guid}"), SessionMethod]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var _ = this.dbScope.Create();

            IOnlyOfficeFileCacheInfo? fileInfo = null;

            bool canDelete = this.session.User.IsAdministrator();
            if (!canDelete)
            {
                // если это не администратор, разрешаем удалять только автору записи
                fileInfo = await this.fileCacheInfoStrategy.TryGetInfoAsync(id, cancellationToken);
                if (fileInfo is null)
                {
                    return this.NoContent();
                }

                canDelete = this.session.User.ID == fileInfo.CreatedByID;
            }

            if (!canDelete)
            {
                return this.Forbid();
            }

            ValidationResult result = await this.fileCache.DeleteAsync(id, fileInfo?.SourceFileVersionID, cancellationToken);
            if (!result.IsSuccessful)
            {
                throw new ValidationException(result);
            }

            return this.NoContent();
        }

        /// <summary>
        /// Удаляет файл из кэша, используемого для взаимодействия с сервером документов OnlyOffice.
        /// </summary>
        /// <remarks>
        /// POST-версия запроса на удаление, которая может быть использована для поддержки браузерного механизма <c>Navigator.sendBeacon</c>,
        /// который поддерживает только POST, и с помощью которого можно гарантированно выполнить запрос перед закрытием страницы.
        /// Во всех остальных случаях используйте запрос <c>DELETE files/{id}</c> <see cref="Delete"/>.
        /// </remarks>
        /// <param name="id">Идентификатор редактируемого файла.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Результат запроса.</returns>
        // POST api/v1/onlyoffice/files/{id}/delete
        [HttpPost("files/{id:guid}/delete"), SessionMethod]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public Task<IActionResult> PostDelete(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default) =>
            Delete(id, cancellationToken);

        #endregion
    }
}