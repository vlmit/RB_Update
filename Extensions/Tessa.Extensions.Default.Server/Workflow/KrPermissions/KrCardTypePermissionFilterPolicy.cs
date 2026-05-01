using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;

namespace Tessa.Extensions.Default.Server.Workflow.KrPermissions
{
    public sealed class KrCardTypePermissionFilterPolicy : IFilterPolicy
    {
        public static KrCardTypePermissionFilterPolicy Instance { get; } = new KrCardTypePermissionFilterPolicy();

        private KrCardTypePermissionFilterPolicy()
        {
        }

        /// <doc path='info[@type="IFilterPolicy" and @item="GetFilterContextAsync"]'/>
        public ValueTask<object> GetFilterContextAsync(
            ExtensionBuildKey buildKey,
            ExtensionExecutionKey executionKey,
            IExtensionPolicyContainer policies,
            object extensionContext) =>
            new ValueTask<object>(extensionContext is IKrPermissionsManagerContext context ? context.CardType : null);

        /// <doc path='info[@type="IFilterPolicy" and @item="FilterAsync"]'/>
        public ValueTask<bool> FilterAsync(
            ExtensionBuildKey buildKey,
            ExtensionResolveKey resolveKey,
            ExtensionExecutionKey executionKey,
            IExtensionPolicyContainer policies,
            IExtension extension,
            object extensionContext,
            object filterContext)
        {
            var policy = policies.TryResolve<ICardTypePolicy>();
            if (policy is null || policy.IsAllowAny)
            {
                return new ValueTask<bool>(true);
            }

            if (filterContext is CardType cardType)
            {
                return new ValueTask<bool>(policy.IsAllowed(cardType));
            }

            return new ValueTask<bool>(false);
        }
    }
}