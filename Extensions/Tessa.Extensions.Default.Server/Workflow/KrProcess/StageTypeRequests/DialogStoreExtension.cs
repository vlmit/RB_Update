using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.ComponentModel;
using Tessa.Cards.Extensions;
using Tessa.Cards.Workflow;
using Tessa.Extensions.Default.Server.Workflow.KrProcess.Workflow.Handlers;
using Tessa.Extensions.Default.Shared;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Files;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using Unity;

namespace Tessa.Extensions.Default.Server.Workflow.KrProcess.StageTypeRequests
{
    public sealed class DialogStoreExtension : CardStoreExtension
    {
        #region Fields

        private readonly ICardGetStrategy getStrategy;

        private readonly ICardMetadata cardMetadata;

        private readonly ICardRepository cardRepositoryEwt;

        private readonly IDbScope dbScope;

        private readonly IKrTokenProvider krTokenProvider;

        private readonly ICardRepository cardRepositoryDwt;

        private readonly ICardStreamServerRepository cardStreamServerRepositoryDwt;

        #endregion

        #region Constructor

        public DialogStoreExtension(
            ICardGetStrategy getStrategy,
            ICardMetadata cardMetadata,
            [Dependency(CardRepositoryNames.ExtendedWithoutTransaction)] ICardRepository cardRepositoryEwt,
            IDbScope dbScope,
            IKrTokenProvider krTokenProvider,
            [Dependency(CardRepositoryNames.DefaultWithoutTransaction)] ICardRepository cardRepositoryDwt,
            [Dependency(CardRepositoryNames.DefaultWithoutTransaction)] ICardStreamServerRepository cardStreamServerRepositoryDwt)
        {
            this.getStrategy = getStrategy;
            this.cardMetadata = cardMetadata;
            this.cardRepositoryEwt = cardRepositoryEwt;
            this.dbScope = dbScope;
            this.krTokenProvider = krTokenProvider;
            this.cardRepositoryDwt = cardRepositoryDwt;
            this.cardStreamServerRepositoryDwt = cardStreamServerRepositoryDwt;
        }

        #endregion

        #region Base overrides

        public override async Task BeforeCommitTransaction(
            ICardStoreExtensionContext context)
        {
            if (!context.ValidationResult.IsSuccessful())
            {
                return;
            }

            // Попадаем сюда при:
            // 1. Сохранение карточки диалога с клиента при наличии файлов (CardTaskDialogUIExtension)
            // 2. Сохранение карточки диалога с сервера без файлов, только режим карточки (CardTaskDialogActionStoreExtension)

            // Если в запросе на сохранение лежит результат нажатия на диалоговую кнопку,
            // значит происходит сохранение карточки диалога. Здесь может сохранятся только ПЕРСИСТЕНТНАЯ карточка
            // Необходимо поднять систему маршрутов и выполнить скрипт сохранения карточки диалога.
            // Это можно сделать отправкой сигнала.
            CardTaskDialogActionResult dialogActionResult;
            if ((dialogActionResult = CardTaskDialogHelper.GetCardTaskDialogActionResult(context.Request)) != null)
            {
                var response = await this.SendSignalToProcessAsync(
                    context.Request.Card,
                    dialogActionResult,
                    context.TransactionStrategy,
                    context.CancellationToken);
                if (response != null)
                {
                    context.ValidationResult.Add(response.ValidationResult);
                    if (context.ValidationResult.IsSuccessful()
                        && response.Info.TryGetValue(DialogStageTypeHandler.ChangedCardKey, out var changedCardObj)
                        && changedCardObj is IDictionary<string, object> changedCardDict)
                    {
                        var changedCard = new Card(changedCardDict.ToDictionaryStorage());
                        if (changedCard.Version == 0)
                        {
                            changedCard.Version = 1;

                        }

                        var storeRequest = new CardStoreRequest
                        {
                            Card = changedCard,
                            DoesNotAffectVersion = true,
                        };

                        var changedCardStoreRequest = await CardHelper.StoreAsync(
                            storeRequest,
                            response.Info.TryGet<IFileContainer>(DialogStageTypeHandler.ChangedCardFileContainerKey),
                            this.cardRepositoryDwt,
                            this.cardStreamServerRepositoryDwt,
                            context.CancellationToken);

                        context.ValidationResult.Add(changedCardStoreRequest.ValidationResult);
                    }
                }
            }
        }

