using System;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Platform.Runtime;
using Tessa.Views;
using Unity;
using Unity.Lifetime;

namespace Tessa.Extensions.Default.Client.Notifications
{
    public sealed class KrViewSessionExecutor
    {
        #region Constructors

        public KrViewSessionExecutor(
            ISessionManager sessionManager,
            IConnectionSettings connectionSettings)
        {
            this.sessionManager = sessionManager;

            this.unityContainerLazy = new Lazy<IUnityContainer>(
                () => new UnityContainer()
                    .RegisterInstance(connectionSettings)
                    .RegisterSessionsOnClient()
                    .RegisterType<ITessaViewService, TessaViewServiceClient>(new ContainerControlledLifetimeManager())
                    .FinalizeSessionsOnClient(),
                LazyThreadSafetyMode.PublicationOnly);
        }

        #endregion

        #region Fields

        private readonly ISessionManager sessionManager;

        private readonly Lazy<IUnityContainer> unityContainerLazy;

        #endregion

        #region Methods

        public async Task<T> TryExecuteInSeparateSessionAsync<T>(
            Func<CancellationToken, Task<T>> funcAsync,
            Guid? applicationID,
            CancellationToken cancellationToken = default)
        {
            IUnityContainer container = unityContainerLazy.Value;

            var newViewService = container.Resolve<ITessaViewService>();
            var newSessionManager = container.Resolve<ISessionManager>();
            var connectionSettings = container.Resolve<IConnectionSettings>();

            newSessionManager.ApplicationID = applicationID ?? this.sessionManager.ApplicationID;
            newSessionManager.Credentials = this.sessionManager.Credentials;

            if (!await newSessionManager.OpenAsync(
                    skipDefaultAuth: connectionSettings.SkipWinAuth,
                    cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                return default;
            }

            try
            {
                await using (TessaViewServiceContext.Create(new TessaViewServiceContext(newViewService)))
                {
                    return await funcAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                await newSessionManager.CloseAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        #endregion
    }
}
