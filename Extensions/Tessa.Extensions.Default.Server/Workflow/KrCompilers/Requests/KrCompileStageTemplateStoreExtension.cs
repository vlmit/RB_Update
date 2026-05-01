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
    /// <summary>
    /// Расширение на сохранение карточки KrStageTemplates
    /// Выполняет компиляцию при наличии соответствующих флагов в Info
    /// При изменении исходных кодов сбрасывается кэш компиляции и кэш этапов
    /// При изменении данных, не относящихся к компиляции, сбрасывается кэш этапов
    /// </summary>
    public sealed class KrCompileStageTemplateStoreExtension :
        KrCompileSourceStoreExtension
    {
        #region Fields

        private readonly IKrCompiler compiler;
        private readonly IExtraSourceSerializer extraSourceSerializer;
        private readonly IKrStageSerializer stageSerializer;

        #endregion

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="KrCompileStageTemplateStoreExtension"/>.
        /// </summary>
        /// <param name="processCache">Кэш данных из карточек подсистемы маршрутов.</param>
        /// <param name="compilationCache">Кэш с результатами компиляции объектов подсистемы маршрутов.</param>
        /// <param name="compilationResultStorage">Объект, предоставляющий доступ к результатам компиляции подсистемы маршрутов.</param>
        /// <param name="compiler">Объект, выполняющий компиляцию объектов подсистемы маршрутов.</param>
        /// <param name="extraSourceSerializer">Сериализатор объектов, содержащих информацию о дополнительных методах.</param>
        /// <param name="stageSerializer">Объект, предоставляющий методы для сериализации параметров этапов.</param>
        public KrCompileStageTemplateStoreExtension(
            IKrProcessCache processCache,
            IKrCompilationCache compilationCache,
            IKrCompilationResultStorage compilationResultStorage,
            IKrCompiler compiler,
            IExtraSourceSerializer extraSourceSerializer,
            IKrStageSerializer stageSerializer)
            : base(processCache, compilationCache, compilationResultStorage)
        {
            this.compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            this.extraSourceSerializer = extraSourceSerializer ?? throw new ArgumentNullException(nameof(extraSourceSerializer));
            this.stageSerializer = stageSerializer ?? throw new ArgumentNullException(nameof(stageSerializer));
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        protected override async Task<IKrCompilationResult> BuildAsync(ICardStoreExtensionContext context)
        {
            var card = context.Request.Card;

            var krCompileContext = new KrCompilationContext();
            krCompileContext.StageTemplates.Add((await KrCompilersSqlHelper.SelectStageTemplatesAsync(context.DbScope, card.ID, context.CancellationToken)).FirstOrDefault());
            krCompileContext.Stages.AddRange(
                await KrCompilersSqlHelper.SelectRuntimeStagesAsync(context.DbScope, this.stageSerializer, this.extraSourceSerializer, card.ID, context.CancellationToken));
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

        /// <inheritdoc/>
        protected override bool CardChanged(Card card) =>
            card.TryGetSections()?.Count > 0;

        #endregion

        #region Private Methods

        private static bool StageScriptsChanged(Card card)
        {
            return card.TryGetKrStageTemplatesSection(out var krStageTemplateSec)
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
