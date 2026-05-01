using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;

namespace Tessa.Extensions.Server.Cards.Store
{
    [Registrator]
    public sealed class Registrator : RegistratorBase
    {
        public override void RegisterExtensions(IExtensionContainer extensionContainer)
        {
            extensionContainer.RegisterExtension<ICardStoreExtension, JSPersonalListStoreExtension>(x => x
            .WithUnity(this.UnityContainer)
            .WithOrder(ExtensionStage.AfterPlatform, 1)
            .WhenAnyStoreMethod()
            .WhenCardTypes(new Guid("483254c0-19a2-4f48-b168-4f0ea7ea804e"))
            
            );
        }
    }
}
