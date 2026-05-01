using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessa.Extensions.Chronos.SSTUIntegration.Models;

namespace Tessa.Extensions.Chronos.SSTUIntegration.Helpers
{
    public enum RequestFormat
    {
        /// <summary>
        /// Электронный. Обращение поступило от иного органа в текущий по каналу МЭДО.
        /// </summary>
        Electronic,

        /// <summary>
        /// Другое. Обращение поступило от иного органа по другим каналам или напрямую от заявителя.
        /// </summary>
        Other
    }

    public enum QuestionStatus
    {
        NotReceived,
        NotRegistered,
        InWork,
        InWorkExtended,
        LeftWithoutAnswer,
        Explained,
        Supported,
        Answered,
        NotSupported,
        Transferred
    }
}
