using System.Collections.Generic;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    /// <summary>
    /// Контекст, передаваемый в компилятор IKrCompiler
    /// </summary>
    public interface IKrCompilationContext
    {
        /// <summary>
        /// Список базовых методов, подлежащих компиляции.
        /// </summary>
        IList<IKrCommonMethod> CommonMethods { get; }

        /// <summary>
        /// Список данных о скриптах, выполняемых в процессе прохождения маршрута.
        /// </summary>
        IList<IKrRuntimeStage> Stages { get; }

        /// <summary>
        /// Список шаблонов этапов, подлежащих компиляции
        /// </summary>
        IList<IKrStageTemplate> StageTemplates { get; }

        /// <summary>
        /// Список групп этапов, подлежащих компиляции.
        /// </summary>
        IList<IKrStageGroup> StageGroups { get; }

        /// <summary>
        /// Список вторичных процессов, подлежащих компиляции.
        /// </summary>
        IList<IKrSecondaryProcess> SecondaryProcesses { get; }

        /// <summary>
        /// Пространства имен, которые необходимо добавить к каждому этапу или базовому методу
        /// </summary>
        HashSet<string> Usings { get; }

        /// <summary>
        /// Дополнительные сборки, с которыми необходимо линковаться
        /// Указывать как имя файла с расширением
        /// Напр: Tessa.Extensions.Shared.dll
        /// </summary>
        HashSet<string> References { get; }
    }
}
