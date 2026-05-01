using System;
using Tessa.Cards;
using Tessa.Extensions.Default.Shared;
using Tessa.Extensions.Shared.Info;
using Tessa.Extensions.Shared;
using Tessa.Extensions.Platform.Client.ViewModels;
using Tessa.Platform.Runtime;
using Tessa.UI.Cards;
using Unity;
using Unity.Lifetime;
using Tessa.Extensions.Client.Helpers.Dialogs;

namespace Tessa.Extensions.Client.UI
{
	[Registrator]
	public sealed class Registrator : RegistratorBase
	{
		public override void RegisterExtensions(IExtensionContainer extensionContainer)
		{
			extensionContainer
				.RegisterExtension<ICardUIExtension, MedoLogbookCardUIExtension>(x => x
					.WithOrder(ExtensionStage.AfterPlatform)
					.WithUnity(this.UnityContainer)
					)
					//.WhenCardTypes(TypeInfo.RB_OutgoingTypeID))
				;
        }
        public override void RegisterUnity()
        {
			this.UnityContainer
				.RegisterType<IDialogHelper, DialogHelper>()
				.RegisterType<MedoLogbookCardUIExtension>(new ContainerControlledLifetimeManager())
				.AddExtension(new Diagnostic())
				;
        }
    }
}
