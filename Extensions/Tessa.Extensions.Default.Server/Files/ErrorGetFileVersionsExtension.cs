using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Server.Workflow.KrPermissions;
using Tessa.Extensions.Default.Shared.Workflow.KrPermissions;
using Tessa.Localization;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Server.Files
{
    /// <summary>
    /// Расширение на загрузку версий виртуальных файлов из справочника ошибок.
    /// </summary>
    public sealed class ErrorGetFileVersionsExtension :
        CardGetFileVersionsExtension
    {

        #region Base Overrides

        public override Task AfterRequest(ICardGetFileVersionsExtensionContext context)
        {
            ListStorage<CardFileVersion> fileVersions;
            if (context.RequestIsSuccessful
                && (fileVersions = context.Response.TryGetFileVersions()) is not null)
            {
                foreach (var fileVersion in fileVersions)
                {
                    fileVersion.Name = LocalizationManager.Format(fileVersion.Name);
                }
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}