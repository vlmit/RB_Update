// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutomaticNodeRefreshSettings.cs" company="Syntellect">
//   Tessa Project
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Shared.Workplaces
{
    /// <summary>
    ///     Настройки автоматического обновления узлов рабочего места.
    /// </summary>
    public class AutomaticNodeRefreshSettings :
        IStorageSerializable,
        IAutomaticNodeRefreshSettings
    {
        // нельзя использовать базовый класс StorageSerializable, чтобы не сломать сериализацию уже существующих настроек,
        // т.к. иначе будет использоваться DataContract сериализация вместо BinaryFormatter

        /// <inheritdoc />
        public AutomaticNodeRefreshSettings()
        {
            this.RefreshInterval = 300;
            this.WithContentDataRefreshing = true;
        }

        /// <inheritdoc />
        public int RefreshInterval { get; set; }

        /// <inheritdoc />
        public bool WithContentDataRefreshing { get; set; }

        #region IStorageSerializable Members

        /// <doc path='info[@type="IStorageSerializable" and @item="Serialize"]'/>
        public void Serialize(Dictionary<string, object> storage)
        {
            storage[nameof(this.RefreshInterval)] = this.RefreshInterval;
            storage[nameof(this.WithContentDataRefreshing)] = this.WithContentDataRefreshing;
        }

        /// <doc path='info[@type="IStorageSerializable" and @item="Deserialize"]'/>
        public object Deserialize(Dictionary<string, object> storage)
        {
            this.RefreshInterval = storage.TryGet<int>(nameof(this.RefreshInterval));
            this.WithContentDataRefreshing = storage.TryGet<bool>(nameof(this.WithContentDataRefreshing));
            return this;
        }

        #endregion
    }
}
