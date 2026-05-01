using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Caching;
using Tessa.Cards.Extensions;
using Tessa.Cards.Workflow;
using Tessa.Extensions.Default.Server.Workflow.KrCompilers;
using Tessa.Extensions.Default.Server.Workflow.KrProcess.Scope;
using Tessa.Extensions.Default.Server.Workflow.KrProcess.Serialization;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using static Tessa.Extensions.Default.Shared.Workflow.KrProcess.KrConstants;

namespace Tessa.Extensions.Default.Server.Workflow.KrProcess.Requests
{
    public sealed class KrCardStoreExtension : CardStoreExtension
    {
        #region fields

        private readonly ICardCache cardCache;
        private readonly IKrStageSerializer serializer;
        private readonly IKrScope krScope;
        private readonly IKrProcessCache krProcessCache;

        #endregion

        #region constructor

        public KrCardStoreExtension(
            ICardCache cardCache,
            IKrStageSerializer serializer,
            IKrScope krScope,
            IKrProcessCache krProcessCache)
        {
            this.cardCache = cardCache;
            this.serializer = serializer;
            this.krScope = krScope;
            this.krProcessCache = krProcessCache;
        }

        #endregion

        #region private

        private bool HasChanges(Card card)
        {
            var storeMode = card.StoreMode;
            switch (storeMode)
            {
                case CardStoreMode.Insert:
                    // Интересует исключительно виртуальная версия для main карточек
                    return card.Sections.TryGetValue(KrApprovalCommonInfo.Virtual, out var aiSec) && aiSec.HasChanges()
                        || this.serializer
                            .SettingsSectionNames
                            .Any(p => card.Sections.TryGetValue(p, out var sec) && sec.HasChanges());
                // Интересует исключительно виртуальная версия для main карточек
                case CardStoreMode.Update:
                    return card.Sections.TryGetValue(KrApprovalCommonInfo.Virtual, out _)
                        || this.serializer
                        .SettingsSectionNames
                        .Any(p => card.Sections.TryGetValue(p, out _));
                default:
                    throw new InvalidOperationException($"Unknown CardStoreMode.{storeMode.ToString()}");
            }
        }

        private async ValueTask<bool> CheckCardAsync(
            Card card,
            Card satellite,
            IDbScope dbScope,
            IValidationResultBuilder validationResult,
            CancellationToken cancellationToken = default)
        {
            if (!this.CheckApprovalInfoSection(card, satellite, validationResult))
            {
                return false;
            }

            var modifiedStages = new HashSet<Guid>();

            foreach (var sectionName in this.serializer.SettingsSectionNames)
            {
                if (card.Sections.TryGetValue(sectionName, out var section))
                {
                    foreach (var row in section.Rows)
                    {
                        if (row.TryGetValue(Keys.ParentStageRowID, out var parentStageRowIDObj)
                            && parentStageRowIDObj is Guid parentStageRowID)
                        {
                            modifiedStages.Add(parentStageRowID);
                        }
                    }
                }
            }

            if (modifiedStages.Count == 0)
            {
                return true;
            }

            var sqlCondition = dbScope.BuilderFactory
                .Select().Top(1)
                .V(1)
                .From(KrStages.Name, "s").NoLock()
                .Where().C("s", RowID).Q(SqlHelper.GetQuotedEqualsExpression(modifiedStages)).N()
                .And().C("s", StateID).NotEquals().V((int) KrStageState.Inactive)
                .Limit(1)
                .Build();
            var hasError = await dbScope.Db
                .SetCommand(sqlCondition)
                .LogCommand()
                .ExecuteAsync<bool>(cancellationToken);
            if (hasError)
            {
                validationResult.AddError(this, "$KrProcess_ProcessWasModified");
                return false;
            }
            return true;
        }

        private async Task FillSatelliteAsync(
            Card mainCard,
            Card satelliteCard,
            IKrProcessCache krProcessCache,
            CancellationToken cancellationToken = default)
        {
            IDictionary<Guid, IDictionary<string, object>> stageStorages = null;
            StringDictionaryStorage<CardSection> rows;
            if (satelliteCard.TryGetStagesSection(out var krStagesSec)
                && (rows = mainCard.TryGetSections()) != null)
            {
                stageStorages = await this.serializer.MergeStageSettingsAsync(krStagesSec, rows, cancellationToken);
            }

            new KrProcessSectionMapper(mainCard, satelliteCard)
                .MapApprovalCommonInfo()
                .MapKrStages();

            await this.serializer.UpdateStageSettingsAsync(satelliteCard, mainCard, stageStorages, krProcessCache, cancellationToken: cancellationToken);
        }

        private bool CheckApprovalInfoSection(
            Card mainCard,
            Card satelliteCard,
            IValidationResultBuilder validationResult)
        {
            var satelliteInfoSection = satelliteCard.GetApprovalInfoSection();
            var stateID = satelliteInfoSection.Fields.TryGet<int?>(StateID);
            if (stateID.HasValue
                && stateID != (int)KrState.Draft
                && mainCard.TryGetKrApprovalCommonInfoSection(out var approvalInfoSec)
                && approvalInfoSec.Fields.TryGetValue(KrApprovalCommonInfo.AuthorID, out var authorID)
                && authorID == null)
            {
                validationResult.AddError(this, "$KrMessages_InitiatorCannotBeRemoved");
                return false;
            }

            return true;
        }

        private bool HasReasonToTransaction(
            ICardStoreExtensionContext context) =>
            this.HasChanges(context.Request.Card)
                || context.Request.TryGetStartingProcessName() != null;

        #endregion

        #region base overrides

        public override async Task BeforeRequest(ICardStoreExtensionContext context)
        {
            if (!context.ValidationResult.IsSuccessful()
                || !await KrComponentsHelper.HasBaseAsync(context.Request.Card.TypeID, this.cardCache, context.CancellationToken)
                || !this.HasReasonToTransaction(context))
            {
                return;
            }

            context.Request.ForceTransaction = true;
        }

        public override async Task AfterBeginTransaction(ICardStoreExtensionContext context)
        {
            var mainCardID = context.Request.Card.ID;
            Card satellite;
            if (!context.ValidationResult.IsSuccessful()
                || (await KrComponentsHelper.GetKrComponentsAsync(context.Request.Card.TypeID, this.cardCache, context.CancellationToken)).HasNot(KrComponents.Base)
                || !this.HasReasonToTransaction(context)
                || (satellite = await this.krScope.GetKrSatelliteAsync(mainCardID, cancellationToken: context.CancellationToken)) == null
                || !await this.CheckCardAsync(context.Request.Card, satellite, context.DbScope, context.ValidationResult, context.CancellationToken))
            {
                return;
            }

            await this.FillSatelliteAsync(context.Request.Card, satellite, krProcessCache, context.CancellationToken);

            if (satellite.StoreMode == CardStoreMode.Insert
                && context.Request.TryGetStartingProcessName() != null)
            {
                await this.krScope.StoreSatelliteExplicitlyAsync(mainCardID, cancellationToken: context.CancellationToken);
            }
        }

        #endregion
    }
}
