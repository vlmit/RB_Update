using System;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;
using static Tessa.Extensions.Default.Shared.Workflow.KrProcess.KrConstants;

namespace Tessa.Extensions.Default.Server.Workflow.KrProcess.Requests
{
    public class KrUpdateParentTaskExtension : CardStoreTaskExtension
    {
        #region private

        private static Task InsertNewCommentAsync(CardTask commentTask, IDbScope dbScope, CancellationToken cancellationToken = default)
        {
            var db = dbScope.Db;
            return db.SetCommand(
                    dbScope.BuilderFactory
                        .InsertInto(KrCommentsInfo.Name,
                            ID,
                            RowID,
                            KrCommentsInfo.Question,
                            KrCommentsInfo.CommentatorID,
                            KrCommentsInfo.CommentatorName)
                        .Values(b => b.P("ID", "RowID", "Question", "CommentatorID", "CommentatorName").N())
                        .Build(),
                    db.Parameter("ID", commentTask.ParentRowID),
                    db.Parameter("RowID", commentTask.RowID),
                    db.Parameter("Question", commentTask.Digest ?? string.Empty),
                    db.Parameter("CommentatorID", commentTask.RoleID),
                    db.Parameter("CommentatorName", commentTask.RoleName ?? string.Empty))
                .LogCommand()
                .ExecuteNonQueryAsync(cancellationToken);
        }

        private static Task UpdateCommentWithAnswerAsync(CardTask commentTask, IDbScope dbScope, CancellationToken cancellationToken = default)
        {
            var db = dbScope.Db;
            return db.SetCommand(
                    dbScope.BuilderFactory
                        .Update(KrCommentsInfo.Name)
                            .C(KrCommentsInfo.Answer).Assign().P("Answer")
                            .C(KrCommentsInfo.CommentatorID).Assign().P("CommentatorID")
                            .C(KrCommentsInfo.CommentatorName).Assign().P("CommentatorName")
                        .Where().C("RowID").Equals().P("RowID")
                        .Build(),
                    db.Parameter("Answer", commentTask.Card.Sections[KrRequestComment.Name].Fields.TryGet<string>(KrRequestComment.Comment)),
                    db.Parameter("CommentatorID", commentTask.UserID),
                    db.Parameter("CommentatorName", commentTask.UserName),
                    db.Parameter("RowID", commentTask.RowID))
                .LogCommand()
                .ExecuteNonQueryAsync(cancellationToken);
        }

        private static Task CancelCommentAsync(CardTask commentTask, IDbScope dbScope, CancellationToken cancellationToken = default)
        {
            var db = dbScope.Db;
            return db.SetCommand(
                    dbScope.BuilderFactory
                        .DeleteFrom(KrCommentsInfo.Name)
                        .Where().C(RowID).Equals().P("RowID")
                        .Build(),
                    db.Parameter("RowID", commentTask.RowID))
                .LogCommand()
                .ExecuteNonQueryAsync(cancellationToken);
        }

        #endregion

        #region base overrides

        public override async Task StoreTaskBeforeCommitTransaction(ICardStoreTaskExtensionContext context)
        {
            if (!context.ValidationResult.IsSuccessful())
            {
                return;
            }

            if (context.Task.TypeID == DefaultTaskTypes.KrRequestCommentTypeID)
            {
                if (!context.Task.ParentRowID.HasValue)
                {
                    throw new InvalidOperationException("Comment task doesn't contain ParentRowID.");
                }

                if (context.State == CardRowState.Inserted)
                {
                    await InsertNewCommentAsync(context.Task, context.DbScope, context.CancellationToken);
                }
                else if (context.Action == CardTaskAction.Complete
                    && context.CompletionOption.ID == DefaultCompletionOptions.AddComment)
                {
                    await UpdateCommentWithAnswerAsync(context.Task, context.DbScope, context.CancellationToken);
                }
                else if (context.Action == CardTaskAction.Complete
                    && context.CompletionOption.ID == DefaultCompletionOptions.Cancel)
                {
                    await CancelCommentAsync(context.Task, context.DbScope, context.CancellationToken);
                }
            }
        }

        #endregion
    }
}
