using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tessa.Test.Default.Shared
{
    /// <summary>
    /// Объект, предоставляющий имена для временных ресурсов.
    /// </summary>
    public interface ITestNameResolver
    {
        /// <summary>
        /// Возвращает имя ресурса, полученное для указанного типа класса, содержащего тесты.
        /// </summary>
        /// <param name="type">Тип, для которого должно быть получено имя ресурса. Может быть не задан.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Имя ресурса.</returns>
        ValueTask<string> GetFixtureNameAsync(Type type, CancellationToken cancellationToken = default);

        /// <summary>
        /// Возвращает значение параметра FixtureDate или текущую дату и время, если параметр не задан в конфигурационном файле.
        /// </summary>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Значение параметра FixtureDate или текущая дата и время, если параметр не задан в конфигурационном файле.</returns>
        ValueTask<DateTime> GetFixtureDateTimeAsync(CancellationToken cancellationToken = default);
    }
}
