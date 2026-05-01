using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tessa.Platform;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.Files.VirtualFiles.Compilation
{
    [Serializable]
    public sealed class KrVirtualFileCompilationResult : IKrVirtualFileCompilationResult
    {
        #region Fields

        private readonly ValidationResult validationResult;

        private readonly string buildVersion;

        private readonly DateTime buildDate;

        [NonSerialized]
        private Dictionary<Guid, IKrVirtualFileScript> scripts;

        [NonSerialized]
        private bool isInitialized;

        private object lockObject = new object();

        #endregion

        #region Constructors

        public KrVirtualFileCompilationResult(
            byte[] assembly,
            ValidationResult validationResult)
        {
            this.AssemblyBytes = assembly;
            this.validationResult = validationResult;

            buildVersion = BuildInfo.Version;
            buildDate = BuildInfo.Date;

            Created = DateTime.UtcNow;
        }

        #endregion

        #region IKrVirtualFileCompilationResult Implementation

        public ValidationResult ValidationResult => validationResult ?? ValidationResult.Empty;

        public byte[] AssemblyBytes { get; }

        public DateTime Created { get; }

        public bool IsValid =>
                this.buildDate == BuildInfo.Date
                && this.buildVersion == BuildInfo.Version;

        public IKrVirtualFileScript GetScript(Guid id)
        {
            if (!isInitialized)
            {
                lock (lockObject)
                {
                    if (!isInitialized)
                    {
                        scripts = InitScripts();
                        isInitialized = true;
                    }
                }
            }

            return scripts.TryGetValue(id, out var value) ? value : null;
        }

        #endregion

        #region Private Methods

        private Dictionary<Guid, IKrVirtualFileScript> InitScripts()
        {
            if (AssemblyBytes == null)
            {
                return new Dictionary<Guid, IKrVirtualFileScript>();
            }

            var types = Assembly.Load(AssemblyBytes)
                .GetTypes()
                .Where(x => x.Implements<IKrVirtualFileScript>() && !x.IsAbstract);

            if (types == null
                || !types.Any())
            {
                return new Dictionary<Guid, IKrVirtualFileScript>();
            }

            return types.ToDictionary(type => type.GUID, type => (IKrVirtualFileScript)Activator.CreateInstance(type));
        }

        #endregion
    }
}
