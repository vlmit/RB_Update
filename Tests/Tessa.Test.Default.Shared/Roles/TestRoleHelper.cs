using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Platform;
using Tessa.Platform.Runtime;
using Tessa.Roles;
using Tessa.Test.Default.Shared.Kr;

namespace Tessa.Test.Default.Shared.Roles
{
    /// <summary>
    /// Предоставляет статические вспомогательные методы для работы с ролями в тестах.
    /// </summary>
    public static class TestRoleHelper
    {
        #region Public Methods

        /// <summary>
        /// Добавляет пользователя со случайным идентификатором и именем.
        /// </summary>
        /// <param name="roleRepository">Репозиторий для управления ролевой моделью.</param>
        /// <param name="modifyAction">Функция используемая для изменения создаваемого пользователя.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Созданный пользователь.</returns>
        public static Task<PersonalRole> CreateUserAsync(
            IRoleRepository roleRepository,
            Action<PersonalRole> modifyAction = null,
            CancellationToken cancellationToken = default)
        {
            var roleID = Guid.NewGuid();
            var roleName = "User__" + TestHelper.GetPseudoRandomNumber().ToString();

            return CreateUserAsync(roleRepository, roleID, roleName, modifyAction, cancellationToken);
        }

        /// <summary>
        /// Добавляет в базу сотрудника с указанным идентификатором и именем.
        /// </summary>
        /// <param name="roleRepository">Репозиторий для управления ролевой моделью.</param>
        /// <param name="roleID">Идентификатор пользователя.</param>
        /// <param name="roleName">Имя пользователя.</param>
        /// <param name="modifyAction">Функция используемая для изменения создаваемого пользователя.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Созданный пользователь.</returns>
        public static async Task<PersonalRole> CreateUserAsync(
            IRoleRepository roleRepository,
            Guid roleID,
            string roleName,
            Action<PersonalRole> modifyAction = null,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(roleRepository, nameof(roleRepository));

            var modifiedBy = await roleRepository.GetPersonalRoleAsync(TestHelper.AdminUserID, cancellationToken);

            var role = new PersonalRole
            {
                ID = roleID,
                Name = roleName,
                Modified = DateTime.UtcNow,
                ModifiedBy = modifiedBy,
                Phone = "123",
                FullName = roleName,
                FirstName = roleName,
            };

            modifyAction?.Invoke(role);

            role.Users = new List<RoleUserRecord>
            {
                new RoleUserRecord
                {
                    RowID = Guid.NewGuid(),
                    ID = role.ID,
                    IsDeputy = false,
                    User = role,
                    UserID = role.ID,
                    UserName = role.Name,
                    Role = role,
                    RoleType = RoleType.Personal
                }
            };
            role.UpdateFromAssociations();

            await roleRepository.InsertAsync(role, cancellationToken);
            return role;
        }

        /// <summary>
        /// Добавляет пользователя со случайным идентификатором и именем.
        /// </summary>
        /// <param name="deps">Зависимости, используемые объектами, управляющими жизненным циклом карточек.</param>
        /// <param name="modifyAction">Функция используемая для изменения создаваемого пользователя.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Созданный пользователь.</returns>
        public static Task<PersonalRoleBuilder> CreateUserAsync(
            ICardLifecycleCompanionDependencies deps,
            Action<PersonalRoleBuilder> modifyAction = null,
            CancellationToken cancellationToken = default)
        {
            var roleID = Guid.NewGuid();
            var roleName = "User__" + TestHelper.GetPseudoRandomNumber().ToString();

            return CreateUserAsync(deps, roleID, roleName, modifyAction, cancellationToken);
        }

