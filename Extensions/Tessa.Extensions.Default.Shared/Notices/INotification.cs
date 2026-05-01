using System.Collections.Generic;

namespace Tessa.Extensions.Default.Shared.Notices
{
    public interface INotification
    {
        /// <summary>
        /// Тело письма. Если значение отлично от <c>null</c>,
        /// то оно будет использовано в качестве тела письма,
        /// в противном случае используется стандартное тело.
        /// </summary>
        string Body { get; }

        void SerializeTo(IDictionary<string, object> storage);
    }
}
