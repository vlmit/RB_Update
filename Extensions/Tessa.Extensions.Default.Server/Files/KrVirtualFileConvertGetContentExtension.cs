using System;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.FileConverters;
using Tessa.Platform.IO;

namespace Tessa.Extensions.Default.Server.Files
{
    /// <summary>
    /// Расширение на конвертацию виртуальных файлов из справочника "Виртуальных файлов" при запросе с конвертацией (например, предпросмотр в ЛК).
    /// Своё расширение необходимо, т.к. конвертация виртуальных файлов данного типа должна происходить без долговременного кеширования.
    /// </summary>
    public sealed class KrVirtualFileConvertGetContentExtension : CardGetFileContentExtension
    {
        #region Fields

        private readonly IFileConverter fileConverter;

        #endregion

        #region Constructors

        public KrVirtualFileConvertGetContentExtension(IFileConverter fileConverter)
        {
            this.fileConverter = fileConverter;
        }

        #endregion

        #region Base Overrides

        public override async Task BeforeRequest(ICardGetFileContentExtensionContext context)
        {
            CardGetFileContentRequest request = context.Request;
            FileConverterFormat? converterFormat = request.TryGetConverterFormat();

            Guid? cardID;
            Guid? fileID;
            Guid? versionID;
            string fileName;

            if (context.Response != null
                || !converterFormat.HasValue
                || converterFormat.Value == FileConverterFormat.Unknown
                || !(cardID = request.CardID).HasValue
                || !(fileID = request.FileID).HasValue
                || !(versionID = request.VersionRowID).HasValue
                || string.IsNullOrEmpty(fileName = request.FileName))
            {
                return;
            }

            var converterRequest = new FileConverterRequest
            {
                EventName = FileConverterEventNames.ClientPreview,
                OutputFormat = converterFormat.Value,
                FileName = fileName,
                CardID = cardID.Value,
                FileID = fileID.Value,
                VersionID = versionID.Value,
                FileTypeID = request.FileTypeID,
                FileTypeName = request.FileTypeName,
                Flags = FileConverterRequestFlags.IgnoreCacheBeforeConversion,
            };

            converterRequest.Parameters["KrVirtualFileSpecialID"] = Guid.NewGuid();
            converterRequest.FileRequestInfo["UserID"] = context.Session.User.ID;
            converterRequest.FileRequestInfo["UserName"] = context.Session.User.Name;
            converterRequest.FileRequestInfo["ClientUtcOffsetTicks"] = context.Session.ClientUtcOffset.Ticks;
            converterRequest.FileRequestInfo["ClientCultureLCID"] = context.Session.ClientCulture.LCID;
            converterRequest.FileRequestInfo["ClientUICultureLCID"] = context.Session.ClientUICulture.LCID;

            IFileConverterResponse converterResponse = await this.fileConverter.ConvertFileAsync(converterRequest, context.CancellationToken);
            bool hasContent = converterResponse.ValidationResult.IsSuccessful;

            var response = new CardGetFileContentResponse { HasContent = hasContent };
            response.ValidationResult.Add(converterResponse.ValidationResult);
            context.Response = response;

            if (hasContent)
            {
                string extension = "." + converterFormat.Value.GetExtension();
                if (extension.Length <= 1)
                {
                    extension = FileHelper.GetExtension(fileName);
                }

                string suggestedFileName =
                    FileHelper.RemoveInvalidFileNameChars(
                        FileHelper.GetFileNameWithoutExtension(fileName, true),
                        FileHelper.InvalidCharReplacement)
                    + extension;

                response.SetSuggestedFileName(suggestedFileName);
                response.Size = converterResponse.Size;

                context.ContentFuncAsync = converterResponse.GetStreamOrThrowAsync;
            }
        }

        #endregion
    }
}
