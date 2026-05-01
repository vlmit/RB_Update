using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tessa.BusinessCalendar;
using Tessa.Cards;
using Tessa.Cards.Caching;
using Tessa.Cards.ComponentModel;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Server.Workflow.KrObjectModel;
using Tessa.Extensions.Default.Server.Workflow.KrProcess.Scope;
using Tessa.Extensions.Default.Shared;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Localization;
using Tessa.Notices;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Runtime;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using Tessa.Roles;
using Unity;
using static Tessa.Extensions.Default.Shared.Workflow.KrProcess.KrConstants;

namespace Tessa.Extensions.Default.Server.Workflow.KrProcess.Workflow.Handlers
{
    /// <summary>
    /// Обработчик этапа <see cref="StageTypeDescriptors.SigningDescriptor"/>.
    /// </summary>
    public class SigningStageTypeHandler : SubtaskStageTypeHandler
    {
        #region Fields

        private Guid[] subTaskTypeIDs;

        #endregion

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SigningStageTypeHandler"/>.
        /// </summary>
        /// <param name="roleRepository">Репозиторий для управления ролевой моделью.</param>
        /// <param name="cardMetadata">Метаинформация, необходимая для использования типов карточек совместно с пакетом карточек.</param>
        /// <param name="cardGetStrategy">Стратегия загрузки карточки.</param>
        /// <param name="cardCache">Потокобезопасный кэш с карточками и дополнительными настройками.</param>
        /// <param name="krScope">Объект предоставляющий методы для работы с текущим контекстом расширений типового расширения и использования разделяемых объектов карточек.</param>
        /// <param name="calendarService">Интерфейс API бизнес календаря.</param>
        /// <param name="session">Сессия пользователя.</param>
        /// <param name="tasksRevoker">Объект выполняющий отзыв заданий этапа.</param>
        /// <param name="notificationManager">Объект для отправки уведомлений, построенных по карточке уведомления.</param>
        /// <param name="dbScope">Объект для взаимодействия с базой данных.</param>
        public SigningStageTypeHandler(
            IRoleRepository roleRepository,
            ICardMetadata cardMetadata,
            ICardGetStrategy cardGetStrategy,
            ICardCache cardCache,
            IKrScope krScope,
            IBusinessCalendarService calendarService,
            ISession session,
            IStageTasksRevoker tasksRevoker,
            [Dependency(NotificationManagerNames.DeferredWithoutTransaction)]
            INotificationManager notificationManager,
            IDbScope dbScope)
            : base(roleRepository, calendarService, cardMetadata, cardGetStrategy, krScope, tasksRevoker, notificationManager, cardCache, session, dbScope)
        {
            this.CardCache = cardCache ?? throw new ArgumentNullException(nameof(cardCache));
        }

        #endregion

        #region Protected Properties and Constants

        /// <summary>
        /// Ключ по которому в <see cref="Stage.InfoStorage"/> содержится общее число исполнителей. Тип значения: <see cref="int"/>.
        /// </summary>
        protected const string TotalPerformerCount = nameof(TotalPerformerCount);

        /// <summary>
        /// Ключ по которому в <see cref="Stage.InfoStorage"/> содержится текущий порядковый номер исполнителя. Тип значения: <see cref="int"/>.
        /// </summary>
        protected const string CurrentPerformerCount = nameof(CurrentPerformerCount);

        /// <inheritdoc cref="Keys.Disapproved"/>
        protected const string Disapproved = Keys.Disapproved;

        /// <summary>
        /// Возвращает или задаёт потокобезопасный кэш с карточками и дополнительными настройками.
        /// </summary>
        protected ICardCache CardCache { get; set; }

        /// <inheritdoc />
        protected override string CommentNameField { get; } = KrSigningStageSettingsVirtual.Comment;

        #endregion

        #region Protected Methods

