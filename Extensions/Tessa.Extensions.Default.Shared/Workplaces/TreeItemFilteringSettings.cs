#region Usings

using System;
using System.Collections.Generic;
using Tessa.Platform.Storage;

#endregion

namespace Tessa.Extensions.Default.Shared.Workplaces
{
    [Serializable]
    public sealed class TreeItemFilteringSettings : 
        IStorageSerializable,
        ITreeItemFilteringSettings
    {
        #region Fields

        private List<string> parameters;
        private List<string> refSections;

        #endregion

        #region Constructors and Destructors

        /// <inheritdoc />
        public TreeItemFilteringSettings()
        {
            this.RefSections = new List<string>();
            this.Parameters = new List<string>();
        }

        #endregion

        #region Public properties

        /// <inheritdoc />
        public List<string> Parameters
        {
            get => this.parameters;
            set => this.parameters = value ?? new List<string>();
        }

        /// <inheritdoc />
        public List<string> RefSections
        {
            get => this.refSections;
            set => this.refSections = value ?? new List<string>();
        }

        #endregion

        #region IStorageSerializable Members

        /// <doc path='info[@type="IStorageSerializable" and @item="Serialize"]'/>
        public void Serialize(Dictionary<string, object> storage)
        {
            storage[nameof(this.Parameters)] = this.Parameters?.Count > 0 ? this.Parameters : null;
            storage[nameof(this.RefSections)] = this.RefSections?.Count > 0 ? this.RefSections : null;
        }

        /// <doc path='info[@type="IStorageSerializable" and @item="Deserialize"]'/>
        public object Deserialize(Dictionary<string, object> storage)
        {
            this.Parameters = storage.TryGet<List<string>>(nameof(this.Parameters)) ?? new List<string>();
            this.RefSections = storage.TryGet<List<string>>(nameof(this.RefSections), new List<string>());
            return this;
        }

        #endregion
    }
}
