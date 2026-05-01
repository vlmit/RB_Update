using Tessa.Platform.Data;
using Tessa.Roles;

namespace Tessa.Test.Default.Shared.Roles
{
    /// <summary>
    /// Фабрика для создания объектов <see cref="IRoleRepository"/>. Используется в юнит-тестах.
    /// </summary>
    public interface IRoleRepositoryFactory
    {
        /// <summary>
        /// Создаёт экземпляр класса <see cref="IRoleRepository"/> для заданного <see cref="DbManager"/>.
        /// </summary>
        /// <param name="dbScope">Область видимости объекта <see cref="DbManager"/>.</param>
        /// <returns>Созданный экземпляр класса <see cref="IRoleRepository"/>.</returns>
        IRoleRepository Create(IDbScope dbScope);
    }
}
