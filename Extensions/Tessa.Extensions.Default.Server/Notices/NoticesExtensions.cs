using System;
using Unity;
using Unity.Lifetime;

namespace Tessa.Extensions.Default.Server.Notices
{
    public static class NoticesExtensions
    {
        #region IUnityContainer Extensions

        public static IUnityContainer RegisterNoticesMessageProcessor(this IUnityContainer unityContainer)
        {
            return unityContainer
                .RegisterType<IMessageProcessor, MessageProcessor>(new ContainerControlledLifetimeManager())
                .RegisterType<KrMessageHandler>(new ContainerControlledLifetimeManager())
                .RegisterHandler<KrMessageHandler>()
                ;
        }


        public static IUnityContainer RegisterHandler<T>(this IUnityContainer unityContainer)
            where T : IMessageHandler
        {
            if (typeof(T) == typeof(IMessageHandler))
            {
                throw new ArgumentException("Invalid type passed in RegisterHandler<T> call.");
            }

            return unityContainer
                .RegisterFactory<Func<IMessageHandler>>(
                    typeof(T).FullName,
                    c => new Func<IMessageHandler>(() => c.Resolve<T>()),
                    new ContainerControlledLifetimeManager())
                ;
        }

        #endregion
    }
}
