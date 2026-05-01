using Tessa.Platform.Data;
using Tessa.Roles;

namespace Tessa.Test.Default.Shared.Roles
{
    /// <summary>
    /// Фабрика для создания объектов <see cref="IRoleRepository"/> для управления
    /// ролевой моделью с помощью базы данных SQL Server. Используется в юнит-тестах.
    /// </summary>
    /// <remarks>
    /// Используется по умолчанию при задании типа <see cref="IRoleRepositoryFactory"/> в атрибутах.
    /// </remarks>
    public class DefaultRoleRepositoryFactory :
        IRoleRepositoryFactory
    {
        #region IRoleRepositoryFactory Members

        /// <summary>
        /// Создаёт экземпляр класса <see cref="IRoleRepository"/> для управления
        /// ролевой моделью с помощью базы данных SQL Server для заданного <see cref="DbManager"/>.
        /// </summary>
        /// <param name="dbScope">Область видимости объекта <see cref="DbManager"/>.</param>
        /// <returns>Созданный экземпляр класса <see cref="IRoleRepository"/>.</returns>
        public IRoleRepository Create(IDbScope dbScope) => new RoleRepository(dbScope);

        #endregion
    }
}
