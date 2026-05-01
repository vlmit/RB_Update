using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tessa.Extensions.Default.Server.Workflow.KrPermissions
{
    /// <summary>
    /// Объект для получения блокировок на чтение и записи правил доступа
    /// </summary>
    public interface IKrPermissionsLockStrategy
    {
        /// <summary>
        /// Метод для получения блокировки на чтение правил доступа. Возвращает null, если блокировку получить не удалось.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены асинхронной операции</param>
        /// <returns>
        /// Возвращает объект блокировки, или null, если блокировку не удалось получить.
        /// Вызов метода <see cref="IDisposable.Dispose"/> возвращаемого объекта снимет блокировку.
        /// </returns>
        Task<IAsyncDisposable> TryObtainReaderLockAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Метод для получения блокировки на запись правил доступа. Возвращает null, если блокировку получить не удалось.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены асинхронной операции</param>
        /// <returns>
        /// Возвращает объект блокировки, или null, если блокировку не удалось получить.
        /// Вызов метода <see cref="IDisposable.Dispose"/> возвращаемого объекта снимет блокировку.
        /// </returns>
        Task<IAsyncDisposable> TryObtainWriterLockAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Метод для сброса всех блокировок.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены асинхронной операции</param>
        /// <returns>Асинхронная задача</returns>
        Task ClearLocksAsync(CancellationToken cancellationToken = default);
    }
}
