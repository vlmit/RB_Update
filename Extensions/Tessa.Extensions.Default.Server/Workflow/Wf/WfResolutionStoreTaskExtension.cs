using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB.Data;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Cards.Extensions.Templates;
using Tessa.Cards.Metadata;
using Tessa.Extensions.Default.Shared;
using Tessa.Extensions.Default.Shared.Workflow.Wf;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;
using Tessa.Roles;

namespace Tessa.Extensions.Default.Server.Workflow.Wf
{
    /// <summary>
    /// При завершении дочерней резолюции с её удалением добавляется запись в родительскую резолюцию
    /// о том, что дочерняя резолюция завершена (если родительская резолюция ещё существует).
    /// При создании любой резолюции добавляет запись в карточку-сателлит с информацией для истории заданий.
    /// </summary>
    public sealed class WfResolutionStoreTaskExtension :
        CardStoreTaskExtension
    {
        #region Private Methods

        private static Task InsertChildRecordAsync(
            CardTask task,
            Guid parentID,
            DateTime storeDateTime,
            IQueryExecutor executor,
            IQueryBuilderFactory builderFactory,
            CancellationToken cancellationToken = default)
        {
            // при вставке задания у него присутствуют все секции,
            // а вся его информация актуальна в объекте task

            // метод следует вызывать только в том случае, если задание parentID ещё не завершено

            Card taskCard = task.Card;
            Dictionary<string, object> resolutionFields = taskCard.Sections[WfHelper.ResolutionSection].RawFields;

            DateTime created = taskCard.Created ?? storeDateTime;
            DateTime planned = task.Planned ?? storeDateTime;
            string parentComment = resolutionFields.TryGet<string>(WfHelper.ResolutionParentCommentField);

            return executor
                .ExecuteNonQueryAsync(
                    builderFactory
                        .InsertInto("WfResolutionChildren",
                            "ID", "RowID", "PerformerID", "PerformerName", "UserID", "UserName", "OptionID",
                            "OptionCaption", "Comment", "Answer", "Created", "Planned", "InProgress", "Completed")
                        .Values(b => b
                            .P("ParentID", "TaskID", "RoleID", "RoleName").V(null).V(null).V(null)
                            .V(null).P("Comment").V(null).P("Created", "Planned").V(null).V(null))
                        .Build(),
                    cancellationToken,
                    executor.Parameter("ParentID", parentID),
                    executor.Parameter("TaskID", task.RowID),
                    executor.Parameter("RoleID", task.RoleID),
                    executor.Parameter("RoleName", SqlHelper.LimitString(task.RoleName, RoleHelper.RoleNameMaxLength)),
                    executor.Parameter("Comment", parentComment),
                    executor.Parameter("Created", created),
                    executor.Parameter("Planned", planned));
        }


        private static Task<bool> HasTaskAsync(
            Guid taskRowID,
            DbManager db,
            IQueryBuilderFactory builderFactory,
            CancellationToken cancellationToken = default)
        {
            return db
                .SetCommand(
                    builderFactory
                        .Select().V(true)
                        .From("Tasks").NoLock()
                        .Where().C("RowID").Equals().P("RowID")
                        .Build(),
                    db.Parameter("RowID", taskRowID))
                .LogCommand()
                .ExecuteAsync<bool>(cancellationToken);
        }


        private static Task UpdateChildRecordAsync(
            CardTask task,
            IQueryExecutor executor,
            IQueryBuilderFactory builderFactory,
            CardMetadataCompletionOption completionOption,
            bool isDeleted,
            bool isCompletion,
            CancellationToken cancellationToken = default)
        {
            // если даже родительское задание уже завершено, то update не упадёт, а просто не обновит строки
            var builder = builderFactory
                .Update("WfResolutionChildren")
                    .C("InProgress").Assign().C("th", "InProgress")
                    .C("PerformerID").Assign().C("th", "RoleID")
                    .C("PerformerName").Assign().C("th", "RoleName")
                    .C("UserID").Assign().C("th", "UserID")
                    .C("UserName").Assign().C("th", "UserName");

            if (isDeleted)
            {
                builder
                    .C("Completed").Assign().C("th", "Completed")
                    .C("OptionID").Assign().C("th", "OptionID")
                    .C("OptionCaption").Assign().C("th", "OptionCaption");
            }

            var parameters = new List<DataParameter>
            {
                executor.Parameter("TaskID", task.RowID),
            };

            if (isCompletion)
            {
                string answer = null;
                Guid optionID = completionOption.ID;
                if (optionID == DefaultCompletionOptions.Complete)
                {
                    Card taskCard = task.TryGetCard();
                    StringDictionaryStorage<CardSection> taskSections;
                    if (taskCard != null
                        && (taskSections = taskCard.TryGetSections()) != null
                        && taskSections.TryGetValue(WfHelper.ResolutionSection, out CardSection resolutionSection))
                    {
                        answer = resolutionSection.RawFields.TryGet<string>(WfHelper.ResolutionCommentField);
                    }
                }
                else
                {
                    // удаляем комментарий автора, если он есть, отделённый переводом строки от прочей информации
                    answer = task.Result;
                    if (answer != null)
                    {
                        int firstLineEnding = answer.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                        if (firstLineEnding > 0)
                        {
                            answer = answer.Substring(firstLineEnding + Environment.NewLine.Length);
                        }
                    }
                }

                if (answer != null)
                {
                    builder
                        .C("Answer").Assign().P("Answer");

                    parameters.Add(
                        executor.Parameter("Answer", answer));
                }
            }

            builder
                .From("TaskHistory", "th").NoLock()
                .Where().C("WfResolutionChildren", "RowID").Equals().C("th", "RowID")
                    .And().C("WfResolutionChildren", "RowID").Equals().P("TaskID");

            return executor
                .ExecuteNonQueryAsync(
                    builder.Build(),
                    cancellationToken,
                    parameters.ToArray());
        }


        private static Task InsertAdditionalTaskHistoryAsync(
            CardTask task,
            Guid cardID,
            IQueryExecutor executor,
            IQueryBuilderFactory builderFactory,
            CancellationToken cancellationToken = default)
        {
            Card taskCard = task.TryGetCard();
            if (taskCard == null)
            {
                return Task.CompletedTask;
            }

            bool useControl = WfHelper.TryGetController(task, out Guid? controllerID, out string controllerName);

            return executor
                .ExecuteNonQueryAsync(
                    builderFactory
                        .InsertInto("WfSatelliteTaskHistory", "ID", "RowID", "ControllerID", "ControllerName", "Controlled")
                        .Select().C("ID").P("TaskID", "ControllerID", "ControllerName", "Controlled")
                        .From(CardSatelliteHelper.SatellitesSectionName).NoLock()
                        .Where().C(CardSatelliteHelper.MainCardIDColumn).Equals().P("CardID")
                            .And().C(CardSatelliteHelper.SatelliteTypeIDColumn).Equals().V(DefaultCardTypes.WfSatelliteTypeID)
                        .Build(),
                    cancellationToken,
                    executor.Parameter("CardID", cardID),
                    executor.Parameter("TaskID", task.RowID),
                    executor.Parameter("ControllerID", controllerID),
                    executor.Parameter("ControllerName", SqlHelper.LimitString(controllerName, RoleHelper.RoleNameMaxLength)),
                    executor.Parameter("Controlled", useControl ? BooleanBoxes.False : null));
        }

        #endregion

        #region Base Overrides

        public override Task StoreTaskBeforeRequest(ICardStoreTaskExtensionContext context)
        {
            // при завершении задания с вариантом "завершить от автора" установим флажок "не брать в работу"
            if (context.IsCompletion && context.CompletionOption.ID == DefaultCompletionOptions.ModifyAsAuthor)
            {
                context.Task.Flags = context.Task.Flags
                    .SetFlag(CardTaskFlags.SuppressAutoTakeInProgressWhenCompleted, true);
            }

            return Task.CompletedTask;
        }


        public override async Task StoreTaskBeforeCommitTransaction(ICardStoreTaskExtensionContext context)
        {
            // актуально только при наличии родительского задания и любом изменении этого задания
            Guid? parentID = context.Task.ParentRowID;

            await using (context.DbScope.Create())
            {
                var executor = context.DbScope.Executor;
                var builderFactory = context.DbScope.BuilderFactory;

                if (context.State == CardRowState.Inserted)
                {
                    await InsertAdditionalTaskHistoryAsync(context.Task, context.Request.Card.ID, executor, builderFactory, context.CancellationToken);

                    // родительское задание расположено в той же карточке, что и дочернее;
                    // поскольку мы находимся в транзакции на сохранение этой карточки,
                    // то никто посторонний гарантированно не может завершить родительское задание,
                    // пока не закончится этот метод

                    // однако задание уже может быть завершено на момент выполнения метода,
                    // поэтому надо проверить его существование перед вставкой строки

                    var db = context.DbScope.Db;
                    if (parentID.HasValue
                        && await HasTaskAsync(parentID.Value, db, builderFactory, context.CancellationToken))
                    {
                        await InsertChildRecordAsync(
                            context.Task,
                            parentID.Value,
                            context.StoreDateTime ?? DateTime.UtcNow,
                            executor,
                            builderFactory,
                            context.CancellationToken);
                    }
                }
                else if (parentID.HasValue)
                {
                    await UpdateChildRecordAsync(
                        context.Task,
                        executor,
                        builderFactory,
                        context.CompletionOption,
                        context.State == CardRowState.Deleted,
                        context.IsCompletion,
                        context.CancellationToken);
                }
            }
        }

        #endregion
    }
}
