using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared;
using Tessa.Platform;
using Tessa.UI.Views.Extensions;
using Unity;
using Unity.Lifetime;

namespace Tessa.Extensions.Default.Client.Cards
{
    [Registrator]
    public sealed class Registrator : RegistratorBase
    {
        public override void RegisterUnity()
        {
            this.UnityContainer
                .RegisterType<OpenFromKrDocStatesOnDoubleClickExtension>(new ContainerControlledLifetimeManager())
                ;
        }

        public override void RegisterExtensions(IExtensionContainer extensionContainer)
        {
            extensionContainer
                .RegisterExtension<ICardStoreExtension, AcquaintanceClientStoreExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 1)
                    .WithSingleton())

                .RegisterExtension<ICardStoreExtension, KrPermissionsMandatoryStoreExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 2)
                    .WithSingleton())

                .RegisterExtension<ICardDeleteExtension, KrDocStateClientDeleteExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 1)
                    .WithSingleton()
                    .WhenCardTypes(DefaultCardTypes.KrDocStateTypeID))
                ;
        }

        public override void FinalizeRegistration()
        {
            this.UnityContainer
                .TryResolve<IWorkplaceExtensionRegistry>()
                ?

                .Register(typeof(OpenFromKrDocStatesOnDoubleClickExtension))
                .RegisterConfiguratorType(
                    typeof(OpenFromKrDocStatesOnDoubleClickExtension),
                    type => this.UnityContainer.Resolve<OpenFromKrDocStatesOnDoubleClickExtensionConfigurator>())
                ;
        }
    }
}