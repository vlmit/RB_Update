using System;
using System.Threading.Tasks;
using Tessa.Platform;
using Tessa.Web.Services;
using Unity;

namespace Tessa.Test.Default.Client
{
    /// <summary>
    /// Предоставляет методы для создания Unity-контейнера используемого на сервере в тестах с настраиваемым сервером приложений.
    /// </summary>
    public sealed class TestContainerProvider :
        ContainerProvider
    {
        #region Fields

        private readonly Func<string, bool, string, IWebContextAccessor, ValueTask<IUnityContainer>> createContainerFuncAsync;
        private readonly IWebContextAccessor webContextAccessor;

        #endregion

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TestContainerProvider"/>.
        /// </summary>
        /// <param name="configurationManager">Объект, управляющий конфигурацией приложений Tessa.</param>
        /// <param name="serviceProvider">Предоставляет методы для получения зависимостей.</param>
        /// <param name="createContainerFunc">Метод создающий серверный контейнер. Параметры соответствуют методу <see cref="IContainerProvider.GetContainer(string, bool, string)"/>.</param>
        /// <param name="backgroundServiceQueue">Очередь действий для асинхронной обработки в фоновом режиме веб-сервером.</param>
        /// <param name="webContextAccessor">Объект, который предоставляет доступ к текущему объекту контекста обработки веб-запроса.</param>
        public TestContainerProvider(
            IConfigurationManager configurationManager,
            IServiceProvider serviceProvider,
            Func<string, bool, string, IWebContextAccessor, ValueTask<IUnityContainer>> createContainerFunc,
            IWebBackgroundServiceQueue backgroundServiceQueue,
            IWebContextAccessor webContextAccessor)
            : base(configurationManager, serviceProvider, backgroundServiceQueue, webContextAccessor)
        {
            Check.ArgumentNotNull(createContainerFunc, nameof(createContainerFunc));

            this.webContextAccessor = webContextAccessor;
            this.createContainerFuncAsync = createContainerFunc;
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        protected override ValueTask<IUnityContainer> CreateContainerAsync(string instanceName, bool multipleInstances, string serverRootPath) => 
            this.createContainerFuncAsync(instanceName, multipleInstances, serverRootPath, this.webContextAccessor);

        #endregion
    }
}
