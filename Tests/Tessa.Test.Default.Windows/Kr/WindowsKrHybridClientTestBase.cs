using System;
using System.Reflection;
using Tessa.Platform.Data;
using Tessa.Test.Default.Client.Kr;
using Tessa.UI;
using Tessa.Views.Json;
using Tessa.Views.Json.Converters;
using Unity;

namespace Tessa.Test.Default.Windows.Kr
{
    /// <summary>
    /// Абстрактный базовый класс, предоставляющий методы для выполнения клиентских тестов 
    /// c поддержкой пользовательского интерфейса, типового решения и маршрутов на специально подготовленном сервере приложений.
    /// </summary>
    public abstract class WindowsKrHybridClientTestBase :
        KrHybridClientTestBase
    {
        #region Base Overrides

        /// <inheritdoc/>
        protected override IUnityContainer InitializeClientContainerBase(
            IUnityContainer unityContainer,
            Func<DbManager> createDbManagerFunc = default,
            IDbScope dbScope = default,
            string baseAddress = null,
            Action<IUnityContainer> beforeRegisterExtensionsOnClientAction = default,
            Action<IUnityContainer> beforeFinalizeClientRegistrationAction = default)
        {
            unityContainer
                .RegisterClient(
                    baseAddress: baseAddress,
                    entryAssembly: Assembly.GetExecutingAssembly())
                .RegisterViews()
                .RegisterType<IJsonViewModelImporter, JsonViewModelImporter>()
                .RegisterType<IJsonViewModelExporter, JsonViewModelExporter>()
                .RegisterType<IJsonViewModelAdapter, JsonViewModelConverter>();

            return this.RegisterClientContainerBase(
                unityContainer,
                createDbManagerFunc,
                dbScope,
                beforeRegisterExtensionsOnClientAction,
                beforeFinalizeClientRegistrationAction);
        }

        #endregion
    }
}