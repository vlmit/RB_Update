using System.Collections.Generic;
using System.Threading.Tasks;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared.Workflow.KrPermissions;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;

namespace Tessa.Extensions.Default.Server.Workflow.KrPermissions
{
    public sealed class KrGetUnavailableTypesForCreationGetExtension : CardRequestExtension
    {
        #region Fields

        private readonly IKrTypesCache typesCache;
        private readonly IKrPermissionsManager permissionsManager;
        private readonly IKrPermissionsCacheContainer permissionsCacheContainer;

        #endregion

        #region Constructors

        public KrGetUnavailableTypesForCreationGetExtension(
            IKrTypesCache typesCache,
            IKrPermissionsManager permissionsManager,
            IKrPermissionsCacheContainer permissionsCacheContainer)
        {
            this.typesCache = typesCache;
            this.permissionsManager = permissionsManager;
            this.permissionsCacheContainer = permissionsCacheContainer;
        }

        #endregion

        #region Base Overrides

        public override async Task AfterRequest(ICardRequestExtensionContext context)
        {
            var cardTypes = await this.typesCache.GetCardTypesAsync(context.CancellationToken);
            var docTypes = await this.typesCache.GetDocTypesAsync(context.CancellationToken);

            var permissionsCache = await this.permissionsCacheContainer.TryGetCacheAsync(context.Response.ValidationResult, context.CancellationToken);
            var getCacheSuccessful = context.Response.ValidationResult.IsSuccessful();

            List<object> unavaibleTypes = new List<object>();
            await using (context.DbScope.Create())
            {
                foreach (var cardType in cardTypes)
                {
                    if (!cardType.UseDocTypes)
                    {
                        if (getCacheSuccessful)
                        {
                            var permContext = await this.permissionsManager.TryCreateContextAsync(
                                new KrPermissionsCreateContextParams
                                {
                                    CardTypeID = cardType.ID,
                                    AdditionalInfo = context.Info,
                                    ExtensionContext = context,
                                    ServerToken = context.Info.TryGetServerToken(),
                                    PermissionsCache = permissionsCache,
                                },
                                cancellationToken: context.CancellationToken);

                            if (permContext is null
                                || !await this.permissionsManager.CheckRequiredPermissionsAsync(
                                    permContext,
                                    KrPermissionFlagDescriptors.CreateCard))
                            {
                                unavaibleTypes.Add(cardType.ID);
                            }
                        }
                        else
                        {
                            unavaibleTypes.Add(cardType.ID);
                        }
                    }
                }

                foreach (var docType in docTypes)
                {
                    if (getCacheSuccessful)
                    {
                        var permContext = await this.permissionsManager.TryCreateContextAsync(
                            new KrPermissionsCreateContextParams
                            {
                                CardTypeID = docType.CardTypeID,
                                DocTypeID = docType.ID,
                                AdditionalInfo = context.Info,
                                ExtensionContext = context,
                                ServerToken = context.Info.TryGetServerToken(),
                                PermissionsCache = permissionsCache,
                            },
                            cancellationToken: context.CancellationToken);

                        if (permContext is null
                            || !await this.permissionsManager.CheckRequiredPermissionsAsync(
                                permContext,
                                KrPermissionFlagDescriptors.CreateCard))
                        {
                            unavaibleTypes.Add(docType.ID);
                        }
                    }
                    else
                    {
                        unavaibleTypes.Add(docType.ID);
                    }
                }
            }

            context.Response.Info.Add(KrPermissionsHelper.UnavaliableTypesKey, unavaibleTypes.ToArray());
        }

        #endregion
    }
}
