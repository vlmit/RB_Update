using System.Collections.Generic;

namespace Tessa.Extensions.Default.Server.Files.VirtualFiles.Compilation
{
    public sealed class KrVirtualFileCompilationContext : IKrVirtualFileCompilationContext
    {
        #region IKrVirtualFileCompilationContext Implementation

        public IList<IKrVirtualFile> Files { get; } = new List<IKrVirtualFile>();

        #endregion
    }
}