        /// <inheritdoc />
        public override Task AfterRequest(
            ICardStoreExtensionContext context)
        {
            // Это случай основной карточки. С помощью ключа в ValidationResult передаем признак в респонс о том
            // что необходимо не закрывать окно диалога.
            var hasDialog = context.ValidationResult.RemoveAll(DefaultValidationKeys.CancelDialog);
            if (hasDialog != 0)
            {
                context.Response.SetKeepTaskDialog();
            }

            if (context.Info.TryGetValue(DialogStageTypeHandler.ChangedCardKey, out var changedCard))
            {
                context.Response.Info[DialogStageTypeHandler.ChangedCardKey] = changedCard;
            }

            if (context.Info.TryGetValue(DialogStageTypeHandler.ChangedCardFileContainerKey, out var changedCardFileContainer))
            {
                context.Response.Info[DialogStageTypeHandler.ChangedCardFileContainerKey] = changedCardFileContainer;
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Private Methods

        private async Task<CardStoreResponse> SendSignalToProcessAsync(
            Card dialogCard,
            CardTaskDialogActionResult dialogActionResult,
            ICardTransactionStrategy transactionStrategy,
            CancellationToken cancellationToken = default)
        {
            var (processRowID, processTypeName) = await GetProcessIDByTaskAsync(dialogActionResult.TaskID, this.dbScope, cancellationToken);
            // Если процесс не относится к Workflow API, то игнорируем отправку сигнала
            if (processRowID == default)
            {
                return null;
            }

            Card card = null;
            var validationResult = new ValidationResultBuilder();
            await transactionStrategy.ExecuteInReaderLockAsync(
                dialogActionResult.MainCardID,
                validationResult,
                async p =>
                {
                    if (p.CardID is null)
                    {
                        return;
                    }

                    var db = p.DbScope.Db;
                    var getContext = await this.getStrategy.TryLoadCardInstanceAsync(
                        p.CardID.Value,
                        db,
                        this.cardMetadata,
                        p.ValidationResult,
                        cancellationToken: p.CancellationToken);

                    card = getContext.Card;
                },
                cancellationToken);

            if (!validationResult.IsSuccessful()
                || card is null)
            {
                return null;
            }

            var storeRequest = new CardStoreRequest
            {
                Card = card,
                DoesNotAffectVersion = true,
            };

            var wq = card.GetWorkflowQueue();
            var sig = wq.AddSignal(processTypeName, KrConstants.DialogSaveActionSignal, processID: processRowID);

            var dialogActionResultCopy = dialogActionResult.DeepClone();
            dialogActionResultCopy.SetDialogCard(dialogCard);
            CardTaskDialogHelper.SetCardTaskDialogActionResult(sig.Signal.Parameters, dialogActionResultCopy);

            var krToken = this.krTokenProvider.CreateFullToken(card);
            krToken.Set(card.Info);

            var storeResponse = await this.cardRepositoryEwt.StoreAsync(storeRequest, cancellationToken);

            return storeResponse;
        }

        private static async Task<(Guid, string)> GetProcessIDByTaskAsync(
            Guid taskID,
            IDbScope dbScope,
            CancellationToken cancellationToken = default)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;

                await using var reader = await db
                    .SetCommand(
                        dbScope.BuilderFactory
                            .Select()
                            .Top(1)
                            .C("WorkflowProcesses", "RowID", "TypeName")
                            .From("WorkflowTasks").NoLock()
                            .InnerJoin("WorkflowProcesses").NoLock()
                            .On().C("WorkflowTasks", "ProcessRowID").Equals().C("WorkflowProcesses", "RowID")
                            .Where().C("WorkflowTasks", "RowID").Equals().P("ID")
                            .Limit(1)
                            .Build(),
                        db.Parameter("ID", taskID))
                    .LogCommand()
                    .ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    return (reader.GetGuid(0), reader.GetString(1));
                }

                return default;
            }
        }

        #endregion

    }
}