        /// <summary>
        /// Обрабатывает завершение задания подписания.
        /// </summary>
        /// <param name="context">Контекст обработчика этапа.</param>
        /// <param name="declined">Значение <see langword="true"/>, если задание подпсиания было завершено вариантом завершения <see cref="DefaultCompletionOptions.Decline"/>, иначе (<see cref="DefaultCompletionOptions.Sign"/>) - <see cref="false"/>.</param>
        /// <returns>Результат выполнения.</returns>
        protected virtual async Task<StageHandlerResult> CompleteSigningTaskAsync(IStageTypeHandlerContext context, bool declined)
        {
            var task = context.TaskInfo.Task;

            await context.WorkflowAPI.RemoveActiveTaskAsync(task.RowID, context.CancellationToken);

            await this.RevokeSubTasksAsync(context, task);

            var field = declined ? KrApprovalCommonInfo.DisapprovedBy : KrApprovalCommonInfo.ApprovedBy;
            var fields = context.ContextualSatellite.Sections[KrApprovalCommonInfo.Name].Fields;
            var result = StringBuilderHelper.Acquire();

            var value = fields.TryGet<string>(field);
            if (!string.IsNullOrEmpty(value))
            {
                result.Append(value).Append("; ");
            }

            var storeContext = (ICardStoreExtensionContext) context.CardExtensionContext;
            var user = storeContext.Session.User;
            result.Append(user.Name);

            if (task.RoleID != user.ID)
            {
                result.Append(" (").Append(LocalizationManager.EscapeIfLocalizationString(task.RoleName)).Append(')');
            }

            fields[field] = result.ToStringAndRelease();

            var stage = context.Stage;
            var total = stage.InfoStorage.TryGet<int>(TotalPerformerCount);
            var current = stage.InfoStorage.TryGet<int>(CurrentPerformerCount);
            stage.InfoStorage[CurrentPerformerCount] = Int32Boxes.Box(++current);

            if (declined)
            {
                stage.InfoStorage[Disapproved] = BooleanBoxes.True;
            }

            if (declined
                || !(await this.CardCache.Cards.GetAsync(KrSettings.Name, context.CancellationToken)).GetValue()
                    .Sections[KrSettings.Name].RawFields.Get<bool>(KrSettings.HideCommentForApprove))
            {
                await HandlerHelper.SetTaskResultAsync(context, task, task.Card.Sections[KrTask.Name].Fields.TryGet<string>(KrTask.Comment));
            }

            if (current == total)
            {
                return stage.InfoStorage.TryGet<bool>(Disapproved)
                    ? await this.DeclineAndCompleteAsync(context)
                    : await this.SignAndCompleteAsync(context);
            }

            if (stage.SettingsStorage.TryGet<bool?>(KrSigningStageSettingsVirtual.IsParallel) != true)
            {
                if (declined
                    && (stage.SettingsStorage.TryGet<bool?>(KrSigningStageSettingsVirtual.ReturnWhenDeclined) ?? default))
                {
                    return await this.DeclineAndCompleteAsync(context);
                }

                await this.SendSigningTaskAsync(context, stage.Performers[current]);
            }

            return StageHandlerResult.InProgressResult;
        }

        /// <summary>
        /// Обрабатывает подпиание.
        /// </summary>
        /// <param name="context">Контекст обработчика этапа.</param>
        /// <returns>Результат выполнения.</returns>
        protected virtual ValueTask<StageHandlerResult> SignAndCompleteAsync(IStageTypeHandlerContext context)
        {
            var stage = context.Stage;
            var notReturnEdit = GetNotReturnEdit(context);

            (var hasPreviouslyDisapproved, var hasNext) = StagePositionInGroup(context.WorkflowProcess.Stages, stage);
            var returnToAuthor = stage.SettingsStorage.TryGet<bool?>(KrSigningStageSettingsVirtual.ReturnToAuthor) ?? default;

            if (stage.SettingsStorage.TryGet<bool?>(KrSigningStageSettingsVirtual.ChangeStateOnEnd) ?? default)
            {
                if (hasPreviouslyDisapproved)
                {
                    this.KrScope.Info[Keys.IgnoreChangeState] = BooleanBoxes.True;
                    context.WorkflowProcess.State = KrState.Declined;
                }
                else if (!returnToAuthor || notReturnEdit)
                {
                    context.WorkflowProcess.State = KrState.Signed;
                }
            }

            if (!notReturnEdit)
            {
                if (hasPreviouslyDisapproved
                    && !hasNext)
                {
                    // Последний этап завершен. Этот подписан, но предыдущие могли быть и не подписаны.
                    // Если были неподписанные, то возвращаемся в начало на доработку.
                    return new ValueTask<StageHandlerResult>(StageHandlerResult.CurrentGroupTransition());
                }

                if (returnToAuthor)
                {
                    // Выполнить переход к этапу "Доработка" расположенному в текущей группе этапов.
                    Guid? editID = null;
                    var cycle = GetCycle(context);
                    context.WorkflowProcess.Stages.ForEachStageInGroup(
                        stage.StageGroupID,
                        currentStage =>
                        {
                            if (currentStage.ID == stage.ID)
                            {
                                return false;
                            }

                            if (currentStage.StageTypeID == StageTypeDescriptors.EditDescriptor.ID)
                            {
                                stage.InfoStorage[Interjected] = Int32Boxes.Box(cycle);
                                currentStage.InfoStorage[EditStageTypeHandler.ReturnToStage] = stage.ID;
                                editID = currentStage.ID;
                                return false;
                            }

                            return true;
                        });

                    // Решарпер считает, что т.к. editID замкнуто в лямбде, то оно может быть изменено в другом потоке
                    // Дадим ему уверенность, переприсвоив в локальную переменную
                    var localEditID = editID;
                    if (localEditID.HasValue)
                    {
                        var transRes = StageHandlerResult.Transition(localEditID.Value, keepStageStates: true);
                        return new ValueTask<StageHandlerResult>(this.StageCompleted(context, transRes));
                    }

                    context.ValidationResult.AddError(this, "$KrMessages_NoEditStage");
                }
            }

            return new ValueTask<StageHandlerResult>(this.StageCompleted(context, StageHandlerResult.CompleteResult));
        }

