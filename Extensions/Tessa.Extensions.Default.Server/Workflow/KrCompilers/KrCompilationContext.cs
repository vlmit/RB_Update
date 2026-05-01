using System.Collections.Generic;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    /// <summary>
    /// Контекст, передаваемый в компилятор IKrCompiler
    /// </summary>
    public sealed class KrCompilationContext : IKrCompilationContext
    {
        /// <inheritdoc />
        public IList<IKrCommonMethod> CommonMethods { get; } = new List<IKrCommonMethod>();

        /// <inheritdoc />
        public IList<IKrRuntimeStage> Stages { get; } = new List<IKrRuntimeStage>();

        /// <inheritdoc />
        public IList<IKrStageTemplate> StageTemplates { get; } = new List<IKrStageTemplate>();

        /// <inheritdoc />
        public IList<IKrStageGroup> StageGroups { get; } = new List<IKrStageGroup>();

        /// <inheritdoc />
        public IList<IKrSecondaryProcess> SecondaryProcesses { get; } = new List<IKrSecondaryProcess>();

        /// <inheritdoc />y>
        public HashSet<string> Usings { get; } = new HashSet<string>();

        /// <inheritdoc />
        public HashSet<string> References { get; } = new HashSet<string>();
    }
}
