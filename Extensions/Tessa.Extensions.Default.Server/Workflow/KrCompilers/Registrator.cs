using Unity;
using Unity.Lifetime;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    [Registrator]
    public sealed class Registrator : RegistratorBase
    {
        public override void RegisterUnity()
        {
            this.UnityContainer
                .RegisterType<IExtraSourceSerializer, ExtraSourceStorageSerializer>(new ContainerControlledLifetimeManager())
                .RegisterType<IKrPreprocessorProvider, KrPreprocessorProvider>(new ContainerControlledLifetimeManager())
                .RegisterType<IKrCompiler, KrCompiler>(new ContainerControlledLifetimeManager())
                .RegisterType<IKrProcessCache, KrProcessCache>(new ContainerControlledLifetimeManager())
                .RegisterType<IKrCompilationCache, KrCompilationCache>(new ContainerControlledLifetimeManager())
                .RegisterType<IKrCompilationResultStorage, KrCompilationResultStorage>(new ContainerControlledLifetimeManager())
                .RegisterType<IKrExecutor, KrStageExecutor>(KrExecutorNames.StageExecutor, new PerResolveLifetimeManager())
                .RegisterType<IKrExecutor, KrGroupExecutor>(KrExecutorNames.GroupExecutor, new PerResolveLifetimeManager())
                .RegisterType<IKrExecutor, KrCacheExecutor>(KrExecutorNames.CacheExecutor, new ContainerControlledLifetimeManager())
            ;
        }
    }
}