        /// <summary>
        /// Обрабатывает отказ в подписании.
        /// </summary>
        /// <param name="context">Контекст обработчика этапа.</param>
        /// <returns>Результат выполнения.</returns>
        protected virtual async Task<StageHandlerResult> DeclineAndCompleteAsync(
            IStageTypeHandlerContext context)
        {
            var storeContext = (ICardStoreExtensionContext) context.CardExtensionContext;
            var user = storeContext.Session.User;
            var stage = context.Stage;

            (_, var hasNext) = StagePositionInGroup(context.WorkflowProcess.Stages, stage);

            if (stage.SettingsStorage.TryGet<bool?>(KrSigningStageSettingsVirtual.ReturnWhenDeclined) != true
                && hasNext)
            {
                context.WorkflowProcess.InfoStorage[Disapproved] = BooleanBoxes.True;
                return this.StageCompleted(context, StageHandlerResult.CompleteResult);
            }

            if (stage.SettingsStorage.TryGet<bool?>(KrSigningStageSettingsVirtual.ChangeStateOnEnd) ?? default)
            {
                context.WorkflowProcess.State = KrState.Declined;
                this.KrScope.Info[Keys.IgnoreChangeState] = BooleanBoxes.True;
            }

            if (GetNotReturnEdit(context))
            {
                return this.StageCompleted(context, StageHandlerResult.CompleteResult);
            }

            var utcNow = DateTime.UtcNow;
            var option = (await this.CardMetadata.GetEnumerationsAsync(context.CancellationToken)).CompletionOptions[DefaultCompletionOptions.RebuildDocument];
            var groupID = await HandlerHelper.GetTaskHistoryGroupAsync(context, this.KrScope);
            // Временная зона текущего сотрудника, для записи в историю заданий
            var userZoneInfo = await this.CalendarService.GetRoleTimeZoneInfoAsync(user.ID, context.CancellationToken);
            var item = new CardTaskHistoryItem
            {
                State = CardTaskHistoryState.Inserted,
                RowID = Guid.NewGuid(),
                TypeID = DefaultTaskTypes.KrRebuildTypeID,
                TypeName = DefaultTaskTypes.KrRebuildTypeName,
                TypeCaption = "$CardTypes_TypesNames_KrRebuild",
                Created = utcNow,
                Planned = utcNow,
                InProgress = utcNow,
                Completed = utcNow,
                CompletedByID = user.ID,
                CompletedByName = user.Name,
                AuthorID = user.ID,
                AuthorName = user.Name,
                UserID = user.ID,
                UserName = user.Name,
                RoleID = user.ID,
                RoleName = user.Name,
                RoleTypeID = RoleHelper.PersonalRoleTypeID,
                OptionID = option.ID,
                OptionName = option.Name,
                OptionCaption = option.Caption,
                GroupRowID = groupID,
                TimeZoneID = userZoneInfo.TimeZoneID,
                TimeZoneUtcOffsetMinutes = (int?) userZoneInfo.TimeZoneUtcOffset.TotalMinutes
            };

            var mainCard = await this.KrScope.GetMainCardAsync(
                storeContext.Request.Card.ID,
                cancellationToken: context.CancellationToken);

            if (mainCard is null)
            {
                return StageHandlerResult.EmptyResult;
            }

            mainCard.TaskHistory.Add(item);

            context.ContextualSatellite.AddToHistory(
                item.RowID,
                context.WorkflowProcess.InfoStorage.TryGet(Keys.Cycle, 1));

            return this.StageCompleted(context, StageHandlerResult.GroupTransition(stage.StageGroupID));
        }

