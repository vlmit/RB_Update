using System;
using Tessa.Compilation;
using Tessa.Extensions.Default.Server.Workflow.KrCompilers.UserAPI;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    /// <summary>
    /// Результат компиляции KrCompiler'a.
    /// </summary>
    public interface IKrCompilationResult
    {
        /// <summary>
        /// Результат компиляции.
        /// </summary>
        ICompilationResult Result { get; }

        /// <summary>
        /// Форматированные ошибки и предупреждения.
        /// </summary>
        ValidationResult ValidationResult { get; }

        /// <summary>
        /// Создаёт объект имеющий указанные: префикс, алиас и идентификатор.
        /// </summary>
        /// <param name="prefix">Префикс объекта.</param>
        /// <param name="alias">Алиас объекта.</param>
        /// <param name="typeID">Идентификатор объекта.</param>
        /// <returns>Созданный объект.</returns>
        /// <exception cref="MissingCompiledClassException">Класс не найден. Проверьте наличие указанного объекта в системе.</exception>
        IKrScript CreateInstance(
            string prefix,
            string alias,
            Guid typeID);
    }
}
