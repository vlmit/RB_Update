using System;
using Tessa.Platform.Data;

namespace Tessa.Test.Default.Shared
{
    /// <summary>
    /// Объект для взаимодействия с базой данные.
    /// Всегда использует только одно подключение. Метод <see cref="CreateNew"/> и его перегрузки не создают новое подключение.
    /// Определяет область видимости объекта <see cref="DbManager"/>.
    /// </summary>
    /// <remarks>
    /// Все открытые методы и свойства класса являются потокобезопасными.
    /// </remarks>
    public sealed class SingleConnectionDbScope : DbScope
    {
        public SingleConnectionDbScope(Func<DbManager> dbFactory) : base(dbFactory) { }

        /// <summary>
        /// Создаёт новый экземпляр области видимости, при вызове метода <see cref="IDisposable.Dispose" /> которой освобождается объект <see cref="DbScope.Db" />, если для него не создана другая область видимости.
        /// </summary>
        /// <returns>Новый экземпляр области видимости объекта <see cref="DbScope.Db" />.</returns>
        public override IDbScopeInstance CreateNew() => this.Create();

        /// <summary>
        /// Создаёт новый экземпляр области видимости, при вызове метода <see cref="IDisposable.Dispose" /> которой освобождается объект <see cref="DbScope.Db" />, если для него не создана другая область видимости.
        /// </summary>
        /// <param name="configurationString">Не используется.</param>
        /// <returns>Новый экземпляр области видимости объекта <see cref="DbScope.Db" />.</returns>
        public override IDbScopeInstance CreateNew(string configurationString) => this.Create();

        /// <summary>
        /// Создаёт новый экземпляр области видимости, при вызове метода <see cref="IDisposable.Dispose" /> которой освобождается объект <see cref="DbScope.Db" />, если для него не создана другая область видимости.
        /// </summary>
        /// <param name="dbFactory">Не используется.</param>
        /// <param name="executorFactory">Не используется.</param>
        /// <returns>Новый экземпляр области видимости объекта <see cref="DbScope.Db" />.</returns>
        public override IDbScopeInstance CreateNew(Func<DbManager> dbFactory, Func<DbManager, IQueryExecutor> executorFactory = null) => this.Create();
    }
}
