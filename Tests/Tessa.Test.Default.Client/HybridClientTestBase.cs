using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Tessa.Platform;
using Tessa.Platform.Runtime;
using Tessa.Test.Default.Client.Web;
using Tessa.Test.Default.Shared;
using Tessa.Test.Default.Shared.Web;
using Tessa.Web;
using Tessa.Web.Services;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace Tessa.Test.Default.Client
{
    /// <summary>
    /// Абстрактный базовый класс, предоставляющий методы для выполнения клиентских тестов 
    /// без поддержки пользовательского интерфейса на специально подготовленном сервере приложений.
    /// </summary>
    public abstract class HybridClientTestBase :
        ClientTestBase
    {
        #region Constants And Static Fields

        private static readonly AsyncSynchronizedOneTimeRegistrator initializeWebServerRegistrator =
            new AsyncSynchronizedOneTimeRegistrator(() => WebHelper.InitializeWebServerAsync());

        /// <summary>
        /// Базовый адрес сервера приложений по умолчанию.
        /// </summary>
        public const string DefaultBaseAddress = "http://localhost/";

        #endregion

        #region Properties

        /// <summary>
        /// Возвращает фабрику, предназначенную для создания объектов, с помощью которых можно реализовать функциональные тесты для web-приложений.
        /// </summary>
        public IWebApplicationFactory WebApplicationFactory { get; private set; }

        /// <summary>
        /// Возвращает значение, показывающее, необходимо ли в качестве источника файлов по умолчанию использовать базу данных или нет.
        /// </summary>
        protected virtual bool UseDatabaseAsDefault { get; }

        /// <summary>
        /// Возвращает Unity-контейнер, используемый на сервере.
        /// </summary>
        /// <remarks>
        /// Не используйте значение этого свойства для регистрации зависимостей. Для регистрации зависимостей, используемых на сервере, необходимо переопределить метод <see cref="InitializeContainerServerAsync(IUnityContainer, IWebContextAccessor)"/>. Для изменения создаваемого серверного контейнера необходимо переопределить метод <see cref="CreateContainerServerAsync()"/>.
        /// </remarks>
        public IUnityContainer UnityContainerServer
        {
            get
            {
                return this.WebApplicationFactory is null
                    ? throw new InvalidOperationException($"{nameof(HybridClientTestBase)}.{nameof(this.WebApplicationFactory)} is not initialized.")
                    : this.WebApplicationFactory.Server.Services.GetRequiredService<IContainerProvider>()
                        .GetContainerAsync(RuntimeHelper.DefaultInstanceName).GetAwaiter().GetResult();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="HybridClientTestBase"/>.
        /// </summary>
        protected HybridClientTestBase()
        {
            this.PlannedInitializeTestServer();
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        protected override string BaseAddressOverride => DefaultBaseAddress;

        /// <inheritdoc/>
        protected override string UserNameOverride => "admin";

        /// <inheritdoc/>
        protected override string PasswordOverride => "admin";

        /// <inheritdoc/>
        protected override async ValueTask InitializeContainerAsync(IUnityContainer container)
        {
            await base.InitializeContainerAsync(container);

            var applicationFolders = container.Resolve<IApplicationFolders>();
            applicationFolders.LocalData = Path.Combine(await this.GetFileStoragePathAsync(), "local_data");
            applicationFolders.RoamingData = Path.Combine(await this.GetFileStoragePathAsync(), "roaming_data");
        }

        /// <inheritdoc/>
        protected override void BeforeRegisterExtensionsOnClient(IUnityContainer unityContainer)
        {
            base.BeforeRegisterExtensionsOnClient(unityContainer);

            unityContainer
                .RegisterType<IHttpClientFactory, TestServerHttpClientFactory>(
                    new ContainerControlledLifetimeManager(),
                    new InjectionConstructor(
                        new InjectionParameter<IWebApplicationFactory>(this.WebApplicationFactory)));
        }

        /// <inheritdoc/>
        protected override async ValueTask<string> GetBaseAddressAsync()
        {
            var address = await base.GetBaseAddressAsync();

            if (!this.GetType().IsDefined(typeof(SetupTempDbAttribute), true))
            {
                return address;
            }

            if (string.IsNullOrEmpty(address))
            {
                address = DefaultBaseAddress;
            }

            var builder = new UriBuilder(address);
            builder.Host += "_" + (await this.GetFixtureDateTimeAsync()).FormatDateTimeCode() + "_" + (await this.GetFixtureNameAsync());
            return builder.ToString();
        }

        /// <inheritdoc/>
        protected override async Task OneTimeTearDownCoreAsync()
        {
            if (this.WebApplicationFactory is not null)
            {
                await this.WebApplicationFactory.Host.StopAsync();
                await this.WebApplicationFactory.DisposeAsync();
            }

            await base.OneTimeTearDownCoreAsync();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Создаёт Unity-контейнер, используемый на сервере.
        /// </summary>
        /// <param name="instanceName">Имя экземпляра сервера. Может быть равно пустой строке или значению <see langword="null"/>, если используется имя по умолчанию.</param>
        /// <param name="multipleInstances">Признак того, что активен режим работы с несколькими экземплярами сервера. При этом в запросах к серверу обязательно передаётся <c>InstanceName</c>, в т.ч. для ссылок на web-клиент.</param>
        /// <param name="serverRootPath">Путь к папке с конфигурационным файлами, или <see langword="null"/>, если путь определяется по умолчанию как значение свойства <see cref="RuntimeHelper.ConfigRootPath"/>.</param>
        /// <returns>Созданный Unity-контейнер.</returns>
        protected virtual async ValueTask<IUnityContainer> CreateContainerServerAsync(
            string instanceName,
            bool multipleInstances,
            string serverRootPath,
            IWebContextAccessor webContextAccessor)
        {
            var container = await this.CreateContainerServerAsync();
            await this.InitializeContainerServerAsync(container, webContextAccessor);
            return container;
        }

        /// <summary>
        /// Создаёт серверный Unity контейнер.
        /// </summary>
        /// <returns>Созданный серверный Unity контейнер.</returns>
        protected virtual ValueTask<IUnityContainer> CreateContainerServerAsync() =>
            new(new UnityContainer());

        /// <summary>
        /// Инициализирует серверный Unity контейнер.
        /// </summary>
        /// <param name="container">Инициализируемый серверный Unity контейнер.</param>
        /// <returns>Асинхронная задача.</returns>
        protected virtual async ValueTask InitializeContainerServerAsync(
            IUnityContainer container,
            IWebContextAccessor webContextAccessor)
        {
            var fileSourceSettings = await this.CreateDefaultFileSourceSettingsAsync(
                randomizeFileBasePath: true,
                useDatabaseAsDefault: this.UseDatabaseAsDefault);

            TestHelper.InitializeServerContainer(
                container,
                createDbManagerFunc: this.DbFactory is null ? null : this.DbFactory.Create,
                dbScope: this.DbScope,
                tryGetTokenFunc: () => webContextAccessor.TryGetWebContext()?.TryGetSessionToken(),
                fileSourceSettings: fileSourceSettings,
                beforeRegisterExtensionsOnServerAction: this.BeforeRegisterExtensionsOnServer,
                beforeFinalizeServerRegistrationAction: this.BeforeFinalizeServerRegistration);
        }

        /// <summary>
        /// Выполняет действия перед поиском и выполнением серверных регистраторов расширений в папке приложения.
        /// </summary>
        /// <param name="unityContainer">Unity-контейнер.</param>
        protected virtual void BeforeRegisterExtensionsOnServer(IUnityContainer unityContainer)
        {

        }

        /// <summary>
        /// Выполняет действия перед завершением регистрации сервера приложений.
        /// </summary>
        /// <param name="unityContainer">Unity-контейнер.</param>
        protected virtual void BeforeFinalizeServerRegistration(IUnityContainer unityContainer)
        {

        }

        #endregion

        #region Private Methods

        private void PlannedInitializeTestServer()
        {
            this.GetTestActions(ActionStage.BeforeInitialize).Add(
                new TestAction(
                    this,
                    static async sender =>
                    {
                        await initializeWebServerRegistrator.RegisterAsync();

                        var senderT = (HybridClientTestBase) sender;

                        var factory = new TessaWebApplicationFactory(senderT.CreateContainerServerAsync);
                        factory.InitializeAndStart();
                        senderT.WebApplicationFactory = factory;
                    }));
        }

        #endregion
    }
}
