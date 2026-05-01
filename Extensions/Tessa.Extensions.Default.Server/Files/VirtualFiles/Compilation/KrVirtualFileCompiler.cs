using System;
using System.Collections.Generic;
using Tessa.Compilation;
using Tessa.Localization;
using Tessa.Platform.Collections;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.Files.VirtualFiles.Compilation
{
    public sealed class KrVirtualFileCompiler : IKrVirtualFileCompiler
    {
        #region Fields

        private readonly Tuple<string, string>[] parameters = new Tuple<string, string>[]
        {
            new Tuple<string, string>(nameof(IKrVirtualFileScriptContext), "context"),
        };

        private readonly ICompiler compiler;
        private readonly ICompilationSourceProvider sourceProvider;

        #endregion

        #region Constructors

        public KrVirtualFileCompiler(
            ICompiler compiler,
            ICompilationSourceProvider sourceProvider)
        {
            this.compiler = compiler;
            this.sourceProvider = sourceProvider;
        }

        #endregion

        #region IKrVirtualFileCompiler Implementation

        public IList<string> DefaultUsings { get; } = new List<string>
        {
            "System",
            "System.Runtime.InteropServices",
            "System.Linq",
            "System.Text",
            "System.Threading",
            "System.Threading.Tasks",
            "System.Collections",
            "System.Collections.Generic",
            "Tessa.Extensions.Default.Server.Files.VirtualFiles",
            "Tessa.Extensions.Default.Server.Files.VirtualFiles.Compilation",
            "Tessa.Platform",
            "Tessa.Platform.Data",
            "Tessa.Platform.Runtime",
            "Tessa.Platform.Storage",
            "Tessa.Platform.Collections",
            "Tessa.Platform.Validation",
            "Tessa.Cards",
            "Tessa.Files",
            "Tessa.Localization",
            "Unity",
        };

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
        public IKrVirtualFileCompilationResult Compile(IKrVirtualFileCompilationContext context)
        {
            var compilationContext = compiler.CreateContext();
            compilationContext.DefaultUsings.AddRange(DefaultUsings);
            compilationContext.References.AddRange(DefaultReferences);

            foreach (var file in context.Files)
            {
                var source = GenerateSource(file);
                if (source != null)
                {
                    compilationContext.Sources.Add(source);
                }
            }

            var compilationResult = compiler.Compile(compilationContext);

            return new KrVirtualFileCompilationResult(
                compilationResult.AssemblyBytes,
                PrepareValidationResult(compilationResult));
        }

        public IKrVirtualFileCompilationContext CreateContext()
        {
            return new KrVirtualFileCompilationContext();
        }

        #endregion

        #region Private Methods

        private ICompilationSource GenerateSource(IKrVirtualFile file)
        {
            if (string.IsNullOrWhiteSpace(file.InitializationScenario))
            {
                return null;
            }

            var builder = sourceProvider.AcquireSyntaxTree();
            builder
                .SetID(file.ID)
                .SetName(file.Name)
                .Namespace("Tessa.Extensions.Default.Servier.Files.VirtualFiles.Generated")
                .Class(
                    "VirtualFile_" + file.ID.ToString("N"),
                    AccessModifier.Public,
                    new[] { nameof(IKrVirtualFileScript) },
                    guid: file.ID);

            builder
                .AddMethod(
                    "Task",
                    nameof(IKrVirtualFileScript.InitializationScenarioAsync),
                    parameters,
                    file.InitializationScenario,
                    isAsync: true);

            return builder.Build();
        }

        private ValidationResult PrepareValidationResult(ICompilationResult compilationResult)
        {
            var validationResult = new ValidationResultBuilder();

            foreach (var compilerOutputItem in compilationResult.CompilerOutput)
            {
                var source = compilerOutputItem.Source;
                var methodName = CompilationHelper.GetMemberName(source, compilerOutputItem);
                string sourceCode = CompilationHelper.FormatErrorIntoMember(source, compilerOutputItem, methodName);

                var errorText = LocalizationManager.Format(
                    "$KrVirtualFiles_CompilationErrorTemplate",
                    source.Name,
                    compilerOutputItem.ErrorText);

                var validator = ValidationSequence
                    .Begin(validationResult)
                    .SetObjectName(this);

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

            return validationResult.Build();
        }

        #endregion
    }
}
