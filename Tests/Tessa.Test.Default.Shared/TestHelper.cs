using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.DataProvider.SqlServer;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using Tessa.BusinessCalendar;
using Tessa.Cards;
using Tessa.Cards.Caching;
using Tessa.Extensions;
using Tessa.Extensions.PostgreSql.Server;
using Tessa.Files;
using Tessa.Localization;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.IO;
using Tessa.Platform.Licensing;
using Tessa.Platform.Operations;
using Tessa.Platform.Runtime;
using Tessa.Platform.SourceProviders;
using Tessa.Platform.Validation;
using Tessa.Roles;
using Tessa.Scheme;
using Tessa.Server;
using Tessa.Test.Default.Shared.Kr;
using Tessa.Test.Default.Shared.Platform.ActionHistory;
using Tessa.Views;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using AssemblyHelper = Tessa.Platform.AssemblyHelper;

namespace Tessa.Test.Default.Shared
{
    /// <summary>
    /// Хэлперы общего назначения для реализации тестов.
    /// </summary>
    public static class TestHelper
    {
        #region Nested Types

        /// <summary>
        /// Предоставляет информацию о тестовом методе.
        /// </summary>
        public readonly struct TestMethodData
        {
            #region Properties

            /// <summary>
            /// Возвращает значение, показывающее, что структура не содержит значений.
            /// </summary>
            public bool IsEmpty => this.TestFixtureFactory is null && this.TestMethod is null;

            /// <summary>
            /// Возвращает метод создающий новый экземпляр класса содержащий выполняемый тест.
            /// </summary>
            public Func<TestBase> TestFixtureFactory { get; }

            /// <summary>
            /// Возвращает информацию о тестовом методе.
            /// </summary>
            public MethodInfo TestMethod { get; }

            /// <summary>
            /// Возвращает параметры передаваемые в тестовый метод.
            /// </summary>
            public object[] TestMethodArgs { get; }

            #endregion

            #region Constructors

            /// <summary>
            /// Инициализирует новый экземпляр структуры <see cref="TestMethodData"/>.
            /// </summary>
            /// <param name="testFixtureFactory">Метод создающий новый экземпляр класса содержащий выполняемый тест.</param>
            /// <param name="testMethod">Информация о тестовом методе.</param>
            /// <param name="testMethodArgs">Параметры передаваемые в тестовый метод.</param>
            public TestMethodData(
                Func<TestBase> testFixtureFactory,
                MethodInfo testMethod,
                params object[] testMethodArgs) : this()
            {
                Check.ArgumentNotNull(testFixtureFactory, nameof(testFixtureFactory));
                Check.ArgumentNotNull(testMethod, nameof(testMethod));
                Check.ArgumentNotNull(testMethodArgs, nameof(testMethodArgs));

                this.TestFixtureFactory = testFixtureFactory;
                this.TestMethod = testMethod;
                this.TestMethodArgs = testMethodArgs;
            }

            #endregion
        }

        #endregion

        #region Constants And Static Fields Private

        private static readonly AsyncSynchronizedOneTimeRegistrator globalTestInitializationRegistrator =
            new AsyncSynchronizedOneTimeRegistrator(() => TessaPlatform.InitializeFromConfigurationAsync());

        private static readonly AsyncSynchronizedOneTimeRegistrator globalTestLocalizationRegistrator =
            new AsyncSynchronizedOneTimeRegistrator(InternalInitializeDefaultLocalizationAsync);

        private static readonly Random random = new Random();

        #endregion

        #region Constants And Static Fields

        /// <summary>
        /// Краткое имя используемое для обозначения объекта предназначенного для использования на базе данных под управлением Sql Server.
        /// </summary>
        public const string ShortSqlServerName = "ms";

        /// <summary>
        /// Краткое имя используемое для обозначения объекта предназначенного для использования на базе данных под управлением PostgreSql.
        /// </summary>
        public const string ShortPostgreSqlName = "pg";

        /// <summary>
        /// Имя строки подключения из конфигурационного файла, используемое по умолчанию.
        /// </summary>
        public const string DefaultConfigurationString = "default";

        /// <summary>
        /// Имя строки подключения из конфигурационного файла, используемое по умолчанию
        /// для тестов на временной базе данных в СУБД MSSQL.
        /// </summary>
        public const string TempConfigurationStringMs = "temp_" + ShortSqlServerName;

        /// <summary>
        /// Имя строки подключения из конфигурационного файла, используемое по умолчанию
        /// для тестов на временной базе данных в СУБД PostgreSQL.
        /// </summary>
        public const string TempConfigurationStringPg = "temp_" + ShortPostgreSqlName;

        /// <summary>
        /// Стандартное значение Dbms
        /// </summary>
        public const Dbms DefaultDbms = Dbms.SqlServer;

        #region Default scripts

        /// <summary>
        /// Имя файла содержащего SQL-скрипт инициализирующий временную базу данных в СУБД MSSQL содержащую только системные объекты схемы.
        /// </summary>
        public const string DbScriptEmptyMs = "Empty_" + ShortSqlServerName + ".sql";

        /// <summary>
        /// Имя файла содержащего SQL-скрипт инициализирующий временную базу данных в СУБД PostgreSQL содержащую только системные объекты схемы.
        /// </summary>
        public const string DbScriptEmptyPg = "Empty_" + ShortPostgreSqlName + ".sql";

        /// <summary>
        /// Имя файла содержащего SQL-скрипт инициализирующий временную базу данных в СУБД MSSQL содержащую данные по умолчанию.
        /// </summary>
        public const string DbScriptDefaultMs = "Default_" + ShortSqlServerName + ".sql";

        /// <summary>
        /// Имя файла содержащего SQL-скрипт инициализирующий временную базу данных в СУБД PostgreSQL содержащую данные по умолчанию.
        /// </summary>
        public const string DbScriptDefaultPg = "Default_" + ShortPostgreSqlName + ".sql";

        #endregion

        #region Admin Session

        /// <summary>
        /// Идентификатор пользователя Admin.
        /// </summary>
        public static readonly Guid AdminUserID =
            new Guid(0x3DB19FA0, 0x228A, 0x497F, 0x87, 0x3A, 0x02, 0x50, 0xBF, 0x0A, 0x4C, 0xCB);

        /// <summary>
        /// Имя пользователя Admin.
        /// </summary>
        public const string AdminUserName = "Admin";

        #endregion

        /// <summary>
        /// Число календарных дней по умолчанию прибавляемых к текущей дате при вычислении правой границы диапазона расчёта календаря.
        /// </summary>
        public const double DefaultCalendarDateEndOffset = 14.0;

        #endregion

        #region Private Methods

        private static async Task DropAndCreateDatabaseAsync(
            ConfigurationConnection connection,
            bool createNew,
            CancellationToken cancellationToken = default)
        {
            string[] databaseName = { null };
            var masterConnection = RewriteConnection(
                connection,
                (databaseName1, dbms1) =>
                {
                    databaseName[0] = databaseName1;

                    return dbms1 switch
                    {
                        Dbms.SqlServer => "master",
                        Dbms.PostgreSql => "postgres",
                        _ => throw new NotSupportedException()
                    };
                },
                out var factory);

            var dbms = factory.GetDbms();

            await using var conn = factory.CreateConnection();
            if (conn == null)
            {
                throw new InvalidOperationException("factory.CreateConnection() is null");
            }

            conn.ConnectionString = masterConnection.ConnectionString;
            await conn.OpenAsync(cancellationToken);

            await using var command = conn.CreateCommand();
            await DropDatabaseAsync(command, dbms, databaseName[0], cancellationToken);
            if (createNew)
            {
                await CreateDatabaseAsync(command, dbms, databaseName[0], cancellationToken);
            }
        }

        private static Task CreateDatabaseAsync(
            DbCommand command,
            Dbms dbms,
            string databaseName,
            CancellationToken cancellationToken = default)
        {
            var builder = StringBuilderHelper.Acquire();

            switch (dbms)
            {
                case Dbms.SqlServer:
                    builder
                        .Append("CREATE DATABASE ")
                        .AppendMsSqlIdentifier(databaseName)
                        .Append("; ALTER DATABASE ")
                        .AppendMsSqlIdentifier(databaseName)
                        .Append(" SET RECOVERY SIMPLE;");

                    break;

                case Dbms.PostgreSql:
                    builder
                        .Append("CREATE DATABASE ")
                        .AppendPgSqlIdentifier(databaseName)
                        .Append(';');
                    break;

                default:
                    throw new NotSupportedException();
            }

            command.CommandText = builder.ToStringAndRelease();
            return command.ExecuteNonQueryAsync(cancellationToken);
        }

        private static Task DropDatabaseAsync(
            DbCommand command,
            Dbms dbms,
            string databaseName,
            CancellationToken cancellationToken = default)
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
                    throw new NotSupportedException();
            }

