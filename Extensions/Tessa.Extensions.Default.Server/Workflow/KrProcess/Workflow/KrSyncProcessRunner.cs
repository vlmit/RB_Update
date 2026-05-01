using System;
using System.Linq;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Caching;
using Tessa.Extensions.Default.Server.Workflow.KrCompilers;
using Tessa.Extensions.Default.Server.Workflow.KrCompilers.SqlProcessing;
using Tessa.Extensions.Default.Server.Workflow.KrObjectModel;
using Tessa.Extensions.Default.Server.Workflow.KrProcess.Scope;
using Tessa.Extensions.Default.Server.Workflow.KrProcess.Serialization;
using Tessa.Extensions.Default.Server.Workflow.KrProcess.Workflow.Handlers;
using Tessa.Extensions.Default.Server.Workflow.KrProcess.Workflow.StateMachine;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Runtime;
using Tessa.Platform.Validation;
using Unity;

namespace Tessa.Extensions.Default.Server.Workflow.KrProcess.Workflow
{
    /// <summary>
    /// Раннер используемый для выполнения синхронных процессов маршрутов документов.
    /// </summary>
    public sealed class KrSyncProcessRunner : KrProcessRunnerBase
    {
        #region Constructors

        public KrSyncProcessRunner(
            IKrProcessContainer container,
            IKrCompilationCache compilationCache,
            [Dependency(KrExecutorNames.CacheExecutor)] Func<IKrExecutor> executorFunc,
            IKrScope scope,
            IDbScope dbScope,
            IKrProcessCache processCache,
            IUnityContainer unityContainer,
            ISession session,
            IKrProcessRunnerProvider runnerProvider,
            IKrTypesCache typesCache,
            ICardMetadata cardMetadata,
            IKrProcessStateMachine stateMachine,
            IKrStageInterrupter interrupter,
            IKrSqlExecutor sqlExecutor,
            ICardCache cardCache,
            IKrStageSerializer serializer,
            IObjectModelMapper mapper,
            IKrTokenProvider tokenProvider,
            [Dependency(CardRepositoryNames.Extended)] ICardRepository cardRepositoryExt,
            [Dependency(CardRepositoryNames.ExtendedWithoutTransaction)] ICardRepository cardRepositoryEwt,
            [Dependency(CardRepositoryNames.Extended)] ICardStreamServerRepository streamServerRepositoryExt,
            [Dependency(CardRepositoryNames.ExtendedWithoutTransaction)] ICardStreamServerRepository streamServerRepositoryEwt)
            : base(
                container,
                compilationCache,
                executorFunc,
                scope,
                dbScope,
                processCache,
                unityContainer,
                session,
                runnerProvider,
                typesCache,
                cardMetadata,
                stateMachine,
                interrupter,
                sqlExecutor,
                cardCache,
                serializer,
                mapper,
                tokenProvider,
                cardRepositoryExt,
                cardRepositoryEwt,
                streamServerRepositoryExt,
                streamServerRepositoryEwt)
        {
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc />
        protected override KrProcessRunnerMode RunnerMode { get; } = KrProcessRunnerMode.Sync;

        /// <inheritdoc />
        protected override async Task<bool> PrepareAsync(IKrProcessRunnerContext context)
        {
            if (context.InitiationCause != KrProcessRunnerInitiationCause.InMemoryLaunching
                && context.InitiationCause != KrProcessRunnerInitiationCause.Resurrection)
            {
                context.ValidationResult.AddError(this, $"{this.GetType().Name} works only with" +
                    $" {nameof(KrProcessRunnerInitiationCause)}.{nameof(KrProcessRunnerInitiationCause.InMemoryLaunching)}");
                return false;
            }

            if (context.WorkflowProcess.Stages.Count == 0)
            {
                if (!context.NotMessageHasNoActiveStages)
                {
                    context.ValidationResult.AddError(this, KrErrorHelper.FormatEmptyRoute(context.SecondaryProcess));
                }

                return false;
            }

            await this.SetAuthorAsync(context);

            return true;
        }

        /// <inheritdoc />
        protected override ValueTask FinalizeAsync(
            IKrProcessRunnerContext context,
            Exception exc = null)
        {
            if (!context.NotMessageHasNoActiveStages
                && context.WorkflowProcess.Stages.Count > 0
                && context.WorkflowProcess.Stages.All(p => p.State == KrStageState.Skipped))
            {
                context.ValidationResult.AddError(this, KrErrorHelper.FormatEmptyRoute(context.SecondaryProcess));
            }

            return new ValueTask();
        }

        /// <inheritdoc />
        protected override async Task<NextAction> ProcessStageHandlerResultAsync(
            Stage stage,
            StageHandlerResult result,
            IKrProcessRunnerContext context)
        {
            if (result.Action != StageHandlerAction.InProgress)
            {
                return await base.ProcessStageHandlerResultAsync(stage, result, context);
            }

            context.ValidationResult.AddError(this,
                $"{this.GetType().Name} can't handle " +
                $"{nameof(StageHandlerAction)}.{nameof(StageHandlerAction.InProgress)}");

            return new NextAction();
        }

        #endregion
    }
}