        /// <summary>
        /// Добавляет в базу сотрудника с указанным идентификатором и именем.
        /// </summary>
        /// <param name="deps">Зависимости, используемые объектами, управляющими жизненным циклом карточек.</param>
        /// <param name="roleID">Идентификатор пользователя.</param>
        /// <param name="roleName">Имя пользователя.</param>
        /// <param name="modifyAction">Функция используемая для изменения создаваемого пользователя.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Созданный пользователь.</returns>
        /// <remarks>
        /// Заполняет e-mail. Тип входа: <see cref="UserLoginType.Tessa"/>. Логин и пароль соответствуют имени пользователя.<para/>
        /// Для создания сотрудника с особыми параметрами используйте <see cref="PersonalRoleBuilder"/>.
        /// </remarks>
        public static async Task<PersonalRoleBuilder> CreateUserAsync(
            ICardLifecycleCompanionDependencies deps,
            Guid roleID,
            string roleName,
            Action<PersonalRoleBuilder> modifyAction = null,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(deps, nameof(deps));

            var clc = new PersonalRoleBuilder(roleID, deps)
                .Create()
                .SetName(roleName)
                .SetFullName(roleName)
                .SetFirstName(roleName)
                .SetEmail($"{roleName}@undef.undef")
                .SetLoginType(UserLoginType.Tessa)
                .SetAccount(roleName)
                .SetPassword(roleName);

            modifyAction?.Invoke(clc);

            return await clc
                .Save()
                .GoAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Добавляет в указанную роль пользователя, если она имеет пустой состав.
        /// </summary>
        /// <param name="roleRepository">Репозиторий для управления ролевой моделью.</param>
        /// <param name="role">Роль.</param>
        /// <param name="modifyAction">Функция используемая для изменения добавляемого пользователя.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        public static Task TryAddUserIfEmptyAsync(
            IRoleRepository roleRepository,
            Role role,
            Action<PersonalRole> modifyAction = null,
            CancellationToken cancellationToken = default)
        {
            return TryAddUserIfEmptyAsync(
                roleRepository,
                role,
                async ct => await CreateUserAsync(roleRepository, modifyAction, ct),
                cancellationToken);
        }

        /// <summary>
        /// Добавляет в указанную роль пользователя, если она имеет пустой состав.
        /// </summary>
        /// <param name="roleRepository">Репозиторий для управления ролевой моделью.</param>
        /// <param name="role">Роль.</param>
        /// <param name="getUserAsync">Функция, возвращающая добавляемого пользователя.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        public static async Task TryAddUserIfEmptyAsync(
            IRoleRepository roleRepository,
            Role role,
            Func<CancellationToken, ValueTask<IRoleUser>> getUserAsync,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(roleRepository, nameof(roleRepository));
            Check.ArgumentNotNull(role, nameof(role));
            Check.ArgumentNotNull(getUserAsync, nameof(getUserAsync));

            role.Users = await roleRepository.GetUsersAsync(role.ID, cancellationToken);

            if (role.Users.Count > 0)
            {
                return;
            }

            var roleUser = await getUserAsync(cancellationToken);

            var roleUserRecord = await AddUserAsync(
                roleRepository,
                role,
                roleUser,
                cancellationToken);

            role.Users = new List<RoleUserRecord>() { roleUserRecord };
        }

        /// <summary>
        /// Добавляет в указанную роль заданного пользователя.
        /// </summary>
        /// <param name="roleRepository">Репозиторий для управления ролевой моделью.</param>
        /// <param name="role">Роль.</param>
        /// <param name="roleUser">Добавляемый пользователь.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Добавленная запись о составе роли.</returns>
        public static async Task<RoleUserRecord> AddUserAsync(
            IRoleRepository roleRepository,
            Role role,
            IRoleUser roleUser,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(roleRepository, nameof(roleRepository));
            Check.ArgumentNotNull(role, nameof(role));
            Check.ArgumentNotNull(roleUser, nameof(roleUser));

            var roleUserRecord = new RoleUserRecord
            {
                RowID = Guid.NewGuid(),
                ID = role.ID,
                IsDeputy = false,
                User = roleUser,
                Role = role
            };
            roleUserRecord.UpdateFromAssociations();

            await roleRepository.InsertAsync(roleUserRecord, cancellationToken);
            return roleUserRecord;
        }

        #endregion
    }
}
