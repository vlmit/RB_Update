using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.Data.SqlClient;
using Tessa.Compilation;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Runtime;
using static Tessa.Extensions.Default.Shared.Workflow.KrProcess.KrConstants.KrStageBuildOutput;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    /// <inheritdoc cref="IKrCompilationResultStorage"/>
    public sealed class KrCompilationResultStorage :
        IKrCompilationResultStorage
    {
        #region Fields

        private readonly IDbScope dbScope;

        private readonly ISession session;

        private readonly IDbmsErrorCodeProvider dbmsErrorCodeProvider;

        #endregion

        #region Constructors

        public KrCompilationResultStorage(
            IDbScope dbScope,
            ISession session,
            IDbmsErrorCodeProvider dbmsErrorCodeProvider)
        {
            this.dbScope = dbScope ?? throw new ArgumentNullException(nameof(dbScope));
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.dbmsErrorCodeProvider = dbmsErrorCodeProvider ?? throw new ArgumentNullException(nameof(dbmsErrorCodeProvider));
        }

        #endregion

        #region IKrCompilationResultStorage Members

        /// <inheritdoc />
        public async Task UpsertAsync(
            Guid cardID,
            IKrCompilationResult compilationResult,
            bool withCompilationResult = false,
            CancellationToken cancellationToken = default)
        {
            var assemblyName = compilationResult.Result.Assembly != null
                ? compilationResult.Result.Assembly.FullName + Environment.NewLine
                : string.Empty;

            byte[] assemblyBytes = null;
            byte[] resultBytes = null;
            if (withCompilationResult)
            {
                if (!string.IsNullOrEmpty(assemblyName))
                {
                    await using (var stream = new MemoryStream(8096))
                    {
                        var formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                        formatter.Serialize(stream, compilationResult);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                        resultBytes = stream.ToArray();
                    }

                    assemblyBytes = compilationResult.Result.AssemblyBytes;
                }
                else
                {
                    // компиляция завершена с ошибками, байты от сборки нельзя поместить в базу
                    withCompilationResult = false;
                }
            }

            // Отдельное соединение, т.к. нужно записать результат, даже если внешняя транзакция будет откатана
            await using (this.dbScope.CreateNew())
            {
                var db = this.dbScope.Db;
                var builder = this.dbScope.BuilderFactory;
                string[] parameterNames;
                DataParameter[] parameters;
                if (withCompilationResult)
                {
                    parameterNames = new[]
                    {
                        KrConstants.ID,
                        BuildDateTime,
                        Output,
                        KrConstants.KrStageBuildOutput.Assembly,
                        KrConstants.KrStageBuildOutput.CompilationResult
                    };
                    parameters = new[]
                    {
                        db.Parameter(KrConstants.ID, cardID),
                        db.Parameter(BuildDateTime, compilationResult.Result.CompilationDateTime.ToUniversalTime()),
                        db.Parameter(Output, assemblyName + compilationResult.Result.RawOutput),
                        db.Parameter(KrConstants.KrStageBuildOutput.Assembly, assemblyBytes),
                        db.Parameter(KrConstants.KrStageBuildOutput.CompilationResult, resultBytes),
                    };
                }
                else
                {
                    parameterNames = new[]
                    {
                        KrConstants.ID,
                        BuildDateTime,
                        Output,
                    };
                    parameters = new[]
                    {
                        db.Parameter(KrConstants.ID, cardID),
                        db.Parameter(BuildDateTime, compilationResult.Result.CompilationDateTime.ToUniversalTime()),
                        db.Parameter(Output, assemblyName + compilationResult.Result.RawOutput),
                    };
                }

                var dbms = await this.dbScope.GetDbmsAsync(cancellationToken);
                switch (dbms)
                {
                    case Dbms.SqlServer:
                        {
                            db
                                .SetCommand(
                                    builder
                                        .InsertInto(Name, parameterNames)
                                        .Select().P(parameterNames)
                                        .Where().NotExists(e => e
                                            .Select().V(null)
                                            .From(Name).NoLock()
                                            .Where().C(KrConstants.ID).Equals().P(KrConstants.ID))
                                        .Build(),
                                    parameters)
                                .LogCommand();

                            var inserted = 0;

                            try
                            {
                                inserted = await db.ExecuteNonQueryAsync(cancellationToken);
                            }
                            catch (SqlException ex)
                            {
                                var dbmsErrorCode = await this.dbmsErrorCodeProvider.GetErrorCodeAsync(ex, cancellationToken);

                                if (dbmsErrorCode == DbmsErrorCode.UniqueViolation)
                                {
                                    // существует race condition между INSERT и WHERE NOT EXISTS, из-за чего при одновременных блокировках
                                    // может возникнуть Violation of PRIMARY KEY constraint, когда выполняются параллельно два INSERT-а с одним и тем же ID;
                                    // поэтому мы просто игнорируем такую ситуацию, говорим, что вставлено 0 строк
                                    inserted = 0;
                                }
                                else
                                {
                                    throw;
                                }
                            }

                            if (inserted == 0)
                            {
                                var updateQuery = builder
                                    .Update(Name)
                                        .C(KrConstants.ID).Assign().P(KrConstants.ID)
                                        .C(BuildDateTime).Assign().P(BuildDateTime)
                                        .C(Output).Assign().P(Output);

                                if (withCompilationResult)
                                {
                                    updateQuery
                                        .C(KrConstants.KrStageBuildOutput.Assembly).Assign().P(KrConstants.KrStageBuildOutput.Assembly)
                                        .C(KrConstants.KrStageBuildOutput.CompilationResult).Assign().P(KrConstants.KrStageBuildOutput.CompilationResult);
                                }

                                updateQuery
                                    .Where()
                                        .C(KrConstants.ID).Equals().P(KrConstants.ID);

                                await db
                                    .SetCommand(updateQuery.Build(), parameters)
                                    .LogCommand()
                                    .ExecuteNonQueryAsync(cancellationToken);
                            }

                            break;
                        }

                    case Dbms.PostgreSql:
                        {
                            var query = builder
                                .InsertInto(Name, parameterNames)
                                .Values(v => v.P(parameterNames)).N()
                                .Q("ON CONFLICT (").C(KrConstants.ID).Q(") DO UPDATE SET ")
                                .C(BuildDateTime).Assign().Q($" EXCLUDED.\"{BuildDateTime}\"").RequireComma()
                                .C(Output).Assign().Q($" EXCLUDED.\"{Output}\"").RequireComma();

                            if (withCompilationResult)
                            {
                                query
                                    .C(KrConstants.KrStageBuildOutput.Assembly).Assign().Q($" EXCLUDED.\"{KrConstants.KrStageBuildOutput.Assembly}\"").RequireComma()
                                    .C(KrConstants.KrStageBuildOutput.CompilationResult).Assign().Q($" EXCLUDED.\"{KrConstants.KrStageBuildOutput.CompilationResult}\"");
                            }

                            await db
                                .SetCommand(query.Build(), parameters)
                                .LogCommand()
                                .ExecuteNonQueryAsync(cancellationToken);
                            break;
                        }

                    default:
                        throw new NotSupportedException("DBMS is not supported: " + dbms);
                }
            }
        }

        /// <inheritdoc />
        public async Task<IKrCompilationResult> GetCompilationResultAsync(
            Guid cardID,
            CancellationToken cancellationToken = default)
        {
            byte[] rawAssembly;
            byte[] rawResult;

            await using (this.dbScope.Create())
            {
                var db = this.dbScope.Db;
                var builder = this.dbScope.BuilderFactory
                    .Select().C(
                        null,
                        KrConstants.KrStageBuildOutput.Assembly,
                        KrConstants.KrStageBuildOutput.CompilationResult)
                    .From(Name)
                    .Where().C(KrConstants.ID).Equals().P("ID");
                db
                    .SetCommand(
                        builder.Build(),
                        db.Parameter("ID", cardID))
                    .LogCommand();
                await using var reader = await db.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                {
                    return null;
                }

                rawAssembly = await reader.GetSequentialNullableBytesAsync(0, cancellationToken);
                rawResult = await reader.GetSequentialNullableBytesAsync(1, cancellationToken);
            }

            if (rawResult is null)
            {
                return null;
            }

            IKrCompilationResult krDeserializedResult;
            try
            {
                var formatter = new BinaryFormatter();
                await using var stream = new MemoryStream(rawResult);
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                krDeserializedResult = (IKrCompilationResult) formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
            }
            catch
            {
                return null;
            }

            Assembly deserializedAssembly = null;
            try
            {
                if (rawAssembly != null)
                {
                    deserializedAssembly = System.Reflection.Assembly.Load(rawAssembly);
                }
            }
            catch
            {
                return null;
            }

            var deserializedResult = krDeserializedResult.Result;
            var compilationResult = new CompilationResult(
                deserializedResult.AssemblyID,
                null,
                deserializedResult.BuildVersion,
                deserializedResult.BuildDate,
                deserializedAssembly,
                deserializedResult.ValidationResult,
                deserializedResult.CompilerOutput,
                deserializedResult.RawOutput,
                deserializedResult.CompilerReturnValue,
                deserializedResult.Info.GetStorage()
            );
            return new KrCompilationResult(
                compilationResult,
                krDeserializedResult.ValidationResult);
        }

        /// <inheritdoc />
        public async Task<KrCompilationOutput> GetCompilationOutputAsync(
            Guid cardID,
            CancellationToken cancellationToken = default)
        {
            var localBuildOutput = string.Empty;
            var globalBuildOutput = string.Empty;
            await using (this.dbScope.Create())
            {
                var db = this.dbScope.Db;
                var builder = this.dbScope.BuilderFactory
                    .Select()
                    .C(null,
                        KrConstants.ID,
                        BuildDateTime,
                        Output)
                    .From(Name)
                    .Where().C(KrConstants.ID).Equals().P("ID")
                    .Or().C(KrConstants.ID).Equals().V(Guid.Empty);

                await using var reader = await db
                    .SetCommand(
                        builder.Build(),
                        db.Parameter("id", cardID))
                    .LogCommand()
                    .ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    var utcDateTime = reader.GetDateTimeUtc(1);
                    var compilerOutput = reader.GetString(2);
                    var text = FormattingHelper.FormatDateTimeWithoutSeconds(utcDateTime + this.session.ClientUtcOffset, false)
                        + Environment.NewLine
                        + compilerOutput;

                    if (reader.GetGuid(0) == Guid.Empty)
                    {
                        globalBuildOutput = text;
                    }
                    else
                    {
                        localBuildOutput = text;
                    }
                }
            }

            return new KrCompilationOutput { Local = localBuildOutput, Global = globalBuildOutput };
        }

        /// <inheritdoc />
        public async Task DeleteCompilationResultAsync(
            Guid cardID,
            CancellationToken cancellationToken = default)
        {
            await using (this.dbScope.CreateNew())
            {
                var query = this.dbScope.BuilderFactory
                    .Update(Name)
                    .C(KrConstants.KrStageBuildOutput.Assembly).Assign().V(null)
                    .C(KrConstants.KrStageBuildOutput.CompilationResult).Assign().V(null)
                    .Where().C(KrConstants.ID).Equals().P("ID")
                    .Build();
                var db = this.dbScope.Db;
                await db
                    .SetCommand(query, db.Parameter("ID", cardID))
                    .LogCommand()
                    .ExecuteNonQueryAsync(cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task DeleteAsync(
            Guid cardID,
            CancellationToken cancellationToken = default)
        {
            await using (this.dbScope.Create())
            {
                var query = this.dbScope.BuilderFactory
                    .DeleteFrom(Name)
                    .Where().C(KrConstants.ID).Equals().P("ID")
                    .Build();
                var db = this.dbScope.Db;
                await db
                    .SetCommand(query, db.Parameter("ID", cardID))
                    .LogCommand()
                    .ExecuteNonQueryAsync(cancellationToken);
            }
        }

        #endregion
    }
}
