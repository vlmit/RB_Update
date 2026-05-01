using System.Collections.Generic;
using Tessa.Cards;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Shared.StorageMapping.StorageMappingHandlers
{
    /// <summary>
    /// Параметры сериализации для выгрузки контента карточек во внешние файлы
    /// для типа <see cref="DefaultCardTypes.KrStageTemplateTypeID"/>.
    /// </summary>
    public class KrStageTemplateStorageMappingHandler : KrProcessStorageMappingHandlerBase
    {
        /// <inheritdoc />
        public override IList<IStorageContentMapping> GetContentMappings(Card card)
        {
            var pathMappings = base.GetContentMappings(card);

            pathMappings.Add(
                new StorageContentMapping(
                    "Sections." + KrConstants.KrStageTemplates.Name + ".Fields." + KrConstants.SourceBefore,
                    KrConstants.SourceBefore + ".cs"));
            pathMappings.Add(
                new StorageContentMapping(
                    "Sections." + KrConstants.KrStageTemplates.Name + ".Fields." + KrConstants.SourceAfter,
                    KrConstants.SourceAfter + ".cs"));
            pathMappings.Add(
                new StorageContentMapping(
                    "Sections." + KrConstants.KrStageTemplates.Name + ".Fields." + KrConstants.SqlCondition,
                    KrConstants.SqlCondition + ".sql"));
            pathMappings.Add(
                new StorageContentMapping(
                    "Sections." + KrConstants.KrStageTemplates.Name + ".Fields." + KrConstants.SourceCondition,
                    KrConstants.SourceCondition + ".cs"));

            return pathMappings;
        }
    }
}