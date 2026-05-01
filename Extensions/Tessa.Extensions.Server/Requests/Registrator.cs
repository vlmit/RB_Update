using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Shared;
using Tessa.Extensions.Shared.Info;

namespace Tessa.Extensions.Server.Requests
{
    [Registrator]
    public sealed class Registrator : RegistratorBase
    {
        public override void RegisterExtensions(IExtensionContainer extensionContainer)
        {
            extensionContainer

                .RegisterExtension<ICardRequestExtension, ShowStampPreviewCardRequestExtension>(x => x
                    .WithOrder(ExtensionStage.AfterPlatform, 1)
                    .WithUnity(this.UnityContainer)
                    .WithSingleton()
                    .WhenRequestTypes(RequestTypes.ShowStampPreviewTypeID))
                
                ;
        }
    }
}
