using System;
using System.Collections.Generic;
using Tessa.Compilation;
using Tessa.Extensions.Default.Server.Workflow.KrCompilers.SourceBuilders;
using Tessa.Platform.Collections;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    /// <inheritdoc cref="IKrCompiler"/>
    public sealed class KrCompiler :
        IKrCompiler
    {
        #region fields

        private readonly ICompiler compiler;

        private readonly IKrSourceBuilderFactory builderFactory;

        #endregion

        #region constructor

        public KrCompiler(
            ICompiler compiler,
            IKrSourceBuilderFactory builderFactory)
        {
            this.compiler = compiler;
            this.builderFactory = builderFactory;
        }

        #endregion

        #region private

        /// <summary>
        /// Получение исходных кодов и подготовка их к компиляции.
        /// </summary>
        /// <param name="krContext">Контекст, передаваемый в компилятор IKrCompiler.</param>
        /// <param name="context">Контекст сеанса компиляции для компилятора.</param>
        /// <param name="anchorsMap">Возвращаемое значение. Коллекция ключ-значение, где ключ - идентификатор исходного кода; значение - объект идентифицирующий элемент компиляции.</param>
        private void SetSources(IKrCompilationContext krContext, ICompilationContext context, out Dictionary<Guid, CompilationAnchor> anchorsMap)
        {
            anchorsMap = new Dictionary<Guid, CompilationAnchor> ();
            var commonMethodBuilder = this.builderFactory.GetKrCommonMethodBuilder();
            commonMethodBuilder.SetSources(krContext.CommonMethods);
            commonMethodBuilder.FillAnchorsMap(anchorsMap);
            context.Sources.AddRange(commonMethodBuilder.BuildSources());

            var stages = krContext.Stages;
            foreach (var stage in stages)
            {
                string stageTemplateName;
                string stageGroupName;
                string secondaryProcessName;

                if (krContext.SecondaryProcesses.TryFirst(i => i.ID == stage.GroupID, out var secondaryProcess))
                {
                    stageTemplateName = null;
                    stageGroupName = null;
                    secondaryProcessName = secondaryProcess.Name;
                }
                else
                {
                    stageTemplateName = stage.TemplateName;
                    stageGroupName = stage.GroupName;
                    secondaryProcessName = null;
                }

                var sources = this.builderFactory.GetKrRuntimeScriptBuilder()
                    .SetClassID(stage.StageID)
                    .SetClassAlias(SourceIdentifiers.StageAlias)
                    .SetLocation(
                        stage.StageName,
                        stageTemplateName,
                        stageGroupName,
                        secondaryProcessName)
                    .SetSources(stage)
                    .SetExtraSources(stage)
                    .FillAnchorsMap(anchorsMap)
                    .BuildSources()
                    ;
                context.Sources.AddRange(sources);
            }

            var templates = krContext.StageTemplates;
            foreach (var template in templates)
            {
                var sources = this.builderFactory.GetKrDesignScriptBuilder()
                        .SetClassID(template.ID)
                        .SetClassAlias(SourceIdentifiers.TemplateAlias)
                        .SetLocation(stageTemplateName: template.Name, stageGroupName: template.StageGroupName)
                        .SetSources(template)
                        .FillAnchorsMap(anchorsMap)
                        .BuildSources()
                    ;
                context.Sources.AddRange(sources);
            }

            var stageGroups = krContext.StageGroups;
            foreach (var stageGroup in stageGroups)
            {
                var sources = this.builderFactory.GetKrDesignScriptBuilder()
                        .SetClassID(stageGroup.ID)

                        .SetClassAlias(SourceIdentifiers.GroupAlias)
                        .SetLocation(stageGroupName: stageGroup.Name)
                        .SetSources(stageGroup)
                        .FillAnchorsMap(anchorsMap)
                        .BuildSources()
                    ;
                context.Sources.AddRange(sources);
                sources = this.builderFactory.GetKrRuntimeScriptBuilder()
                        .SetClassID(stageGroup.ID)
                        .SetClassAlias(SourceIdentifiers.GroupAlias)
                        .SetLocation(stageGroupName: stageGroup.Name)
                        .SetSources(stageGroup)
                        .FillAnchorsMap(anchorsMap)
                        .BuildSources()
                    ;
                context.Sources.AddRange(sources);
            }

            var secondaryProcesses = krContext.SecondaryProcesses;
            foreach (var secondaryProcess in secondaryProcesses)
            {
                var sources = this.builderFactory.GetKrExecutionScriptBuilder()
                        .SetClassID(secondaryProcess.ID)
                        .SetClassAlias(SourceIdentifiers.SecondaryProcessAlias)
                        .SetLocation(secondaryProcessName: secondaryProcess.Name)
                        .SetSources(secondaryProcess)
                        .FillAnchorsMap(anchorsMap)
                        .BuildSources()
                    ;
                context.Sources.AddRange(sources);

                if (secondaryProcess is IKrProcessButton button)
                {
                    sources = this.builderFactory.GetKrVisibilityScriptBuilder()
                            .SetClassID(button.ID)
                            .SetClassAlias(SourceIdentifiers.SecondaryProcessAlias)
                            .SetLocation(secondaryProcessName: button.Name)
                            .SetSources(button)
                            .FillAnchorsMap(anchorsMap)
                            .BuildSources()
                        ;
                    context.Sources.AddRange(sources);
                }
            }
        }

        /// <summary>
        /// Заполнение ValidationResult по выводу компилятора.
        /// </summary>
        /// <param name="compilationResult">Результат компиляции.</param>
        /// <param name="anchorsMap">Коллекция ключ-значение, где ключ - идентификатор исходного кода; значение - объект идентифицирующий элемент компиляции.</param>
        /// <returns>Результат валидации.</returns>
        private static ValidationResult FillValidationResult(
            ICompilationResult compilationResult,
            Dictionary<Guid, CompilationAnchor> anchorsMap)
        {
            var result = new ValidationResultBuilder();
            foreach (var compilerOutputItem in compilationResult.CompilerOutput)
            {
                var source = compilerOutputItem.Source;
                var name = source?.Name ?? string.Empty;

                var sourceCode = source != null && anchorsMap.TryGetValue(source.ID, out var anchorItem)
                    ? CompilationHelper.FormatErrorIntoMember(source, compilerOutputItem, anchorItem.Name, anchorItem.SyntaxKind)
                    : string.Empty;
                var errorText = string.IsNullOrWhiteSpace(name)
                    ? string.Empty + compilerOutputItem.ErrorText
                    : name + Environment.NewLine + compilerOutputItem.ErrorText;

                var validator = ValidationSequence
                    .Begin(result)
                    .SetObjectName(name);
                if (compilerOutputItem.IsWarning)
                {
                    validator.WarningDetails(errorText, sourceCode);
                }
                else
                {
                    validator.ErrorDetails(errorText, sourceCode);
                }
                validator.End();
            }

            if (result.Count != 0)
            {
                result.AddWarning(nameof(IKrCompiler), "$KrProcess_ErrorMessage_FullScriptInDetailsWithErrorPointer");
            }

            return result.Build();
        }

        #endregion

        #region implementation

        /// <inheritdoc />
        public IList<string> DefaultUsings { get; } = new List<string>
        {
            "System.Linq",
            "System.Text",
            "System.Threading",
            "System.Threading.Tasks",
            "System.Collections",
            "System.Collections.Generic",
            "Tessa.Platform",
            "Tessa.Platform.Data", // reader.GetValue<int>("Column"), SchemeDbType
            "Tessa.Platform.Runtime",
            "Tessa.Platform.Storage",
            "Tessa.Platform.Collections",
            "Tessa.Platform.Validation",
            "Tessa.Cards",
            "Tessa.Cards.Extensions",
            "Tessa.Files",
            "Tessa.Localization",
            "Tessa.Extensions.Default.Shared", // DefaultCardTypes.SomeTypeID
            "Tessa.Extensions.Default.Shared.Workflow.KrProcess",
            "Tessa.Extensions.Default.Server.Workflow.KrObjectModel",
            "Tessa.Extensions.Default.Server.Workflow.KrCompilers.UserAPI",
            "Tessa.Extensions.Default.Server.Workflow.KrProcess.Workflow",
            "Tessa.Extensions.Default.Server.Workflow.KrProcess.Workflow.Handlers",
            "Unity",
            "Unity.Injection",
            "Unity.Lifetime"
        };

        /// <inheritdoc />
        public IList<string> DefaultReferences { get; } = new List<string>
        {
            "linq2db",
            "DocumentFormat.OpenXml",
            "NLog",
            "Unity.Abstractions",
            "Unity.Container",
            "Tessa",
            "Tessa.Extensions.Default.Server",
            "Tessa.Extensions.Default.Shared",
            "Tessa.Extensions.Server",
            "Tessa.Extensions.Shared",
        };

        /// <inheritdoc/>
        public IList<string> DefaultIgnoreWarnings { get; } = new List<string>
        {
            "CS1998"
        };

        /// <inheritdoc />
        public IKrCompilationResult Compile(IKrCompilationContext krContext)
        {
            var context = this.compiler.CreateContext();
            this.SetSources(krContext, context, out var anchorsMap);
            context.DefaultUsings.UnionWith(krContext.Usings);
            context.DefaultUsings.UnionWith(this.DefaultUsings);
            context.References.UnionWith(krContext.References);
            context.References.UnionWith(this.DefaultReferences);
            context.IgnoreWarnings.UnionWith(this.DefaultIgnoreWarnings);
            var result = this.compiler.Compile(context);
            return new KrCompilationResult(result, FillValidationResult(result, anchorsMap));
        }

        #endregion
    }
}
