using Tessa.Extensions.Shared.Services;
using Unity;
using Unity.Lifetime;

namespace Tessa.Extensions.Client.Services
{
    [Registrator]
    public sealed class Registrator : RegistratorBase
    {
        public override void RegisterUnity()
        {
            this.UnityContainer
                .RegisterType<IService, ServiceClient>(new ContainerControlledLifetimeManager())
                ;
        }
    }
}
