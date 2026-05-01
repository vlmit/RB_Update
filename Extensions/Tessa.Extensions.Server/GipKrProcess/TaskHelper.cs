using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Tessa.BusinessCalendar;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Server.Workflow.KrCompilers.UserAPI;
using Tessa.Extensions.Default.Server.Workflow.KrProcess.Workflow.Handlers;
using Tessa.Files;
using Tessa.Localization;
using Tessa.Notices;
using Tessa.Platform.Data;
using Tessa.Platform.EDS;
using Tessa.Platform.IO;
using Tessa.Platform.Placeholders.Extensions;
using Tessa.Platform.Runtime;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using Tessa.Roles;
using Unity;

namespace Tessa.Extensions.Server.GipKrProcess
{
    public static class TaskHelper
    {
        public static (IEDSCertificate certificate, string errorText) DecodeCertificateFromSignature(
            byte[] encodedSignature,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cms = new SignedCms();
                try
                {
                    // Декодируем подпись
                    cms.Decode(encodedSignature);
                }
                catch (CryptographicException e)
                {
                    var errorText = e.Message.Trim();
                    return (null, errorText);
                }
                var certEnum = cms.Certificates.GetEnumerator();
                certEnum.MoveNext();
                var certificate = certEnum.Current;
                return (new EDSCertificate(certificate), null);
            }
            catch (Exception e)
            {
                return (null, e.Message.Trim());
            }
        }
    }
}
