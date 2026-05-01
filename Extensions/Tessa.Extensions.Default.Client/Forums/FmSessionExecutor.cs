using System;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Forums.ForumCached;
using Tessa.Forums.Notifications;
using Tessa.Platform.Runtime;
using Tessa.UI.Files;
using Unity;
using Unity.Lifetime;

namespace Tessa.Extensions.Default.Client.Forums
{
    public sealed class FmSessionExecutor
    {
        #region Constructors

        public FmSessionExecutor(ISessionManager sessionManager, IConnectionSettings connectionSettings)
        {
            this.sessionManager = sessionManager;

            this.unityContainerLazy = new Lazy<IUnityContainer>(
                () => new UnityContainer()
                .RegisterInstance(connectionSettings)
                .RegisterSessionsOnClient()
                .RegisterCardsOnClient()
                .RegisterExtensionContainers()
                .RegisterFilesOnClient()
                .RegisterType<IFmNotificationProvider, FmNotificationProvider>(new ContainerControlledLifetimeManager())
                .RegisterType<IForumClientCachedDataManager, ForumClientCachedDataManager>(new ContainerControlledLifetimeManager())
                .RegisterType<IForumCachedData, ForumCachedData>(new ContainerControlledLifetimeManager())
                .FinalizeSessionsOnClient(),
                LazyThreadSafetyMode.PublicationOnly);
        }

        #endregion

        #region Fields

        private readonly ISessionManager sessionManager;

        private readonly Lazy<IUnityContainer> unityContainerLazy;

        #endregion

        #region Methods

        public async Task<T> TryExecuteInSeparateSessionAsync<T>(Func<IFmNotificationProvider, CancellationToken, Task<T>> funcAsync, Guid? applicationID, CancellationToken cancellationToken = default)
        {
            IUnityContainer container = this.unityContainerLazy.Value;

            var notificationProvider = container.Resolve<IFmNotificationProvider>();
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
                return await funcAsync(notificationProvider, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (this.sessionManager.IsOpened)
                {
                    await newSessionManager.CloseAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        #endregion
    }
}