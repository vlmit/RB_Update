using System.Threading;
using Tessa.Cards;
using Tessa.Files;
using Tessa.Platform.Data;
using Tessa.Platform.Runtime;
using Unity;

namespace Tessa.Extensions.Default.Server.Files.VirtualFiles.Compilation
{
    public sealed class KrVirtualFileScriptContext : IKrVirtualFileScriptContext
    {
        #region IKrVirtualFileScriptContext Implementation

        public IUnityContainer Container { get; set; }

        public IDbScope DbScope { get; set; }

        public ISession Session { get; set; }
        
        public Card Card { get; set; }

        public IFile File { get; set; }

        public CardFile CardFile { get; set; }

        public CancellationToken CancellationToken { get; set; }

        #endregion
    }
}
