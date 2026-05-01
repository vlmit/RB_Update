using System;
using System.Reflection;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Caching;
using Tessa.Cards.Metadata;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Runtime;
using Tessa.Test.Default.Shared.Cards;
using Tessa.Test.Default.Shared.GC;
using Tessa.Test.Default.Shared.GC.Handlers;
using Tessa.Test.Default.Shared.Kr;
using Tessa.Views;
using Tessa.Views.Json;
using Tessa.Views.Json.Converters;
using Tessa.Views.Parser;
using Tessa.Views.Parser.SyntaxTree.ExchangeFormat;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace Tessa.Test.Default.Shared
{
    /// <summary>
    /// Базовый абстрактный класс для серверных тестов.
    /// </summary>
    public abstract class ServerTestBase :
        TestBase,
        ICardTypeRepositoryContainer
    {
        #region Properties

        /// <summary>
        /// Возвращает значение, показывающее, необходимо ли в качестве источника файлов по умолчанию использовать базу данных или нет.
        /// </summary>
        protected virtual bool UseDatabaseAsDefault { get; }

        #region ICardTypeRepositoryContainer Members

        /// <inheritdoc/>
        public ICardTypeServerRepository CardTypeRepository { get; set; }

        #endregion

        #endregion

        #region Base override

        /// <inheritdoc/>
        protected override async ValueTask InitializeContainerAsync(IUnityContainer container)
        {
            await base.InitializeContainerAsync(container);

            var fileSourceSettings = await this.CreateDefaultFileSourceSettingsAsync(
                randomizeFileBasePath: true,
                useDatabaseAsDefault: this.UseDatabaseAsDefault);

            var token = new SessionToken(
                 Tessa.Platform.Runtime.Session.SystemID,
                 Tessa.Platform.Runtime.Session.SystemName,
                 seal: true);

            TestHelper.InitializeServerContainer(
                container,
                this.DbFactory is null ? null : this.DbFactory.Create,
                this.DbScope,
                () => token,
                fileSourceSettings,
                this.BeforeRegisterExtensionsOnServer,
                this.BeforeFinalizeServerRegistration);

            container.RegisterType<ICardLifecycleCompanionRequestExtender, CardLifecycleCompanionDefaultRequestExtender>(new ContainerControlledLifetimeManager());
            container.RegisterType<ICardLifecycleCompanionDependencies, CardLifecycleCompanionDependencies>(
                new TransientLifetimeManager(),
                new InjectionConstructor(
                    typeof(ICardRepository),
                    typeof(ICardMetadata),
                    typeof(Func<ICardFileManager>),
                    typeof(Func<ICardStreamServerRepository>),
                    typeof(ICardCache),
                    typeof(IDbScope),
                    typeof(ICardLifecycleCompanionRequestExtender)));

            container.RegisterType<TestConfigurationBuilder>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new InjectionParameter<Assembly>(this.ResourceAssembly),
                    typeof(IDbScope),
                    typeof(ICardManager),
                    typeof(ICardRepository),
                    typeof(ICardFileSourceSettings),
                    typeof(ICardLifecycleCompanionDependencies),
                    typeof(ICardTypeServerRepository),
                    typeof(CardMetadataCache),
                    typeof(IExchangeFormatInterpreter),
                    typeof(IIndentationStrategy),
                    typeof(ITessaViewService),
                    typeof(IJsonViewModelImporter),
                    typeof(IJsonViewModelAdapter)));
        }

        #endregion

        #region Protected Methods

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
    }
}
