using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared;
using Unity;
using Unity.Lifetime;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers.Requests
{
    [Registrator]
    public class Registrator: RegistratorBase
    {

        public override void RegisterUnity()
        {
            this.UnityContainer

                .RegisterType<KrCompileCommonMethodStoreExtension>(new ContainerControlledLifetimeManager())

                .RegisterType<KrStageTemplateNewGetExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrStageTemplateStoreExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrCompileStageTemplateStoreExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrCompileStageGroupStoreExtension>(new ContainerControlledLifetimeManager())

                .RegisterType<KrSecondaryProcessNewGetExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrSecondaryProcessStoreExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrCompileSecondaryProcessStoreExtension>(new ContainerControlledLifetimeManager())

                .RegisterType<KrRecalcStagesStoreExtension>(new PerResolveLifetimeManager())
                .RegisterType<KrSourceGetExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrCompileSourceDeleteExtension>(new ContainerControlledLifetimeManager())
                ;

        }

        public override void RegisterExtensions(IExtensionContainer extensionContainer)
        {
            extensionContainer
                .RegisterExtension<ICardStoreExtension, KrCompileCommonMethodStoreExtension>(x => x
                    .WithOrder(ExtensionStage.BeforePlatform, 1)
                    .WithUnity(this.UnityContainer)
                    .WhenAnyStoreMethod()
                    .WhenCardTypes(DefaultCardTypes.KrStageCommonMethodTypeID))


                .RegisterExtension<ICardNewExtension, KrStageTemplateNewGetExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(DefaultCardTypes.KrStageTemplateTypeID)
                    .WhenMethod(CardNewMethod.Default, CardNewMethod.Template))
                .RegisterExtension<ICardGetExtension, KrStageTemplateNewGetExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(DefaultCardTypes.KrStageTemplateTypeID)
                    .WhenMethod(CardGetMethod.Default))
                .RegisterExtension<ICardStoreExtension, KrStageTemplateStoreExtension>(x => x
                    .WithOrder(ExtensionStage.BeforePlatform, 1)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(DefaultCardTypes.KrStageTemplateTypeID))
                // Должно выполнятся после KrStageTemplateStoreExtension
                .RegisterExtension<ICardStoreExtension, KrCompileStageTemplateStoreExtension>(x => x
                    .WithOrder(ExtensionStage.BeforePlatform, 2)
                    .WithUnity(this.UnityContainer)
                    .WhenAnyStoreMethod()
                    .WhenCardTypes(DefaultCardTypes.KrStageTemplateTypeID))


                .RegisterExtension<ICardNewExtension, KrStageGroupNewGetExtension>(x => x
                    .WithOrder(ExtensionStage.BeforePlatform)
                    .WithSingleton()
                    .WhenCardTypes(DefaultCardTypes.KrStageGroupTypeID)
                    .WhenMethod(CardNewMethod.Default, CardNewMethod.Template))
                .RegisterExtension<ICardGetExtension, KrStageGroupNewGetExtension>(x => x
                    .WithOrder(ExtensionStage.BeforePlatform)
                    .WithSingleton()
                    .WhenCardTypes(DefaultCardTypes.KrStageGroupTypeID)
                    .WhenMethod(CardGetMethod.Default))
                .RegisterExtension<ICardStoreExtension, KrCompileStageGroupStoreExtension>(x => x
                    .WithOrder(ExtensionStage.BeforePlatform, 1)
                    .WithUnity(this.UnityContainer)
                    .WhenAnyStoreMethod()
                    .WhenCardTypes(DefaultCardTypes.KrStageGroupTypeID))


                .RegisterExtension<ICardNewExtension, KrSecondaryProcessNewGetExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(DefaultCardTypes.KrSecondaryProcessTypeID)
                    .WhenMethod(CardNewMethod.Default, CardNewMethod.Template))
                .RegisterExtension<ICardGetExtension, KrSecondaryProcessNewGetExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(DefaultCardTypes.KrSecondaryProcessTypeID)
                    .WhenMethod(CardGetMethod.Default))
                .RegisterExtension<ICardStoreExtension, KrSecondaryProcessStoreExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 1)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(DefaultCardTypes.KrSecondaryProcessTypeID)
                    .WhenMethod(CardStoreMethod.Default, CardStoreMethod.Import))
                .RegisterExtension<ICardStoreExtension, KrCompileSecondaryProcessStoreExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform)
                    .WithUnity(this.UnityContainer)
                    .WhenAnyStoreMethod()
                    .WhenCardTypes(DefaultCardTypes.KrSecondaryProcessTypeID))


                .RegisterExtension<ICardStoreExtension, KrRecalcStagesStoreExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 10)
                    .WithUnity(this.UnityContainer))

                .RegisterExtension<ICardGetExtension, KrSourceGetExtension>(x => x
                    .WithOrder(ExtensionStage.BeforePlatform, 1)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(
                        DefaultCardTypes.KrStageTemplateTypeID,
                        DefaultCardTypes.KrStageCommonMethodTypeID,
                        DefaultCardTypes.KrStageGroupTypeID,
                        DefaultCardTypes.KrSecondaryProcessTypeID))

                .RegisterExtension<ICardDeleteExtension, KrCompileSourceDeleteExtension>(x => x
                    .WithOrder(ExtensionStage.BeforePlatform, 1)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(
                        DefaultCardTypes.KrStageTemplateTypeID,
                        DefaultCardTypes.KrStageCommonMethodTypeID,
                        DefaultCardTypes.KrStageGroupTypeID,
                        DefaultCardTypes.KrSecondaryProcessTypeID))

                ;
        }
    }
}
