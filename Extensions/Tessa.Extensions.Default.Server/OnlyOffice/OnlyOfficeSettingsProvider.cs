using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Caching;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.OnlyOffice
{
    public sealed class OnlyOfficeSettingsProvider :
        IOnlyOfficeSettingsProvider
    {
        #region Constructors

        public OnlyOfficeSettingsProvider(ICardCache cardCache) =>
            this.cardCache = cardCache ?? throw new ArgumentNullException(nameof(cardCache));

        #endregion

        #region Fields

        private readonly ICardCache cardCache;

        private volatile Card cachedCard;

        private volatile OnlyOfficeSettings cachedSettings;

        #endregion

        #region IOnlyOfficeSettingsProvider Members

        /// <inheritdoc />
        public async ValueTask<IOnlyOfficeSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
        {
            CardCacheValue<Card> result = await this.cardCache.Cards.GetAsync("OnlyOfficeSettings", cancellationToken);
            if (!result.IsSuccess)
            {
                throw new ValidationException(result.ValidationResult);
            }

            Card card = result.GetValue();
            if (ReferenceEquals(card, this.cachedCard))
            {
                OnlyOfficeSettings settings = this.cachedSettings;
                if (settings is not null)
                {
                    return settings;
                }
            }

            Dictionary<string, object> fields = card.Sections["OnlyOfficeSettings"].RawFields;

            var converterUrl = fields.Get<string>("ConverterUrl");
            var loadTimeout = fields.Get<DateTime>("LoadTimeout").TimeOfDay;

            this.cachedCard = card;
            return this.cachedSettings = new OnlyOfficeSettings(converterUrl, loadTimeout);
        }

        #endregion
    }
}