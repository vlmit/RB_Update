using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Extensions.Chronos.SEDMigration.Helpers;
using Tessa.Files;
using Tessa.Platform.EDS;

namespace Tessa.Extensions.Chronos.Helpers.Sign
{
    public static class SignFile
    {
        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        public static async Task<bool> SignFileAsync(
            byte[] content,
            DateTime? signed,
            string comment,
            IFileVersion toSignVersion,
            ICAdESManager cadesManager,
            CancellationToken cancellationToken = default)
        {
            //logger.Info($"beforeSign {path}");

            byte[] signatureBytes = content;

            var (certificate, errorText) = cadesManager.DecodeCertificateFromSignature(signatureBytes);

            if (certificate is null)
            {
                logger.Error($"Certificate is null, errorText: {errorText}");
                return false;
            }

            SignatureType signatureType = SignatureType.CAdES;
            SignatureProfile signatureProfile = SignatureProfile.BES;

            try
            {
                var validationInfos = await cadesManager.CheckExtendedSignatureAsync(new SignedData
                {
                    Signature = signatureBytes,
                    SignatureType = signatureType,
                    SignatureProfile = signatureProfile,
                }, cancellationToken);
                var firstValInfo = validationInfos?.FirstOrDefault();
                signatureType = firstValInfo?.ReachedSignatureType ?? SignatureType.CAdES;
                signatureProfile = firstValInfo?.ReachedSignatureProfile ?? SignatureProfile.BES;
            }
            catch (OperationCanceledException)
            {
                logger.Error($"OperationCanceledException");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error($"Can't parse certificate from file , {ex}");
                throw new InvalidOperationException($"Can't parse certificate from file \"\" stacktrace {ex.StackTrace} message {ex.Message}", ex);
            }

            IFileSignatureCreationToken signatureToken = await toSignVersion.Source.GetSignatureCreationTokenAsync(cancellationToken).ConfigureAwait(false);
            signatureToken.Comment = comment;
            signatureToken.EventType = FileSignatureEventType.Imported;
            signatureToken.Company = certificate.Company;
            signatureToken.SubjectName = certificate.SubjectName;
            signatureToken.SerialNumber = certificate.SerialNumber;
            signatureToken.IssuerName = certificate.IssuerName;
            signatureToken.Data = signatureBytes;
            signatureToken.SignatureType = signatureType;
            signatureToken.SignatureProfile = signatureProfile;
            signatureToken.Signed = signed;
            IFileSignature signature = await toSignVersion.Source.CreateSignatureAsync(signatureToken, toSignVersion, cancellationToken).ConfigureAwait(false);

            await toSignVersion.Signatures.AddWithNotificationAsync(signature, cancellationToken);

            return true;
        }
    }
}
