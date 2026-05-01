using Tessa.Cards.Caching;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared.Notices;
using Tessa.Notices;
using Tessa.Platform.Data;
using Tessa.Platform.Licensing;
using Tessa.Platform.Runtime;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace Tessa.Extensions.Default.Server.Notices
{
    [Registrator]
    public sealed class Registrator : RegistratorBase
    {
        public override void RegisterUnity()
        {
            this.UnityContainer
                .RegisterType<INotificationResolver, NotificationResolver>(
                    new ContainerControlledLifetimeManager(),
                    new InjectionConstructor(
                        new ResolvedParameter<IMailService>(MailServiceNames.Default),
                        new ResolvedParameter<IMailService>(MailServiceNames.WithoutTransaction),
                        typeof(IDbScope),
                        typeof(ISession),
                        typeof(ICardCache),
                        typeof(IUnityContainer),
                        typeof(ILicenseManager)))

                .RegisterType<NotificationStoreExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<NotificationGetExtension>(new ContainerControlledLifetimeManager())

                // .RegisterType<INotificationSender, CustomNotificationSender>(
                //     CustomNotification.Key,
                //     new ContainerControlledLifetimeManager())

                .RegisterType<INotificationDefaultLanguagePicker, KrNotificationDefaultLanguagePicker>(new ContainerControlledLifetimeManager())
                .RegisterType<INotificationSubscriptionPermissionManager, KrNotificationSubscriptionPermissionManagerServer>(new ContainerControlledLifetimeManager())
                ;
        }


        public override void RegisterExtensions(IExtensionContainer extensionContainer)
        {
            extensionContainer
                .RegisterExtension<ICardStoreExtension, NotificationStoreExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 1)
                    .WithUnity(this.UnityContainer))
                .RegisterExtension<ICardGetExtension, NotificationGetExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 1)
                    .WithUnity(this.UnityContainer))
                ;
        }
    }
}
