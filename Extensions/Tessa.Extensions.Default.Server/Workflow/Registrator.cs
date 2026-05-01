using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared;

namespace Tessa.Extensions.Default.Server.Workflow
{
    [Registrator]
    public class Registrator : RegistratorBase
    {
        #region Base Overrides

        /// <inheritdoc/>
        public override void RegisterExtensions(IExtensionContainer extensionContainer)
        {
            extensionContainer
                .RegisterExtension<ICardStoreTaskExtension, KrClearWasteInAdditionalApprovalStoreTaskExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 998)
                    .WithSingleton()
                    .WhenTaskTypes(DefaultTaskTypes.KrAdditionalApprovalTypeID))
                .RegisterExtension<ICardStoreTaskExtension, KrClearWasteInApprovalStoreTaskExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 999)
                    .WithSingleton()
                    .WhenTaskTypes(DefaultTaskTypes.KrApproveTypeID, DefaultTaskTypes.KrSigningTypeID, DefaultTaskTypes.KrAdditionalApprovalTypeID))
                ;
        }

        #endregion
    }
}
