using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using NLog;
using Tessa.Cards;
using Tessa.Platform;
using Tessa.Platform.Data;

namespace Tessa.Extensions.Default.Server.OnlyOffice
{
    /// <inheritdoc />
    public sealed class OnlyOfficeFileCacheInfoStrategy :
        IOnlyOfficeFileCacheInfoStrategy
    {
        #region Constructors

        /// <summary>
        /// Создаёт экземпляр класса с указанием его зависимостей.
        /// </summary>
        /// <param name="dbScope">Объект, посредством которого выполняется взаимодействие с базой данных.</param>
        public OnlyOfficeFileCacheInfoStrategy(IDbScope dbScope) =>
            this.dbScope = dbScope ?? throw new ArgumentNullException(nameof(dbScope));

        #endregion

        #region Fields

        private readonly IDbScope dbScope;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region IOnlyOfficeFileCacheInfoStrategy Members

        /// <inheritdoc />
        public async Task InsertAsync(IOnlyOfficeFileCacheInfo info, CancellationToken cancellationToken = default)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            await using var _ = this.dbScope.Create();

            IQueryExecutor executor = this.dbScope.Executor;

            await executor.ExecuteNonQueryAsync(
                this.dbScope.BuilderFactory
                    .InsertInto("OnlyOfficeFileCache",
                        "ID",
                        "SourceFileVersionID",
                        "CreatedByID",
                        "SourceFileName",
                        "ModifiedFileUrl",
                        "LastModifiedFileUrlTime",
                        "LastAccessTime",
                        "HasChangesAfterClose")
                    .Values(b => b.P(
                        "ID",
                        "SourceFileVersionID",
                        "CreatedByID",
                        "SourceFileName",
                        "ModifiedFileUrl",
                        "LastModifiedFileUrlTime",
                        "LastAccessTime",
                        "HasChangesAfterClose"))
                    .Build(),
                cancellationToken,
                executor.Parameter("ID", info.ID, DataType.Guid),
                executor.Parameter("SourceFileVersionID", info.SourceFileVersionID, DataType.Guid),
                executor.Parameter("CreatedByID", info.CreatedByID, DataType.Guid),
                executor.Parameter("SourceFileName", SqlHelper.LimitString(info.SourceFileName, CardHelper.FileNameMaxLength), DataType.NVarChar),
                executor.Parameter("ModifiedFileUrl", info.ModifiedFileUrl, DataType.NVarChar),
                executor.Parameter("LastModifiedFileUrlTime", info.LastModifiedFileUrlTime, DataType.DateTime),
                executor.Parameter("LastAccessTime", info.LastAccessTime, DataType.DateTime),
                executor.Parameter("HasChangesAfterClose", info.HasChangesAfterClose, DataType.Boolean));
        }


        /// <inheritdoc />
        public async Task<IOnlyOfficeFileCacheInfo?> TryGetInfoAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await using var _ = this.dbScope.Create();

            OnlyOfficeFileCacheInfo result = null;
            var lastAccessTime = DateTime.UtcNow;

            DbManager db = this.dbScope.Db;

            await using (DbDataReader reader = await db
                .SetCommand(
                    this.dbScope.BuilderFactory
                        .Select().C(null,
                            "SourceFileVersionID",
                            "CreatedByID",
                            "SourceFileName",
                            "ModifiedFileUrl",
                            "LastModifiedFileUrlTime",
                            "HasChangesAfterClose",
                            "EditorWasOpen")
                        .From("OnlyOfficeFileCache").NoLock()
                        .Where().C("ID").Equals().P("ID")
                        .Build(),
                    db.Parameter("ID", id, DataType.Guid))
                .LogCommand()
                .ExecuteReaderAsync(cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    result = new OnlyOfficeFileCacheInfo
                    {
                        ID = id,
                        SourceFileVersionID = reader.GetGuid(0),
                        CreatedByID = reader.GetGuid(1),
                        SourceFileName = reader.GetNullableString(2),
                        ModifiedFileUrl = reader.GetNullableString(3),
                        LastModifiedFileUrlTime = reader.GetNullableDateTime(4),
                        HasChangesAfterClose = reader.GetNullableBoolean(5),
                        EditorWasOpen = reader.GetBoolean(6),
                    };
                }
            }

            if (result is null)
            {
                return null;
            }

            await db
                .SetCommand(
                    this.dbScope.BuilderFactory
                        .Update("OnlyOfficeFileCache")
                        .C("LastAccessTime").Assign().P("LastAccessTime")
                        .Where().C("ID").Equals().P("ID")
                        .Build(),
                    db.Parameter("ID", id, DataType.Guid),
                    db.Parameter("LastAccessTime", lastAccessTime, DataType.DateTime))
                .LogCommand()
                .ExecuteNonQueryAsync(cancellationToken);

