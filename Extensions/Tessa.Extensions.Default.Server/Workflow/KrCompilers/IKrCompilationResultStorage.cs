using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    /// <summary>
    /// Объект, предоставляющий доступ к результатам компиляции подсистемы маршрутов.
    /// </summary>
    public interface IKrCompilationResultStorage
    {
        /// <summary>
        /// Сохраняет результаты компиляции сценариев карточки с указанным идентификатором.
        /// </summary>
        /// <param name="cardID">Идентификатор карточки.</param>
        /// <param name="compilationResult">Результаты компиляции.</param>
        /// <param name="withCompilationResult">Значение <see langword="true"/>, если необходимо сохранить результаты компиляции, иначе будет сохранена только информация по компиляции без сборки.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        Task UpsertAsync(
            Guid cardID,
            IKrCompilationResult compilationResult,
            bool withCompilationResult = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Возвращает результаты компиляции сценариев карточки с указанным идентификатором.
        /// </summary>
        /// <param name="cardID">Идентификатор карточки.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Результаты компиляции.</returns>
        Task<IKrCompilationResult> GetCompilationResultAsync(
            Guid cardID,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Возвращает информацию по компиляции для карточки с указанным идентификатором.
        /// </summary>
        /// <param name="cardID">Идентификатор карточки.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Информация по компиляции.</returns>
        Task<KrCompilationOutput> GetCompilationOutputAsync(
            Guid cardID,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет информацию о сборке и результатах валидации для карточки с указанным идентификатором.
        /// </summary>
        /// <param name="cardID">Идентификатор карточки.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        Task DeleteCompilationResultAsync(
            Guid cardID,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет информацию о компиляции сценариев карточки с указанным идентификатором.
        /// </summary>
        /// <param name="cardID">Идентификатор карточки.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        Task DeleteAsync(
            Guid cardID,
            CancellationToken cancellationToken = default);
    }
}