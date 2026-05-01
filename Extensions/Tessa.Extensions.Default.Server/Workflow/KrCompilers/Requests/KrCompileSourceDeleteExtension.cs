using System;
using System.Threading.Tasks;
using Tessa.Cards.Extensions;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers.Requests
{
    public sealed class KrCompileSourceDeleteExtension :
        CardDeleteExtension
    {
        #region Fields

        private readonly IKrCompilationCache compilationCache;

        private readonly IKrProcessCache processCache;

        private readonly IKrCompilationResultStorage compilationResultStorage;

        #endregion

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="KrCompileSourceDeleteExtension"/>.
        /// </summary>
        /// <param name="compilationCache">Кэш с результатами компиляции объектов подсистемы маршрутов.</param>
        /// <param name="processCache">Кэш данных из карточек подсистемы маршрутов.</param>
        /// <param name="compilationResultStorage">Объект, предоставляющий доступ к результатам компиляции подсистемы маршрутов.</param>
        public KrCompileSourceDeleteExtension(
            IKrCompilationCache compilationCache,
            IKrProcessCache processCache,
            IKrCompilationResultStorage compilationResultStorage)
        {
            this.compilationCache = compilationCache ?? throw new ArgumentNullException(nameof(compilationCache));
            this.processCache = processCache ?? throw new ArgumentNullException(nameof(processCache));
            this.compilationResultStorage = compilationResultStorage ?? throw new ArgumentNullException(nameof(compilationResultStorage));
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override async Task AfterRequest(ICardDeleteExtensionContext context)
        {
            if (!context.RequestIsSuccessful)
            {
                return;
            }

            await this.processCache.InvalidateAsync(context.CancellationToken);
            await this.compilationCache.InvalidateAsync(context.CancellationToken);

            var cardID = context.Request.CardID;
            if (cardID.HasValue)
            {
                await this.compilationResultStorage.DeleteAsync(cardID.Value, context.CancellationToken);
            }
        }

        #endregion
    }
}