        /// <summary>
        /// Обрабатывает создание заданий дополнительного согласования.
        /// </summary>
        /// <param name="context">Контекст обработчика этапа.</param>
        /// <returns>Результат выполнения.</returns>
        protected virtual async Task<StageHandlerResult> StartAdditionalApprovalTaskAsync(IStageTypeHandlerContext context)
        {
            var task = context.TaskInfo.Task;

            var firstIsResponsible = task.Card.Sections
                .TryGet(KrAdditionalApproval.Name)
                ?.TryGetRawFields()
                ?.TryGet<bool>(KrAdditionalApproval.FirstIsResponsible) ?? false;

            if (firstIsResponsible)
            {
                var isResponsibleSet = task.Card.Sections[KrAdditionalApprovalInfo.Name].Rows
                    .Any(static u => u.State != CardRowState.Deleted && u.TryGet<bool>(KrAdditionalApprovalInfo.IsResponsible));
                if (isResponsibleSet)
                {
                    context.ValidationResult.AddError(this, "$KrMessages_MoreThenOneResponsible");
                    return StageHandlerResult.EmptyResult;
                }
            }

            var utcNow = DateTime.UtcNow;

            var krAdditionalApprovalSectionFields = task.Card.Sections.TryGet(KrAdditionalApproval.Name)?.TryGetRawFields();
            var comment = krAdditionalApprovalSectionFields?.TryGet<string>(KrAdditionalApproval.Comment);
            var plannedDays = krAdditionalApprovalSectionFields?.TryGet<double?>(KrAdditionalApproval.TimeLimitation);

            var isResponsible = firstIsResponsible;
            var roles = task.Card.Sections[KrAdditionalApprovalUsers.Name].Rows
                .OrderBy(u => u.Get<int>(KrAdditionalApprovalUsers.Order))
                .Select(u => new RoleUser(
                    u.Get<Guid>(KrAdditionalApprovalUsers.RoleID),
                    u.Get<string>(KrAdditionalApprovalUsers.RoleName)));

            var storeContext = (ICardStoreExtensionContext) context.CardExtensionContext;
            var user = storeContext.Session.User;

            foreach (var role in roles)
            {
                await this.SendAdditionalApprovalTaskAsync(
                    context,
                    task,
                    role.ID,
                    role.Name,
                    isResponsible,
                    utcNow,
                    user.ID,
                    user.Name,
                    comment,
                    plannedDays);

                if (isResponsible)
                {
                    isResponsible = false;
                }
            }

            return StageHandlerResult.InProgressResult;
        }

