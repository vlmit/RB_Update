using System;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Platform.Runtime;
using Tessa.Platform.Storage;

namespace Tessa.Test.Default.Shared.Platform.ActionHistory
{
    /// <summary>
    /// Стратегия работы с историей действий, которая ничего не выполняет.
    /// За счет неё снижается нагрузка на базу данных при запуске тестов.
    /// </summary>
    public sealed class FakeActionHistoryStrategy :
        IActionHistoryStrategy
    {
        public Task<Guid> InsertAsync(
            ActionType actionType,
            Guid cardID,
            Guid cardTypeID,
            string cardTypeCaption,
            string digest,
            IStorageObjectProvider request,
            IUser user,
            DateTime modified,
            Guid? sessionID = null,
            Guid? rowID = null,
            Guid? applicationID = null,
            CancellationToken cancellationToken = default)
        {
            rowID = Guid.NewGuid();
            return Task.FromResult(rowID.Value);
        }

        public Task DeleteAsync(Guid cardID, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<ActionHistoryRecord> TryGetAsync(Guid rowID, CancellationToken cancellationToken) =>
            Task.FromResult(default(ActionHistoryRecord));
    }
}
