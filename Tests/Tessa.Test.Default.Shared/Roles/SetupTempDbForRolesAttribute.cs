using System;
using System.Threading.Tasks;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using Tessa.Platform.Data;
using Tessa.Roles;

namespace Tessa.Test.Default.Shared.Roles
{
    /// <summary>
    /// <para>Создаёт временную базу данных, выполняет на ней заданный скрипт и устанавливает
    /// свойства на объекте <see cref="IRoleRepositoryContainer"/>, являющемся TestFixture,
    /// с помощью фабрик указанных типов <see cref="IRoleRepositoryFactory"/> и <see cref="IDbFactory"/>
    /// таким образом, чтобы объекты <see cref="IRoleRepository"/> и <see cref="DbManager"/>
    /// ссылались на временную базу данных.</para>
    /// <para>Затем выполняются все тесты в TestFixture, которые могут считывать и изменять
    /// временную базу данных. После завершения тестов временная база данных удаляется.</para>
    /// </summary>
    /// <remarks>
    /// <para>Атрибут должен быть применён только на класс с тестом при условии,
    /// что соответствующий TestFixture (класс) реализует интерфейсы: <see cref="IDbScopeContainer"/>, <see cref="ITestActionsContainer"/>, <see cref="IFixtureNameProvider"/> и <see cref="IRoleRepositoryContainer"/>.
    /// В противном случае перед выполнением теста будет сгенерировано исключение <see cref="InvalidOperationException"/>.</para>
    /// <para>Фабрика, доступная через свойство <see cref="IDbScopeContainer.DbFactory"/> на объекте
    /// <see cref="IDbScopeContainer"/>, создаёт объекты <see cref="DbManager"/>, указывающие
    /// на временную базу данных.</para>
    /// </remarks>
    public class SetupTempDbForRolesAttribute :
        SetupTempDbAttribute
    {
        #region Constructors

        /// <summary>
        /// Создаёт экземпляр атрибута с указанием имени скрипта, выполняемого на временной базе данных.
        /// </summary>
        /// <param name="testScriptFileNames">
        /// Имена SQL-скриптов, добавленных в ресурсы сборки (Build Action = Embedded Resource) и выполняемых в заданном порядке.
        /// Скрипты должны помещаться в папку <c>Sql</c> проекта содержащего класс к которому применён данный атрибут.
        /// Пример: <c>"Default.sql"</c>, <c>"SubFolder/Cards.sql"</c>.
        /// </param>
        public SetupTempDbForRolesAttribute(params string[] testScriptFileNames)
            : base(testScriptFileNames) =>
            this.roleRepositoryFactoryType = typeof(DefaultRoleRepositoryFactory);

        #endregion

        #region Properties

        private Type roleRepositoryFactoryType;

        /// <summary>
        /// Тип фабрики для создания объектов <see cref="IRoleRepository"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"> Задаваемое значение равно <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// Задаваемое значение является типом, не реализующим интерфейс <see cref="IRoleRepositoryFactory"/>.
        /// </exception>
        public Type RoleRepositoryFactoryType
        {
            get => this.roleRepositoryFactoryType;
            set
            {
                if (this.roleRepositoryFactoryType != value)
                {
                    TestHelper.CheckTypeOf<IRoleRepositoryFactory>(value, nameof(this.roleRepositoryFactoryType));
                    this.roleRepositoryFactoryType = value;
                }
            }
        }

        #endregion

        #region Protected Declarations

        private IRoleRepositoryFactory roleRepositoryFactory;

        /// <summary>
        /// Экземпляр фабрики, используемой для заполнения свойств объекта <see cref="IRoleRepositoryFactory"/>.
        /// </summary>
        /// <remarks>
        /// Значение свойства может быть получено через отложенную инициализацию.
        /// </remarks>
        protected IRoleRepositoryFactory RoleRepositoryFactory =>
            TestHelper.InitValue(this.roleRepositoryFactoryType, ref this.roleRepositoryFactory);

        #endregion

        #region Base Overrides

        /// <summary>
        /// Метод, выполняемый перед запуском теста.
        /// </summary>
        /// <param name="test">Информация о тесте.</param>
        public override void BeforeTest(ITest test)
        {
            base.BeforeTest(test);

            var testActions = test.Get<ITestActionsContainer>();
            testActions.GetTestActions(ActionStage.BeforeInitialize).Add(new TestAction(this, BeforeInitializeAsync));
            testActions.GetTestActions(ActionStage.AfterOneTimeTearDown).Add(new TestAction(this, AfterOneTimeTearDownActionAsync));
        }

        #endregion

        #region Private Methods

        private static ValueTask BeforeInitializeAsync(object sender)
        {
            var instance = (SetupTempDbForRolesAttribute) sender;
            var test = TestExecutionContext.CurrentContext.CurrentTest;

            var container = test.Get<IRoleRepositoryContainer>();
            if (container.RoleRepository is null)
            {
                var repository = instance.RoleRepositoryFactory.Create(container.DbScope);
                container.RoleRepository = repository;
            }

            return new ValueTask();
        }

        private static ValueTask AfterOneTimeTearDownActionAsync(object sender)
        {
            var test = TestExecutionContext.CurrentContext.CurrentTest;
            var container = test.Get<IRoleRepositoryContainer>();

            container.RoleRepository = null;

            return new ValueTask();
        }

        #endregion
    }
}
