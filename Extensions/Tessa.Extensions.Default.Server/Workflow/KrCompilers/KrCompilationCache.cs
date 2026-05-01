using System;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards.Caching;
using Tessa.Platform;
using Tessa.Platform.Collections;
using Unity;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    /// <inheritdoc cref="IKrCompilationCache"/>
    public sealed class KrCompilationCache :
        IKrCompilationCache,
        IDisposable
    {
        #region Constants And Static Fields

        private const string CacheKey = "KrCompilationCache";

        private const string IsRebuildKey = CacheKey + "_IsRebuild";

        private static readonly Guid MainCompilationResultID = Guid.Empty;

        #endregion

        #region Fields

        private readonly ICardCache cardCache;
        private readonly IKrProcessCache processCache;
        private readonly IKrCompiler krCompiler;
        private readonly IKrCompilationResultStorage сompilationResultStorage;

        private readonly AsyncLock asyncLock = new AsyncLock();

        #endregion

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="KrCompilationCache"/>.
        /// </summary>
        /// <param name="cardCache">Потокобезопасный кэш с карточками и дополнительными настройками.</param>
        /// <param name="processCache">Кэш данных из карточек подсистемы маршрутов.</param>
        /// <param name="krCompiler">Объект, выполняющий компиляцию объектов подсистемы маршрутов.</param>
        /// <param name="сompilationResultStorage">Объект, предоставляющий доступ к результатам компиляции подсистемы маршрутов.</param>
        /// <param name="container">Контейнер, содержащий объекты <see cref="IDisposable"/>, которые будут освобождены при закрытии контейнеров <see cref="IUnityContainer"/>.</param>
        public KrCompilationCache(
            ICardCache cardCache,
            IKrProcessCache processCache,
            IKrCompiler krCompiler,
            IKrCompilationResultStorage сompilationResultStorage,
            [OptionalDependency] IUnityDisposableContainer container = null)
        {
            this.cardCache = cardCache ?? throw new ArgumentNullException(nameof(cardCache));
            this.processCache = processCache ?? throw new ArgumentNullException(nameof(processCache));
            this.krCompiler = krCompiler ?? throw new ArgumentNullException(nameof(krCompiler));
            this.сompilationResultStorage = сompilationResultStorage ?? throw new ArgumentNullException(nameof(сompilationResultStorage));

            container?.Register(this);
        }

        #endregion

        #region Private Methods

        private static bool TessaVersionIsEqual(IKrCompilationResult res) =>
            res.Result.BuildDate == BuildInfo.Date && res.Result.BuildVersion == BuildInfo.Version;

        private async Task<IKrCompilationResult> CompileAsync(string key, CancellationToken cancellationToken = default)
        {
            var res = await this.cardCache.Settings.TryGetAlreadyCachedAsync<IKrCompilationResult>(CacheKey, cancellationToken);
            if (res is not null)
            {
                return res;
            }

            using (await this.asyncLock.EnterAsync(cancellationToken))
            {
                res = await this.cardCache.Settings.TryGetAlreadyCachedAsync<IKrCompilationResult>(CacheKey, cancellationToken);
                if (res is not null)
                {
                    return res;
                }

                // Запрошена пересборка?
                var isRebuild = await this.cardCache.Settings.TryGetAlreadyCachedAsync<bool?>(IsRebuildKey, cancellationToken) ?? false;

                if (!isRebuild)
                {
                    res = await this.сompilationResultStorage.GetCompilationResultAsync(MainCompilationResultID, cancellationToken);

                    if (res is not null
                        && TessaVersionIsEqual(res))
                    {
                        return res;
                    }
                }

                var krCompileContext = new KrCompilationContext();

                krCompileContext.Stages.AddRange((await this.processCache.GetAllRuntimeStagesAsync(cancellationToken)).Values);
                krCompileContext.StageTemplates.AddRange((await this.processCache.GetAllStageTemplatesAsync(cancellationToken)).Values);
                krCompileContext.CommonMethods.AddRange(await this.processCache.GetAllCommonMethodsAsync(cancellationToken));
                krCompileContext.StageGroups.AddRange((await this.processCache.GetAllStageGroupsAsync(cancellationToken)).Values);
                krCompileContext.SecondaryProcesses.AddRange((await this.processCache.GetAllPureProcessesAsync(cancellationToken)).Values);
                krCompileContext.SecondaryProcesses.AddRange((await this.processCache.GetAllButtonsAsync(cancellationToken)).Values);
                krCompileContext.SecondaryProcesses.AddRange((await this.processCache.GetAllActionsAsync(cancellationToken)).Values);

                var result = this.krCompiler.Compile(krCompileContext);
                await this.сompilationResultStorage.UpsertAsync(
                    MainCompilationResultID,
                    result,
                    true,
                    cancellationToken);

                if (isRebuild)
                {
                    await this.cardCache.Settings.InvalidateAsync(IsRebuildKey, cancellationToken);
                }

                return result;
            }
        }

        #endregion

        #region IKrCompilationCache Members

        /// <inheritdoc/>
        public async Task<IKrCompilationResult> BuildAsync(CancellationToken cancellationToken = default)
        {
            if (await this.cardCache.Settings.ContainsAsync(CacheKey, cancellationToken))
            {
                return null;
            }

            return await this.cardCache.Settings.GetAsync(CacheKey, this.CompileAsync, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IKrCompilationResult> RebuildAsync(CancellationToken cancellationToken = default)
        {
            await this.InvalidateAsync(cancellationToken);
            return await this.cardCache.Settings.GetAsync(CacheKey, this.CompileAsync, cancellationToken);
        }

        /// <inheritdoc/>
        public ValueTask<IKrCompilationResult> GetAsync(CancellationToken cancellationToken = default) =>
            this.cardCache.Settings.GetAsync(CacheKey, this.CompileAsync, cancellationToken);

        /// <inheritdoc/>
        public async Task InvalidateAsync(CancellationToken cancellationToken = default)
        {
            using (await this.asyncLock.EnterAsync(cancellationToken))
            {
                await this.cardCache.Settings.GetAsync(IsRebuildKey, static _ => true, cancellationToken);
                await this.сompilationResultStorage.DeleteCompilationResultAsync(MainCompilationResultID, cancellationToken);
                await this.cardCache.Settings.InvalidateAsync(CacheKey, cancellationToken);
            }
        }

        #endregion

        #region IDisposable Members

        /// <inheritdoc/>
        public void Dispose() =>
            this.asyncLock.Dispose();

        #endregion
    }
}
