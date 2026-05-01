using Unity;
using Unity.Lifetime;

namespace Tessa.Extensions.Default.Server.OnlyOffice
{
    [Registrator]
    public sealed class Registrator :
        RegistratorBase
    {
        public override void RegisterUnity()
        {
            this.UnityContainer
                .RegisterType<IOnlyOfficeSettingsProvider, OnlyOfficeSettingsProvider>(new ContainerControlledLifetimeManager())
                .RegisterType<IOnlyOfficeFileCacheInfoStrategy, OnlyOfficeFileCacheInfoStrategy>(new ContainerControlledLifetimeManager())
                .RegisterType<IOnlyOfficeFileCache, OnlyOfficeFileCache>(new ContainerControlledLifetimeManager())
                ;
        }
    }
}