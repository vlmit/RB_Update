using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.ComponentModel;
using Tessa.Cards.Extensions;
using Tessa.Cards.Workflow;
using Tessa.Extensions.Default.Server.Workflow.KrProcess.Scope;
using Tessa.Extensions.Default.Shared;
using Tessa.Extensions.Default.Shared.Workflow.Wf;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Runtime;
using Tessa.Platform.Storage;
using Tessa.Roles;
using Unity;
using static Tessa.Extensions.Default.Shared.Workflow.KrProcess.KrConstants;

namespace Tessa.Extensions.Default.Server.Workflow.KrProcess.Workflow.Handlers
{
    /// <summary>
    /// Обработчик этапа <see cref="Shared.Workflow.KrProcess.StageTypeDescriptors.ResolutionDescriptor"/>.
    /// </summary>
    public class ResolutionStageTypeHandler : StageTypeHandlerBase
    {
        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ResolutionStageTypeHandler"/>.
        /// </summary>
        /// <param name="cardRepository">Репозиторий для управления карточками.</param>
        /// <param name="cardMetadata">Метаинформация, необходимую для использования типов карточек совместно с пакетом карточек.</param>
        /// <param name="cardGetStrategy">Стратегия загрузки карточки.</param>
        /// <param name="krScope">Объект предоставляющий методы для работы с текущим контекстом расширений типового расширения и использования разделяемых объектов карточек.</param>
        /// <param name="tokenProvider">Объект, обеспечивающий создание и валидацию токена безопасности для типового решения.</param>
        /// <param name="roleRepository">Репозиторий для управления ролевой моделью.</param>
        /// <param name="session">Сессия пользователя.</param>
        public ResolutionStageTypeHandler(
            [Dependency(CardRepositoryNames.ExtendedWithoutTransaction)] ICardRepository cardRepository,
            ICardMetadata cardMetadata,
            ICardGetStrategy cardGetStrategy,
            IKrScope krScope,
            IKrTokenProvider tokenProvider,
            IRoleRepository roleRepository,
            ISession session)
        {
            this.CardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
            this.CardMetadata = cardMetadata ?? throw new ArgumentNullException(nameof(cardMetadata));
            this.CardGetStrategy = cardGetStrategy ?? throw new ArgumentNullException(nameof(cardGetStrategy));
            this.KrScope = krScope ?? throw new ArgumentNullException(nameof(krScope));
            this.TokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            this.RoleRepository = roleRepository;
            this.Session = session;
        }

        #endregion

        #region Protected Properties and Constants

        /// <summary>
        /// Имя ключа, по которому в <see cref="KrObjectModel.Stage.InfoStorage"/> содержится идентификатор первого задания бизнес процесса <see cref="WfHelper.ResolutionProcessName"/>. Тип значения: <see cref="Guid"/>.
        /// </summary>
        protected const string TaskRowID = nameof(TaskRowID);

        /// <summary>
        /// Возвращает или задаёт репозиторий для управления карточками.
        /// </summary>
        protected ICardRepository CardRepository { get; set; }

        /// <summary>
        /// Возвращает или задаёт метаинформацию, необходимую для использования типов карточек совместно с пакетом карточек.
        /// </summary>
        protected ICardMetadata CardMetadata { get; set; }

        /// <summary>
        /// Возвращает или задаёт стратегию загрузки карточки.
        /// </summary>
        protected ICardGetStrategy CardGetStrategy { get; set; }

        /// <summary>
        /// Возвращает или задаёт объект предоставляющий методы для работы с текущим контекстом расширений типового расширения и использования разделяемых объектов карточек.
        /// </summary>
        protected IKrScope KrScope { get; set; }

        /// <summary>
        /// Возвращает или задаёт объект, обеспечивающий создание и валидацию токена безопасности для типового решения.
        /// </summary>
        protected IKrTokenProvider TokenProvider { get; set; }

        /// <summary>
        /// Возвращает или задаёт репозиторий для управления ролевой моделью.
        /// </summary>
        protected IRoleRepository RoleRepository { get; set; }

