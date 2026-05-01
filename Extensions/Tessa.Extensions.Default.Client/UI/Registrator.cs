using Tessa.Cards;
using Tessa.Extensions.Default.Client.UI.CardFiles;
using Tessa.Extensions.Default.Client.UI.KrProcess;
using Tessa.Extensions.Default.Client.UI.WorkflowEngine;
using Tessa.Extensions.Default.Shared;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.UI.Cards;
using Unity;
using Unity.Lifetime;

namespace Tessa.Extensions.Default.Client.UI
{
    /// <summary>
    /// Регистрация расширений для UI карточки.
    /// </summary>
    [Registrator]
    public sealed class Registrator : RegistratorBase
    {
        public override void RegisterUnity()
        {
            this.UnityContainer
                .RegisterType<KrGetCycleFileInfoUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<CarUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrFilesUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrHideCardTypeSettingsUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrHideApprovalTabOrDocStateBlockUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrHideApprovalStagePermissionsDisclaimer>(new ContainerControlledLifetimeManager())
                .RegisterType<KrDocumentWorkspaceInfoUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<OutgoingPartnerUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<CalendarUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrAdditionalApprovalCardUIExtension>(new PerResolveLifetimeManager())
                .RegisterType<KrRecalcStagesUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrStageSourceUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrStageTemplateUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrStageUIExtension>(new PerResolveLifetimeManager())
                .RegisterType<KrTilesUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrSecondaryProcessUIExtension>(new PerResolveLifetimeManager())
                .RegisterType<KrTemplateUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrEditModeToolbarUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<CreateAndSelectToolbarUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrVirtualFilesUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<KrPermissionsUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<FilesViewCardControlInitializationStrategy>(new ContainerControlledLifetimeManager())
                .RegisterType<InitializeFilesViewUIExtension>(new PerResolveLifetimeManager())
                .RegisterType<KrRoutesInWorkflowEngineUIExtension>(new ContainerControlledLifetimeManager())
                .RegisterType<OpenCardInViewUIExtension>(new ContainerControlledLifetimeManager())
                ;
        }

        public override void RegisterExtensions(IExtensionContainer extensionContainer)
        {
            extensionContainer
                .RegisterExtension<ICardUIExtension, KrGetCycleFileInfoUIExtension>(x => x
                    .WithOrder(ExtensionStage.BeforePlatform, 1)
                    .WithUnity(this.UnityContainer))

                .RegisterExtension<ICardUIExtension, CarUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 1)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(DefaultCardTypes.CarTypeID))

                .RegisterExtension<ICardUIExtension, KrFilesUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 2)
                    .WithUnity(this.UnityContainer))

                .RegisterExtension<ICardUIExtension, KrUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 3)
                    .WithUnity(this.UnityContainer))

                .RegisterExtension<ICardUIExtension, CalendarUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 4)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(CardHelper.CalendarTypeID))

                .RegisterExtension<ICardUIExtension, KrAdditionalApprovalCardUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 5)
                    .WithUnity(this.UnityContainer))

                .RegisterExtension<ICardUIExtension, KrHideCardTypeSettingsUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 7)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(DefaultCardTypes.KrSettingsTypeID, DefaultCardTypes.KrDocTypeTypeID))

                .RegisterExtension<ICardUIExtension, KrHideApprovalTabOrDocStateBlockUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 8)
                    .WithUnity(this.UnityContainer))

                .RegisterExtension<ICardUIExtension, KrRecalcStagesUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 9)
                    .WithUnity(this.UnityContainer))

                .RegisterExtension<ICardUIExtension, KrStageSourceUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 10)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(KrConstants.CompiledCardTypes))

                .RegisterExtension<ICardUIExtension, KrStageTemplateUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 11)
                    .WithSingleton()
                    .WhenCardTypes(DefaultCardTypes.KrStageTemplateTypeID, DefaultCardTypes.KrSecondaryProcessTypeID))

                .RegisterExtension<ICardUIExtension, KrStageUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 12)
                    .WithUnity(this.UnityContainer))

                .RegisterExtension<ICardUIExtension, KrTilesUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 13)
                    .WithUnity(this.UnityContainer))

                .RegisterExtension<ICardUIExtension, KrSecondaryProcessUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 14)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(DefaultCardTypes.KrSecondaryProcessTypeID))

                .RegisterExtension<ICardUIExtension, KrTemplateUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 15)
                    .WithUnity(this.UnityContainer))

                .RegisterExtension<ICardUIExtension, KrEditModeToolbarUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 16)
                    .WithSingleton()
                    .WhenDefaultDialog())

                .RegisterExtension<ICardUIExtension, CreateAndSelectToolbarUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 17)
                    .WithSingleton()
                    .WhenDefaultDialog())

                .RegisterExtension<ICardUIExtension, KrDocStateUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 18)
                    .WithSingleton()
                    .WhenCardTypes(DefaultCardTypes.KrDocStateTypeID))

                .RegisterExtension<ICardUIExtension, KrVirtualFilesUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 19)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(DefaultCardTypes.KrVirtualFileTypeID))

                .RegisterExtension<ICardUIExtension, KrPermissionsUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 20)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(DefaultCardTypes.KrPermissionsTypeID))

                .RegisterExtension<ICardUIExtension, KrSettingsForumsLicenseUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 20)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(DefaultCardTypes.KrSettingsTypeID, DefaultCardTypes.KrDocTypeTypeID))

                .RegisterExtension<ICardUIExtension, KrHideApprovalStagePermissionsDisclaimer>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 50)
                    .WithUnity(this.UnityContainer))

                .RegisterExtension<ICardUIExtension, KrDocumentWorkspaceInfoUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 101)
                    .WithUnity(this.UnityContainer))

                .RegisterExtension<ICardUIExtension, OutgoingPartnerUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 102)
                    .WithUnity(this.UnityContainer)
                    .WhenCardTypes(DefaultCardTypes.OutgoingTypeID))

                .RegisterExtension<ICardUIExtension, KrRequestCommentUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 103)
                    .WithSingleton())

                .RegisterExtension<ICardUIExtension, KrExtendedPermissionsUIExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 104)
                    .WithSingleton())
                
                .RegisterExtension<ICardUIExtension, InitializeFilesViewUIExtension>(x => x
                    .WithUnity(this.UnityContainer)
                    .WithOrder(ExtensionStage.AfterPlatform, 106))
                
                .RegisterExtension<ICardUIExtension, KrRoutesInWorkflowEngineUIExtension>(x => x
                    .WithUnity(this.UnityContainer)
                    .WithOrder(ExtensionStage.AfterPlatform, 107))

                .RegisterExtension<ICardUIExtension, OpenCardInViewUIExtension>(x => x
                    .WithUnity(this.UnityContainer)
                    .WithOrder(ExtensionStage.AfterPlatform, 108))
                ;
        }
    }
}