            command.CommandText = builder.ToStringAndRelease();
            return command.ExecuteNonQueryAsync(cancellationToken);
        }

        private static async Task TestWorkerAsync(
            TestMethodData testData,
            CancellationToken cancellationToken = default)
        {
            using var testExecutionContext = new TestExecutionContext.IsolatedContext();

            var testFixture = testData.TestFixtureFactory();
            var testFixtureType = testFixture.GetType();
            var methodInfo = new MethodWrapper(testFixtureType, testData.TestMethod);
            var test = new TestMethod(methodInfo) { Fixture = testFixture };
            test.FullName += "_" + Guid.NewGuid().ToString();

            TestExecutionContext.CurrentContext.CurrentTest = test;
            var testActions = testFixtureType.GetCustomAttributes(typeof(TestActionAttribute), true).Cast<TestActionAttribute>().ToArray();

            try
            {
                try
                {
                    foreach (var testAction in testActions)
                    {
                        testAction.BeforeTest(test);
                    }

                    await testFixture.SetUpAsync();

                    if (test.Method.MethodInfo.ReturnType == typeof(void))
                    {
                        test.Method.Invoke(testFixture, testData.TestMethodArgs);
                    }
                    else
                    {
                        await (Task) test.Method.Invoke(testFixture, testData.TestMethodArgs);
                    }
                }
                finally
                {
                    await testFixture.TearDownAsync();

                    foreach (var testAction in testActions)
                    {
                        testAction.AfterTest(test);
                    }
                }
            }
            finally
            {
                await testFixture.OneTimeTearDownAsync();
            }
        }

        /// <summary>
        /// Инициализирует локализацию по умолчанию.
        /// </summary>
        /// <returns>Асинхронная задача.</returns>
        /// <remarks>Язык локализации по умолчанию: английский.</remarks>
        private static ValueTask InternalInitializeDefaultLocalizationAsync()
        {
            LocalizationManager.SetEnglishLocalization();

            var testExecutionContext = TestExecutionContext.CurrentContext;

            if (testExecutionContext is TestExecutionContext.AdhocContext)
            {
                throw new InvalidOperationException("The current test execution context is not specified.");
            }

            var fixture = testExecutionContext.CurrentTest?.Fixture;

            if (fixture is null)
            {
                throw new InvalidOperationException("The fixture object is not specified.");
            }

            var assembly = fixture is ResourceAssemblyManager resourceAssemblyManager
                ? resourceAssemblyManager.ResourceAssembly
                : fixture.GetType().Assembly;

            var path = Path.Combine(ResourcesPaths.Resources, ResourcesPaths.Localization);
            var resourceNames = AssemblyHelper.GetFileNameEnumerableFromEmbeddedResources(
                    assembly,
                    path)
                .Select(i => i.FullName)
                .ToArray();

            if (resourceNames.Length == 0)
            {
                TestContext.Progress.WriteLine($"Assembly \"{assembly.FullName}\" (location: {assembly.Location}) not contains localizations located in the path \"{path}\".");
            }

            // Тесты из разных сборок выполняются в разных процессах.

            return LocalizationManager.InitializeDefaultLocalizationAsync(
                detectLanguage: false,
                localizationServices: new ILocalizationService[]
                {
                    new JsonResourceFileLocalizationService(
                        assembly,
                        resourceNames)
                });
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Доступный только для чтения словарь содержащий информацию о тестовых часовых поясах. Ключ - идентификатор часового пояса; значение - смещение в минутах от UTC.
        /// </summary>
        /// <seealso cref="InsertTestTimeZonesAsync"/>
        public static ReadOnlyDictionary<short, int> TestTimeZones { get; } =
            new ReadOnlyDictionary<short, int>(new Dictionary<short, int>
            {
                { TimeZonesHelper.DefaultZoneID, TimeZonesHelper.DefaultUtcOffsetMinutes },
                { 1, 240 },
                { 2, 300 },
                { 3, 600 },
                { 4, -240 },
                { 5, -300 },
                { 6, -600 },
            });

        #endregion

        #region Methods

        /// <summary>
        /// Выполняет инициализацию платформы для выполнения тестов. Действие выполняется ровно один раз и игнорируется при повторных вызовах.
        /// </summary>
        /// <returns>Асинхронная задача.</returns>
        public static ValueTask InitializeTestPlatformAsync() => globalTestInitializationRegistrator.RegisterAsync();

        /// <summary>
        /// Возвращает неотрицательное, псевдослучайное число.
        /// </summary>
        /// <returns>32-разрядное целое число со знаком, которое больше или равно нулю и меньше чем <see cref="int.MaxValue"/>.</returns>
        public static int GetPseudoRandomNumber()
        {
            // Убрать, если random.Next() является потокобезопасным
            lock (random)
            {
                return random.Next();
            }
        }

        /// <summary>
        /// Проверяет, что указанный асинхронный метод выбрасывает исключение типа <typeparamref name="TException"/>,
        /// в противном случае вызывает <c>Assert.Fail</c>. Возвращает выброшенное исключение или <c>null</c>, если оно не было выброшено.
        /// </summary>
        /// <param name="funcAsync">Асинхронная функция, для которой проверяется исключение.</param>
        /// <param name="messageToCheck">Текст исключения, который должен совпасть, или <c>null</c>, если текст исключения не проверяется.</param>
        /// <typeparam name="TException">Тип ожидаемого исключения.</typeparam>
        /// <returns>Асинхронная задача.</returns>
        public static async ValueTask<TException> AssertThatThrowsAsync<TException>(
            Func<ValueTask> funcAsync,
            string messageToCheck = null)
            where TException : Exception
        {
            Check.ArgumentNotNull(funcAsync, nameof(funcAsync));

            try
            {
                await funcAsync();
                Assert.Fail($"Function doesn't throw exceptions, but expected exception of type \"{typeof(TException).AssemblyQualifiedName}\"");

                return null;
            }
            catch (TException ex)
            {
                if (messageToCheck != null && messageToCheck != ex.Message)
                {
                    Assert.Fail($"Function has thrown \"{typeof(TException).AssemblyQualifiedName}\", expected message \"{messageToCheck}\", but actual message was \"{ex.Message}\"");
                }

                return ex;
            }
        }

        /// <summary>
        /// Проверяет, что указанный асинхронный метод не выбрасывает исключение.
        /// в противном случае вызывает <c>Assert.Fail</c>.
        /// </summary>
        /// <param name="funcAsync">Асинхронная функция, для которой проверяется исключение.</param>
        /// <returns>Асинхронная задача.</returns>
        public static async ValueTask AssertThatDoesNotThrowAsync(
            Func<ValueTask> funcAsync)
        {
            Check.ArgumentNotNull(funcAsync, nameof(funcAsync));

            try
            {
                await funcAsync();
            }
            catch (Exception ex)
            {
                Assert.Fail($"Function has thrown an exception of type \"{ex.GetType()}\"{Environment.NewLine}{ex.GetFullText()}");
                throw;
            }
        }

        /// <summary>
        /// Быстро сравнивает два больших неограниченных по размеру массива байт путём сравнения вычисленных хешей.
        /// Используется метод <see cref="Assert"/>. Оба массива должны быть непустыми.
        /// </summary>
        /// <param name="actual">Первый сравниваемый массив байт.</param>
        /// <param name="expected">Второй сравниваемый массив байт.</param>
        public static void AssertThatBytesAreEqual(byte[] actual, byte[] expected)
        {
            Assert.That(actual, Is.Not.Null);
            Assert.That(expected, Is.Not.Null);
            Assert.That(actual.Length, Is.EqualTo(expected.Length));

            byte[] actualHash, expectedHash;

            using (var sha512 = new SHA512Managed())
            {
                actualHash = sha512.ComputeHash(actual);
                expectedHash = sha512.ComputeHash(expected);
            }

            Assert.That(actualHash, Is.EquivalentTo(expectedHash));
        }

        /// <summary>
        /// Проверяет, что заданный тип <paramref name="type"/> реализует интерфейс <typeparamref name="T"/>.
        /// В противном случае вызывает исключение с указанием <paramref name="typeParameterName"/> в качестве имени параметра.
        /// </summary>
        /// <typeparam name="T">Тип интерфейса, реализацию которого типом <paramref name="type"/> требуется проверить.</typeparam>
        /// <param name="type">Тип, для которого требуется проверить, что он реализует интерфейс <typeparamref name="T"/>.</param>
        /// <param name="typeParameterName">
        /// Имя параметра <paramref name="type"/>, указываемое в сообщении исключений, выбрасываемых в случае возникновения
        /// ошибок проверки <paramref name="type"/> на реализацию интерфейса <typeparamref name="T"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Параметры <paramref name="type"/> или <paramref name="typeParameterName"/> равны <c>null</c>.
        /// Если <paramref name="type"/> равен <c>null</c>, то в качестве имени параметра в сообщении используется значение
        /// параметра <paramref name="typeParameterName"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Тип <paramref name="type"/> не реализует интерфейс <typeparamref name="T"/>. В качестве имени параметра
        /// <paramref name="type"/> в сообщении используется значение параметра <paramref name="typeParameterName"/>.
        /// </exception>
        public static void CheckTypeOf<T>(Type type, string typeParameterName)
            where T : class
        {
            Check.ArgumentNotNull(type, nameof(type));
            Check.ArgumentNotNull(typeParameterName, nameof(typeParameterName));

            var checkingType = typeof(T);
            if (!checkingType.IsAssignableFrom(type))
            {
                throw new ArgumentException($"Instances of type '{typeParameterName}' should implement {checkingType.Name}.");
            }
        }

        /// <summary>
        /// Инициализирует значение параметра <paramref name="value"/> экземпляром класса <paramref name="type"/>,
        /// если оно ещё не было инициализировано, и возвращает новое значение параметра <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">
        /// Тип параметра <paramref name="value"/>, конструктор по умолчанию которого используется
        /// для инициализации значения параметра <paramref name="value"/>.
        /// </typeparam>
        /// <param name="type">Тип класса, экземпляр которого инициализирует параметр <paramref name="value"/>.</param>
        /// <param name="value">Инициализируемое значение.</param>
        /// <returns>Новое значение параметра <paramref name="value"/>.</returns>
        public static T InitValue<T>(Type type, ref T value)
            where T : class =>
            value ??= (T) Activator.CreateInstance(type);

        /// <summary>
        /// Инициализирует контейнер Unity для выполнения тестов карточек на стороне сервера с расширениями.
        /// Рекомендуется использовать <see cref="InitializeServerContainer(IUnityContainer, Func{DbManager}, IDbScope, Func{ISessionToken}, ICardFileSourceSettings, Action{IUnityContainer}, Action{IUnityContainer}, string)"/>, поскольку он также инициализирует серверные зависимости.
        /// </summary>
        /// <param name="container">Unity-контейнер.</param>
        /// <param name="createDbManagerFunc">Функция, создающая и возвращающая <see cref="DbManager"/>. Если задано значение по умолчанию для типа, то перерегистрация не выполняется.</param>
        /// <param name="dbScope">Экземпляр объекта осуществляющий взаимодействие с базой данных. Если задано значение по умолчанию для типа, то перерегистрация не выполняется.</param>
        /// <param name="tryGetTokenFunc">
        /// Функция, возвращающая токен, по которому определяются поля сессии,
        /// или <c>null</c>, если сессия определяется только внутри области, созданной в <see cref="SessionContext"/>,
        /// т.е. токен сессии недоступен в текущий момент.
        /// </param>
        /// <param name="fileSourceSettings">Настройки файлов, используемые по умолчанию. Если задано значение по умолчанию для типа, то перерегистрация не выполняется. Для инициализации объекта <see cref="ICardFileSourceSettings"/> в тестах рекомендуется использовать метод <see cref="TestBase.CreateDefaultFileSourceSettingsAsync(bool, bool, CancellationToken)"/>.</param>
        /// <param name="beforeRegisterExtensionsOnServerAction">Метод выполняющий действия перед поиском и выполнением серверных регистраторов расширений в папке приложения.</param>
        /// <param name="beforeFinalizeServerRegistrationAction">Метод выполняющий действия перед завершением регистрации сервера приложений.</param>
        /// <returns>Созданный контейнер.</returns>
        public static IUnityContainer InitializeServerContainerBase(
            IUnityContainer container,
            Func<DbManager> createDbManagerFunc = default,
            IDbScope dbScope = default,
            Func<ISessionToken> tryGetTokenFunc = default,
            ICardFileSourceSettings fileSourceSettings = default,
            Action<IUnityContainer> beforeRegisterExtensionsOnServerAction = default,
            Action<IUnityContainer> beforeFinalizeServerRegistrationAction = default)
        {
            // Значение параметра container будет проверено в RegisterServer.

            container
                .RegisterServer(
                    RuntimeHelper.DefaultInstanceName,
                    registerLicensingOnServer: false,
                    tryGetTokenFunc: tryGetTokenFunc ?? (static () => Session.CreateSystemToken(SessionType.Server)),
                    enableInterprocessCommunication: false)
                .RegisterType<IActionHistoryStrategy, FakeActionHistoryStrategy>(new ContainerControlledLifetimeManager());

            if (createDbManagerFunc is not null)
            {
                container
                    .RegisterDbManager(createDbManagerFunc);
            }

            container
                .RegisterApplicationServerSettingsFromConfig(flags: TessaServerConfigFlags.TestDefaults);

            var tessaServerSettings = container.Resolve<ITessaServerSettings>();
            var licenseFileExists = System.IO.File.Exists(tessaServerSettings.LicensePath);

            if (licenseFileExists)
            {
                container
                    .RegisterLicensingOnServer();
            }
            else
            {
                container
                    .RegisterLicensingOnServer((_, _) => new ValueTask<byte[]>((byte[]) null));
            }

            if (fileSourceSettings is not null)
            {
                container
                    .RegisterInstance(fileSourceSettings);
            }

            if (dbScope is not null)
            {
                container
                    .RegisterInstance(dbScope, new ContainerControlledLifetimeManager());
            }

            beforeRegisterExtensionsOnServerAction?.Invoke(container);

            container
                .FindAndRegisterExtensionsOnServer(out var actualFoldersList, tags: RegistratorTag.ServerDefault);

            beforeFinalizeServerRegistrationAction?.Invoke(container);

            container
                    .FinalizeServerRegistration(actualFoldersList);

            if (!licenseFileExists)
            {
                container
                    .RegisterType<ILicenseManager, TransientLicenseManager>(new ContainerControlledLifetimeManager());
            }

            return container;
        }

        /// <summary>
        /// Создаёт и конфигурирует контейнер Unity для выполнения тестов карточек на стороне сервера с расширениями.
        /// Дополнительно инициализирует серверные зависимости.
        /// </summary>
        /// <param name="container">Unity-контейнер.</param>
        /// <param name="createDbManagerFunc">Функция, создающая и возвращающая <see cref="DbManager"/>. Если задано значение по умолчанию для типа, то перерегистрация не выполняется.</param>
        /// <param name="dbScope">Экземпляр объекта осуществляющий взаимодействие с базой данных. Если задано значение по умолчанию для типа, то перерегистрация не выполняется.</param>
        /// <param name="tryGetTokenFunc">
        /// Функция, возвращающая токен, по которому определяются поля сессии,
        /// или <c>null</c>, если сессия определяется только внутри области, созданной в <see cref="SessionContext"/>,
        /// т.е. токен сессии недоступен в текущий момент.
        /// </param>
        /// <param name="fileSourceSettings">Настройки файлов, используемые по умолчанию.</param>
        /// <param name="beforeRegisterExtensionsOnServerAction">Метод выполняющий действия перед поиском и выполнением серверных регистраторов расширений в папке приложения.</param>
        /// <param name="beforeFinalizeServerRegistrationAction">Метод выполняющий действия перед завершением регистрации сервера приложений.</param>
        public static IUnityContainer InitializeServerContainer(
            IUnityContainer container,
            Func<DbManager> createDbManagerFunc = default,
            IDbScope dbScope = default,
            Func<ISessionToken> tryGetTokenFunc = default,
            ICardFileSourceSettings fileSourceSettings = default,
            Action<IUnityContainer> beforeRegisterExtensionsOnServerAction = default,
            Action<IUnityContainer> beforeFinalizeServerRegistrationAction = default)
        {
            if (TessaPlatform.ServerDependencies is not TessaServerDependencies)
            {
                TessaPlatform.ServerDependencies = new TessaServerDependencies();
            }

            return InitializeServerContainerBase(
                container,
                createDbManagerFunc,
                dbScope,
                tryGetTokenFunc,
                fileSourceSettings,
                beforeRegisterExtensionsOnServerAction,
                beforeFinalizeServerRegistrationAction);
        }

        /// <summary>
        /// Выполняет регистрацию зависимостей для функционирования подсистемы представлений на серверной стороне.
        /// </summary>
        /// <param name="container">Контейнер в котором выполняется регистрация.</param>
        /// <param name="connectionFactory">Функция позволяющая создать объект подключения к базе данных.</param>
        public static void RegisterViewsInContainerForTests(IUnityContainer container, Func<DbConnection> connectionFactory)
        {
            Check.ArgumentNotNull(container, nameof(container));
            Check.ArgumentNotNull(connectionFactory, nameof(connectionFactory));

            container
                .RegisterFactory<ISession>(
                    c => Session.CreateSystemSession(SessionType.Server),
                    new ContainerControlledLifetimeManager())
                .RegisterFactory<IUser>(c => c.Resolve<ISession>().User, new PerResolveLifetimeManager())
                .RegisterFactory<ISchemeService>(
                    c => new DatabaseSchemeService(
                        c.Resolve<IDbScope>(),
                        c.Resolve<IConfigurationVersionProvider>(),
                        c.Resolve<ISession>(),
                        SchemeServiceOptions.ReadOnly),
                    new ContainerControlledLifetimeManager())
                .RegisterType<IDbmsErrorCodeProvider, UnityErrorCodeProvider>()
                .RegisterType<IDbmsErrorCodeProvider, SqlServerErrorCodeProvider>(Dbms.SqlServer.ToString())
                .RegisterType<IDbmsErrorCodeProvider, PostgreSqlErrorCodeProvider>(Dbms.PostgreSql.ToString())
                .RegisterType<IValidationResultBuilder, ValidationResultBuilder>(new PerResolveLifetimeManager(), new InjectionConstructor())
                .RegisterDbManager(() =>
                {
                    var connection = connectionFactory();

                    var provider = connection.GetDbms() switch
                    {
                        Dbms.SqlServer => SqlServerTools.GetDataProvider(provider: SqlServerProvider.MicrosoftDataSqlClient),
                        Dbms.PostgreSql => PostgreSQLTools.GetDataProvider(PostgreSQLVersion.v95),
                        _ => default
                    };

                    return new DbManager(provider, connection);
                })
                .RegisterDbScope()
                .RegisterDataDependencies()
                .RegisterServerSettings()
                .RegisterViewsOnServer()
                .RegisterType<IDbmsQueryResultMetadataProvider, MsSqlQueryResultMetadataProvider>(
                    Dbms.SqlServer.ToString(), new PerResolveLifetimeManager())
                .RegisterType<IDbmsQueryResultMetadataProvider, PostgresQueryResultMetadataProvider>(
                    Dbms.PostgreSql.ToString(), new PerResolveLifetimeManager())
                .RegisterType<QueryResultMetadataProvider>(new ContainerControlledLifetimeManager())
                .RegisterFactory<IViewQueryExecutor>(
                    c => new ViewQueryExecutor(
                        c.Resolve<IDbScope>(),
                        c.Resolve<IDbmsErrorCodeProvider>(),
                        c.Resolve<Func<ITessaViewResult>>(),
                        c.Resolve<QueryResultMetadataProvider>(),
                        c.Resolve<ISession>(),
                        c.Resolve<IErrorManager>(),
                        c.Resolve<ICardCache>(),
                        commandTimeoutLazy: new AsyncLazy<int>(() => 0)),
                    new ContainerControlledLifetimeManager())
                ;
        }

        /// <summary>
        /// Удаляет базу данных, если она есть, и создаёт новую.
        /// </summary>
        /// <param name="connection">Имя в конфигурационном файле для строки подключения к базе данных.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        public static Task CreateDatabaseAsync(ConfigurationConnection connection, CancellationToken cancellationToken = default) =>
            DropAndCreateDatabaseAsync(connection, true, cancellationToken);

        /// <summary>
        /// Удаляет базу данных, если она есть.
        /// </summary>
        /// <param name="connection">Имя в конфигурационном файле для строки подключения к базе данных.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        public static Task DropDatabaseAsync(ConfigurationConnection connection, CancellationToken cancellationToken = default) =>
            DropAndCreateDatabaseAsync(connection, false, cancellationToken);

        /// <summary>
        /// Создаёт объект, описывающий поставщик данных для строки подключения, на основе указанного.
        /// </summary>
        /// <param name="connection">Объект, описывающий поставщик данных для строки подключения на основании которого выполняется создание.</param>
        /// <param name="getDatabaseNameFunc">Функция возвращающая имя базы данных по указанному предыдущему имени и типу.</param>
        /// <param name="factory">Фабрика объектов <see cref="DbProviderFactory"/>.</param>
        /// <returns>Созданный объект <see cref="ConfigurationConnection"/>.</returns>
        public static ConfigurationConnection RewriteConnection(
            ConfigurationConnection connection,
            Func<string, Dbms, string> getDatabaseNameFunc,
            out DbProviderFactory factory)
        {
            Check.ArgumentNotNull(connection, nameof(connection));
            Check.ArgumentNotNull(getDatabaseNameFunc, nameof(getDatabaseNameFunc));

            var connectionString = connection.ConnectionString;

            factory = ConfigurationManager
                .GetConfigurationDataProviderFromType(connection.DataProvider)
                .GetDbProviderFactory();

            var builder = factory.CreateConnectionStringBuilder();
            if (builder is null)
            {
                throw new InvalidOperationException(
                    $"Factory {factory.GetType().FullName} has returned null as connection string builder.");
            }

            builder.ConnectionString = connectionString;

            var dbms = factory.GetDbms();
            var prevDatabaseName = dbms switch
            {
                Dbms.SqlServer => (string) builder["Initial Catalog"],
                Dbms.PostgreSql => (string) builder["Database"],
                _ => throw new NotSupportedException($"Dbms {dbms:G} is not supported.")
            };

            var databaseName = getDatabaseNameFunc(prevDatabaseName, dbms);
            builder["Database"] = databaseName;

            return new ConfigurationConnection(builder.ConnectionString, connection.DataProvider);
        }

        /// <summary>
        /// Выполняет указанную коллекцию SQL-скриптов расположенных в встроенных ресурсах сборки по пути <see cref="ResourcesPaths.Resources"/>\<see cref="ResourcesPaths.Sql"/>.
        /// </summary>
        /// <param name="dbScope">Объект для взаимодействия с базой данных.</param>
        /// <param name="assembly">Сборка содержащая искомые встроенные ресурсы.</param>
        /// <param name="scriptFileNames">Перечисление имён файлов SQL-скриптов.</param>
        /// <param name="timeoutSeconds">Таймаут в секундах или значение <see langword="null"/>, если используется таймаут по умолчанию. Укажите <c>0</c>, чтобы таймаут был неограниченным.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        public static Task ExecuteSqlScriptsFromEmbeddedResourcesAsync(
            IDbScope dbScope,
            Assembly assembly,
            IEnumerable<string> scriptFileNames,
            int? timeoutSeconds = null,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(assembly, nameof(assembly));
            Check.ArgumentNotNull(scriptFileNames, nameof(scriptFileNames));

            var sqlTextScripts = GetSqlTextScripts(assembly, scriptFileNames);
            return ExecuteSqlScriptsAsync(dbScope, sqlTextScripts, timeoutSeconds, cancellationToken);
        }

        /// <summary>
        /// Возвращает массив содержащий содержимое текстовых файлов размещённых в встроенных ресурсах сборки по пути <see cref="ResourcesPaths.Resources"/>\<see cref="ResourcesPaths.Sql"/>.
        /// </summary>
        /// <param name="assembly">Сборка содержащая искомые встроенные ресурсы.</param>
        /// <param name="scriptFileNames">Перечисление имён файлов SQL-скриптов.</param>
        /// <returns>Массив содержащий содержимое текстовых файлов размещённых в встроенных ресурсах расположенных по пути <see cref="ResourcesPaths.Resources"/>\<see cref="ResourcesPaths.Sql"/>.</returns>
        public static string[] GetSqlTextScripts(
            Assembly assembly,
            IEnumerable<string> scriptFileNames)
        {
            Check.ArgumentNotNull(assembly, nameof(assembly));
            Check.ArgumentNotNull(scriptFileNames, nameof(scriptFileNames));

            return scriptFileNames
                .Select(resourceName =>
                    AssemblyHelper.GetResourceTextFile(
                        assembly,
                        Path.Combine(
                            ResourcesPaths.Resources,
                            ResourcesPaths.Sql,
                            resourceName)))
                .ToArray();
        }

        /// <summary>
        /// Выполняет указанную коллекцию SQL-скриптов.
        /// </summary>
        /// <param name="dbScope">Объект для взаимодействия с базой данных.</param>
        /// <param name="sqlTextScripts">Перечисление содержащее выполняемые SQL-скрипты.</param>
        /// <param name="timeoutSeconds">Таймаут в секундах или значение <see langword="null"/>, если используется таймаут по умолчанию. Укажите <c>0</c>, чтобы таймаут был неограниченным.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        public static async Task ExecuteSqlScriptsAsync(
            IDbScope dbScope,
            IEnumerable<string> sqlTextScripts,
            int? timeoutSeconds = null,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(dbScope, nameof(dbScope));
            Check.ArgumentNotNull(sqlTextScripts, nameof(sqlTextScripts));

            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                var dbms = db.GetDbms();
                switch (dbms)
                {
                    case Dbms.SqlServer:
                        // создание отдельного соединения с базой нужно для того, чтобы избежать багов с новой версией Linq2DB
                        foreach (var sqlText in sqlTextScripts)
                        {
                            var commands =
                                SqlHelper.SplitGo(sqlText)
                                    .Select(x => x.Trim())
                                    .Where(x => x.Length > 0);

                            foreach (var commandText in commands)
                            {
                                db.SetCommand(commandText);

                                if (timeoutSeconds.HasValue)
                                {
                                    db.SetCommandTimeout(timeoutSeconds.Value);
                                }

                                await db.ExecuteNonQueryAsync(cancellationToken);
                            }
                        }

                        break;
                    case Dbms.PostgreSql:
                        foreach (var sqlText in sqlTextScripts)
                        {
                            db.SetCommand(sqlText);

                            if (timeoutSeconds.HasValue)
                            {
                                db.SetCommandTimeout(timeoutSeconds.Value);
                            }

                            await db.ExecuteNonQueryAsync(cancellationToken);
                        }

                        break;
                    default:
                        throw new NotSupportedException($"Dbms {dbms:G} is not supported.");
                }
            }
        }

        /// <summary>
        /// Проверяет содержится ли в указанной таблице хотя бы одно значение.
        /// </summary>
        /// <param name="dbScope">Объект для взаимодействия с базой данных.</param>
        /// <param name="tableName">Имя проверяемой таблицы.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Значение <see langword="true"/>, если <paramref name="tableName"/> содержит хотя бы одно значение, иначе - <see langword="false"/>.</returns>
        public static async Task<bool> ExistsValuesAsync(
            IDbScope dbScope,
            string tableName,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(dbScope, nameof(dbScope));
            Check.ArgumentNotNullOrEmpty(tableName, nameof(tableName));

            await using (dbScope.Create())
            {
                var db = dbScope.Db;

                var query = dbScope.BuilderFactory
                    .SelectExists(b => b
                        .Select()
                            .V(null)
                        .From(tableName).NoLock())
                    .Build();

                return await db
                    .SetCommand(query)
                    .LogCommand()
                    .ExecuteAsync<bool>(cancellationToken);
            }
        }

        /// <summary>
        /// Выполняет построение бизнес-календаря в диапазоне (Дата понедельника предыдущей недели относительно <paramref name="startDate"/>; <paramref name="startDate"/> + <paramref name="dateEndOffset"/> к.д.).
        /// </summary>
        /// <param name="dbScope">Объект, определяющий доступ к базе данных.</param>
        /// <param name="startDate">Начальная дата, от которой отсчитывается диапазон построения календаря. Если не задана, то используется текущая дата.</param>
        /// <param name="dateEndOffset">Число календарных дней прибавляемых к начальной дате при вычислении правой границы диапазона расчёта календаря.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        public static Task BuildCalendarAsync(
            IDbScope dbScope,
            DateTime? startDate = default,
            double dateEndOffset = DefaultCalendarDateEndOffset,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(dbScope, nameof(dbScope));

            if (dateEndOffset <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dateEndOffset), dateEndOffset, "The value must be greater than zero.");
            }

            var operationRepository = new OperationServerRepository(dbScope, Session.CreateSystemSession(SessionType.Server));
            var businessCalendar = new BusinessCalendarService(operationRepository, dbScope);

            // считаем относительно понедельника предыдущей недели, чтобы тест корректно работал в любой день недели, в т.ч. на несколько дней в прошлом
            var currentDate = DateTime.UtcNow;
            var date = (startDate ?? currentDate).Date.AddDays(-7.0).StartOfWeek(DayOfWeek.Monday);

            return businessCalendar.RebuildCalendarInternalAsync(
                date,
                currentDate.Date.AddDays(dateEndOffset),
                date.AddHours(6.0),
                date.AddHours(15.0),
                date.AddHours(10.0),
                date.AddHours(11.0),
                cancellationToken);
        }

        /// <summary>
        /// Добавляет в БД информацию о часовом поясе по умолчанию (<see cref="TimeZonesHelper.DefaultTimeZoneSection"/>).
        /// </summary>
        /// <param name="dbScope">Объект для взаимодействия с базой данных.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        public static async Task InsertDefaultTimeZoneAsync(
            IDbScope dbScope,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(dbScope, nameof(dbScope));

            await using (dbScope.Create())
            {
                var db = dbScope.Db;

                var idParam = Guid.NewGuid();
                var modifiedParam = DateTime.UtcNow;

                var insertIntoInstancesCommand =
                    dbScope.BuilderFactory
                        .InsertInto("Instances",
                            "ID", "TypeID", "TypeCaption", "Version", "Readers", "WritePending", "Created",
                            "CreatedByID", "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName")
                        .Values(p => p.P("ID", "TypeID", "TypeCaption").V(1).V(0).V(false).P("Modified", "ModifiedByID",
                            "ModifiedByName", "Modified", "ModifiedByID", "ModifiedByName"))
                        .Build();

                await db.SetCommand(insertIntoInstancesCommand,
                        db.Parameter("ID", idParam),
                        db.Parameter("TypeID", CardHelper.TimeZonesTypeID),
                        db.Parameter("TypeCaption", CardHelper.TimeZonesTypeName),
                        db.Parameter("Modified", modifiedParam),
                        db.Parameter("ModifiedByID", Session.SystemID),
                        db.Parameter("ModifiedByName", Session.SystemName))
                    .LogCommand()
                    .ExecuteNonQueryAsync(cancellationToken);

                var insertIntoDefaultTimeZonesCommand =
                    dbScope.BuilderFactory
                        .InsertInto(TimeZonesHelper.DefaultTimeZoneSection,
                            "ID", "CodeName", "UtcOffsetMinutes", "DisplayName", "ShortName",
                            "IsNegativeOffsetDirection", "OffsetTime", "ZoneID")
                        .Values(p => p.P("ID", "CodeName", "UtcOffsetMinutes", "DisplayName", "ShortName",
                            "IsNegativeOffsetDirection", "OffsetTime", "ZoneID"))
                        .Build();

                await db.SetCommand(insertIntoDefaultTimeZonesCommand,
                        db.Parameter("ID", idParam),
                        db.Parameter("CodeName", TimeZonesHelper.DefaultCodeName),
                        db.Parameter("UtcOffsetMinutes", TimeZonesHelper.DefaultUtcOffsetMinutes),
                        db.Parameter("DisplayName", TimeZonesHelper.DefaultDisplayName),
                        db.Parameter("ShortName", TimeZonesHelper.DefaultShortName),
                        db.Parameter("IsNegativeOffsetDirection", TimeZonesHelper.DefaultIsNegativeOffsetDirection),
                        db.Parameter("OffsetTime", TimeZonesHelper.DefaultOffsetTime),
                        db.Parameter("ZoneID", TimeZonesHelper.DefaultZoneID))
                    .LogCommand()
                    .ExecuteNonQueryAsync(cancellationToken);

                var insertIntoTimeZonesSettingsCommand =
                    dbScope.BuilderFactory
                        .InsertInto(TimeZonesHelper.TimeZonesSettingsSection,
                            "ID", "AllowToModify")
                        .Values(p => p.P("ID", "AllowToModify"))
                        .Build();

                await db.SetCommand(insertIntoTimeZonesSettingsCommand,
                        db.Parameter("ID", idParam),
                        db.Parameter("AllowToModify", true))
                    .LogCommand()
                    .ExecuteNonQueryAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Добавляет в БД информацию о часовых поясах содержащихся в <see cref="TestTimeZones"/>.
        /// </summary>
        /// <param name="dbScope">Объект для взаимодействия с базой данных.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        public static async Task InsertTestTimeZonesAsync(IDbScope dbScope, CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(dbScope, nameof(dbScope));

            await using (dbScope.Create())
            {
                var db = dbScope.Db;

                var insertIntoTimeZonesCommand =
                    dbScope.BuilderFactory
                        .InsertInto(TimeZonesHelper.TimeZonesEnumSection,
                            "ID", "CodeName", "UtcOffsetMinutes", "DisplayName", "ShortName", "IsNegativeOffsetDirection", "OffsetTime")
                        .Values(p => p.P(
                            "ID", "CodeName", "UtcOffsetMinutes", "DisplayName", "ShortName", "IsNegativeOffsetDirection", "OffsetTime"))
                        .Build();

                for (short i = 1; i < TestTimeZones.Count; i++)
                {
                    var offsetMinutes = TestTimeZones[i];
                    var offsetHours = offsetMinutes / 60;
                    var offsetHoursStr = offsetHours.ToString();
                    var isNegativeOffsetDirection = offsetMinutes < 0;
                    var shortName = "UTC";

                    if (!isNegativeOffsetDirection)
                    {
                        shortName += "+";
                    }

                    shortName += offsetHoursStr;
                    await db.SetCommand(insertIntoTimeZonesCommand,
                        db.Parameter("ID", i),
                        db.Parameter("CodeName", $"Test {offsetMinutes} m"),
                        db.Parameter("UtcOffsetMinutes", offsetMinutes),
                        db.Parameter("DisplayName", $"Test {offsetHoursStr} Display name"),
                        db.Parameter("ShortName", shortName),
                        db.Parameter("IsNegativeOffsetDirection", isNegativeOffsetDirection),
                        db.Parameter("OffsetTime", CardHelper.DefaultDateTime.Date.Add(new TimeSpan(0, offsetHours, 0, 0))))
                    .LogCommand()
                    .ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }
        /// <summary>
        /// Создаёт потокобезопасный словарь, содержащий: ключ - элемент из <paramref name="keys"/>; значение - новый экземпляр структуры <see cref="Guid"/>.
        /// </summary>
        /// <param name="keys">Массив ключей для которых должен быть сгенерирован объект <see cref="Guid"/>.</param>
        /// <returns>Потокобезопасный словарь, содержащий: ключ - элемент из <paramref name="keys"/>; значение - новый экземпляр структуры <see cref="Guid"/>.</returns>
        public static ConcurrentDictionary<string, Guid> GenerateIDs(params string[] keys)
        {
            Check.ArgumentNotNull(keys, nameof(keys));

            var dict = new ConcurrentDictionary<string, Guid>(StringComparer.Ordinal);
            foreach (var key in keys)
            {
                dict[key] = Guid.NewGuid();
            }

            return dict;
        }

        /// <summary>
        /// Возвращает название категории теста в соответствии с заданным типом СУБД.
        /// </summary>
        /// <param name="dbms">Тип СУБД.</param>
        /// <returns>
        /// Название категории теста.<para/>
        /// Возможные значения приведены в таблице:<para/>
        /// <list type="table">
        ///     <listheader>
        ///         <description>Тип СУБД</description>
        ///         <description>Название категории</description>
        ///     </listheader>
        ///     <item>
        ///         <description><see cref="Dbms.SqlServer"/></description>
        ///         <description>db-<see cref="ShortSqlServerName"/></description>
        ///     </item>
        ///     <item>
        ///         <description><see cref="Dbms.PostgreSql"/></description>
        ///         <description>db-<see cref="ShortPostgreSqlName"/></description>
        ///     </item>
        ///     <item>
        ///         <description>Любое другое значение</description>
        ///         <description><see langword="null"/></description>
        ///     </item>
        /// </list>
        /// </returns>
        public static string GetTestCategoryName(
            Dbms dbms)
        {
            var shortName = GetShortNameDbms(dbms);
            return string.IsNullOrEmpty(shortName) ? default : "db-" + shortName;
        }

        /// <summary>
        /// Возвращает краткое имя для заданного значения перечисления типа СУБД.
        /// </summary>
        /// <param name="dbms">Тип СУБД.</param>
        /// <returns>
        /// Краткое имя типа.<para/>
        /// <list type="table">
        ///     <listheader>
        ///         <description>Тип СУБД</description>
        ///         <description>Название категории</description>
        ///     </listheader>
        ///     <item>
        ///         <description><see cref="Dbms.SqlServer"/></description>
        ///         <description>db-<see cref="ShortSqlServerName"/></description>
        ///     </item>
        ///     <item>
        ///         <description><see cref="Dbms.PostgreSql"/></description>
        ///         <description>db-<see cref="ShortPostgreSqlName"/></description>
        ///     </item>
        ///     <item>
        ///         <description>Любое другое значение</description>
        ///         <description><see langword="null"/></description>
        ///     </item>
        /// </list>
        /// </returns>
        public static string GetShortNameDbms(Dbms dbms)
        {
            return dbms switch
            {
                Dbms.SqlServer => ShortSqlServerName,
                Dbms.PostgreSql => ShortPostgreSqlName,
                _ => default,
            };
        }

        /// <summary>
        /// Устанавливает категорию теста в соответствии с указанным типом СУБД.
        /// </summary>
        /// <param name="properties">Коллекция в которой располагается информация о категории теста.</param>
        /// <param name="dbms">Тип СУБД.</param>
        public static void SetTestCategory(
            IPropertyBag properties,
            Dbms dbms)
        {
            Check.ArgumentNotNull(properties, nameof(properties));

            var categoryName = GetTestCategoryName(dbms);
            SetTestCategory(properties, categoryName);
        }

        /// <summary>
        /// Устанавливает категорию теста в соответствии с указанным названием.
        /// </summary>
        /// <param name="properties">Коллекция в которой располагается информация о категории теста.</param>
        /// <param name="categoryName">Название категории.</param>
        public static void SetTestCategory(
            IPropertyBag properties,
            string categoryName)
        {
            Check.ArgumentNotNull(properties, nameof(properties));

            if (!string.IsNullOrEmpty(categoryName))
            {
                properties.Add(PropertyNames.Category, categoryName);
            }
        }

        /// <summary>
        /// Инициализирует локализацию по умолчанию. Действие выполняется ровно один раз и игнорируется при повторных вызовах.
        /// </summary>
        /// <returns>Асинхронная задача.</returns>
        /// <remarks>
        /// Язык локализации по умолчанию: английский.<para/>
        /// Локализация загружается из сборки, содержащей текущий выполняющийся тест.<para/>
        /// Поиск локализации выполняется по пути: <see cref="ResourcesPaths.Resources"/>\<see cref="ResourcesPaths.Localization"/>.
        /// </remarks>
        public static ValueTask InitializeDefaultLocalizationAsync() => globalTestLocalizationRegistrator.RegisterAsync();

        /// <summary>
        /// Добавляет в карточку, которой управляет указанный объект, один файл.
        /// </summary>
        /// <param name="fileContainer">Контейнер содержащий файлы.</param>
        /// <param name="isLocal">
        /// Признак того, что содержимое является локальным, т.е. оно сохраняется во временную папку.
        /// Установите значение <see langword="true"/>, если файл доступен пользователю в UI перед сохранением.
        /// Во всех остальных случаях задайте значение <see langword="false"/>.
        /// </param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Объект содержащий информацию о добавленном файле.</returns>
        public static async ValueTask<TestFileInfo> AddFileAsync(
            IFileContainer fileContainer,
            bool isLocal = default,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(fileContainer, nameof(fileContainer));

            var data = Guid.NewGuid();
            var fileName = data.ToString();
            var content = data.ToByteArray();

            (var file, var result) = await fileContainer
                .BuildFile(fileName)
                .SetContent(content, isLocal)
                .AddWithNotificationAsync(cancellationToken: cancellationToken);

            ValidationAssert.HasEmpty(result);

            return new TestFileInfo(file.ID, fileName, content);
        }

        /// <summary>
        /// Сравнивает файл, информация о котором задана в <paramref name="fileInfo"/>, с содержащимся в карточке, которой управляет объект <paramref name="clc"/>.
        /// </summary>
        /// <param name="clc">Объект, управляющий жизненным циклом карточки, в которой должен содержаться проверяемый файл.</param>
        /// <param name="fileInfo">Информация о проверяемом файле.</param>
        /// <param name="findById">Значение <see langword="true"/>, если наличие файла в карточке определяется по идентификатору, иначе по имени.</param>
        /// <param name="testCardFileFuncAsync">Метод выполняющий дополнительную проверку карточки файла.</param>
        /// <param name="testFileFuncAsync">Метод выполняющий дополнительную проверку файла.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        /// <remarks>Проверяет наличие карточки файла и объекта файла, его размер и возможность получения контента.</remarks>
        public static async ValueTask CheckFileAsync(
            ICardLifecycleCompanion clc,
            TestFileInfo fileInfo,
            bool findById = default,
            Func<Card, CardFile, CancellationToken, ValueTask> testCardFileFuncAsync = default,
            Func<Card, IFile, CancellationToken, ValueTask> testFileFuncAsync = default,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(clc, nameof(clc));

            var fileID = fileInfo.ID;
            var fileName = fileInfo.Name;

            // Проверка карточки файла.
            var testCardFile = clc.Card.Files.FirstOrDefault(
                findById
                ? i => i.RowID == fileID
                : i => string.Equals(i.Name, fileName, StringComparison.Ordinal));

            Assert.NotNull(testCardFile, "The card does not contain the specified file. ID: {0:B}. Name: \"{1}\".", fileID, fileName);
            Assert.That(testCardFile.Name, Is.EqualTo(fileName));
            Assert.That(testCardFile.VersionNumber, Is.EqualTo(1));
            Assert.That(testCardFile.Size, Is.EqualTo(fileInfo.Content.Length));

            if (testCardFileFuncAsync != null)
            {
                await testCardFileFuncAsync(clc.Card, testCardFile, cancellationToken);
            }

            // Проверка файла.
            var testFile = (await clc.GetCardFileContainerAsync(cancellationToken: cancellationToken)).FileContainer.TryGetFile(testCardFile.RowID);
            Assert.NotNull(testFile);

            if (!testFile.Content.HasData)
            {
                var result = await testFile.EnsureContentDownloadedAsync(cancellationToken: cancellationToken);
                ValidationAssert.HasEmpty(result);
            }

            await using (var contentStream = await testFile.Content.GetAsync(cancellationToken))
            {
                var content = await contentStream.ReadAllBytesAsync(cancellationToken);

                AssertThatBytesAreEqual(content, fileInfo.Content);
            }

            if (testFileFuncAsync != null)
            {
                await testFileFuncAsync(clc.Card, testFile, cancellationToken);
            }
        }

        /// <summary>
        /// Параллельно выполняет указанный набор тестов.
        /// </summary>
        /// <param name="tests">Коллекция содержащая информацию о параллельно выполняемых наборах тестов.</param>
        /// <param name="maxDegreeOfParallelism">Максимальное количество параллельных задач. Новые задачи запускаются по мере того, как текущие задачи завершаются.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        public static async Task RunTestFixtureParallelAsync(
            IEnumerable<TestMethodData> tests,
            int maxDegreeOfParallelism,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(tests, nameof(tests));

            if (maxDegreeOfParallelism <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism), maxDegreeOfParallelism, "The value must be greater than zero.");
            }

            var testArr = tests.ToArray();

            if (testArr.Length == 0)
            {
                return;
            }

            foreach (var test in testArr)
            {
                if (test.IsEmpty)
                {
                    throw new ArgumentException("The collection contains an uninitialized element.", nameof(tests));
                }

                var returnType = test.TestMethod.ReturnType;

                if (returnType != typeof(void) && returnType != typeof(Task))
                {
                    throw new ArgumentException($"Only test methods that return a value of the \"{typeof(void).Name}\" or \"{nameof(Task)}\" type are allowed. Method name: \"{test.TestMethod.Name}\".", nameof(tests));
                }
            }

            await testArr.RunWithMaxDegreeOfParallelismAsync(
                maxDegreeOfParallelism,
                TestWorkerAsync,
                cancellationToken);
        }

        /// <summary>
        /// Устанавливает результат выполнения теста как <see cref="ResultState.Error"/> содержащий заданную информацию об ошибке.
        /// </summary>
        /// <param name="site">Этап выполнения теста на котором произошла ошибка.</param>
        /// <param name="ex">Исключение содержащее информацию по ошибке.</param>
        public static void SetAssertionResult(FailureSite site, Exception ex)
        {
            Check.ArgumentNotNull(ex, nameof(ex));

            var message = NUnit.Framework.Internal.ExceptionHelper.BuildMessage(ex);
            var stackTrace = NUnit.Framework.Internal.ExceptionHelper.BuildStackTrace(ex);

            SetAssertionResult(site, message, stackTrace);
        }

        /// <summary>
        /// Устанавливает результат выполнения теста как <see cref="ResultState.Error"/> содержащий заданную информацию об ошибке.
        /// </summary>
        /// <param name="site">Этап выполнения теста на котором произошла ошибка.</param>
        /// <param name="message">Сообщение описывающее ошибку.</param>
        /// <param name="stackTrace">Стек-трейс.</param>
        public static void SetAssertionResult(
            FailureSite site,
            string message = default,
            string stackTrace = default)
        {
            var currentResult = TestExecutionContext.CurrentContext.CurrentResult;

            if (currentResult.ResultState.Status != TestStatus.Failed
                || currentResult.ResultState.Status != TestStatus.Inconclusive)
            {
                var resultState = ResultState.Error.WithSite(site);
                currentResult.SetResult(resultState, message, stackTrace);
            }

            currentResult.RecordAssertion(new AssertionResult(AssertionStatus.Error, message, stackTrace));
        }

        /// <summary>
        /// Заполняет таблицы ролей, инстансов и замещений тестовыми значениями.
        /// </summary>
        /// <param name="dbScope">Объект для взаимодействия с базой данных.</param>
        /// <returns>Асинхронная задача.</returns>
        public static async Task FillTestRoleDeputiesAsync(IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                var builderFactory = dbScope.BuilderFactory;

                var id1 = new Guid("4ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var instanceTypeId1 = RoleHelper.PersonalRoleTypeID;
                var instanceTypeCaption1 = RoleHelper.PersonalRoleTypeCaption;
                var name1 = "Аликин С.С.";
                var typeId1 = 1;
                Guid? parentId1 = null;
                var modifiedDateTime1 = DateTime.Parse("2013-06-17T14:43:09");
                var modifiedByUserId1 = new Guid("11111111-1111-1111-1111-111111111111");
                var modifiedByUserName1 = "System";
                var fullName1 = "Аликин Сергей Сергеевич";
                var phone1 = "+79991234567";
                var timeZoneId1 = 0;
                var timeZoneCodeName1 = "Default";
                var timeZoneShortName1 = "UTC+03:00";
                var timeZoneUtcOffsetMinutes1 = 180;
                var insertQuery11 = builderFactory
                    .InsertInto("Instances", "ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName")
                    .Values(b => b.P("ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery11,
                        db.Parameter("ID", id1),
                        db.Parameter("TypeID", instanceTypeId1),
                        db.Parameter("TypeCaption", instanceTypeCaption1),
                        db.Parameter("Version", 1, DataType.Int16),
                        db.Parameter("Readers", 0, DataType.Int16),
                        db.Parameter("WritePending", false),
                        db.Parameter("Created", modifiedDateTime1),
                        db.Parameter("CreatedByID", modifiedByUserId1),
                        db.Parameter("CreatedByName", modifiedByUserName1),
                        db.Parameter("Modified", modifiedDateTime1),
                        db.Parameter("ModifiedByID", modifiedByUserId1),
                        db.Parameter("ModifiedByName", modifiedByUserName1))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
                var insertQuery12 = builderFactory
                    .InsertInto("Roles", "ID", "Name", "TypeID", "ParentID", "TimeZoneID",
                        "TimeZoneShortName", "TimeZoneCodeName", "TimeZoneUtcOffsetMinutes")
                    .Values(b => b.P("ID", "Name", "TypeID", "ParentID", "TimeZoneID",
                        "TimeZoneShortName", "TimeZoneCodeName", "TimeZoneUtcOffsetMinutes"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery12,
                        db.Parameter("ID", id1),
                        db.Parameter("Name", name1),
                        db.Parameter("TypeID", typeId1),
                        db.Parameter("ParentID", parentId1),
                        db.Parameter("TimeZoneID", timeZoneId1),
                        db.Parameter("TimeZoneShortName", timeZoneShortName1),
                        db.Parameter("TimeZoneCodeName", timeZoneCodeName1),
                        db.Parameter("TimeZoneUtcOffsetMinutes", timeZoneUtcOffsetMinutes1))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
                var insertQuery13 = builderFactory
                    .InsertInto("PersonalRoles", "ID", "Name", "FullName", "Phone")
                    .Values(b => b.P("ID", "Name", "FullName", "Phone"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery13,
                        db.Parameter("ID", id1),
                        db.Parameter("Name", name1),
                        db.Parameter("FullName", fullName1),
                        db.Parameter("Phone", phone1))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);

                var id2 = new Guid("2ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var instanceTypeId2 = new Guid("825DBACC-DDEC-00D1-A550-2A837792542E");
                var instanceTypeCaption2 = "Статическая роль";
                var name2 = "My role";
                var typeId2 = 0;
                Guid? parentId2 = null;
                var modifiedDateTime2 = DateTime.Parse("2013-06-17T14:43:09");
                var modifiedByUserId2 = new Guid("11111111-1111-1111-1111-111111111111");
                var modifiedByUserName2 = "System";
                var timeZoneId2 = 0;
                var timeZoneCodeName2 = "Default";
                var timeZoneShortName2 = "UTC+03:00";
                var timeZoneUtcOffsetMinutes2 = 180;
                var insertQuery21 = builderFactory
                    .InsertInto("Instances", "ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName")
                    .Values(b => b.P("ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery21,
                        db.Parameter("ID", id2),
                        db.Parameter("TypeID", instanceTypeId2),
                        db.Parameter("TypeCaption", instanceTypeCaption2),
                        db.Parameter("Version", 1, DataType.Int16),
                        db.Parameter("Readers", 0, DataType.Int16),
                        db.Parameter("WritePending", false),
                        db.Parameter("Created", modifiedDateTime2),
                        db.Parameter("CreatedByID", modifiedByUserId2),
                        db.Parameter("CreatedByName", modifiedByUserName2),
                        db.Parameter("Modified", modifiedDateTime2),
                        db.Parameter("ModifiedByID", modifiedByUserId2),
                        db.Parameter("ModifiedByName", modifiedByUserName2))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
                var insertQuery22 = builderFactory
                    .InsertInto("Roles", "ID", "Name", "TypeID", "ParentID", "TimeZoneID",
                        "TimeZoneShortName", "TimeZoneCodeName", "TimeZoneUtcOffsetMinutes")
                    .Values(b => b.P("ID", "Name", "TypeID", "ParentID", "TimeZoneID",
                        "TimeZoneShortName", "TimeZoneCodeName", "TimeZoneUtcOffsetMinutes"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery22,
                        db.Parameter("ID", id2),
                        db.Parameter("Name", name2),
                        db.Parameter("TypeID", typeId2),
                        db.Parameter("ParentID", parentId2),
                        db.Parameter("TimeZoneID", timeZoneId2),
                        db.Parameter("TimeZoneShortName", timeZoneShortName2),
                        db.Parameter("TimeZoneCodeName", timeZoneCodeName2),
                        db.Parameter("TimeZoneUtcOffsetMinutes", timeZoneUtcOffsetMinutes2))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);

                var rowId3 = new Guid("4bb280e4-e939-4f70-9c66-dfdfff352eae");
                var id3 = new Guid("2ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var userId3 = new Guid("4ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var typeId3 = 0;
                var isDeputy3 = false;
                var userName3 = "Аликин С.С.";
                var insertQuery31 = builderFactory
                    .InsertInto("RoleUsers", "RowID", "ID", "TypeID", "IsDeputy", "UserID", "UserName")
                    .Values(b => b.P("RowID", "ID", "TypeID", "IsDeputy", "UserID", "UserName"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery31,
                        db.Parameter("RowID", rowId3),
                        db.Parameter("ID", id3),
                        db.Parameter("TypeID", typeId3),
                        db.Parameter("IsDeputy", isDeputy3),
                        db.Parameter("UserID", userId3),
                        db.Parameter("UserName", userName3))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);

                var id4 = new Guid("3ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var instanceTypeId4 = new Guid("825DBACC-DDEC-00D1-A550-2A837792542E");
                var instanceTypeCaption4 = "Статическая роль";
                var name4 = "Other role";
                var typeId4 = 0;
                Guid? parentId4 = new Guid("2ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var modifiedDateTime4 = DateTime.Parse("2013-06-17T14:43:09");
                var modifiedByUserId4 = new Guid("4ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var modifiedByUserName4 = "Аликин С.С.";
                var timeZoneId4 = 0;
                var timeZoneCodeName4 = "Default";
                var timeZoneShortName4 = "UTC+03:00";
                var timeZoneUtcOffsetMinutes4 = 180;
                var insertQuery41 = builderFactory
                    .InsertInto("Instances", "ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName")
                    .Values(b => b.P("ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery41,
                        db.Parameter("ID", id4),
                        db.Parameter("TypeID", instanceTypeId4),
                        db.Parameter("TypeCaption", instanceTypeCaption4),
                        db.Parameter("Version", 1, DataType.Int16),
                        db.Parameter("Readers", 0, DataType.Int16),
                        db.Parameter("WritePending", false),
                        db.Parameter("Created", modifiedDateTime4),
                        db.Parameter("CreatedByID", modifiedByUserId4),
                        db.Parameter("CreatedByName", modifiedByUserName4),
                        db.Parameter("Modified", modifiedDateTime4),
                        db.Parameter("ModifiedByID", modifiedByUserId4),
                        db.Parameter("ModifiedByName", modifiedByUserName4))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
                var insertQuery42 = builderFactory
                    .InsertInto("Roles", "ID", "Name", "TypeID", "ParentID", "TimeZoneID",
                        "TimeZoneShortName", "TimeZoneCodeName", "TimeZoneUtcOffsetMinutes")
                    .Values(b => b.P("ID", "Name", "TypeID", "ParentID", "TimeZoneID",
                        "TimeZoneShortName", "TimeZoneCodeName", "TimeZoneUtcOffsetMinutes"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery42,
                        db.Parameter("ID", id4),
                        db.Parameter("Name", name4),
                        db.Parameter("TypeID", typeId4),
                        db.Parameter("ParentID", parentId4),
                        db.Parameter("TimeZoneID", timeZoneId4),
                        db.Parameter("TimeZoneShortName", timeZoneShortName4),
                        db.Parameter("TimeZoneCodeName", timeZoneCodeName4),
                        db.Parameter("TimeZoneUtcOffsetMinutes", timeZoneUtcOffsetMinutes4))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);

                var id5 = new Guid("5ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var instanceTypeId5 = new Guid("ABE57CB7-E1CB-06F6-B7CA-AD1668BEBD72");
                var instanceTypeCaption5 = "System";
                var name5 = "Канцелярия";
                var typeId5 = 2;
                Guid? parentId5 = null;
                var modifiedDateTime5 = DateTime.Parse("2013-06-17T14:43:09");
                var modifiedByUserId5 = new Guid("11111111-1111-1111-1111-111111111111");
                var modifiedByUserName5 = "System";
                var timeZoneId5 = 0;
                var timeZoneCodeName5 = "Default";
                var timeZoneShortName5 = "UTC+03:00";
                var timeZoneUtcOffsetMinutes5 = 180;
                var headUserId5 = new Guid("4ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var headUserName5 = "Аликин С.С.";
                var insertQuery51 = builderFactory
                    .InsertInto("Instances", "ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName")
                    .Values(b => b.P("ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery51,
                        db.Parameter("ID", id5),
                        db.Parameter("TypeID", instanceTypeId5),
                        db.Parameter("TypeCaption", instanceTypeCaption5),
                        db.Parameter("Version", 1, DataType.Int16),
                        db.Parameter("Readers", 0, DataType.Int16),
                        db.Parameter("WritePending", false),
                        db.Parameter("Created", modifiedDateTime5),
                        db.Parameter("CreatedByID", modifiedByUserId5),
                        db.Parameter("CreatedByName", modifiedByUserName5),
                        db.Parameter("Modified", modifiedDateTime5),
                        db.Parameter("ModifiedByID", modifiedByUserId5),
                        db.Parameter("ModifiedByName", modifiedByUserName5))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
                var insertQuery52 = builderFactory
                    .InsertInto("Roles", "ID", "Name", "TypeID", "ParentID", "TimeZoneID",
                        "TimeZoneShortName", "TimeZoneCodeName", "TimeZoneUtcOffsetMinutes")
                    .Values(b => b.P("ID", "Name", "TypeID", "ParentID", "TimeZoneID",
                        "TimeZoneShortName", "TimeZoneCodeName", "TimeZoneUtcOffsetMinutes"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery52,
                        db.Parameter("ID", id5),
                        db.Parameter("Name", name5),
                        db.Parameter("TypeID", typeId5),
                        db.Parameter("ParentID", parentId5),
                        db.Parameter("TimeZoneID", timeZoneId5),
                        db.Parameter("TimeZoneShortName", timeZoneShortName5),
                        db.Parameter("TimeZoneCodeName", timeZoneCodeName5),
                        db.Parameter("TimeZoneUtcOffsetMinutes", timeZoneUtcOffsetMinutes5))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
                var insertQuery53 = builderFactory
                    .InsertInto("DepartmentRoles", "ID", "HeadUserID", "HeadUserName")
                    .Values(b => b.P("ID", "HeadUserID", "HeadUserName"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery53,
                        db.Parameter("ID", id5),
                        db.Parameter("HeadUserID", headUserId5),
                        db.Parameter("HeadUserName", headUserName5))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);

                var id6 = new Guid("6ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var instanceTypeId6 = new Guid("97A945BC-58F5-07FA-A274-B6A7F0F1282C");
                var instanceTypeCaption6 = "Динамическая роль";
                var name6 = "Динамическая роль";
                var typeId6 = 3;
                Guid? parentId6 = null;
                var modifiedDateTime6 = DateTime.Parse("2013-06-17T14:43:09");
                var modifiedByUserId6 = new Guid("4ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var modifiedByUserName6 = "Аликин С.С.";
                var sqlText6 = "SELECT t.UserID, t.FullName FROM [list_of_users] t WHERE t.FullName LIKE 'Аликин %'";
                var cronScheduling6 = "0/5 * * * * ?";
                var schedulingTypeId6 = 1;
                int? periodSchedulin6 = null;
                var timeZoneId6 = 0;
                var timeZoneCodeName6 = "Default";
                var timeZoneShortName6 = "UTC+03:00";
                var timeZoneUtcOffsetMinutes6 = 180;
                string lastErrorText6 = null;
                DateTime? lastErrorDate6 = null;
                var insertQuery61 = builderFactory
                    .InsertInto("Instances", "ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName")
                    .Values(b => b.P("ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery61,
                        db.Parameter("ID", id6),
                        db.Parameter("TypeID", instanceTypeId6),
                        db.Parameter("TypeCaption", instanceTypeCaption6),
                        db.Parameter("Version", 1, DataType.Int16),
                        db.Parameter("Readers", 0, DataType.Int16),
                        db.Parameter("WritePending", false),
                        db.Parameter("Created", modifiedDateTime6),
                        db.Parameter("CreatedByID", modifiedByUserId6),
                        db.Parameter("CreatedByName", modifiedByUserName6),
                        db.Parameter("Modified", modifiedDateTime6),
                        db.Parameter("ModifiedByID", modifiedByUserId6),
                        db.Parameter("ModifiedByName", modifiedByUserName6))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
                var insertQuery62 = builderFactory
                    .InsertInto("Roles", "ID", "Name", "TypeID", "ParentID", "TimeZoneID",
                        "TimeZoneShortName", "TimeZoneCodeName", "TimeZoneUtcOffsetMinutes")
                    .Values(b => b.P("ID", "Name", "TypeID", "ParentID", "TimeZoneID",
                        "TimeZoneShortName", "TimeZoneCodeName", "TimeZoneUtcOffsetMinutes"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery62,
                        db.Parameter("ID", id6),
                        db.Parameter("Name", name6),
                        db.Parameter("TypeID", typeId6),
                        db.Parameter("ParentID", parentId6),
                        db.Parameter("TimeZoneID", timeZoneId6),
                        db.Parameter("TimeZoneShortName", timeZoneShortName6),
                        db.Parameter("TimeZoneCodeName", timeZoneCodeName6),
                        db.Parameter("TimeZoneUtcOffsetMinutes", timeZoneUtcOffsetMinutes6))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
                var insertQuery63 = builderFactory
                    .InsertInto("DynamicRoles", "ID", "Name", "SqlText", "CronScheduling",
                        "PeriodScheduling", "SchedulingTypeID", "LastErrorText", "LastErrorDate")
                    .Values(b => b.P("ID", "Name", "SqlText", "CronScheduling", "PeriodScheduling",
                        "SchedulingTypeID", "LastErrorText", "LastErrorDate"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery63,
                            db.Parameter("ID", id6),
                            db.Parameter("Name", name6),
                            db.Parameter("SqlText", sqlText6),
                            db.Parameter("CronScheduling", cronScheduling6),
                            db.Parameter("PeriodScheduling", periodSchedulin6),
                            db.Parameter("SchedulingTypeID", schedulingTypeId6),
                            db.Parameter("LastErrorText", lastErrorText6),
                            db.Parameter("LastErrorDate", lastErrorDate6))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);

                var id7 = new Guid("7ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var instanceTypeId7 = new Guid("B672E00C-0241-0485-9B07-4764BC96C9D3");
                var instanceTypeCaption7 = "Контекстная роль";
                var name7 = "Контекстная роль";
                var typeId7 = 4;
                Guid? parentId7 = null;
                var modifiedDateTime7 = DateTime.Parse("2013-06-17T14:43:09");
                var modifiedByUserId7 = new Guid("4ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var modifiedByUserName7 = "Аликин С.С.";
                var sqlText7 = "SELECT * FROM [RoleUsers]";
                var timeZoneId7 = 0;
                var timeZoneCodeName7 = "Default";
                var timeZoneShortName7 = "UTC+03:00";
                var timeZoneUtcOffsetMinutes7 = 180;
                var insertQuery71 = builderFactory
                    .InsertInto("Instances", "ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName")
                    .Values(b => b.P("ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery71,
                        db.Parameter("ID", id7),
                        db.Parameter("TypeID", instanceTypeId7),
                        db.Parameter("TypeCaption", instanceTypeCaption7),
                        db.Parameter("Version", 1, DataType.Int16),
                        db.Parameter("Readers", 0, DataType.Int16),
                        db.Parameter("WritePending", false),
                        db.Parameter("Created", modifiedDateTime7),
                        db.Parameter("CreatedByID", modifiedByUserId7),
                        db.Parameter("CreatedByName", modifiedByUserName7),
                        db.Parameter("Modified", modifiedDateTime7),
                        db.Parameter("ModifiedByID", modifiedByUserId7),
                        db.Parameter("ModifiedByName", modifiedByUserName7))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
                var insertQuery72 = builderFactory
                    .InsertInto("Roles", "ID", "Name", "TypeID", "ParentID", "TimeZoneID",
                        "TimeZoneShortName", "TimeZoneCodeName", "TimeZoneUtcOffsetMinutes")
                    .Values(b => b.P("ID", "Name", "TypeID", "ParentID", "TimeZoneID",
                        "TimeZoneShortName", "TimeZoneCodeName", "TimeZoneUtcOffsetMinutes"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery72,
                        db.Parameter("ID", id7),
                        db.Parameter("Name", name7),
                        db.Parameter("TypeID", typeId7),
                        db.Parameter("ParentID", parentId7),
                        db.Parameter("TimeZoneID", timeZoneId7),
                        db.Parameter("TimeZoneShortName", timeZoneShortName7),
                        db.Parameter("TimeZoneCodeName", timeZoneCodeName7),
                        db.Parameter("TimeZoneUtcOffsetMinutes", timeZoneUtcOffsetMinutes7))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
                var insertQuery73 = builderFactory
                    .InsertInto("ContextRoles", "ID", "SqlText", "SqlTextForCard", "SqlTextForUser")
                    .Values(b => b.P("ID", "SqlText", "SqlTextForCard", "SqlTextForUser"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery73,
                            db.Parameter("ID", id7),
                            db.Parameter("SqlText", sqlText7),
                            db.Parameter("SqlTextForCard", sqlText7),
                            db.Parameter("SqlTextForUser", sqlText7))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);

                var id8 = new Guid("8ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var instanceTypeId8 = new Guid("890379D8-651E-01D9-85C7-12644B5364B8");
                var instanceTypeCaption8 = "Генератор метаролей";
                var name8 = "Генератор 1";
                var modifiedDateTime8 = DateTime.Parse("2013-06-17T14:43:09");
                var modifiedByUserId8 = new Guid("4ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var modifiedByUserName8 = "Аликин С.С.";
                var sqlText8 = "SELECT 1, 2, 3";
                string? cronScheduling8 = null;
                var schedulingTypeId8 = 0;
                int? periodSchedulin8 = 300;
                string lastErrorText8 = null;
                DateTime? lastErrorDate8 = null;
                var insertQuery81 = builderFactory
                    .InsertInto("Instances", "ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName")
                    .Values(b => b.P("ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery81,
                        db.Parameter("ID", id8),
                        db.Parameter("TypeID", instanceTypeId8),
                        db.Parameter("TypeCaption", instanceTypeCaption8),
                        db.Parameter("Version", 1, DataType.Int16),
                        db.Parameter("Readers", 0, DataType.Int16),
                        db.Parameter("WritePending", false),
                        db.Parameter("Created", modifiedDateTime8),
                        db.Parameter("CreatedByID", modifiedByUserId8),
                        db.Parameter("CreatedByName", modifiedByUserName8),
                        db.Parameter("Modified", modifiedDateTime8),
                        db.Parameter("ModifiedByID", modifiedByUserId8),
                        db.Parameter("ModifiedByName", modifiedByUserName8))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
                var insertQuery82 = builderFactory
                    .InsertInto("RoleGenerators", "ID", "Name", "SqlText", "CronScheduling",
                        "PeriodScheduling", "SchedulingTypeID", "LastErrorText", "LastErrorDate")
                    .Values(b => b.P("ID", "Name", "SqlText", "CronScheduling",
                        "PeriodScheduling", "SchedulingTypeID", "LastErrorText", "LastErrorDate"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery82,
                            db.Parameter("ID", id8),
                            db.Parameter("Name", name8),
                            db.Parameter("SqlText", sqlText8),
                            db.Parameter("CronScheduling", cronScheduling8),
                            db.Parameter("PeriodScheduling", periodSchedulin8),
                            db.Parameter("SchedulingTypeID", schedulingTypeId8),
                            db.Parameter("LastErrorText", lastErrorText8),
                            db.Parameter("LastErrorDate", lastErrorDate8))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);

                var id9 = new Guid("9ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var instanceTypeId9 = new Guid("C6C9E585-C053-0AA0-994A-F80225F8585F");
                var instanceTypeCaption9 = "Метароль";
                var name9 = "Метароль 1";
                var typeId9 = 5;
                Guid? parentId9 = null;
                var modifiedDateTime9 = DateTime.Parse("2013-06-17T14:43:09");
                var modifiedByUserId9 = new Guid("11111111-1111-1111-1111-111111111111");
                var modifiedByUserName9 = "System";
                var timeZoneId9 = 0;
                var timeZoneCodeName9 = "Default";
                var timeZoneShortName9 = "UTC+03:00";
                var timeZoneUtcOffsetMinutes9 = 180;
                var metaTypeId9 = 1;
                Guid? idGuid9 = null;
                int? idInteger9 = 1;
                string? idString9 = null;
                var generatorId9 = new Guid("8ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var generatorName9 = "Генератор 1";
                var insertQuery91 = builderFactory
                    .InsertInto("Instances", "ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName")
                    .Values(b => b.P("ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery91,
                        db.Parameter("ID", id9),
                        db.Parameter("TypeID", instanceTypeId9),
                        db.Parameter("TypeCaption", instanceTypeCaption9),
                        db.Parameter("Version", 1, DataType.Int16),
                        db.Parameter("Readers", 0, DataType.Int16),
                        db.Parameter("WritePending", false),
                        db.Parameter("Created", modifiedDateTime9),
                        db.Parameter("CreatedByID", modifiedByUserId9),
                        db.Parameter("CreatedByName", modifiedByUserName9),
                        db.Parameter("Modified", modifiedDateTime9),
                        db.Parameter("ModifiedByID", modifiedByUserId9),
                        db.Parameter("ModifiedByName", modifiedByUserName9))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
                var insertQuery92 = builderFactory
                    .InsertInto("Roles", "ID", "Name", "TypeID", "ParentID", "TimeZoneID",
                        "TimeZoneShortName", "TimeZoneCodeName", "TimeZoneUtcOffsetMinutes")
                    .Values(b => b.P("ID", "Name", "TypeID", "ParentID", "TimeZoneID",
                        "TimeZoneShortName", "TimeZoneCodeName", "TimeZoneUtcOffsetMinutes"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery92,
                        db.Parameter("ID", id9),
                        db.Parameter("Name", name9),
                        db.Parameter("TypeID", typeId9),
                        db.Parameter("ParentID", parentId9),
                        db.Parameter("TimeZoneID", timeZoneId9),
                        db.Parameter("TimeZoneShortName", timeZoneShortName9),
                        db.Parameter("TimeZoneCodeName", timeZoneCodeName9),
                        db.Parameter("TimeZoneUtcOffsetMinutes", timeZoneUtcOffsetMinutes9))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
                var insertQuery93 = builderFactory
                    .InsertInto("MetaRoles", "ID", "Name", "TypeID", "IDGuid", "IDInteger",
                        "IDString", "GeneratorID", "GeneratorName")
                    .Values(b => b.P("ID", "Name", "TypeID", "IDGuid", "IDInteger",
                        "IDString", "GeneratorID", "GeneratorName"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery93,
                        db.Parameter("ID", id9),
                        db.Parameter("Name", name9),
                        db.Parameter("TypeID", metaTypeId9),
                        db.Parameter("IDGuid", idGuid9),
                        db.Parameter("IDInteger", idInteger9),
                        db.Parameter("IDString", idString9),
                        db.Parameter("GeneratorID", generatorId9),
                        db.Parameter("GeneratorName", generatorName9))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);

                var id10 = new Guid("237e60aa-7f5d-4903-9dcB-5e4f1df65802");
                var instanceTypeId10 = new Guid("E97C253C-9102-0440-AC7E-4876E8F789DA");
                var instanceTypeCaption10 = "Роль задания";
                var name10 = "My task role";
                var typeId10 = 6;
                Guid? parentId10 = null;
                var modifiedDateTime10 = DateTime.Parse("2013-06-17T14:43:09");
                var modifiedByUserId10 = new Guid("11111111-1111-1111-1111-111111111111");
                var modifiedByUserName10 = "System";
                var timeZoneId10 = 0;
                var timeZoneCodeName10 = "Default";
                var timeZoneShortName10 = "UTC+03:00";
                var timeZoneUtcOffsetMinutes10 = 180;
                var insertQuery101 = builderFactory
                    .InsertInto("Instances", "ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName")
                    .Values(b => b.P("ID", "TypeID", "TypeCaption",
                        "Version", "Readers", "WritePending", "Created", "CreatedByID",
                        "CreatedByName", "Modified", "ModifiedByID", "ModifiedByName"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery101,
                        db.Parameter("ID", id10),
                        db.Parameter("TypeID", instanceTypeId10),
                        db.Parameter("TypeCaption", instanceTypeCaption10),
                        db.Parameter("Version", 1, DataType.Int16),
                        db.Parameter("Readers", 0, DataType.Int16),
                        db.Parameter("WritePending", false),
                        db.Parameter("Created", modifiedDateTime10),
                        db.Parameter("CreatedByID", modifiedByUserId10),
                        db.Parameter("CreatedByName", modifiedByUserName10),
                        db.Parameter("Modified", modifiedDateTime10),
                        db.Parameter("ModifiedByID", modifiedByUserId10),
                        db.Parameter("ModifiedByName", modifiedByUserName10))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
                var insertQuery102 = builderFactory
                    .InsertInto("Roles", "ID", "Name", "TypeID", "ParentID", "TimeZoneID",
                        "TimeZoneShortName", "TimeZoneCodeName", "TimeZoneUtcOffsetMinutes")
                    .Values(b => b.P("ID", "Name", "TypeID", "ParentID", "TimeZoneID",
                        "TimeZoneShortName", "TimeZoneCodeName", "TimeZoneUtcOffsetMinutes"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery102,
                        db.Parameter("ID", id10),
                        db.Parameter("Name", name10),
                        db.Parameter("TypeID", typeId10),
                        db.Parameter("ParentID", parentId10),
                        db.Parameter("TimeZoneID", timeZoneId10),
                        db.Parameter("TimeZoneShortName", timeZoneShortName10),
                        db.Parameter("TimeZoneCodeName", timeZoneCodeName10),
                        db.Parameter("TimeZoneUtcOffsetMinutes", timeZoneUtcOffsetMinutes10))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);

                var rowId11 = new Guid("0238b068-46e8-4f7a-b249-80059152a6d7");
                var id11 = new Guid("2ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var deputyId11 = new Guid("4ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var deputyName11 = "Аликин С.С.";
                Guid? deputizedId11 = null;
                string? deputizedName11 = null;
                var minDate11 = DateTime.Parse("2013-04-23T00:00:00");
                var maxDate11 = DateTime.Parse("2013-06-01T00:00:00");
                var typeId11 = 0;
                var isActive11 = false;
                var insertQuery111 = builderFactory
                    .InsertInto("RoleDeputies", "RowID", "ID", "DeputyID", "DeputyName",
                        "DeputizedID", "DeputizedName", "MinDate", "MaxDate", "TypeID", "IsActive")
                    .Values(b => b.P("RowID", "ID", "DeputyID", "DeputyName",
                        "DeputizedID", "DeputizedName", "MinDate", "MaxDate", "TypeID", "IsActive"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery111,
                        db.Parameter("RowID", rowId11),
                        db.Parameter("ID", id11),
                        db.Parameter("DeputyID", deputyId11),
                        db.Parameter("DeputyName", deputyName11),
                        db.Parameter("DeputizedID", deputizedId11),
                        db.Parameter("DeputizedName", deputizedName11),
                        db.Parameter("MinDate", minDate11),
                        db.Parameter("MaxDate", maxDate11),
                        db.Parameter("TypeID", typeId11),
                        db.Parameter("IsActive", isActive11))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);

                var rowId12 = new Guid("182ce94d-f529-42a5-acd8-4ecfa5758a4b");
                var id12 = new Guid("2ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var deputyId12 = new Guid("4ed31597-de86-491a-9b6c-67f3dd3f40d7");
                var deputyName12 = "Аликин С.С.";
                Guid? deputizedId12 = null;
                string? deputizedName12 = null;
                var minDate12 = DateTime.Parse("2013-04-23T00:00:00");
                var maxDate12 = DateTime.Parse("2013-06-01T00:00:00");
                var typeId12 = 0;
                var isActive12 = false;
                var insertQuery112 = builderFactory
                    .InsertInto("RoleDeputies", "RowID", "ID", "DeputyID", "DeputyName",
                        "DeputizedID", "DeputizedName", "MinDate", "MaxDate", "TypeID", "IsActive")
                    .Values(b => b.P("RowID", "ID", "DeputyID", "DeputyName",
                        "DeputizedID", "DeputizedName", "MinDate", "MaxDate", "TypeID", "IsActive"))
                    .Build();
                await db
                    .SetCommand(
                        insertQuery112,
                        db.Parameter("RowID", rowId12),
                        db.Parameter("ID", id12),
                        db.Parameter("DeputyID", deputyId12),
                        db.Parameter("DeputyName", deputyName12),
                        db.Parameter("DeputizedID", deputizedId12),
                        db.Parameter("DeputizedName", deputizedName12),
                        db.Parameter("MinDate", minDate12),
                        db.Parameter("MaxDate", maxDate12),
                        db.Parameter("TypeID", typeId12),
                        db.Parameter("IsActive", isActive12))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Сравнивает на равенство значение двух ресурсов.
        /// </summary>
        /// <param name="file1">Первый ресурс.</param>
        /// <param name="file2">Второй ресурс.</param>
        /// <returns>Значение <see langword="true"/>, если оба источника данных существуют и значения ресурсов равны, иначе - <see langword="false"/>.</returns>
        public static async ValueTask<bool> FileCompareAsync(ISourceContentProvider file1, ISourceContentProvider file2)
        {
            Check.ArgumentNotNull(file1, nameof(file1));
            Check.ArgumentNotNull(file2, nameof(file2));

            if (!await file1.IsExistsAsync())
            {
                throw new ArgumentException($"File {file1.GetFullName()} does not exists.", nameof(file1));
            }

            if (ReferenceEquals(file1, file2))
            {
                return true;
            }

            if (!await file2.IsExistsAsync())
            {
                throw new ArgumentException($"File {file2.GetFullName()} does not exists.", nameof(file2));
            }

            await using var fs1 = await file1.CreateStreamReadAsync();
            await using var fs2 = await file2.CreateStreamReadAsync();
            return StreamCompare(fs1, fs2);
        }

        /// <summary>
        /// Сравнивает на равенство значения двух потоков. Потоки должны поддерживать чтение.
        /// </summary>
        /// <param name="stream1">Первый поток.</param>
        /// <param name="stream2">Второй поток.</param>
        /// <returns>Значение <see langword="true"/>, если оба потока равны <see langword="null"/> или равны значения содержащиеся в потоках, иначе - <see langword="false"/>.</returns>
        public static bool StreamCompare(Stream stream1, Stream stream2)
        {
            if (ReferenceEquals(stream1, stream2))
            {
                return true;
            }

            // Любой из них null, но не оба, т.к. проверили выше, значит не равны.
            if (stream1 is null || stream2 is null)
            {
                return false;
            }

            // Проверяем длину.
            if (stream1.CanSeek && stream2.CanSeek
                && stream1.Length != stream2.Length)
            {
                return false;
            }

            int stream1Byte;
            int stream2Byte;

            // Сравниваем байты в потоках.
            do
            {
                // Read one byte from each file.
                stream1Byte = stream1.ReadByte();
                stream2Byte = stream2.ReadByte();
            } while ((stream1Byte == stream2Byte) && (stream1Byte != -1));

            // Проверяем последний байт после цикла.
            return (stream1Byte - stream2Byte) == 0;
        }

        /// <summary>
        /// Конвертирует указанную дату и время в строку в соответствии с форматом yyMMddHHmm не зависящем от языка и культуры. Значение используется в API тестов при формировании идентификаторов временных ресурсов.
        /// </summary>
        /// <param name="dt">Дата и время.</param>
        /// <returns>Строка представляющая дату и время в формате yyMMddHHmm.</returns>
        public static string FormatDateTimeCode(this DateTime dt) =>
            dt.ToString("yyMMddHHmm", CultureInfo.InvariantCulture);

        /// <summary>
        /// Возвращает имя теста в виде строки <b>&lt;имя_класса&gt;.&lt;имя_теста&gt;</b> ограниченной до максимальной длины <paramref name="maxLength"/>,
        /// вставляя постоянную соль на конце, если длина полученной строки больше максимальной.
        ///
        /// Длина возвращаемого значения гарантированно не больше <paramref name="maxLength"/>
        /// вместе с солью, если <paramref name="maxLength"/> не меньше <c>10</c>,
        /// в противном случае длина строки ограничивается до <c>10</c> символов.
        /// </summary>
        /// <param name="maxLength">Максимальная длина возвращаемой строки.</param>
        /// <returns>Имя теста ограниченное до <paramref name="maxLength"/> символов.</returns>
        public static string GetTestNameLimited(
            int maxLength = 128)
        {
            var currentTest = TestExecutionContext.CurrentContext.CurrentTest;
            var name = $"{currentTest.TypeInfo?.Name}.{currentTest.Name}";

            return name.Length > maxLength
                ? $"{name[..Math.Max(1, maxLength - 9)]}_{currentTest.FullName.GetConstantHashCode():x}"
                : name;
        }

        /// <summary>
        /// Обрабатывает заданный список действий.
        /// </summary>
        /// <param name="testActions">Список действий.</param>
        /// <param name="safeRecordAction">Действие, выполняемое при возникновении исключения отличного от <see cref="OperationCanceledException"/>. Если задано значение <see langword="null"/>, то исключения не перехватываются.</param>
        /// <param name="isReverse">Значение <see langword="true"/>, если список действий должен быть обработан в обратном порядке, иначе - <see langword="false"/>.</param>
        /// <returns>Асинхронная задача.</returns>
        /// <exception cref="OperationCanceledException"><inheritdoc cref="OperationCanceledException" path="/summary"/></exception>
        public static async Task SafeExecuteAllActionsAsync(
            IList<ITestAction> testActions,
            Action<Exception> safeRecordAction = null,
            bool isReverse = false)
        {
            Check.ArgumentNotNull(testActions, nameof(testActions));

            if (isReverse)
            {
                for (var i = testActions.Count - 1; i >= 0; i--)
                {
                    var testAction = testActions[i];
                    await SafeExecuteActionAsync(testAction, safeRecordAction);
                }
            }
            else
            {
                for (var i = 0; i < testActions.Count; i++)
                {
                    var testAction = testActions[i];
                    await SafeExecuteActionAsync(testAction, safeRecordAction);
                }
            }
        }

        /// <summary>
        /// Выполняет <paramref name="testAction"/> и обрабатывает исключения.
        /// </summary>
        /// <param name="testAction">Выполняемое действие.</param>
        /// <param name="safeRecordAction">Действие, выполняемое при возникновении исключения отличного от <see cref="OperationCanceledException"/>. Если задано значение <see langword="null"/>, то исключения не перехватываются.</param>
        /// <returns>Асинхронная задача.</returns>
        /// <exception cref="OperationCanceledException"><inheritdoc cref="OperationCanceledException" path="/summary"/></exception>
        public static Task SafeExecuteActionAsync(
            ITestAction testAction,
            Action<Exception> safeRecordAction = null) =>
            SafeExecuteAsync(() => testAction.ExecuteAsync().AsTask(), safeRecordAction);

        /// <summary>
        /// Выполняет <paramref name="funcAsync"/> и обрабатывает исключения.
        /// </summary>
        /// <param name="funcAsync">Выполняемый метод.</param>
        /// <param name="safeRecordAction">Действие, выполняемое при возникновении исключения отличного от <see cref="OperationCanceledException"/>. Если задано значение <see langword="null"/>, то исключения не перехватываются.</param>
        /// <returns>Асинхронная задача.</returns>
        /// <exception cref="OperationCanceledException"><inheritdoc cref="OperationCanceledException" path="/summary"/></exception>
        public static async Task SafeExecuteAsync(
            Func<Task> funcAsync,
            Action<Exception> safeRecordAction = null)
        {
            try
            {
                await funcAsync();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e) when (safeRecordAction is not null)
            {
                safeRecordAction(e);
            }
        }

        #endregion
    }
}