        /// <summary>
        /// Возвращает или задаёт сессию пользователя.
        /// </summary>
        protected ISession Session { get; set; }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override async Task<StageHandlerResult> HandleStageStartAsync(IStageTypeHandlerContext context)
        {
            var performers = context.Stage.Performers;
            if (performers.Count == 0)
            {
                return StageHandlerResult.CompleteResult;
            }

            if (!(context.CardExtensionContext is ICardStoreExtensionContext storeContext))
            {
                return StageHandlerResult.EmptyResult;
            }
            var author = await HandlerHelper.GetStageAuthorAsync(context, this.RoleRepository, this.Session);
            if (author is null)
            {
                return StageHandlerResult.EmptyResult;
            }
            var authorID = author.AuthorID;
            var authorName = author.AuthorName;
            var task = new CardTask() { TypeID = DefaultTaskTypes.WfResolutionProjectTypeID };
            var settings = context.Stage.SettingsStorage;
            var resolutionFields = task.Card.Sections.GetOrAddEntry(WfHelper.ResolutionSection).Fields;
            resolutionFields[WfHelper.ResolutionKindIDField] = settings.TryGet<object>(KrResolutionSettingsVirtual.KindID);
            resolutionFields[WfHelper.ResolutionKindCaptionField] = settings.TryGet<object>(KrResolutionSettingsVirtual.KindCaption);
            resolutionFields[WfHelper.ResolutionAuthorIDField] = authorID;
            resolutionFields[WfHelper.ResolutionAuthorNameField] = authorName;
            resolutionFields[WfHelper.ResolutionControllerIDField] = settings.TryGet<object>(KrResolutionSettingsVirtual.ControllerID);
            resolutionFields[WfHelper.ResolutionControllerNameField] = settings.TryGet<object>(KrResolutionSettingsVirtual.ControllerName);
            resolutionFields[WfHelper.ResolutionCommentField] = settings.TryGet<object>(KrResolutionSettingsVirtual.Comment);
            resolutionFields[WfHelper.ResolutionPlannedField] = settings.TryGet<object>(KrResolutionSettingsVirtual.Planned);
            resolutionFields[WfHelper.ResolutionDurationInDaysField] = settings.TryGet<object>(KrResolutionSettingsVirtual.DurationInDays);
            resolutionFields[WfHelper.ResolutionWithControlField] = settings.TryGet<object>(KrResolutionSettingsVirtual.WithControl);
            resolutionFields[WfHelper.ResolutionMassCreationField] = settings.TryGet<object>(KrResolutionSettingsVirtual.MassCreation);
            resolutionFields[WfHelper.ResolutionMajorPerformerField] = settings.TryGet<object>(KrResolutionSettingsVirtual.MajorPerformer);

            var performerRows = task.Card.Sections.GetOrAddTable(WfHelper.ResolutionPerformersSection).Rows;
            for (var i = 0; i < performers.Count; i++)
            {
                var performerRow = performerRows.Add();
                performerRow.RowID = Guid.NewGuid();
                performerRow.State = CardRowState.Inserted;

                var performer = performers[i];
                performerRow[WfHelper.ResolutionPerformerRoleIDField] = performer.PerformerID;
                performerRow[WfHelper.ResolutionPerformerRoleNameField] = performer.PerformerName;
                performerRow[WfHelper.ResolutionPerformerOrderField] = Int32Boxes.Box(i);
            }

            var card = await this.KrScope.GetMainCardAsync(storeContext.Request.Card.ID,
                context.ValidationResult);

            if (card is null)
            {
                return StageHandlerResult.EmptyResult;
            }

            card = card.Clone();
            
            // Дайджест нужно получить до очистки секций и остальных полей карточки
            var digest = await this.CardRepository.GetDigestAsync(card, CardDigestEventNames.ActionHistoryStoreRouteCreateCard, context.CancellationToken);

            var token = this.TokenProvider.CreateFullToken(card);

            card.Sections.Clear();
            card.Files.Clear();
            card.Tasks.Clear();
            card.TaskHistory.Clear();
            card.TaskHistoryGroups.Clear();
            card.Info.Clear();
            card.Permissions.Clear();
            token.Set(card.Info);

            var historyGroupID = await HandlerHelper.GetTaskHistoryGroupAsync(context, this.KrScope);
            var rowID = Guid.NewGuid();
            var storeRequest = new CardStoreRequest { Card = card }
                .SetStartingProcessTaskGroupRowID(historyGroupID)
                .SetStartingProcessNextTask(task)
                .SetStartingProcessName(WfHelper.ResolutionProcessName)
                .SetStartingProcessTaskRowID(rowID);
            if (digest is not null)
            {
                storeRequest.SetDigest(digest); 
            }
            var response = await this.CardRepository.StoreAsync(storeRequest,
                context.CancellationToken);

            context.ValidationResult.Add(response.ValidationResult);
            if (!response.ValidationResult.IsSuccessful())
            {
                return StageHandlerResult.EmptyResult;
            }

            var process = context.ProcessInfo;
            var scope = context.CardExtensionContext.DbScope;
            var db = scope.Db;

            await db
                .SetCommand(
                    scope.BuilderFactory
                        .Update("TaskHistory")
                        .C("ProcessID").Assign().P("ProcessID")
                        .C("ProcessKind").Assign().P("ProcessKind")
                        .Where().C("RowID").Equals().P("RowID")
                        .Build(),
                    db.Parameter("RowID", rowID),
                    db.Parameter("ProcessID", process.ProcessID),
                    db.Parameter("ProcessKind", process.ProcessTypeName))
                .LogCommand()
                .ExecuteNonQueryAsync(context.CancellationToken);

            context.Stage.InfoStorage[TaskRowID] = rowID;
            return StageHandlerResult.InProgressResult;
        }

