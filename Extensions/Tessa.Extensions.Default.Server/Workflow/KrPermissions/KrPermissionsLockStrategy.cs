using System;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Platform.Data;
using static Tessa.Extensions.Default.Shared.Workflow.KrPermissions.KrPermissionsHelper;

namespace Tessa.Extensions.Default.Server.Workflow.KrPermissions
{
    /// <inheritdoc />
    public sealed class KrPermissionsLockStrategy : IKrPermissionsLockStrategy
    {
        #region Nested Types

        private class KrPermissionLockSlim : IAsyncDisposable
        {
            #region Fields

            private readonly string lockType;
            private readonly KrPermissionsLockStrategy lockStrategy;

            private bool isDisposed; // = false;

            #endregion

            #region Constrcutors

            public KrPermissionLockSlim(
                string lockType,
                KrPermissionsLockStrategy lockStrategy)
            {
                this.lockType = lockType;
                this.lockStrategy = lockStrategy;
            }

            #endregion

            #region IAsyncDisposable Implementation

            public async ValueTask DisposeAsync()
            {
                if (isDisposed)
                {
                    return;
                }

                isDisposed = true;
                await lockStrategy.ReleaseLockAsync(lockType);
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly IDbScope dbScope;

        #endregion

        #region Constructors

        public KrPermissionsLockStrategy(IDbScope dbScope)
        {
            this.dbScope = dbScope;
        }

        #endregion

        #region IKrPermissionsLockStrategy Implementation

        /// <inheritdoc />
        public async Task<IAsyncDisposable> TryObtainReaderLockAsync(
            CancellationToken cancellationToken = default)
        {
            return await TryObtainLockAsync(
                "Readers",
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IAsyncDisposable> TryObtainWriterLockAsync(
            CancellationToken cancellationToken = default)
        {
            return await TryObtainLockAsync(
                "Writers",
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task ClearLocksAsync(CancellationToken cancellationToken = default)
        {
            await using (dbScope.CreateNew())
            {
                var executor = dbScope.Executor;

                await executor.ExecuteNonQueryAsync(
                    dbScope.BuilderFactory
                        .Update(SystemTable).C("Readers").Equals().V(0).C("Writers").Equals().V(0)
                        .Build(),
                    cancellationToken);
            }
        }

        #endregion

        #region Private Methods

        private async Task<IAsyncDisposable> TryObtainLockAsync(
            string lockType,
            CancellationToken cancellationToken = default)
        {
            await using (dbScope.CreateNew())
            {
                var db = this.dbScope.Db;
                var result =
                    await db
                        .SetCommand(
                            this.dbScope.BuilderFactory
                                .Call("KrPermissionsObtain" + lockType + "Lock", p => p.P("RetryCount", "RetryTimeout"))
                                .Build(),
                            db.Parameter("RetryCount", 50),
                            db.Parameter("RetryTimeout", 100))
                        .SetCommandTimeout(300)
                        .LogCommand()
                        .ExecuteAsync<int>(cancellationToken);

                // 0 - успех
                if (result == 0)
                {
                    return new KrPermissionLockSlim(lockType, this);
                }

                return null;
            }
        }

        private async Task ReleaseLockAsync(
            string lockType,
            CancellationToken cancellationToken = default)
        {
            await using (dbScope.CreateNew())
            {
                var executor = dbScope.Executor;

                await executor.ExecuteNonQueryAsync(
                    dbScope.BuilderFactory
                        .Update(SystemTable)
                        .C(lockType).Assign().C(lockType).Substract(1)
                        .Where().C(lockType).Greater().V(0)
                        .Build(),
                    cancellationToken);
            }
        }

        #endregion
    }
}
