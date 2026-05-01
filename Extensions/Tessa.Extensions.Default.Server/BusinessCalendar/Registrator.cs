using Tessa.Cards;
using Tessa.Cards.Extensions;
using Unity;
using Unity.Lifetime;

namespace Tessa.Extensions.Default.Server.BusinessCalendar
{
    [Registrator]
    public sealed class Registrator : RegistratorBase
    {
        public override void RegisterUnity()
        {
            this.UnityContainer
                .RegisterType<CalendarStoreExtension>(new ContainerControlledLifetimeManager())
                ;
        }


        public override void RegisterExtensions(IExtensionContainer extensionContainer)
        {
            extensionContainer
                // New
                .RegisterExtension<ICardNewExtension, CalendarNewExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 1)
                    .WithSingleton()
                    .WhenCardTypes(CardHelper.CalendarTypeID))

                // Get
                .RegisterExtension<ICardGetExtension, CalendarGetExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 1)
                    .WithSingleton()
                    .WhenCardTypes(CardHelper.CalendarTypeID))

                // Store
                .RegisterExtension<ICardStoreExtension, CalendarStoreExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 1)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(CardHelper.CalendarTypeID))
                ;
        }
    }
}
