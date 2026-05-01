using Tessa.Platform.Data;
using Tessa.Roles;

namespace Tessa.Test.Default.Shared.Roles
{
    /// <summary>
    /// Контейнер для объектов <see cref="IRoleRepository"/> и <see cref="DbManager"/>.
    /// 
    /// Интерфейс должен быть реализован на TestFixture для успешного применения атрибута
    /// <see cref="SetupTempDbForRolesAttribute"/>, который автоматически заполнит свойства для каждого теста
    /// с помощью фабрик <see cref="IRoleRepositoryFactory"/> и <see cref="IDbFactory"/>.
    /// </summary>
    public interface IRoleRepositoryContainer :
        IDbScopeContainer
    {
        /// <summary>
        /// Репозиторий для управления ролевой моделью,
        /// заполненный с помощью фабрики <see cref="IRoleRepositoryFactory"/>.
        /// </summary>
        IRoleRepository RoleRepository { get; set; }
    }
}
