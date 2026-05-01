using System.Collections.Generic;

namespace Tessa.Extensions.Default.Server.Files.VirtualFiles.Compilation
{
    /// <summary>
    /// Контекст компиляции для компилятора <see cref="IKrVirtualFileCompiler"/>
    /// </summary>
    public interface IKrVirtualFileCompilationContext
    {
        /// <summary>
        /// Набор виртуальных файлов для компиляции.
        /// Полученный объект может быть не полным, но обязательно должен иметь <see cref="IKrVirtualFile.ID"/>, <see cref="IKrVirtualFile.Name"/>
        /// и <see cref="IKrVirtualFile.InitializationScenario"/>.
        /// </summary>
        IList<IKrVirtualFile> Files { get; }
    }
}