using System.Collections.Generic;

namespace Tessa.Extensions.Default.Server.Files.VirtualFiles.Compilation
{
    /// <summary>
    /// Компилятор скриптов виртуальных файлов
    /// </summary>
    public interface IKrVirtualFileCompiler
    {
        /// <summary>
        /// Список стандартных пространств имен, подставляемых в каждую единицу компиляции.
        /// Доступно для изменения, но не потокобезопасно.
        /// </summary>
        IList<string> DefaultUsings { get; }

        /// <summary>
        /// Список стандартных зависимостей, используемых при компиляции.
        /// Доступно для изменения, но не потокобезопасно.
        /// </summary>
        IList<string> DefaultReferences { get; }

        /// <summary>
        /// Метод для создания контекста компиляции
        /// </summary>
        /// <returns>Возвращает контекст компиляции</returns>
        IKrVirtualFileCompilationContext CreateContext();

        /// <summary>
        /// Выполняет компиляцию скриптов виртуальных файлов
        /// </summary>
        /// <param name="context">Контекст компиляции скриптов виртуальных файлов</param>
        /// <returns>Возвращает результат компиляции скриптов виртуальных файлов</returns>
        IKrVirtualFileCompilationResult Compile(IKrVirtualFileCompilationContext context);
    }
}
