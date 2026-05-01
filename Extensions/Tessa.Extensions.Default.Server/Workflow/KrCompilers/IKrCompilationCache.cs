using System.Threading;
using System.Threading.Tasks;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    /// <summary>
    /// Кэш с результатами компиляции объектов подсистемы маршрутов.
    /// </summary>
    public interface IKrCompilationCache
    {
        /// <summary>
        /// Выполняет сборку, если в кэше нет сборки или сборка invalidate.
        /// Если в кэше есть актуальная сборка, возвращается null.
        /// </summary>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Результат сборки или значение null, если в кэше есть актуальная сборка.</returns>
        Task<IKrCompilationResult> BuildAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Явно сбрасывает кэш сборки и выполняет пересборку.
        /// Кэш KrStageTenolateCache не сбрасывается.
        /// </summary>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Результат компиляции.</returns>
        Task<IKrCompilationResult> RebuildAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Возвращает значения из кэша.
        /// </summary>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Результат сборки.</returns>
        ValueTask<IKrCompilationResult> GetAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Сбрасывает значения кэша.
        /// </summary>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        Task InvalidateAsync(CancellationToken cancellationToken = default);
    }
}
