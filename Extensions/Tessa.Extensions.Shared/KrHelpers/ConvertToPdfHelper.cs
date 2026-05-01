using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tessa.FileConverters;

namespace Tessa.Extensions.Shared.KrHelpers
{
    public static class ConvertToPdfHelper
    {
        private static readonly string[] validExtensions =
        {
            ".PDF",
            ".DOC",
            ".DOCX",
            ".RTF",
            ".XLS",
            ".XLSX",
            ".PPTX",
            ".PPT",
            ".RTF"
        };

        public static bool ValidExtension(string extension) => validExtensions.Contains(extension.ToUpperInvariant());

        /// <summary>
        /// отправляем запрос на конвертацию файла
        /// </summary>
        /// <param name="fileConverter">IFileConverter</param>
        /// <param name="cardID">id карточки</param>
        /// <param name="versionID">id версии файла</param>
        public static async Task ConvertToPdfAsync(IFileConverter fileConverter, Guid cardID, Guid versionID)
        {
            var request = await fileConverter.GetRequestOrThrowAsync(FileConverterEventNames.Unknown, FileConverterFormat.Pdf, versionID,
                new Dictionary<string, object> { { "ConvertBeforeSign", cardID } });

            request.Flags |= FileConverterRequestFlags.DoNotAwaitResult | FileConverterRequestFlags.DoNotCacheResult | FileConverterRequestFlags.WithoutResponse;

            await fileConverter.ConvertFileAsync(request);
        }
    }
}