        /// <summary>
        /// Обрабатывает завершение задания дополнительного согласования.
        /// </summary>
        /// <param name="context">Контекст обработчика этапа.</param>
        /// <param name="disapproved">Значение <see langword="true"/>, если задание было завершено вариантом <see cref="DefaultCompletionOptions.Disapprove"/>, иначе (<see cref="DefaultCompletionOptions.Approve"/>) - <see langword="false"/>.</param>
        /// <returns>Результат выполнения.</returns>
        protected virtual async Task<StageHandlerResult> CompleteAdditionalApprovalTaskAsync(IStageTypeHandlerContext context, bool disapproved)
        {
            var task = context.TaskInfo.Task;

            await context.WorkflowAPI.RemoveActiveTaskAsync(task.RowID, context.CancellationToken);
            this.SubTaskCompleted(context);
            await this.RevokeSubTasksAsync(context, task);

            var parentTaskID = task.ParentRowID;
            if (parentTaskID is null)
            {
                context.ValidationResult.AddError(
                    this,
                    LocalizationManager.Format(
                        "$KrProcess_ErrorMessage_ErrorFormat2",
                        KrErrorHelper.GetTraceTextFromStage(context.Stage),
                        LocalizationManager.Format("$KrStages_Approval_AdditionalApprovalTaskParentRowIDNotSpecified", task.RowID)));
                return StageHandlerResult.EmptyResult;
            }

            var taskComment = context
                .TaskInfo
                .Task
                .Card
                .Sections[KrAdditionalApprovalTaskInfo.Name]
                .RawFields
                .TryGet<string>(KrAdditionalApprovalTaskInfo.Comment);
            await HandlerHelper.SetTaskResultAsync(context, task, taskComment);

            Guid? approverID;
            Guid? responsibleID;
            int notCompleted;

            var storeContext = (ICardStoreExtensionContext) context.CardExtensionContext;
            var scope = storeContext.DbScope;
            await using (scope.Create())
            {
                approverID = await scope.Db
                    .SetCommand(
                        scope.BuilderFactory
                            .Select().C("UserID")
                            .From("Tasks").NoLock()
                            .Where().C("RowID").Equals().P("RowID")
                            .Build(),
                        scope.Db.Parameter("RowID", parentTaskID.Value))
                    .LogCommand()
                    .ExecuteAsync<Guid?>(context.CancellationToken);

                var isResponsible = task.Card.Sections[KrAdditionalApprovalTaskInfo.Name].Fields
                    .TryGet<bool>(KrAdditionalApprovalTaskInfo.IsResponsible);

                responsibleID = isResponsible
                    ? await scope.Db
                        .SetCommand(
                            scope.BuilderFactory
                                .Select().C("t", "UserID")
                                .From("Tasks", "t").NoLock()
                                .InnerJoin(KrAdditionalApprovalInfo.Name, "i").NoLock()
                                .On().C("i", KrAdditionalApprovalInfo.RowID).Equals().C("t", "RowID")
                                .Where().C("i", KrAdditionalApprovalInfo.ID).Equals().P("RowID")
                                .And().C("i", KrAdditionalApprovalInfo.IsResponsible).Equals().V(true)
                                .Build(),
                            scope.Db.Parameter("RowID", parentTaskID.Value))
                        .LogCommand()
                        .ExecuteAsync<Guid?>(context.CancellationToken)
                    : null;

                notCompleted = await scope.Db
                    .SetCommand(
                        scope.BuilderFactory
                            .Select().Count().Substract(1)
                            .From(KrAdditionalApprovalInfo.Name).NoLock()
                            .Where().C(KrAdditionalApprovalInfo.ID).Equals().P("RowID")
                            .And().C(KrAdditionalApprovalInfo.Completed).IsNull()
                            .Build(),
                        scope.Db.Parameter("RowID", parentTaskID.Value))
                    .LogCommand()
                    .ExecuteAsync<int>(context.CancellationToken);
            }

            var roleList = new List<Guid>();
            if (approverID.HasValue)
            {
                roleList.Add(approverID.Value);
            }

            if (responsibleID.HasValue)
            {
                roleList.Add(responsibleID.Value);
            }

            if (roleList.Count > 0)
            {
                var isCompleted = notCompleted == 0;
                var cardID = context.MainCardID ?? Guid.Empty;
                context.ValidationResult.Add(
                    await this.NotificationManager
                        .SendAsync(
                            isCompleted
                                ? DefaultNotifications.AdditionalApprovalNotificationCompleted
                                : DefaultNotifications.AdditionalApprovalNotification,
                            roleList,
                            new NotificationSendContext()
                            {
                                MainCardID = cardID,
                                Info = Shared.Notices.NotificationHelper.GetInfoWithTask(task),
                                GetCardFuncAsync = (validationResult, ct) =>
                                    context.MainCardAccessStrategy.GetCardAsync(
                                        validationResult: validationResult,
                                        cancellationToken: ct),
                                ModifyEmailActionAsync = async (email, ct) =>
                                {
                                    if (!isCompleted)
                                    {
                                        email.PlaceholderAliases.SetReplacement(
                                            "subjectLabel",
                                            disapproved
                                                ? "$DisapprovedAdditionalApprovalNotificationTemplate_SubjectLabel"
                                                : "$ApprovedAdditionalApprovalNotificationTemplate_SubjectLabel");
                                        email.PlaceholderAliases.SetReplacement(
                                            "taskCount",
                                            $"text:{notCompleted}");
                                    }

                                    email.PlaceholderAliases.SetReplacement(
                                        "resultLabel",
                                        disapproved
                                            ? "$DisapprovedNotificationTemplate_BodyLabel"
                                            : "$ApprovedNotificationTemplate_BodyLabel");
                                }
                            },
                            context.CancellationToken));
            }

            return StageHandlerResult.InProgressResult;
        }

        /// <summary>
        /// Отправляет задание подписания.
        /// </summary>
        /// <param name="context">Контекст обработчика этапа.</param>
        /// <param name="performer">Исполнитель.</param>
        /// <returns>Асинхронная задача.</returns>
        protected virtual async Task SendSigningTaskAsync(
            IStageTypeHandlerContext context,
            Performer performer)
        {
            var author = await HandlerHelper.GetStageAuthorAsync(context, this.RoleRepository, this.Session);
            if (author is null)
            {
                return;
            }

            var authorID = author.AuthorID;
            var groupID = await HandlerHelper.GetTaskHistoryGroupAsync(context, this.KrScope);
            var stage = context.Stage;
            var (kindID, kindCaption) = HandlerHelper.GetTaskKind(context);

            var task = await this.SendTaskAsync(
                context,
                DefaultTaskTypes.KrSigningTypeID,
                this.GetTaskDigest(context),
                performer,
                (t, ct) =>
                {
                    t.AuthorID = authorID;
                    t.AuthorName = null; // AuthorName и AuthorPosition определяются системой, когда явно указано null
                    t.GroupRowID = groupID;
                    t.Planned = stage.Planned;
                    t.PlannedQuants = stage.PlannedQuants;
                    HandlerHelper.SetTaskKind(t, kindID, kindCaption, context);

                    return ValueTask.CompletedTask;
                });

            if (task is null)
            {
                return;
            }

            task.Card.Sections[KrSigningTaskOptions.Name].Fields[KrSigningTaskOptions.AllowAdditionalApproval] =
                BooleanBoxes.Box(stage.SettingsStorage.TryGet<bool?>(KrSigningStageSettingsVirtual.AllowAdditionalApproval) ?? default);
        }

