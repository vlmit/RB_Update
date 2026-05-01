using System;
using System.Threading.Tasks;
using NLog;
using Tessa.Platform;
using Tessa.Test.Default.Shared.Kr;
using Unity;

namespace Tessa.Test.Default.Client
{
    /// <summary>
    /// Область действия сессии.
    /// </summary>
    public sealed class TestSessionScope :
        IAsyncDisposable
    {
        #region Constants And Static Fields

        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        private readonly IUnityContainer outerUnityContainer;

        #endregion

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр сервера <see cref="TestSessionScope"/>.
        /// </summary>
        /// <param name="outerUnityContainer">Unity контейнер внешней сессии.</param>
        public TestSessionScope(
            IUnityContainer outerUnityContainer)
        {
            this.outerUnityContainer = outerUnityContainer ?? throw new ArgumentNullException(nameof(outerUnityContainer));
        }

        #endregion

        #region IAsyncDisposable Members

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            var currentUnityContainer = KrTestContext.CurrentContext.UnityContainer;

            if (currentUnityContainer is not null)
            {
                try
                {
                    await currentUnityContainer
                        .Resolve<ITestSessionManager>()
                        .CloseAsync();
                }
                catch (OperationCanceledException)
                {
                    // ignored
                }
                catch (Exception ex)
                {
                    logger.LogException(ex);
                }
                finally
                {
                    await currentUnityContainer.DisposeAllRegistrationsAsync();
                    currentUnityContainer.Dispose();
                }
            }

            KrTestContext.CurrentContext.UnityContainer = this.outerUnityContainer;
        }

        #endregion
    }
}
