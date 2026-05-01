using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Server.Files.VirtualFiles;
using Tessa.Localization;
using Tessa.Platform.Runtime;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Server.Files
{
    /// <summary>
    /// Расширение на загрузку конткнта виртуальных файлов из справочника "Виртуальные файлы".
    /// </summary>
    public sealed class KrVirtualFileGetContentExtension :
        CardGetFileContentExtension
    {
        #region Fields

        private readonly IKrVirtualFileManager krVirtualFileManager;
        private readonly IKrVirtualFileCache krVirtualFileCache;
        private readonly ICardStreamServerRepository cardStreamRepository;

        #endregion

        #region Constructors

        public KrVirtualFileGetContentExtension(
            IKrVirtualFileManager krVirtualFileManager,
            IKrVirtualFileCache krVirtualFileCache,
            ICardStreamServerRepository cardStreamRepository)
        {
            this.krVirtualFileManager = krVirtualFileManager;
            this.krVirtualFileCache = krVirtualFileCache;
            this.cardStreamRepository = cardStreamRepository;
        }

        #endregion

        #region Base Overrides

        public override async Task BeforeRequest(ICardGetFileContentExtensionContext context)
        {
            if (context.Response is not null
                || !context.ValidationResult.IsSuccessful())
            {
                return;
            }

            var cardID = context.Request.CardID;
            var fileID = context.Request.FileID;
            var versionID = context.Request.VersionRowID;

            if (!cardID.HasValue
                || !fileID.HasValue
                || !versionID.HasValue)
            {
                return;
            }

            Guid? userID = null;
            string userName = null;
            CultureInfo clientCulture = null;
            CultureInfo clientUICulture = null;
            TimeSpan? clientUtcOffset = null;
            // Проверяем доступ к контенту файлов только при клиентских запросах
            if (context.Request.ServiceType != CardServiceType.Default)
            {
                context.ValidationResult.Add(
                    await this.krVirtualFileManager.CheckAccessForFileAsync(cardID.Value, fileID.Value, context.CancellationToken));

                if (!context.ValidationResult.IsSuccessful())
                {
                    return;
                }
            }
            else
            {
                userID = context.Request.Info.TryGet<Guid?>("UserID");
                userName = context.Request.Info.TryGet<string>("UserName");
                clientUtcOffset = context.Request.Info.TryGet<long?>("ClientUtcOffsetTicks") is long ticks
                    ? new TimeSpan(ticks)
                    : null;
                clientCulture = context.Request.Info.TryGet<int?>("ClientCultureLCID") is int lcid
                    ? CultureInfo.GetCultureInfo(lcid)
                    : null;
                clientUICulture = context.Request.Info.TryGet<int?>("ClientUICultureLCID") is int uiLcid
                    ? CultureInfo.GetCultureInfo(uiLcid)
                    : null;
            }

            var virtualFile = await this.krVirtualFileCache.TryGetAsync(fileID.Value, context.CancellationToken);
            var version = virtualFile?.Versions.FirstOrDefault(x => x.ID == versionID.Value);

            if (version is null)
            {
                return;
            }

            // Если с сервера передана информация о сотруднике или языке, дял кого генерируется файл, то используем эту информацию. 
            await using var sessionScope = userID.HasValue
                ? SessionContext.Create(
                    new SessionToken(
                        userID.Value,
                        userName,
                        serverCode: context.Session.ServerCode,
                        instanceName: context.Session.InstanceName,
                        utcOffset: clientUtcOffset,
                        timeZoneUtcOffset: clientUtcOffset,
                        seal: true))
                : null;
            using var localeScope = clientUICulture is not null && clientCulture is not null
                ? LocalizationManager.CreateScope(clientUICulture, clientCulture)
                : null;

            var result = await this.cardStreamRepository.GenerateFileFromTemplateAsync(
                version.FileTemplateID,
                cardID,
                cancellationToken: context.CancellationToken);

            result.Response.ValidationResult.Add(context.ValidationResult);
            context.Response = result.Response;

            string fileName = await this.krVirtualFileManager.GetSuggestedFileNameAsync(
                version.Name,
                cardID: context.Request.CardID,
                cancellationToken: context.CancellationToken);

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = context.Request.FileName;
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                context.Response.SetSuggestedFileName(fileName);
            }

            context.ContentFuncAsync = result.GetContentOrThrowAsync;
        }

        #endregion
    }
}
