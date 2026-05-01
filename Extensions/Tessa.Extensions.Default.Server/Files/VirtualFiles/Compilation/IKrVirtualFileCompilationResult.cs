using System;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.Files.VirtualFiles.Compilation
{
    /// <summary>
    /// Результат компиляции компилятора <see cref="IKrVirtualFileCompiler"/>
    /// </summary>
    public interface IKrVirtualFileCompilationResult
    {
        /// <summary>
        /// Результат валидации компиляции
        /// </summary>
        ValidationResult ValidationResult { get; }

        /// <summary>
        /// Массив байт сборки. Генерируется, если при компиляции была задана генерация файла сборки.
        /// </summary>
        byte[] AssemblyBytes { get; }

        /// <summary>
        /// Дата создания результата компиляции
        /// </summary>
        DateTime Created { get; }

        /// <summary>
        /// Определяет, является ли результат компиляции валидным
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Метод для получения скомпилированного объекта по его идентификатору
        /// </summary>
        /// <param name="id">Идентификатор виртуального файла</param>
        /// <returns>Возвращает скомпилированнвый объект по идентификатору, или null, если по данному идентификатору нет скомпилированного объекта</returns>
        IKrVirtualFileScript GetScript(Guid id);
    }
}