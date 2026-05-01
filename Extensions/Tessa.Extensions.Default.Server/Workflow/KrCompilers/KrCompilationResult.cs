using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tessa.Compilation;
using Tessa.Extensions.Default.Server.Workflow.KrCompilers.UserAPI;
using Tessa.Platform;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    /// <inheritdoc cref="IKrCompilationResult"/>
    [Serializable]
    public sealed class KrCompilationResult : IKrCompilationResult
    {
        #region Fields

        [NonSerialized]
        private readonly Lazy<IDictionary<string, Func<IKrScript>>> typesFactories;

        #endregion

        #region Constructors

        private KrCompilationResult()
        {
            this.typesFactories = new Lazy<IDictionary<string, Func<IKrScript>>>(
                this.BuildTypesCache,
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="KrCompilationResult"/>.
        /// </summary>
        /// <param name="result">Результат компиляции.</param>
        /// <param name="validationResult">Сообщения полученные при компиляции.</param>
        public KrCompilationResult(
            ICompilationResult result,
            ValidationResult validationResult) : this()
        {
            this.Result = result ?? throw new ArgumentNullException(nameof(result));
            this.ValidationResult = validationResult ?? throw new ArgumentNullException(nameof(validationResult));
        }

        #endregion

        #region IKrCompilationResult Members

        /// <inheritdoc />
        public ICompilationResult Result { get; }

        /// <inheritdoc />
        public ValidationResult ValidationResult { get; }

        /// <inheritdoc />
        public IKrScript CreateInstance(
            string prefix,
            string alias,
            Guid typeID)
        {
            var className = KrCompilersHelper.FormatClassName(prefix, alias, typeID);

            return this.typesFactories.Value.TryGetValue(className, out var script)
                ? script()
                : throw new MissingCompiledClassException(className);
        }

        #endregion

        #region Private Methods

        private IDictionary<string, Func<IKrScript>> BuildTypesCache()
        {
            if (this.Result.Assembly is null)
            {
                return new Dictionary<string, Func<IKrScript>>(StringComparer.Ordinal);
            }
            var types = this.Result
                .Assembly
                .GetTypes()
                .Where(a => a.Implements<IKrScript>() && !a.IsAbstract);

            var typesCache = new Dictionary<string, Func<IKrScript>>(StringComparer.Ordinal);
            foreach (var type in types)
            {
                if (KrCompilersHelper.CorrectClassName(type.Name))
                {
                    typesCache.Add(
                        type.Name,
                        () => (IKrScript) Activator.CreateInstance(type));
                }
            }
            return typesCache;
        }

        #endregion
    }
}
