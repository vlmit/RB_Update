using System;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Caching;
using Tessa.Platform;
using Tessa.Platform.Collections;
using Tessa.Platform.Placeholders.Compilation;
using Unity;

namespace Tessa.Extensions.Default.Server.Files.VirtualFiles.Compilation
{
    public sealed class KrVirtualFileCompilationCache :
        IKrVirtualFileCompilationCache,
        IDisposable
    {
        #region Fields

        private readonly IPlaceholderCompilationStorage compilationStorage;
        private readonly IKrVirtualFileCompiler compiler;
        private readonly IKrVirtualFileCache virtualFileCache;
        private readonly ICardCache cardCache;
        private readonly AsyncLock asyncLock = new AsyncLock();

        private const string CacheKey = CardHelper.SystemKeyPrefix + nameof(KrVirtualFileCompilationCache);

        // {3B39D324-413E-40C4-8BDF-D581FD560B40}
        private readonly Guid cacheStorageID = new Guid(0x3b39d324, 0x413e, 0x40c4, 0x8b, 0xdf, 0xd5, 0x81, 0xfd, 0x56, 0xb, 0x40);

        #endregion

        #region Constructors

        public KrVirtualFileCompilationCache(
            IPlaceholderCompilationStorage compilationStorage,
            IKrVirtualFileCompiler compiler,
            IKrVirtualFileCache virtualFileCache,
            ICardCache cardCache,
            [OptionalDependency] IUnityDisposableContainer container = null)
        {
            this.compilationStorage = compilationStorage;
            this.compiler = compiler;
            this.virtualFileCache = virtualFileCache;
            this.cardCache = cardCache;

            container?.Register(this);
        }

        #endregion

        #region IKrVirtualFileCompilationCache Implementation

        public ValueTask<IKrVirtualFileCompilationResult> GetAsync(CancellationToken cancellationToken = default)
        {
            return this.cardCache.Settings.GetAsync(CacheKey, this.GetCompilationResultAsync, cancellationToken);
        }

        public async Task InvalidateAsync(CancellationToken cancellationToken = default)
        {
            using (await this.asyncLock.EnterAsync(cancellationToken))
            {
                await this.cardCache.Settings.InvalidateAsync(CacheKey, cancellationToken);
                await this.compilationStorage.DeleteAsync(cacheStorageID, cancellationToken);
            }
        }

        #endregion

        #region Private Methods

        private async Task<IKrVirtualFileCompilationResult> GetCompilationResultAsync(string arg, CancellationToken cancellationToken = default)
        {
            using (await this.asyncLock.EnterAsync(cancellationToken))
            {
                var result = await this.cardCache.Settings.TryGetAlreadyCachedAsync<IKrVirtualFileCompilationResult>(CacheKey, cancellationToken);
                if (result != null)
                {
                    return result;
                }

                result = await this.compilationStorage.GetAsync<IKrVirtualFileCompilationResult>(cacheStorageID, cancellationToken);
                if (result != null
                    && result.IsValid)
                {
                    return result;
                }

                var compilationCotext = this.compiler.CreateContext();
                compilationCotext.Files.AddRange(await virtualFileCache.GetAllAsync(cancellationToken));
                result = this.compiler.Compile(compilationCotext);

                if (result.ValidationResult.IsSuccessful)
                {
                    await this.compilationStorage.StoreAsync(result, cacheStorageID, result.Created, cancellationToken);
                }

                return result;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() => this.asyncLock.Dispose();

        #endregion
    }
}