        /// <summary>
        /// Отправляет задание дополнительного согласования.
        /// </summary>
        /// <param name="context">Контекст обработчика этапа.</param>
        /// <param name="approvalTask">Родительское задание согласования.</param>
        /// <param name="roleID">Идентификатор роли на которую отправляется задание.</param>
        /// <param name="roleName">Имя роли на которую отправляется задание.</param>
        /// <param name="isResponsible">Значение <see langword="true"/>, если <paramref name="roleID"/> является ответственным исполнителем, иначе - <see langword="false"/>.</param>
        /// <param name="created">Дата создания задания.</param>
        /// <param name="authorID">Идентификатор роли автора задания.</param>
        /// <param name="authorName">Имя роли автора задания.</param>
        /// <param name="comment">Комментария к заданию.</param>
        /// <param name="plannedDays">Длительность (рабочие дни).</param>
        /// <returns>Асинхронная задача.</returns>
        protected virtual async Task SendAdditionalApprovalTaskAsync(
            IStageTypeHandlerContext context,
            CardTask approvalTask,
            Guid roleID,
            string roleName,
            bool isResponsible,
            DateTime created,
            Guid? authorID,
            string authorName,
            string comment,
            double? plannedDays = null)
        {
            var storeContext = (ICardStoreExtensionContext) context.CardExtensionContext;
            var user = storeContext.Session.User;

            // Временная зона текущего сотрудника, для записи в историю заданий.
            var userZoneInfo = await this.CalendarService.GetRoleTimeZoneInfoAsync(user.ID, context.CancellationToken);
            var groupID = await HandlerHelper.GetTaskHistoryGroupAsync(context, this.KrScope);
            var option = (await this.CardMetadata.GetEnumerationsAsync(context.CancellationToken)).CompletionOptions[DefaultCompletionOptions.AdditionalApproval];
            var infoItem = new CardTaskHistoryItem
            {
                State = CardTaskHistoryState.Inserted,
                ParentRowID = approvalTask.RowID,
                RowID = Guid.NewGuid(),
                TypeID = DefaultTaskTypes.KrInfoAdditionalApprovalTypeID,
                TypeName = DefaultTaskTypes.KrInfoAdditionalApprovalTypeName,
                TypeCaption = "$CardTypes_TypesNames_KrAdditionalApproval",
                Created = created,
                Planned = created,
                InProgress = created,
                Completed = created,
                CompletedByID = user.ID,
                CompletedByName = user.Name,
                AuthorID = user.ID,
                AuthorName = user.Name,
                UserID = user.ID,
                UserName = user.Name,
                RoleID = user.ID,
                RoleName = user.Name,
                RoleTypeID = RoleHelper.PersonalRoleTypeID,
                OptionID = option.ID,
                OptionName = option.Name,
                OptionCaption = option.Caption,
                GroupRowID = groupID,
                Result = comment ?? (isResponsible
                    ? "{$KrMessages_ResponsibleAdditionalApprovalComment}"
                    : "{$KrMessages_DefaultAdditionalApprovalComment}"),
                TimeZoneID = userZoneInfo.TimeZoneID,
                TimeZoneUtcOffsetMinutes = (int?) userZoneInfo.TimeZoneUtcOffset.TotalMinutes
            };

            var cycle = context.WorkflowProcess.InfoStorage.TryGet(Keys.Cycle, 1);
            var mainCard = await this.KrScope.GetMainCardAsync(
                storeContext.Request.Card.ID,
                cancellationToken: context.CancellationToken);

            if (mainCard is null)
            {
                return;
            }

            mainCard.TaskHistory.Add(infoItem);

            var satellite = context.ContextualSatellite;
            satellite.AddToHistory(infoItem.RowID, cycle);

            var authorComment = context.WorkflowProcess.AuthorComment;
            var digest = string.IsNullOrWhiteSpace(authorComment)
                ? comment
                : comment + Environment.NewLine + authorComment;

            if (isResponsible)
            {
                digest = string.IsNullOrWhiteSpace(digest)
                    ? "{$KrMessages_ResponsibleAdditionalDigest}"
                    : "{$KrMessages_ResponsibleAdditionalDigest}." + Environment.NewLine + digest;
            }

            var additionalTask =
                await this.SendSubTaskAsync(
                    context,
                    approvalTask,
                    DefaultTaskTypes.KrAdditionalApprovalTypeID,
                    digest,
                    roleID,
                    roleName,
                    modifyTask: (t, ct) =>
                    {
                        t.AuthorID = authorID;
                        t.AuthorName = authorName;
                        t.ParentRowID = approvalTask.RowID;
                        t.Planned = null;
                        t.HistoryItemParentRowID = infoItem.RowID;
                        t.GroupRowID = groupID;

                        if (plannedDays.HasValue)
                        {
                            t.Planned = null;
                            t.PlannedQuants = (int) (plannedDays * 32.0); // округление до кванта в меньшую сторону
                        }
                        else
                        {
                            t.Planned = context.Stage.Planned;
                            t.PlannedQuants = context.Stage.PlannedQuants;
                        }

                        return ValueTask.CompletedTask;
                    },
                    createHistory: true);

            if (additionalTask is null)
            {
                return;
            }

            var additionalCard = additionalTask.Card;
            additionalCard.Sections[TaskCommonInfo.Name].Fields[Info] = comment;
            additionalCard.Sections[KrAdditionalApprovalTaskInfo.Name].Fields[KrAdditionalApprovalTaskInfo.AuthorRoleID] = approvalTask.RoleID;
            additionalCard.Sections[KrAdditionalApprovalTaskInfo.Name].Fields[KrAdditionalApprovalTaskInfo.AuthorRoleName] = approvalTask.RoleName;
            additionalCard.Sections[KrAdditionalApprovalTaskInfo.Name].Fields[KrAdditionalApprovalTaskInfo.IsResponsible] = BooleanBoxes.Box(isResponsible);
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override async Task<StageHandlerResult> HandleStageStartAsync(IStageTypeHandlerContext context)
        {
            var stage = context.Stage;
            if (IsInterjected(context))
            {
                if (stage.SettingsStorage.TryGet<bool?>(KrSigningStageSettingsVirtual.ChangeStateOnEnd) ?? default)
                {
                    context.WorkflowProcess.State = KrState.Signed;
                }

                stage.InfoStorage.Remove(Interjected);
                return StageHandlerResult.CompleteResult;
            }

            var baseResult = await base.HandleStageStartAsync(context);
            if (baseResult != StageHandlerResult.EmptyResult)
            {
                return baseResult;
            }

            var performers = stage.Performers;
            if (performers.Count == 0)
            {
                if (stage.SettingsStorage.TryGet<bool?>(KrSigningStageSettingsVirtual.ChangeStateOnEnd) ?? default)
                {
                    context.WorkflowProcess.State = KrState.Signed;
                }

                return StageHandlerResult.CompleteResult;
            }

            if (stage.SettingsStorage.TryGet<bool?>(KrSigningStageSettingsVirtual.ChangeStateOnStart) ?? default)
            {
                context.WorkflowProcess.State = KrState.Signing;
            }

            stage.InfoStorage[TotalPerformerCount] = Int32Boxes.Box(performers.Count);
            stage.InfoStorage[CurrentPerformerCount] = Int32Boxes.Zero;
            stage.InfoStorage[Disapproved] = BooleanBoxes.False;

            if (stage.SettingsStorage.TryGet<bool?>(KrSigningStageSettingsVirtual.IsParallel) ?? default)
            {
                foreach (var performer in performers)
                {
                    await this.SendSigningTaskAsync(context, performer);
                }
            }
            else
            {
                await this.SendSigningTaskAsync(context, performers[0]);
            }

            return StageHandlerResult.InProgressResult;
        }

        /// <inheritdoc />
        public override async Task<StageHandlerResult> HandleTaskCompletionAsync(IStageTypeHandlerContext context)
        {
            var baseResult = await base.HandleTaskCompletionAsync(context);
            if (baseResult != StageHandlerResult.EmptyResult)
            {
                return baseResult;
            }

            var stage = context.Stage;
            var task = context.TaskInfo.Task;
            HandlerHelper.AppendToCompletedTasksWithPreparing(stage, task);
            var optionID = task.OptionID;

            if (task.TypeID == DefaultTaskTypes.KrAdditionalApprovalTypeID)
            {
                if (optionID == DefaultCompletionOptions.Approve)
                {
                    return await this.CompleteAdditionalApprovalTaskAsync(context, false);
                }

                if (optionID == DefaultCompletionOptions.Disapprove)
                {
                    return await this.CompleteAdditionalApprovalTaskAsync(context, true);
                }

                if (optionID == DefaultCompletionOptions.Revoke)
                {
                    await this.RevokeSubTasksAsync(context, task);

                    return this.SubTaskCompleted(context);
                }

                if (optionID == DefaultCompletionOptions.AdditionalApproval)
                {
                    return await this.StartAdditionalApprovalTaskAsync(context);
                }
            }
            else
            {
                if (optionID == DefaultCompletionOptions.Sign)
                {
                    return await this.CompleteSigningTaskAsync(context, false);
                }

                if (optionID == DefaultCompletionOptions.Decline)
                {
                    return await this.CompleteSigningTaskAsync(context, true);
                }

                if (optionID == DefaultCompletionOptions.AdditionalApproval
                    && (stage.SettingsStorage.TryGet<bool?>(KrSigningStageSettingsVirtual.AllowAdditionalApproval) ?? default))
                {
                    return await this.StartAdditionalApprovalTaskAsync(context);
                }
            }

            return StageHandlerResult.EmptyResult;
        }

        /// <inheritdoc />
        public override Task<bool> HandleStageInterruptAsync(IStageTypeHandlerContext context)
            => this.HandleStageInterruptAsync(
                context,
                new[]
                {
                    DefaultTaskTypes.KrSigningTypeID,
                    DefaultTaskTypes.KrAdditionalApprovalTypeID,
                    DefaultTaskTypes.KrRequestCommentTypeID
                },
                t => t.Result = "$ApprovalHistory_TaskCancelled");

        /// <inheritdoc/>
        protected override Task HandleTaskDelegateAsync(IStageTypeHandlerContext context, CardTask delegatedTask)
        {
            if (delegatedTask.TypeID == DefaultTaskTypes.KrSigningTypeID)
            {
                var stage = context.Stage;
                var allowAdditionalApproval = stage.SettingsStorage.TryGet<bool?>(KrSigningStageSettingsVirtual.AllowAdditionalApproval) ?? false;
                delegatedTask.Card.Sections[KrSigningTaskOptions.Name].Fields[KrSigningTaskOptions.AllowAdditionalApproval] = BooleanBoxes.Box(allowAdditionalApproval);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override Guid[] GetSubTaskTypesToRevoke() =>
            this.subTaskTypeIDs ??= base.GetSubTaskTypesToRevoke().Append(DefaultTaskTypes.KrAdditionalApprovalTypeID).ToArray();

        /// <inheritdoc/>
        protected override Task<int> RevokeSubTasksAsync(
            IStageTypeHandlerContext context,
            CardTask parentTask)
        {
            return base.RevokeSubTasksAsync(
                context,
                parentTask,
                this.GetSubTaskTypesToRevoke(),
                t =>
                {
                    t.OptionID = t.TypeID == DefaultTaskTypes.KrAdditionalApprovalTypeID
                        ? DefaultCompletionOptions.Revoke
                        : DefaultCompletionOptions.Cancel;
                    t.Result = "$ApprovalHistory_ParentTaskIsCompleted";
                });
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Определяет в текущей группе этапов:
        /// <list type="number">
        ///     <item>
        ///         <description>Были ли до текущего этапа отклонённые этапы "Подписания".</description>
        ///     </item>
        ///     <item>
        ///         <description>Есть ли этапы "Подписания" после этого этапа.</description>
        ///     </item>
        /// </list>
        /// </summary>
        /// <param name="processStages">Коллекция этапов текущего процесса.</param>
        /// <param name="currentStage">Текущий этап.</param>
        /// <returns>Кортеж &lt;Значение <see langword="true"/>, если до текущего этапа были не согласованные этапы "Подписания", иначе - <see langword="false"/>; Значение <see langword="true"/>, если есть этапы "Подписания" после текущего этапа, иначе - <see langword="false"/>&gt;.</returns>
        private static (bool HasPreviouslyDisapproved, bool HasNext) StagePositionInGroup(
            IList<Stage> processStages,
            Stage currentStage)
        {
            var hasPreviouslyDisapprovedClosure = false;
            var hasNextClosure = false;

            // Признак нахождения текущего этапа среди этапов "Подписания" в текущей группе этапов.
            var equator = false;
            processStages.ForEachStageInGroup(
                currentStage.StageGroupID,
                currStage =>
                {
                    if (currStage.ID == currentStage.ID)
                    {
                        equator = true;
                        return;
                    }

                    if (!equator
                        && currStage.State == KrStageState.Completed
                        && currStage.StageTypeID == StageTypeDescriptors.SigningDescriptor.ID
                        && currStage.InfoStorage.TryGet<bool?>(Disapproved) == true)
                    {
                        hasPreviouslyDisapprovedClosure = true;
                    }

                    if (!hasNextClosure
                        && equator
                        && currStage.StageTypeID == StageTypeDescriptors.SigningDescriptor.ID)
                    {
                        hasNextClosure = true;
                    }
                });

            return (hasPreviouslyDisapprovedClosure, hasNextClosure);
        }

        /// <summary>
        /// Возвращает значение параметра "Не возвращать на доработку".
        /// </summary>
        /// <param name="context">Контекст обработчика этапа.</param>
        /// <returns>Значение параметра "Не возвращать на доработку".</returns>
        private static bool GetNotReturnEdit(IStageTypeHandlerContext context) =>
            (context.ProcessInfo.ProcessParameters.TryGet<bool?>(Keys.NotReturnEdit, true) ?? true)
            && (context.Stage.SettingsStorage.TryGet<bool?>(KrSigningStageSettingsVirtual.NotReturnEdit) ?? default);

        #endregion
    }
}
