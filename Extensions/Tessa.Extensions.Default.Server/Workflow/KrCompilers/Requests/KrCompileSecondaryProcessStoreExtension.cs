using System;
using System.Linq;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Server.Workflow.KrProcess.Serialization;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform.Collections;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers.Requests
{
    public sealed class KrCompileSecondaryProcessStoreExtension :
        KrCompileSourceStoreExtension
    {
        #region Fields

        private readonly IKrCompiler compiler;

        private readonly IKrStageSerializer stageSerializer;

        private readonly IExtraSourceSerializer extraSourceSerializer;

        #endregion

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="KrCompileSecondaryProcessStoreExtension"/>.
        /// </summary>
        /// <param name="processCache">Кэш данных из карточек подсистемы маршрутов.</param>
        /// <param name="compilationCache">Кэш с результатами компиляции объектов подсистемы маршрутов.</param>
        /// <param name="compilationResultStorage">Объект, предоставляющий доступ к результатам компиляции подсистемы маршрутов.</param>
        /// <param name="compiler">Объект, выполняющий компиляцию объектов подсистемы маршрутов.</param>
        /// <param name="stageSerializer">Объект, предоставляющий методы для сериализации параметров этапов.</param>
        /// <param name="extraSourceSerializer">Сериализатор объектов, содержащих информацию о дополнительных методах.</param>
        public KrCompileSecondaryProcessStoreExtension(
            IKrProcessCache processCache,
            IKrCompilationCache compilationCache,
            IKrCompilationResultStorage compilationResultStorage,
            IKrCompiler compiler,
            IKrStageSerializer stageSerializer,
            IExtraSourceSerializer extraSourceSerializer)
            : base(processCache, compilationCache, compilationResultStorage)
        {
            this.compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            this.stageSerializer = stageSerializer ?? throw new ArgumentNullException(nameof(stageSerializer));
            this.extraSourceSerializer = extraSourceSerializer ?? throw new ArgumentNullException(nameof(extraSourceSerializer));
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc />
        protected override async Task<IKrCompilationResult> BuildAsync(
            ICardStoreExtensionContext context)
        {
            var card = context.Request.Card;

            var krCompileContext = new KrCompilationContext();
            (var pure, var act, var buttons) = await KrCompilersSqlHelper.SelectKrSecondaryProcessesAsync(
                context.DbScope,
                card.ID,
                context.CancellationToken);
            krCompileContext.SecondaryProcesses.AddRange(pure);
            krCompileContext.SecondaryProcesses.AddRange(act);
            krCompileContext.SecondaryProcesses.AddRange(buttons);
            krCompileContext.Stages.AddRange(
                await KrCompilersSqlHelper.SelectSecondaryProcessRuntimeStagesAsync(
                    context.DbScope,
                    this.stageSerializer,
                    this.extraSourceSerializer,
                    card.ID,
                    context.CancellationToken));
            krCompileContext.CommonMethods.AddRange(await this.ProcessCache.GetAllCommonMethodsAsync(context.CancellationToken));

            return this.compiler.Compile(krCompileContext);
        }

        /// <inheritdoc/>
        protected override bool SourceChanged(Card card)
        {
            return StageScriptsChanged(card)
                || RuntimeScriptsChanged(card)
                || card.Info.TryGet<bool?>(KrConstants.Keys.ExtraSourcesChanged) == true;
        }

        /// <inheritdoc />
        protected override bool CardChanged(
            Card card) => card.TryGetSections()?.Count > 0;

        #endregion

        #region Private Methods

        private static bool StageScriptsChanged(Card card)
        {
            return card.Sections.TryGetValue(KrConstants.KrSecondaryProcesses.Name, out var sec)
                && (sec.Fields.ContainsKey(KrConstants.KrSecondaryProcesses.VisibilitySourceCondition)
                    || sec.Fields.ContainsKey(KrConstants.KrSecondaryProcesses.ExecutionSourceCondition))
                || card.TryGetKrStageTemplatesSection(out var krStageTemplateSec)
                && (krStageTemplateSec.Fields.ContainsKey(KrConstants.SourceCondition)
                    || krStageTemplateSec.Fields.ContainsKey(KrConstants.SourceBefore)
                    || krStageTemplateSec.Fields.ContainsKey(KrConstants.SourceAfter));
        }

        private static bool RuntimeScriptsChanged(Card card)
        {
            return card.Sections.TryGetValue(KrConstants.KrStages.Virtual, out var sec)
                && sec.Rows.Any(p =>
                    p.State == CardRowState.Deleted
                    || p.State == CardRowState.Inserted
                    || p.ContainsKey(KrConstants.RuntimeSourceAfter)
                    || p.ContainsKey(KrConstants.RuntimeSourceBefore)
                    || p.ContainsKey(KrConstants.RuntimeSourceCondition));
        }

        #endregion
    }
}
