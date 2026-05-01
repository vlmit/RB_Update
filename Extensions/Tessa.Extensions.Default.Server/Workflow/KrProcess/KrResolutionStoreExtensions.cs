using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tessa.Cards.Extensions;
using Tessa.Cards.Workflow;
using Tessa.Extensions.Default.Shared;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Extensions.Default.Shared.Workflow.Wf;
using Tessa.Extensions.Default.Shared.Workflow.WorkflowEngine;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;
using Tessa.Workflow;
using Tessa.Workflow.Helpful;
using Tessa.Workflow.Signals;
using WorkflowSignalTypes = Tessa.Workflow.Signals.WorkflowSignalTypes;

namespace Tessa.Extensions.Default.Server.Workflow.KrProcess
{
    /// <summary>
    /// Расширение на сохранение задания. Уведомляет подсистему маршрутов или Workflow Engine о завершении следующих типов заданий: <see cref="DefaultTaskTypes.WfResolutionTypeID"/>, <see cref="DefaultTaskTypes.WfResolutionChildTypeID"/>, <see cref="DefaultTaskTypes.WfResolutionControlTypeID"/>.
    /// </summary>
    public sealed class KrResolutionStoreExtensions : CardStoreTaskExtension
    {
        #region Constants

        private const string KrResolutionRevokingChildren = nameof(KrResolutionRevokingChildren);

        #endregion

        #region Fields

        /// <summary>
        /// Объект-обработчик процессов WorkflowEngine на сервере.
        /// </summary>
        private readonly IWorkflowEngineProcessor workflowProcessor;

        #endregion

        #region Constructor

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="KrResolutionStoreExtensions"/>.
        /// </summary>
        /// <param name="workflowProcessor">Объект-обработчик процессов WorkflowEngine на сервере.</param>
        public KrResolutionStoreExtensions(
            IWorkflowEngineProcessor workflowProcessor)
        {
            this.workflowProcessor = workflowProcessor ?? throw new ArgumentNullException(nameof(workflowProcessor));
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override async Task StoreTaskBeforeCommitTransaction(ICardStoreTaskExtensionContext context)
        {
            if (!context.IsCompletion
                || context.CompletionOption.ID != DefaultCompletionOptions.Complete
                && context.CompletionOption.ID != DefaultCompletionOptions.Revoke
                || !context.ValidationResult.IsSuccessful())
            {
                return;
            }

            var parentContext = WorkflowScopeContext.Current.StoreContext;
            if (parentContext is not null && parentContext.Info.TryGet<bool>(KrResolutionRevokingChildren))
            {
                return;
            }

            if (context.Task.Card
                .Sections[WfHelper.ResolutionSection]
                .Fields.Get<bool>(WfHelper.ResolutionRevokeChildrenField))
            {
                context.StoreContext.Info[KrResolutionRevokingChildren] = BooleanBoxes.True;
            }

            string processKind = default;
            Guid processID = default;
            Guid parentTaskID = default;

            await using (context.DbScope.Create())
            {
                var db = context.DbScope.Db;
                await using var reader = await db
                    .SetCommand(
                        context.DbScope.BuilderFactory
                            .With("ParentTaskHistory", static e => e
                                    .Select().C("t", "ParentRowID")
                                    .From("TaskHistory", "t").NoLock()
                                    .Where().C("t", "TypeID").NotEquals().P("ProjectTypeID")
                                    .And().C("t", "RowID").Equals().P("RowID")
                                    .UnionAll()
                                    .Select().C("t", "ParentRowID")
                                    .From("TaskHistory", "t").NoLock()
                                    .InnerJoin("ParentTaskHistory", "p")
                                    .On().C("p", "RowID").Equals().C("t", "RowID")
                                    .Where().C("t", "TypeID").NotEquals().P("ProjectTypeID"),
                                columnNames: new[] { "RowID" },
                                recursive: true)
                            .Select().C("t", "ProcessID", "ProcessKind", "RowID")
                            .From("TaskHistory", "t").NoLock()
                            .InnerJoin("ParentTaskHistory", "p").NoLock()
                            .On().C("p", "RowID").Equals().C("t", "RowID")
                            .Where().C("t", "TypeID").Equals().P("ProjectTypeID")
                            .If(context.TaskType.ID != DefaultTaskTypes.WfResolutionTypeID
                                || context.CompletionOption.ID != DefaultCompletionOptions.Revoke,
                                static e => e
                                    .And().NotExists(static e => e
                                        .Select().V(null)
                                        .From("TaskHistory", "t").NoLock()
                                        .InnerJoin("ParentTaskHistory", "p").NoLock()
                                        .On().C("p", "RowID").Equals().C("t", "ParentRowID")
                                        .InnerJoin("WfSatelliteTaskHistory", "s").NoLock()
                                        .On().C("s", "RowID").Equals().C("t", "RowID")
                                        .Where().C("t", "Completed").IsNull()
                                        .Or().C("s", "Controlled").Equals().V(false)))
                            .Build(),
                        db.Parameter("RowID", context.Task.RowID),
                        db.Parameter("ProjectTypeID", DefaultTaskTypes.WfResolutionProjectTypeID))
                    .ExecuteReaderAsync(context.CancellationToken);
                if (await reader.ReadAsync(context.CancellationToken))
                {
                    processID = reader.GetGuid(0);
                    processKind = reader.GetString(1);
                    parentTaskID = reader.GetGuid(2);
                }
            }

            switch (processKind)
            {
                case KrConstants.KrProcessName:
                case KrConstants.KrSecondaryProcessName:
                case KrConstants.KrNestedProcessName:
                    context.Request.Card
                        .GetWorkflowQueue()
                        .AddSignal(name: KrConstants.KrPerformSignal, processID: processID, processTypeName: processKind);
                    break;
                case WorkflowEngineHelper.WorkflowEngineProcessName:
                    {
                        var signal = new WorkflowEngineTaskSignal(
                            WorkflowSignalTypes.CompleteTask,
                            new List<object> { parentTaskID })
                        {
                            OptionID = context.CompletionOption.ID
                        };
                        signal.Hash[WorkflowConstants.NamesKeys.CompletedTaskRowID] = context.Task.RowID;

                        var result =
                            await this.workflowProcessor.SendSignalToAllSubscribersAsync(
                                processID,
                                signal,
                                WorkflowEngineProcessFlags.DefaultRuntime,
                                storeCard: context.Request.Card,
                                cancellationToken: context.CancellationToken);
                        context.ValidationResult.Add(result.ValidationResult);

                        break;
                    }
            }
        }

        #endregion
    }
}