        /// <inheritdoc/>
        public override async Task<bool> HandleStageInterruptAsync(IStageTypeHandlerContext context)
        {
            var storeContext = (ICardStoreExtensionContext) context.CardExtensionContext;
            var scope = storeContext.DbScope;
            await using (scope.Create())
            {
                var db = scope.Db;
                var tasksToRevoke = await db
                    .SetCommand(
                        scope.BuilderFactory
                            .With("ChildTaskHistory", e => e
                                    .Select().C("t", "RowID")
                                    .From("TaskHistory", "t").NoLock()
                                    .Where().C("t", "ParentRowID").Equals().P("RootRowID")
                                    .UnionAll()
                                    .Select().C("t", "RowID")
                                    .From("TaskHistory", "t").NoLock()
                                    .InnerJoin("ChildTaskHistory", "p")
                                    .On().C("p", "RowID").Equals().C("t", "ParentRowID"),
                                columnNames: new[] { "RowID" },
                                recursive: true)
                            .Select().C("t", "RowID")
                            .From("Tasks", "t").NoLock()
                            .InnerJoin("ChildTaskHistory", "h")
                            .On().C("h", "RowID").Equals().C("t", "RowID")
                            .Build(),
                        db.Parameter("RootRowID", context.Stage.InfoStorage.Get<Guid>(TaskRowID)))
                    .LogCommand()
                    .ExecuteListAsync<Guid>(context.CancellationToken);

                if (tasksToRevoke.Count == 0)
                {
                    return true;
                }

                var tasksToLoad = tasksToRevoke;

                var card = await this.KrScope.GetMainCardAsync(
                    storeContext.Request.Card.ID,
                    cancellationToken: context.CancellationToken);

                if (card is null)
                {
                    return false;
                }

                var cardTasks = card.TryGetTasks();
                if (cardTasks is not null)
                {
                    tasksToLoad = new List<Guid>(tasksToLoad);

                    foreach (var cardTask in cardTasks)
                    {
                        tasksToLoad.Remove(cardTask.RowID);
                    }
                }

                if (tasksToLoad.Count > 0)
                {
                    var taskContexts = await this.CardGetStrategy.TryLoadTaskInstancesAsync(
                        card.ID, card, db, this.CardMetadata, context.ValidationResult, storeContext.Session.User.ID,
                        getTaskMode: CardGetTaskMode.All, loadCalendarInfo: false, taskRowIDList: tasksToLoad);
                    foreach (var taskContext in taskContexts)
                    {
                        await this.CardGetStrategy.LoadSectionsAsync(taskContext, context.CancellationToken);
                    }
                }

                cardTasks = card.TryGetTasks();
                if (cardTasks is null)
                {
                    return true;
                }

                foreach (var taskToRevoke in cardTasks)
                {
                    if (tasksToRevoke.Contains(taskToRevoke.RowID))
                    {
                        taskToRevoke.Action = CardTaskAction.Complete;
                        taskToRevoke.State = CardRowState.Deleted;
                        taskToRevoke.Flags = taskToRevoke.Flags & ~CardTaskFlags.Locked | CardTaskFlags.UnlockedForAuthor | CardTaskFlags.HistoryItemCreated;
                        taskToRevoke.Result = "$ApprovalHistory_TaskCancelled";
                        taskToRevoke.OptionID = DefaultCompletionOptions.Cancel;

                        var resolutionFields = taskToRevoke.Card.Sections.GetOrAddEntry(WfHelper.ResolutionSection).Fields;
                        resolutionFields[WfHelper.ResolutionRevokeChildrenField] = BooleanBoxes.False;
                    }
                }

                // Всегда true, поскольку задачи находятся в процессе WfHelper.ResolutionProcessName.
                return true;
            }
        }

        /// <inheritdoc/>
        public override Task<StageHandlerResult> HandleSignalAsync(IStageTypeHandlerContext context)
        {
            var signal = context.SignalInfo;
            if (signal?.Signal?.Name == KrPerformSignal)
            {
                return Task.FromResult(StageHandlerResult.CompleteResult);
            }

            return Task.FromResult(StageHandlerResult.EmptyResult);
        }

        #endregion
    }
}
