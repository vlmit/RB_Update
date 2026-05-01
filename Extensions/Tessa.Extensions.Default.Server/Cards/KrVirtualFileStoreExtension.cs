using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Server.Files.VirtualFiles;
using Tessa.Extensions.Default.Server.Files.VirtualFiles.Compilation;
using Tessa.Extensions.Default.Shared;
using Tessa.Extensions.Platform.Server.Cards;
using Tessa.Platform.Collections;
using Tessa.Platform.Conditions;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.Cards
{
    /// <summary>
    /// Расширение для сброса кеша виртуальных файлов при сохранении виртуального файла
    /// </summary>
    public sealed class KrVirtualFileStoreExtension : CardStoreExtension
    {
        #region Fields

        private readonly IKrVirtualFileCache virtualFileCache;
        private readonly IKrVirtualFileCompilationCache compilationCache;
        private readonly IKrVirtualFileCompiler compiler;
        private readonly ICardMetadata cardMetadata;
        private readonly IConditionTypesProvider conditionTypesProvider;

        private bool wasInTransaction;

        #endregion

        #region Constructors

        public KrVirtualFileStoreExtension(
            IKrVirtualFileCache virtualFileCache,
            IKrVirtualFileCompilationCache compilationCache,
            IKrVirtualFileCompiler compiler,
            ICardMetadata cardMetadata,
            IConditionTypesProvider conditionTypesProvider)
        {
            this.virtualFileCache = virtualFileCache ?? throw new ArgumentNullException(nameof(virtualFileCache));
            this.compilationCache = compilationCache ?? throw new ArgumentNullException(nameof(compilationCache));
            this.compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            this.cardMetadata = cardMetadata ?? throw new ArgumentNullException(nameof(cardMetadata));
            this.conditionTypesProvider = conditionTypesProvider ?? throw new ArgumentNullException(nameof(conditionTypesProvider));
        }

        #endregion

        #region Base Overrides

        public override Task BeforeRequest(ICardStoreExtensionContext context)
        {
            if (context.Method == CardStoreMethod.Default)
            {
                return this.UpdateConditionsAsync(context);
            }

            return Task.CompletedTask;
        }

        public override Task BeforeCommitTransaction(ICardStoreExtensionContext context)
        {
            this.wasInTransaction = true;
            return Task.CompletedTask;
        }

        public override async Task AfterRequest(ICardStoreExtensionContext context)
        {
            if (!context.RequestIsSuccessful)
            {
                return;
            }

            if (this.wasInTransaction)
            {
                await this.virtualFileCache.InvalidateAsync(context.CancellationToken);
                if (context.Request.Card.Sections.GetOrAdd("KrVirtualFiles").RawFields.ContainsKey("InitializationScenario"))
                {
                    await this.compilationCache.InvalidateAsync(context.CancellationToken);
                }
            }

            if (context.Request.Info.GetCompileMark())
            {
                var card = context.Request.Card;
                var mainSection = card.Sections.GetOrAdd("KrVirtualFiles");
                var name = mainSection.RawFields.TryGet<string>("Name");
                var initializationScenario = mainSection.RawFields.TryGet<string>("InitializationScenario");

                bool needLoadName = name == null,
                     needLoadScenario = initializationScenario == null;

                if (needLoadName || needLoadScenario)
                {
                    await using var _ = context.DbScope.Create();

                    var db = context.DbScope.Db;
                    var builder = context.DbScope.BuilderFactory.Select().Top(1);
                    int scenarioIndex = 0;

                    if (needLoadName)
                    {
                        builder.C("Name");
                        scenarioIndex++;
                    }
                    if (needLoadScenario)
                    {
                        builder.C("InitializationScenario");
                    }
                    builder.From("KrVirtualFiles").NoLock()
                        .Where().C("ID").Equals().P("ID")
                        .Limit(1);

                    db.SetCommand(
                        builder.Build(),
                        db.Parameter("ID", card.ID))
                        .LogCommand();

                    await using var reader = await db.ExecuteReaderAsync(context.CancellationToken);
                    if (await reader.ReadAsync(context.CancellationToken))
                    {
                        if (needLoadName)
                        {
                            name = reader.GetValue<string>(0);
                        }
                        if (needLoadScenario)
                        {
                            initializationScenario = reader.GetValue<string>(scenarioIndex);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(initializationScenario))
                {
                    context.ValidationResult.AddInfo(this, "$KrVirtualFiles_ScriptsNotFound");
                    return;
                }

                var krVirtualFile = new KrVirtualFile
                {
                    ID = card.ID,
                    Name = name,
                    InitializationScenario = initializationScenario,
                };

                var compilationContext = this.compiler.CreateContext();
                compilationContext.Files.Add(krVirtualFile);
                var result = this.compiler.Compile(compilationContext);

                if (result.ValidationResult.Items.Count > 0)
                {
                    context.ValidationResult.Add(result.ValidationResult);
                }
                else
                {
                    context.ValidationResult.AddInfo(this, "$KrVirtualFiles_CompilationSuccessful");
                }
            }
        }

        #endregion

        #region Private Methods


        private async Task UpdateConditionsAsync(ICardStoreExtensionContext context)
        {
            // Алгортим сохранения
            // 1. Проверяем наличие изменений секций с условиями. Если есть, продолжаем
            // 2. Загружаем текущие настройки и десериализуем
            // 3. Мержим изменения
            // 4. Сериализуем настройки и записываем в поле карточки
            var mainCard = context.Request.Card;
            HashSet<string> checkSections = new HashSet<string>() { ConditionHelper.ConditionSectionName };

            var conditionBaseType = await this.cardMetadata.GetMetadataForTypeAsync(ConditionHelper.ConditionsBaseTypeID, context.CancellationToken);
            var sections = await conditionBaseType.GetSectionsAsync(context.CancellationToken);
            checkSections.AddRange(sections.Select(x => x.Name));

            if (mainCard.Sections.Any(x => checkSections.Contains(x.Key)))
            {
                await using (context.DbScope.Create())
                {
                    var db = context.DbScope.Db;
                    var oldSettings =
                        await db.SetCommand(
                            context.DbScope.BuilderFactory
                                .Select().Top(1).C("Conditions")
                                .From("KrVirtualFiles").NoLock()
                                .Where().C("ID").Equals().P("CardID")
                                .Limit(1)
                                .Build(),
                            db.Parameter("CardID", mainCard.ID))
                            .LogCommand()
                            .ExecuteAsync<string>(context.CancellationToken);

                    var oldCard = new Card();
                    oldCard.Sections.GetOrAdd("KrVirtualFiles").RawFields["Conditions"] = oldSettings;
                    await ConditionHelper.DeserializeConditionsToEntrySectionAsync(
                        oldCard,
                        this.cardMetadata,
                        this.conditionTypesProvider,
                        "KrVirtualFiles",
                        "Conditions",
                        false,
                        context.CancellationToken);

                    foreach (var section in mainCard.Sections.Values)
                    {
                        if (checkSections.Contains(section.Name))
                        {
                            var oldSection = oldCard.Sections.GetOrAdd(section.Name);
                            oldSection.Type = section.Type;

                            CardHelper.MergeSection(section, oldSection);
                            mainCard.Sections.Remove(section.Name);
                        }
                    }
                    var conditionsSection = oldCard.Sections.GetOrAddTable(ConditionHelper.ConditionSectionName);

                    foreach (var conditionRow in conditionsSection.Rows)
                    {
                        await ConditionHelper.SerializeConditionRowAsync(
                            conditionRow,
                            oldCard,
                            DefaultCardTypes.KrVirtualFileTypeID,
                            this.cardMetadata,
                            this.conditionTypesProvider,
                            true,
                            context.CancellationToken);
                    }

                    mainCard.Sections.GetOrAdd("KrVirtualFiles").RawFields["Conditions"] =
                        StorageHelper.SerializeToTypedJson((List<object>)conditionsSection.Rows.GetStorage(), false);
                }
            }
        }

        #endregion
    }
}
