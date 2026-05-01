using System;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.FileConverters;
using Unity;
using Unity.Lifetime;

namespace Tessa.Extensions.Server.Chronos
{
    [Registrator]
    public sealed class Registrator : RegistratorBase
    {
        public override void RegisterUnity()
        {
            this.UnityContainer
                .RegisterType<FileConvertAdditionAction>(new ContainerControlledLifetimeManager())

                ;
        }

        public override void RegisterExtensions(IExtensionContainer extensionContainer)
        {
            extensionContainer
                .RegisterExtension<IFileConverterExtension, FileConvertAdditionAction>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 1)
                    .WithUnity(this.UnityContainer))
                    //.WhenFileConverterEventNames(FileConverterEventNames.Unknown))
                ;
        }
    }
}