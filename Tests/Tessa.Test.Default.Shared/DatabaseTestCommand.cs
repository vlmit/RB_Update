using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using Tessa.Platform;
using Tessa.Platform.Data;

namespace Tessa.Test.Default.Shared
{
    /// <summary>
    /// Команда выполняющая удаление, создание и инициализацию тестовой базы данных.
    /// </summary>
    public sealed class DatabaseTestCommand :
        DelegatingTestCommand
    {
        #region Nested Types

        private struct CommandScope :
            IDisposable
        {
            #region Fields

            private readonly IDbConnection connection;

            private IDbCommand command;

            #endregion

            #region Constructors

            public CommandScope(DbProviderFactory factory, string connectionString)
            {
                if (factory is null)
                {
                    throw new ArgumentNullException(nameof(factory));
                }

                this.connection = factory.CreateConnection()
                    ?? throw new ArgumentException("Can't creation connection with " + factory.GetType().FullName, nameof(factory));

                this.connection.ConnectionString = connectionString;
                this.command = null;
            }

            #endregion

            #region IDisposable Overrides

            public void Dispose()
            {
                this.command?.Dispose();
                this.connection?.Dispose();
            }

            #endregion

            #region Properties

            public IDbCommand Command
            {
                get
                {
                    if (this.command == null)
                    {
                        this.command = this.connection.CreateCommand();
                        this.connection.Open();
                    }

                    return this.command;
                }
            }

            #endregion
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DatabaseTestCommand"/>.
        /// </summary>
        /// <param name="innerCommand">Внутренняя команда.</param>
        public DatabaseTestCommand(TestCommand innerCommand)
            : base(innerCommand)
        {
        }

        #endregion

        #region TestCommand Overrides

        /// <inheritdoc/>
        public override TestResult Execute(TestExecutionContext context)
        {
            Check.ArgumentNotNull(context, nameof(context));

            var test = this.Test;
            var properties = test.Properties;

            var connectionString = (string) properties.Get(DatabasePropertyNames.ConnectionString);
            if (connectionString == null)
            {
                return SkipExecution(context, "Connection string is not specified");
            }

            var providerName = (string) properties.Get(DatabasePropertyNames.ProviderName);
            if (providerName == null)
            {
                return SkipExecution(context, "Provider name is not specified");
            }

            DbProviderFactory factory = ConfigurationManager
                .GetConfigurationDataProviderFromType(providerName)
                .GetDbProviderFactory();

            DbConnectionStringBuilder builder = CreateConnectionStringBuilderChecked(factory);
            builder.ConnectionString = connectionString;

            Dbms dbms = factory.GetDbms();

            string databaseName;
            switch (dbms)
            {
                case Dbms.SqlServer:
                    databaseName = (string) builder["Initial Catalog"];
                    builder["Database"] = "master";
                    break;

                case Dbms.PostgreSql:
                    databaseName = (string) builder["Database"];
                    builder["Database"] = "postgres";
                    break;

                default:
                    throw new NotSupportedException("Unknown database type.");
            }

            var masterConnectionString = builder.ConnectionString;
            var scriptFileNames = properties[DatabasePropertyNames.ScriptFileNamesProvider].Cast<string>();
            try
            {
                using (var scope = new CommandScope(factory, masterConnectionString))
                {
                    DropDatabase(scope.Command, dbms, databaseName);
                    CreateDatabase(scope.Command, dbms, databaseName);
                }

                using (var scope = new CommandScope(factory, connectionString))
                {
                    PrepareDatabase(scope.Command, dbms, scriptFileNames, test.TypeInfo.Assembly);
                }

                context.CurrentResult = this.innerCommand.Execute(context);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
            catch (Exception ex)
            {
                context.CurrentResult.RecordException(ex);
            }
            finally
            {
                using var scope = new CommandScope(factory, masterConnectionString);
                DropDatabase(scope.Command, dbms, databaseName);
            }

            return context.CurrentResult;
        }

        #endregion

        #region Private methods

        private static DbConnectionStringBuilder CreateConnectionStringBuilderChecked(DbProviderFactory factory) =>
            factory.CreateConnectionStringBuilder()
            ?? throw new InvalidOperationException(
                $"Method {nameof(factory.CreateConnectionStringBuilder)} has returned null for {factory.GetType().FullName}");

        private static TestResult SkipExecution(TestExecutionContext context, string reason)
        {
            var result = context.CurrentResult;
            result.SetResult(ResultState.NotRunnable, reason);
            return result;
        }

        private static void CreateDatabase(IDbCommand command, Dbms dbms, string databaseName)
        {
            var builder = StringBuilderHelper.Acquire();
            switch (dbms)
            {
                case Dbms.SqlServer:
                    builder
                        .Append("CREATE DATABASE ")
                        .AppendMsSqlIdentifier(databaseName)
                        .Append(';');
                    break;

                case Dbms.PostgreSql:
                    builder
                        .Append("CREATE DATABASE ")
                        .AppendPgSqlIdentifier(databaseName)
                        .Append(';');
                    break;

                default:
                    throw new NotSupportedException($"Dbms {dbms:G} is not supported.");
            }

            command.CommandText = builder.ToStringAndRelease();
            command.ExecuteNonQuery();
        }

        private static void PrepareDatabase(
            IDbCommand command,
            Dbms dbms,
            IEnumerable<string> scriptFileNames,
            Assembly assembly)
        {
            var sqlTextScripts = TestHelper.GetSqlTextScripts(
                assembly,
                scriptFileNames);

            switch (dbms)
            {
                case Dbms.SqlServer:
                    foreach (var sqlText in sqlTextScripts)
                    {
                        var commands =
                            SqlHelper.SplitGo(sqlText)
                                .Select(x => x.Trim())
                                .Where(x => x.Length > 0);

                        foreach (var scriptCommand in commands)
                        {
                            command.CommandText = scriptCommand;
                            command.ExecuteNonQuery();
                        }
                    }

                    break;

                case Dbms.PostgreSql:
                    foreach (var sqlText in sqlTextScripts)
                    {
                        command.CommandText = sqlText;
                        command.ExecuteNonQuery();
                    }

                    break;
                default:
                    throw new NotSupportedException($"Dbms {dbms:G} is not supported.");
            }
        }

        private static void DropDatabase(IDbCommand command, Dbms dbms, string databaseName)
        {
            var builder = StringBuilderHelper.Acquire();
            switch (dbms)
            {
                case Dbms.SqlServer:
                    builder
                        .Append("IF EXISTS (SELECT * FROM [sys].[databases] WHERE [name] = '")
                        .AppendEscaped(databaseName, '\'', '\'')
                        .Append("')")
                        .AppendLine()
                        .Append("BEGIN")
                        .AppendLine()
                        .Append("\tALTER DATABASE ")
                        .AppendMsSqlIdentifier(databaseName)
                        .Append(" SET OFFLINE WITH ROLLBACK IMMEDIATE;")
                        .AppendLine()
                        .Append("\tALTER DATABASE ")
                        .AppendMsSqlIdentifier(databaseName)
                        .Append(" SET ONLINE;")
                        .AppendLine()
                        .Append("\tDROP DATABASE ")
                        .AppendMsSqlIdentifier(databaseName)
                        .Append(';')
                        .AppendLine()
                        .Append("END;");
                    break;

                case Dbms.PostgreSql:
                    builder
                        .Append("SELECT pg_terminate_backend(pid)")
                        .AppendLine()
                        .Append("FROM pg_stat_activity")
                        .AppendLine()
                        .Append("WHERE pg_stat_activity.datname = '")
                        .AppendEscaped(databaseName, '\'', '\'')
                        .Append("';")
                        .AppendLine()
                        .Append("DROP DATABASE IF EXISTS ")
                        .AppendPgSqlIdentifier(databaseName)
                        .Append(';');
                    break;

                default:
                    throw new NotSupportedException($"Dbms {dbms:G} is not supported.");
            }

            command.CommandText = builder.ToStringAndRelease();
            command.ExecuteNonQuery();
        }

        #endregion
    }
}
