// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Settings.cs" company="Syntellect">
//   Tessa Project
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Shared.Workplaces
{
    /// <summary>
    /// Настройки для рабочего места руководителя.
    /// </summary>
    [Serializable]
    public class ManagerWorkplaceSettings :
        IStorageSerializable
    {
        /// <summary>
        /// Имя столбца содержащего изображение для выбранной плитки.
        /// </summary>
        public string ActiveImageColumnName { get; set; }

        /// <summary>
        /// Идентификатор карточки.
        /// </summary>
        public Guid CardId { get; set; }

        /// <summary>
        /// Имя столбца содержащего количество.
        /// </summary>
        public string CountColumnName { get; set; }

        /// <summary>
        /// Имя столбца содержащего изображение для плитки над которой находится курсор.
        /// </summary>
        public string HoverImageColumnName { get; set; }

        /// <summary>
        /// Имя столбца содержащего изображение для невыбранной плитки.
        /// </summary>
        public string InactiveImageColumnName { get; set; }

        /// <summary>
        /// Имя столбца содержащего заголовок плитки.
        /// </summary>
        public string TileColumnName { get; set; }

        #region IStorageSerializable Members

        /// <doc path='info[@type="IStorageSerializable" and @item="Serialize"]'/>
        public void Serialize(Dictionary<string, object> storage)
        {
            storage[nameof(this.ActiveImageColumnName)] = this.ActiveImageColumnName;
            storage[nameof(this.CardId)] = this.CardId;
            storage[nameof(this.CountColumnName)] = this.CountColumnName;
            storage[nameof(this.HoverImageColumnName)] = this.HoverImageColumnName;
            storage[nameof(this.InactiveImageColumnName)] = this.InactiveImageColumnName;
            storage[nameof(this.TileColumnName)] = this.TileColumnName;
        }

        /// <doc path='info[@type="IStorageSerializable" and @item="Deserialize"]'/>
        public object Deserialize(Dictionary<string, object> storage)
        {
            this.ActiveImageColumnName = storage.TryGet<string>(nameof(this.ActiveImageColumnName));
            this.CardId = storage.TryGet<Guid>(nameof(this.CardId));
            this.CountColumnName = storage.TryGet<string>(nameof(this.CountColumnName));
            this.HoverImageColumnName = storage.TryGet<string>(nameof(this.HoverImageColumnName));
            this.InactiveImageColumnName = storage.TryGet<string>(nameof(this.InactiveImageColumnName));
            this.TileColumnName = storage.TryGet<string>(nameof(this.TileColumnName));
            return this;
        }

        #endregion
    }
}
