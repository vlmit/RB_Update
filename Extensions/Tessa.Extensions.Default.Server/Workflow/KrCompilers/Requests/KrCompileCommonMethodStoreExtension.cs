using System;
using System.Linq;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform.Collections;
using Tessa.Platform.Data;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers.Requests
{
    public sealed class KrCompileCommonMethodStoreExtension :
        KrCompileSourceStoreExtension
    {
        #region Fields

        private readonly IKrCompiler compiler;

        #endregion

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="KrCompileCommonMethodStoreExtension"/>.
        /// </summary>
        /// <param name="processCache">Кэш данных из карточек подсистемы маршрутов.</param>
        /// <param name="compilationCache">Кэш с результатами компиляции объектов подсистемы маршрутов.</param>
        /// <param name="compilationResultStorage">Объект, предоставляющий доступ к результатам компиляции подсистемы маршрутов.</param>
        /// <param name="compiler">Объект, выполняющий компиляцию объектов подсистемы маршрутов.</param>
        public KrCompileCommonMethodStoreExtension(
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

        /// <inheritdoc/>
        protected override async Task<IKrCompilationResult> BuildAsync(ICardStoreExtensionContext context)
        {
            var card = context.Request.Card;

            var krCompileContext = new KrCompilationContext();
            var methods = (await this.ProcessCache.GetAllCommonMethodsAsync(context.CancellationToken)).Where(p => p.ID != card.ID);
            krCompileContext.CommonMethods.AddRange(methods);
            krCompileContext.CommonMethods.Add((await KrCompilersSqlHelper.SelectCommonMethodsAsync(
                context.DbScope,
                card.ID,
                context.CancellationToken)).FirstOrDefault());

            return this.compiler.Compile(krCompileContext);
        }

        /// <inheritdoc/>
        protected override bool SourceChanged(Card card)
        {
            if (card.TryGetKrStageCommonMethodsSection(out var sec))
            {
                return sec.Fields.ContainsKey(KrConstants.Name) || sec.Fields.ContainsKey(KrConstants.KrStageCommonMethods.Source);
            }
            return false;
        }

        /// <inheritdoc/>
        protected override bool CardChanged(Card card) =>
            card.TryGetKrStageCommonMethodsSection(out _);

        #endregion
    }
}
