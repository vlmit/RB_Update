using System.Threading;
using System.Threading.Tasks;

namespace Tessa.Extensions.Default.Server.Files.VirtualFiles.Compilation
{
    /// <summary>
    /// Кеш компиляции результатов компилятора <see cref="IKrVirtualFileCompiler"/>
    /// </summary>
    public interface IKrVirtualFileCompilationCache
    {
        /// <summary>
        /// Метод для получения закешированного результата компиляции скриптов виртуальных файлов.
        /// Если результат компиляции еще не был закеширован, то производит компиляцию.
        /// </summary>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Возвращает результат компиляции скриптов виртуальных файлов</returns>
        ValueTask<IKrVirtualFileCompilationResult> GetAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Метод для сброса кеша компиляции скриптов виртуальных файлов
        /// </summary>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        Task InvalidateAsync(CancellationToken cancellationToken = default);
    }
}