            result.LastAccessTime = lastAccessTime;
            return result;
        }


        /// <inheritdoc />
        public async Task UpdateInfoAsync(
            Guid id,
            string? newModifiedFileUrl,
            bool? hasChangesAfterClose,
            CancellationToken cancellationToken = default)
        {
            await using var _ = this.dbScope.Create();

            DbManager db = this.dbScope.Db;

            var builder = this.dbScope.BuilderFactory
                .Update("OnlyOfficeFileCache")
                .C("HasChangesAfterClose").Assign().P("HasChangesAfterClose")
                .C("LastAccessTime").Assign().P("LastAccessTime")
                .C("ModifiedFileUrl").Assign().P("ModifiedFileUrl");

            if (newModifiedFileUrl is not null)
            {
                builder = builder
                    .C("LastModifiedFileUrlTime").Assign().P("LastModifiedFileUrlTime");
            }

            builder = builder.Where().C("ID").Equals().P("ID");

            int updated = await db
                .SetCommand(
                    builder.Build(),
                    db.Parameter("ID", id, DataType.Guid),
                    db.Parameter("HasChangesAfterClose", hasChangesAfterClose, DataType.Boolean),
                    db.Parameter("LastAccessTime", DateTime.UtcNow, DataType.DateTime),
                    db.Parameter("ModifiedFileUrl", newModifiedFileUrl, DataType.NVarChar),
                    db.Parameter("LastModifiedFileUrlTime", DateTime.UtcNow, DataType.DateTime))
                .LogCommand()
                .ExecuteNonQueryAsync(cancellationToken);

            if (updated == 0)
            {
                throw new InvalidOperationException($"Can't find file with ID={id:B}");
            }
        }


        /// <inheritdoc />
        public async Task UpdateInfoOnEditorOpenedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await using var _ = this.dbScope.Create();

            DbManager db = this.dbScope.Db;

            int updated = await db
                .SetCommand(
                    this.dbScope.BuilderFactory
                        .Update("OnlyOfficeFileCache")
                        .C("EditorWasOpen").Assign().P("EditorWasOpen")
                        .C("LastAccessTime").Assign().P("LastAccessTime")
                        .Where().C("ID").Equals().P("ID")
                        .Build(),
                    db.Parameter("ID", id, DataType.Guid),
                    db.Parameter("EditorWasOpen", BooleanBoxes.True, DataType.Boolean),
                    db.Parameter("LastAccessTime", DateTime.UtcNow, DataType.DateTime))
                .LogCommand()
                .ExecuteNonQueryAsync(cancellationToken);

            if (updated == 0)
            {
                throw new InvalidOperationException($"Can't find file with ID={id:B}");
            }
        }


        /// <inheritdoc />
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await using var _ = this.dbScope.Create();

            IQueryExecutor executor = this.dbScope.Executor;

            await executor.ExecuteNonQueryAsync(
                this.dbScope.BuilderFactory
                    .DeleteFrom("OnlyOfficeFileCache")
                    .Where().C("ID").Equals().P("ID")
                    .Build(),
                cancellationToken,
                executor.Parameter("ID", id, DataType.Guid));
        }


        /// <inheritdoc />
        public async Task CleanCacheInfoAsync(
            DateTime? oldestPreviewRequestTime = null,
            CancellationToken cancellationToken = default)
        {
            oldestPreviewRequestTime = oldestPreviewRequestTime?.ToUniversalTime();

            if (logger.IsTraceEnabled)
            {
                if (oldestPreviewRequestTime.HasValue)
                {
                    logger.Trace(
                        "Cleaning OnlyOffice cache, oldest allowed date/time: {0} UTC",
                        FormattingHelper.FormatDateTime(oldestPreviewRequestTime.Value, convertToLocal: false));
                }
                else
                {
                    logger.Trace("Cleaning OnlyOffice cache, removing all files info");
                }
            }

            await using var _ = this.dbScope.Create();

            var db = this.dbScope.Db;
            var builderFactory = this.dbScope.BuilderFactory;

            int deleted = oldestPreviewRequestTime.HasValue
                ? await db
                    .SetCommand(
                        builderFactory
                            .DeleteFrom("OnlyOfficeFileCache")
                            .Where().C("LastAccessTime").Less().P("OldestPreviewRequestTime")
                            .Build(),
                        db.Parameter("OldestPreviewRequestTime", oldestPreviewRequestTime.Value, DataType.DateTime))
                    .LogCommand()
                    .ExecuteNonQueryAsync(cancellationToken)
                : await db
                    .SetCommand(
                        builderFactory
                            .DeleteFrom("OnlyOfficeFileCache")
                            .Build())
                    .LogCommand()
                    .ExecuteNonQueryAsync(cancellationToken);

            logger.Trace(
                deleted == 0
                    ? "There are no files info to remove from OnlyOffice cache."
                    : "Files are successfully removed from OnlyOffice cache.");
        }

        #endregion
    }
}