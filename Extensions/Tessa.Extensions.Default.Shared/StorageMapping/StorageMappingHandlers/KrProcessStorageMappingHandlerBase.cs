using System.Collections;
using System.Collections.Generic;
using Tessa.Cards;
using Tessa.Extensions.Default.Shared.Workflow.KrCompilers;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform.Storage;
using Tessa.Platform.Storage.Mapping;

namespace Tessa.Extensions.Default.Shared.StorageMapping.StorageMappingHandlers
{
    /// <summary>
    /// Базовый абстрактный класс с параметрами сериализации для выгрузки контента карточек во внешние файлы
    /// для некоторых типов workflow.
    /// </summary>
    public abstract class KrProcessStorageMappingHandlerBase : IStorageMappingHandler
    {
        /// <inheritdoc />
        public virtual IList<IStorageContentMapping> GetContentMappings(Card card)
        {
            var result = new List<IStorageContentMapping>
            {
                new StorageContentMapping(
                    "Sections." + KrConstants.KrStages.Name + ".Rows[]." + KrConstants.RuntimeSourceAfter,
                    KrConstants.RuntimeSourceAfter + ".cs",
                    KrConstants.RowID),
                new StorageContentMapping(
                    "Sections." + KrConstants.KrStages.Name + ".Rows[]." + KrConstants.RuntimeSourceBefore,
                    KrConstants.RuntimeSourceBefore + ".cs",
                    KrConstants.RowID),
                new StorageContentMapping(
                    "Sections." + KrConstants.KrStages.Name + ".Rows[]." + KrConstants.RuntimeSourceCondition,
                    KrConstants.RuntimeSourceCondition + ".cs",
                    KrConstants.RowID),
                new StorageContentMapping(
                    "Sections." + KrConstants.KrStages.Name + ".Rows[]." + KrConstants.RuntimeSqlCondition,
                    KrConstants.RuntimeSqlCondition + ".sql",
                    KrConstants.RowID),
                new StorageContentMapping(
                    "Sections." + KrConstants.KrStages.Name + ".Rows[]." + KrConstants.KrStages.SqlApproverRole,
                    KrConstants.KrStages.SqlApproverRole + ".sql",
                    KrConstants.RowID),
            };

            if (card.Sections.TryGetValue(KrConstants.KrStages.Name, out var krStagesSection))
            {
                for (var i = 0; i < krStagesSection.Rows.Count; i++)
                {
                    var stageRow = krStagesSection.Rows[i];

                    // Объект параметров этапа маршрута может не содержать полей относящихся к другим типам этапов добавленных после создания.
                    // Она не требуется для функционирования экспортируемого типа этапа.
                    var settings = stageRow.Fields.TryGet<IDictionary<string, object>>(KrConstants.KrStages.Settings);

                    if (settings is not null
                        && settings.ContainsKey(KrConstants.KrAcquaintanceSettingsVirtual.AliasMetadata))
                    {
                        result.Add(
                            new StorageContentMapping(
                                "Sections." + KrConstants.KrStages.Name + $".Rows[{i}]." +
                                KrConstants.KrStages.Settings + "." +
                                KrConstants.KrAcquaintanceSettingsVirtual.AliasMetadata,
                                KrConstants.KrAcquaintanceSettingsVirtual.AliasMetadata + ".cs",
                                "^." + KrConstants.RowID));
                    }

                    var extraSources = stageRow.Fields.TryGet<IList>(KrConstants.KrStages.ExtraSources +
                        CardHelper.PlainJsonSuffix);

                    if (extraSources is null)
                    {
                        continue;
                    }

                    for (var j = 0; j < extraSources.Count; j++)
                    {
                        var extraSource = (IDictionary<string, object>)extraSources[j];
                        result.Add(
                            new StorageContentMapping(
                                "Sections." + KrConstants.KrStages.Name + $".Rows[{i}]." +
                                KrConstants.KrStages.ExtraSources + CardHelper.PlainJsonSuffix + $"[{j}]." + nameof(IExtraSource.Source),
                                extraSource.Get<string>(nameof(IExtraSource.Name)) + ".cs",
                                "^.^." + KrConstants.RowID));
                    }
                }
            }

            return result;
        }
    }
}