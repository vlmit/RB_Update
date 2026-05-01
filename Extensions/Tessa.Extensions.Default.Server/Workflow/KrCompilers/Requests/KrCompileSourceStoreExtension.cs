using System;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Localization;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers.Requests
{
    public abstract class KrCompileSourceStoreExtension :
        CardStoreExtension
    {
        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="KrCompileSourceStoreExtension"/>.
        /// </summary>
        /// <param name="processCache">Кэш данных из карточек подсистемы маршрутов.</param>
        /// <param name="compilationCache">Кэш с результатами компиляции объектов подсистемы маршрутов.</param>
        /// <param name="compilationResultStorage">Объект, предоставляющий доступ к результатам компиляции подсистемы маршрутов.</param>
        protected KrCompileSourceStoreExtension(
            IKrProcessCache processCache,
            IKrCompilationCache compilationCache,
            IKrCompilationResultStorage compilationResultStorage)
        {
            this.ProcessCache = processCache ?? throw new ArgumentNullException(nameof(processCache));
            this.CompilationCache = compilationCache ?? throw new ArgumentNullException(nameof(compilationCache));
            this.CompilationResultStorage = compilationResultStorage ?? throw new ArgumentNullException(nameof(compilationResultStorage));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Кэш данных из карточек подсистемы маршрутов.
        /// </summary>
        protected IKrProcessCache ProcessCache { get; }

        /// <summary>
        /// Кэш с результатами компиляции объектов подсистемы маршрутов.
        /// </summary>
        protected IKrCompilationCache CompilationCache { get; }

        /// <summary>
        /// Объект, предоставляющий доступ к результатам компиляции подсистемы маршрутов.
        /// </summary>
        protected IKrCompilationResultStorage CompilationResultStorage { get; }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Возвращает значение, показывающее, возможна ли компиляция сценариев или нет.
        /// </summary>
        /// <param name="context">Контекст процесса сохранения карточки.</param>
        /// <returns>значение, показывающее, возможна ли компиляция сценариев или нет.</returns>
        protected virtual bool CanBuild(
            ICardStoreExtensionContext context) => true;

        /// <summary>
        /// Выполняет компиляцию сценариев.
        /// </summary>
        /// <param name="context">Контекст процесса сохранения карточки.</param>
        /// <returns>Результат компиляции.</returns>
        protected abstract Task<IKrCompilationResult> BuildAsync(ICardStoreExtensionContext context);

        /// <summary>
        /// Возвращает значение, показывающее, наличие изменений в сценариях.
        /// </summary>
        /// <param name="card">Проверяемая карточка.</param>
        /// <returns>Значение, показывающее, наличие изменений в сценариях.</returns>
        protected abstract bool SourceChanged(Card card);

        /// <summary>
        /// Возвращает значение, показывающее, наличие изменений в карточке.
        /// </summary>
        /// <param name="card">Проверяемая карточка.</param>
        /// <returns>Значение, показывающее, наличие изменений в карточке.</returns>
        protected abstract bool CardChanged(Card card);

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override async Task AfterRequest(ICardStoreExtensionContext context)
        {
            if (!context.ValidationResult.IsSuccessful())
            {
                return;
            }

            if (context.Request.Info.ContainsKey(KrConstants.Keys.Compile)
                && this.CanBuild(context))
            {
                var result = await this.BuildAsync(context);
                await this.SetLastBuildOutputAsync(context, result, context.CancellationToken);
            }
            else if (context.Request.Info.ContainsKey(KrConstants.Keys.CompileWithValidationResult)
                && this.CanBuild(context))
            {
                var result = await this.BuildAsync(context);
                await this.SetLastBuildOutputAsync(context, result, context.CancellationToken);
                await this.FillValidationResultAsync(context, result);
            }
            else if (context.Request.Info.ContainsKey(KrConstants.Keys.CompileAll)
                && this.CanBuild(context))
            {
                await this.RebuildAllAsync(context);
            }
            else if (context.Request.Info.ContainsKey(KrConstants.Keys.CompileAllWithValidationResult)
                && this.CanBuild(context))
            {
                var result = await this.RebuildAllAsync(context);
                await this.FillValidationResultAsync(context, result);
            }
            else if (context.Request.Card.StoreMode == CardStoreMode.Insert
                || this.SourceChanged(context.Request.Card))
            {
                await this.ProcessCache.InvalidateAsync(context.CancellationToken);
                await this.CompilationCache.InvalidateAsync(context.CancellationToken);
            }
            else if (context.Request.Card.StoreMode == CardStoreMode.Insert
                || this.CardChanged(context.Request.Card))
            {
                await this.ProcessCache.InvalidateAsync(context.CancellationToken);
            }
        }

        #endregion

        #region Private Methods

        private Task SetLastBuildOutputAsync(
            ICardStoreExtensionContext context,
            IKrCompilationResult result,
            CancellationToken cancellationToken = default)
        {
            return this.CompilationResultStorage.UpsertAsync(
                 context.Request.Card.ID,
                 result,
                 cancellationToken: cancellationToken);
        }

        private async ValueTask FillValidationResultAsync(
            ICardStoreExtensionContext context,
            IKrCompilationResult result)
        {
            context.ValidationResult.AddInfo(
                this,
                result.Result.Assembly is not null ?
                    await LocalizationManager.GetStringAsync("KrMessages_KrStageSourceSuccessfulBuild", context.CancellationToken) :
                    await LocalizationManager.GetStringAsync("KrMessages_KrStageSourceFailedBuild", context.CancellationToken));
            context.ValidationResult.Add(result.ValidationResult);
        }

        private async Task<IKrCompilationResult> RebuildAllAsync(ICardStoreExtensionContext context)
        {
            await this.ProcessCache.InvalidateAsync(context.CancellationToken);
            return await this.CompilationCache.RebuildAsync(context.CancellationToken);
        }

        #endregion
    }
}
