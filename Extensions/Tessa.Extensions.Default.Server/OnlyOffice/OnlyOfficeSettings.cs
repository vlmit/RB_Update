using System;

namespace Tessa.Extensions.Default.Server.OnlyOffice
{
    /// <summary>
    /// Представляет собой класс, содержащий настройки редактора OnlyOffice.
    /// </summary>
    public sealed class OnlyOfficeSettings :
        IOnlyOfficeSettings
    {
        #region Constructors

        public OnlyOfficeSettings(
            string? converterUrl,
            TimeSpan loadTimeout)
        {
            this.ConverterUrl = converterUrl;
            this.LoadTimeout = loadTimeout;
        }

        #endregion

        #region IOnlyOfficeSettings Members

        /// <inheritdoc />
        public string? ConverterUrl { get; }

        /// <inheritdoc />
        public TimeSpan LoadTimeout { get; }

        #endregion
    }
}