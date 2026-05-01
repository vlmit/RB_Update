using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform;

namespace Tessa.Extensions.Default.Shared.Workflow.KrPermissions
{
    public static class KrPermissionExtensions
    {
        #region ICardExtensionContext Extensions

        public static KrToken TryGetServerToken(this IDictionary<string, object> info)
        {
            if (info.TryGetValue(KrPermissionsHelper.ServerTokenKey, out var token))
            {
                return token as KrToken;
            }

            return null;
        }

        public static KrToken GetOrCreateServerToken(this IDictionary<string, object> info)
        {
            KrToken token = info.TryGetServerToken();
            if (token == null)
            {
                token = new KrToken
                {
                    ExtendedCardSettings = new KrPermissionExtendedCardSettings(),
                };

                info[KrPermissionsHelper.ServerTokenKey] = token;
            }
            return token;
        }

        public static async Task SetCardAccessAsync(
            this ICardExtensionContext context,
            string section,
            ICollection<string> fields)
        {
            Check.ArgumentNotNull(context, nameof(context));

            var token = GetOrCreateServerToken(context.Info);
            await token.ExtendedCardSettings.SetCardAccessAsync(
                true,
                context.CardMetadata,
                section,
                fields,
                context.CancellationToken);
        }

        public static async Task SetCardAccessAsync(
            this ICardExtensionContext context,
            string section,
            params string[] fields)
        {
            await SetCardAccessAsync(context, section, (ICollection<string>)fields);
        }

        #endregion

        #region KrPermissionExtendedCardSettings Extensions

        public static async Task SetCardAccessAsync(
            this IKrPermissionExtendedCardSettings extendedCardSettings,
            bool isAllowed,
            ICardMetadata cardMetadata,
            string section,
            ICollection<string> fields,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(extendedCardSettings, nameof(extendedCardSettings));

            if ((await cardMetadata.GetSectionsAsync(cancellationToken)).TryGetValue(section, out var sectionMeta))
            {
                extendedCardSettings.SetCardAccess(
                    isAllowed,
                    sectionMeta.ID,
                    fields
                        .Select(x => sectionMeta.Columns.TryGetValue(x, out var result) ? result.ID : Guid.Empty)
                        .Where(x => x != Guid.Empty)
                        .ToArray());
            }
        }

        public static async Task SetCardAccessAsync(
            this IKrPermissionExtendedCardSettings extendedCardSettings,
            bool isAllowed,
            ICardMetadata cardMetadata,
            string section,
            CancellationToken cancellationToken = default,
            params string[] fields)
        {
            await extendedCardSettings.SetCardAccessAsync(isAllowed, cardMetadata, section, fields, cancellationToken);
        }

        #endregion
    }
}
