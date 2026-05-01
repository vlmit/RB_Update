using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;

namespace Tessa.Extensions.Default.Server.Workflow.KrPermissions
{
    public sealed class KrCardTaskTypePermissionFilterPolicy : IFilterPolicy
    {
        public static KrCardTaskTypePermissionFilterPolicy Instance { get; } = new KrCardTaskTypePermissionFilterPolicy();

        private KrCardTaskTypePermissionFilterPolicy()
        {
        }

        /// <doc path='info[@type="IFilterPolicy" and @item="GetFilterContextAsync"]'/>
        public ValueTask<object> GetFilterContextAsync(
            ExtensionBuildKey buildKey,
            ExtensionExecutionKey executionKey,
            IExtensionPolicyContainer policies,
            object extensionContext) =>
            new ValueTask<object>(extensionContext is ITaskPermissionsExtensionContext context ? context.TaskType : null);

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
            var policy = policies.TryResolve<ICardTaskTypePolicy>();
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