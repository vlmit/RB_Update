// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAutomaticNodeRefreshSettings.cs" company="Syntellect">
//   Tessa Project
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Tessa.Extensions.Default.Shared.Workplaces
{
    /// <summary>
    ///     Описание интерфейса настроек автоматического обновления узлов рабочего места
    /// </summary>
    public interface IAutomaticNodeRefreshSettings
    {
        /// <summary>
        ///     Gets or sets Интервал автоматического обновления
        /// </summary>
        int RefreshInterval { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Признак необходимости обновления табличных данных
        /// </summary>
        bool WithContentDataRefreshing { get; set; }
    }
}