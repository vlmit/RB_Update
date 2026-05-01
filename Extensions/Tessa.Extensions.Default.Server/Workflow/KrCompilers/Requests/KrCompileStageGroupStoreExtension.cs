using System;
using System.Linq;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform.Collections;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers.Requests
{
    public sealed class KrCompileStageGroupStoreExtension :
        KrCompileSourceStoreExtension
    {
        #region Fields

        private readonly IKrCompiler compiler;

        #endregion

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="KrCompileStageGroupStoreExtension"/>.
        /// </summary>
        /// <param name="processCache">Кэш данных из карточек подсистемы маршрутов.</param>
        /// <param name="compilationCache">Кэш с результатами компиляции объектов подсистемы маршрутов.</param>
        /// <param name="compilationResultStorage">Объект, предоставляющий доступ к результатам компиляции подсистемы маршрутов.</param>
        /// <param name="compiler">Объект, выполняющий компиляцию объектов подсистемы маршрутов.</param>
        public KrCompileStageGroupStoreExtension(
            IKrProcessCache processCache,
            IKrCompilationCache compilationCache,
            IKrCompilationResultStorage compilationResultStorage,
            IKrCompiler compiler)
            : base(processCache, compilationCache, compilationResultStorage)
        {
            this.compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc />
        protected override async Task<IKrCompilationResult> BuildAsync(
            ICardStoreExtensionContext context)
        {
            var card = context.Request.Card;

            var krCompileContext = new KrCompilationContext();
            krCompileContext.StageGroups.Add((await KrCompilersSqlHelper.SelectStageGroupsAsync(
                context.DbScope,
                card.ID,
                context.CancellationToken)).FirstOrDefault());

            krCompileContext.CommonMethods.AddRange(await this.ProcessCache.GetAllCommonMethodsAsync(context.CancellationToken));

            return this.compiler.Compile(krCompileContext);
        }

        /// <inheritdoc />
        protected override bool SourceChanged(
            Card card)
        {
            return card.Sections.TryGetValue(KrConstants.KrStageGroups.Name, out var sec)
                && (sec.Fields.ContainsKey(KrConstants.SourceAfter)
                    || sec.Fields.ContainsKey(KrConstants.SourceBefore)
                    || sec.Fields.ContainsKey(KrConstants.SourceCondition)
                    || sec.Fields.ContainsKey(KrConstants.RuntimeSourceAfter)
                    || sec.Fields.ContainsKey(KrConstants.RuntimeSourceBefore)
                    || sec.Fields.ContainsKey(KrConstants.RuntimeSourceCondition));
        }

        /// <inheritdoc />
        protected override bool CardChanged(
            Card card) => card.TryGetSections()?.Count > 0;

        #endregion
    }
}
