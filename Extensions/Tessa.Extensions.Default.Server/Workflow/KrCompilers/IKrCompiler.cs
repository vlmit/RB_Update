using System.Collections.Generic;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    /// <summary>
    /// Объект, выполняющий компиляцию объектов подсистемы маршрутов.
    /// </summary>
    public interface IKrCompiler
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
        /// Возвращает список стандартных игнорируемых предупреждений.
        /// </summary>
        IList<string> DefaultIgnoreWarnings { get; }

        /// <summary>
        /// Выполнить компиляцию на основе контекста.
        /// В контексте должны быть указаны шаблоны этапов, базовые методы, 
        /// пространства имен и референсы.
        /// </summary>
        /// <param name="context">Контекст компиляции.</param>
        /// <returns>Результат компиляции.</returns>
        IKrCompilationResult Compile(IKrCompilationContext context);
    }
}
