using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.ComponentModel;
using Tessa.Cards.Numbers;
using Tessa.Cards.Workflow;
using Tessa.Extensions.Default.Server.Workflow.KrProcess.Serialization;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Scopes;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.Workflow.KrProcess.Scope
{
    /// <summary>
    /// Объект, предоставляющий методы для управления текущим контекстом <see cref="KrScopeContext"/>.
    /// </summary>
    /// <remarks>После завершения работы с объектом, для выполнения задач связанных с освобождением ресурсов, вызовите метод <see cref="ExitAsync(IValidationResultBuilder)"/>.</remarks>
    public sealed class KrScopeLevel
    {
        #region Fields

        private readonly ICardRepository cardRepository;
        private readonly IKrTokenProvider tokenProvider;
        private readonly IKrTypesCache krTypesCache;
        private readonly IKrStageSerializer serializer;
        private readonly ICardGetStrategy getStrategy;
        private readonly ICardStreamServerRepository streamServerRepository;

        private readonly IInheritableScopeInstance<KrScopeContext> scope;
        private readonly IDbScope dbScope;
        private readonly ICardMetadata cardMetadata;

        #endregion

        #region Constructors

        public KrScopeLevel(
            ICardRepository cardRepository,
            IKrTokenProvider tokenProvider,
            IKrTypesCache krTypesCache,
            IKrStageSerializer serializer,
            ICardGetStrategy getStrategy,
            ICardTransactionStrategy cardCardTransactionStrategy,
            IDbScope dbScope,
            ICardMetadata cardMetadata,
            ICardStreamServerRepository streamServerRepository,
            bool withReaderLocks)
        {
            Check.ArgumentNotNull(cardRepository, nameof(cardRepository));
            Check.ArgumentNotNull(tokenProvider, nameof(tokenProvider));
            Check.ArgumentNotNull(krTypesCache, nameof(krTypesCache));
            Check.ArgumentNotNull(serializer, nameof(serializer));
            Check.ArgumentNotNull(getStrategy, nameof(getStrategy));
            Check.ArgumentNotNull(cardCardTransactionStrategy, nameof(cardCardTransactionStrategy));
            Check.ArgumentNotNull(dbScope, nameof(dbScope));
            Check.ArgumentNotNull(cardMetadata, nameof(cardMetadata));
            Check.ArgumentNotNull(streamServerRepository, nameof(streamServerRepository));

            this.cardRepository = cardRepository;
            this.tokenProvider = tokenProvider;
            this.krTypesCache = krTypesCache;
            this.serializer = serializer;
            this.getStrategy = getStrategy;
            this.CardTransactionStrategy = cardCardTransactionStrategy;
            this.dbScope = dbScope;
            this.cardMetadata = cardMetadata;
            this.streamServerRepository = streamServerRepository;
            this.WithReaderLocks = withReaderLocks;

            this.scope = KrScopeContext.Create();
            this.scope.Value.LevelStack.Push(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Идентификатор объекта.
        /// </summary>
        public Guid LevelID { get; } = Guid.NewGuid();

        /// <summary>
        /// Значение, показывающее, что для этого объекта были выполнены действия по освобождению ресурсов.
        /// </summary>
        public bool Exited { get; private set; } // = false;

        /// <inheritdoc cref="ICardTransactionStrategy" path="/summary"/>
        public ICardTransactionStrategy CardTransactionStrategy { get; }

        public bool WithReaderLocks { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Обрабатывает изменения в основной карточке и её сателлитах, содержащих информацию о процессах.
        /// </summary>
        /// <param name="mainCardID">Идентификатор карточки документа, в которой запущен процесс.</param>
        /// <param name="validationResult"><inheritdoc cref="IValidationResultBuilder" path="/summary"/></param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        public async Task ApplyChangesAsync(
            Guid mainCardID,
            IValidationResultBuilder validationResult,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(validationResult, nameof(validationResult));

            var scopeContext = KrScopeContext.Current;
            if (scopeContext is null)
            {
                return;
            }

            var locks = scopeContext.Locks;

            if (scopeContext.MainKrSatellites.TryGetItem(mainCardID, out var satellite)
                && !locks.Contains(satellite.ID))
            {
                ProcessInfoCacheHelper.Update(this.serializer, satellite);
                if (satellite.StoreMode == CardStoreMode.Insert || satellite.HasChanges())
                {
                    await this.StoreCardAsync(
                        satellite,
                        null,
                        validationResult,
                        cancellationToken: cancellationToken);
                }
            }

            if (!validationResult.IsSuccessful())
            {
                return;
            }

            var forceAffectVersion = scopeContext.ForceIncrementCardVersion.Remove(mainCardID);
            if (scopeContext.Cards.TryGetValue(mainCardID, out var mainCard)
                && !locks.Contains(mainCard.ID))
            {
                if (mainCard.HasChanges()
                    || mainCard.TryGetWorkflowQueue()?.Items?.Count > 0
                    || mainCard.HasNumberQueueToProcess()
                    || forceAffectVersion)
                {
                    scopeContext.CardFileContainers.TryGetValue(mainCardID, out var container);
                    await this.StoreCardAsync(
                        mainCard,
                        container,
                        validationResult,
                        forceAffectVersion: forceAffectVersion,
                        cancellationToken: cancellationToken);
                    // Освобождение ресурсов файлового контейнера будет выполнено в методе KrScopeLevel.Exit() после завершения сохранения карточки или при вызове KrScopeLevel.Dispose().
                }
            }
            else if (forceAffectVersion)
            {
                // Карточка не загружена, но нужно обязательно увеличить ее версию
                await this.ForceIncrementVersionAsync(
                    mainCardID,
                    validationResult,
                    cancellationToken);
            }

            if (!validationResult.IsSuccessful())
            {
                return;
            }

            var deletedSecondaryKrSatellites = new List<KeyValuePair<Guid, Card>>(scopeContext.SecondaryKrSatellites.Count);
            foreach (var secondarySatellitePair in scopeContext.SecondaryKrSatellites)
            {
                var secondarySatellite = secondarySatellitePair.Value;
                var secondaryProcessMainCardID = secondarySatellite
                    .GetApprovalInfoSection()
                    .Fields[KrConstants.KrProcessCommonInfo.MainCardID];

                if (locks.Contains(secondarySatellite.ID)
                    || !secondaryProcessMainCardID.Equals(mainCardID))
                {
                    continue;
                }

                var currentRowID = secondarySatellite
                    .GetApprovalInfoSection()
                    .Fields[KrConstants.KrProcessCommonInfo.CurrentApprovalStageRowID];

                // Процесс по сателлиту закончился, сателлит больше не нужен.
                if (currentRowID is null)
                {
                    // Если карточка уже создана, ее нужно удалить
                    if (secondarySatellite.StoreMode == CardStoreMode.Update)
                    {
                        await this.DeleteCardAsync(
                            secondarySatellite,
                            validationResult,
                            cancellationToken);
                    }
                    // Если карточка только создана, но не сохранялась, можно просто забыть про нее.

                    deletedSecondaryKrSatellites.Add(secondarySatellitePair);
                }
                else
                {
                    ProcessInfoCacheHelper.Update(this.serializer, secondarySatellite);
                    if (secondarySatellite.StoreMode == CardStoreMode.Insert
                        || secondarySatellite.HasChanges())
                    {
                        await this.StoreCardAsync(
                            secondarySatellite,
                            null,
                            validationResult,
                            cancellationToken: cancellationToken);
                    }
                }
            }

            foreach (var deletedSecondarySatellitePair in deletedSecondaryKrSatellites)
            {
                scopeContext.SecondaryKrSatellites.Remove(deletedSecondarySatellitePair.Key);
                scopeContext.Cards.Remove(deletedSecondarySatellitePair.Value.ID);
            }
        }

        /// <summary>
        /// Выполняет задачи, связанные с высвобождением ресурсов этого объекта.
        /// </summary>
        /// <param name="validationResult"><inheritdoc cref="IValidationResultBuilder" path="/summary"/></param>
        public async ValueTask ExitAsync(
            IValidationResultBuilder validationResult)
        {
            Check.ArgumentNotNull(validationResult, nameof(validationResult));

            if (this.Exited)
            {
                return;
            }

            this.Exited = true;

            if (this.scope.Value.LevelStack.Pop() != this)
            {
                validationResult.AddError(this, "Trying to exit from non-top level.");
                return;
            }

            var ctx = KrScopeContext.Current;
            this.scope.Dispose();

            if (!ctx.IsDisposed)
            {
                return;
            }

            foreach ((_, var container) in ctx.CardFileContainers)
            {
                if (container is not null)
                {
                    try
                    {
                        await container.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        validationResult.AddException(this, ex, true);
                    }
                }
            }

            foreach (var disposableObject in ctx.DisposableObjects)
            {
                try
                {
                    disposableObject?.Dispose();
                }
                catch (Exception ex)
                {
                    validationResult.AddException(this, ex, true);
                }
            }

            foreach (var disposableObject in ctx.AsyncDisposableObjects)
            {
                if (disposableObject is not null)
                {
                    try
                    {
                        await disposableObject.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        validationResult.AddException(this, ex, true);
                    }
                }
            }

            if (ctx.Locks.Count != 0)
            {
                validationResult.AddError(
                    this,
                    $"Disposed KrScope contains locks ({string.Join(", ", ctx.Locks)}). " +
                    "All cards inside scope must be saved at the end (no card should be locked).");
            }
        }

        #endregion

        #region Private Methods

        private async Task StoreCardAsync(
            Card card,
            ICardFileContainer fileContainer,
            IValidationResultBuilder validationResult,
            bool forceAffectVersion = false,
            CancellationToken cancellationToken = default)
        {
            var storedCard = card.Clone();
            card.RemoveChanges(CardRemoveChangesDeletedHandling.Remove);
            card.RemoveWorkflowQueue();
            card.RemoveNumberQueue();

            await this.StoreCardAsync(
                card,
                storedCard,
                fileContainer,
                validationResult,
                forceAffectVersion,
                cancellationToken);
        }

        private async Task StoreCardAsync(
            Card originalCard,
            Card storedCard,
            ICardFileContainer fileContainer,
            IValidationResultBuilder validationResult,
            bool forceAffectVersion = false,
            CancellationToken cancellationToken = default)
        {
            var storeMode = storedCard.StoreMode;
            if (storeMode == CardStoreMode.Update)
            {
                storedCard.UpdateStates();
            }

            storedCard.RemoveAllButChanged(storeMode);
            var request = new CardStoreRequest
            {
                Card = storedCard,
                AffectVersion = forceAffectVersion,
            };

            if (await KrComponentsHelper.HasBaseAsync(originalCard.TypeID, this.krTypesCache, cancellationToken))
            {
                this.tokenProvider.CreateFullToken(storedCard).Set(storedCard.Info);
            }

            var digest = await this.cardRepository.GetDigestAsync(
                originalCard,
                CardDigestEventNames.ActionHistoryStoreRouteProcess,
                cancellationToken);

            if (digest is not null)
            {
                request.SetDigest(digest);
            }

            var response = await CardHelper.StoreAsync(
                request,
                fileContainer?.FileContainer,
                this.cardRepository,
                this.streamServerRepository,
                cancellationToken);

            validationResult.Add(response.ValidationResult);
            originalCard.Version = response.CardVersion;
        }

        private async Task DeleteCardAsync(
            Card card,
            IValidationResultBuilder validationResult,
            CancellationToken cancellationToken = default)
        {
            var request = new CardDeleteRequest
            {
                CardID = card.ID,
                CardTypeID = card.TypeID,
                DeletionMode = CardDeletionMode.WithoutBackup,
            };
            var resp = await this.cardRepository.DeleteAsync(request, cancellationToken);
            validationResult.Add(resp.ValidationResult);
        }

        private Task ForceIncrementVersionAsync(
            Guid mainCardID,
            IValidationResultBuilder validationResult,
            CancellationToken cancellationToken = default)
        {
            return this.CardTransactionStrategy.ExecuteInReaderLockAsync(
                mainCardID,
                validationResult,
                this.ForceIncrementVersionInternalAsync,
                cancellationToken
            );
        }

        private async Task ForceIncrementVersionInternalAsync(
            ICardTransactionParameter p)
        {
            var getContext = await this.getStrategy.TryLoadCardInstanceAsync(
                p.CardID.Value,
                this.dbScope.Db,
                this.cardMetadata,
                p.ValidationResult,
                cancellationToken: p.CancellationToken);

            if (!p.ValidationResult.IsSuccessful())
            {
                p.ReportError = true;
                return;
            }

            var card = getContext.Card;
            var token = this.tokenProvider.CreateFullToken(card);
            token.Set(card.Info);

            string digest;
            var workflowRequest = WorkflowScopeContext.Current.StoreContext?.Request;
            if (workflowRequest is not null)
            {
                digest = workflowRequest.TryGetDigest();
            }
            else if (await this.getStrategy.LoadSectionsAsync(getContext, p.CancellationToken))
            {
                digest = await this.cardRepository.GetDigestAsync(
                    card,
                    CardDigestEventNames.ActionHistoryStoreRouteProcess,
                    p.CancellationToken);

                card.RemoveAllButChanged();
            }
            else
            {
                digest = null;
            }

            var storeRequest = new CardStoreRequest
            {
                Card = card,
                AffectVersion = true,
            };
            storeRequest.SetDigest(digest);

            var storeResponse = await this.cardRepository.StoreAsync(
                storeRequest,
                p.CancellationToken);

            p.ValidationResult.Add(storeResponse.ValidationResult);

            if (!p.ValidationResult.IsSuccessful())
            {
                p.ReportError = true;
            }
        }

        #endregion
    }
}
