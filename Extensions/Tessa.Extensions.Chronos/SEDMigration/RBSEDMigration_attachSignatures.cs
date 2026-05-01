using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Chronos.Contracts;
using NLog;
using Tessa.Notices;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Validation;
using Unity;
using Tessa.Platform.Licensing;
using Tessa.Platform.Runtime;
using System.IO;
using Tessa.Cards;
using System.Text;
using Tessa.Extensions.Default.Shared;
using Tessa.Files;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using System.Linq;
using Tessa.Cards.Workflow;
using Tessa.Extensions.Default.Shared.Workflow.Wf;
using System.Globalization;
using Tessa.Extensions.Chronos.Helpers;
using System.Runtime.CompilerServices;
using NLog.Fluent;
using DocumentFormat.OpenXml.Office2010.Excel;
using Tessa.Platform.EDS;
using DocumentFormat.OpenXml.Bibliography;
using Tessa.Roles;
using Tessa.Extensions.Chronos.SEDMigration.Helpers;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Tessa.Extensions.Chronos.Plugins
{
    /// <summary>
    /// Пример плагина, который может работать через серверное API.
    /// </summary>
    [Plugin(
        Name = "RBSEDMigration_attachSignatures",
        Description = "Плагин добавления подписей к файлам",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class RBSEDMigration_attachSignatures :
        Plugin
    {
        #region Constants

        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RBSEDMigration_attachSignatures.xml";

        private const string inputPath = "/home/tessa/tessa/share/Migration/EDSFiles/";

        //введено
        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Base Overrides

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("Starting plugin RBSEDMigration_attachSignatures");

            // конфигурируем контейнер Unity для использования стандартных серверных API (в т.ч. API карточек)
            // а также для получения прямого доступа к базе данных через IDbScope по строке подключения из app.config;
            // предполагаем, что все действия, совершаемые плагином, будут выполняться от имени пользователя System
            //logger.Trace("Configuring container");

            // настраиваем контейнер Unity для работы с карточками
            await TessaPlatform.InitializeFromConfigurationAsync();

            IUnityContainer container = await new UnityContainer()
                .RegisterServerForPluginAsync()
                ;

            ICardRepository cardRepository = container.Resolve<ICardRepository>();
            ICardServerPermissionsProvider permissionsProvider = container.Resolve<ICardServerPermissionsProvider>();
            ICardFileManager fileManager = container.Resolve<ICardFileManager>();
            ICAdESManager cadesManager = container.Resolve<ICAdESManager>();

            var mainDir = new DirectoryInfo(inputPath);

            int cardsNum = 0;

            if (!mainDir.Exists)
            {
                logger.Error("Main dir for input not exists by path: " + mainDir.FullName);
                return;
            }
            else
            {
                cardsNum = mainDir.GetDirectories().Length;
                logger.Info($"To sign {cardsNum} cards");
            }

            //var inputData = RBSEDMigration_ParseFileSignatures.GetCardsToSign(inputPath);

            var ouputPath = Path.Combine(inputPath, "OUT");

            if (!Directory.Exists(ouputPath))
            {
                Directory.CreateDirectory(ouputPath);
            }

            var okPath = Path.Combine(ouputPath, "OK");

            if (!Directory.Exists(okPath))
            {
                Directory.CreateDirectory(okPath);
            }

            var errorPath = Path.Combine(ouputPath, "ERROR");

            if (!Directory.Exists(errorPath))
            {
                Directory.CreateDirectory(errorPath);
            }

            var dirsToSign = mainDir.GetDirectories();

            string lastCardName = "";
            int lastCardStatus = 0;
            int cardsCount = 0;

            foreach (var dir in dirsToSign)
            {
                try
                {
                    IDbScope dbScope = container.Resolve<IDbScope>();

                    if (!string.IsNullOrWhiteSpace(lastCardName) && lastCardStatus != -1)
                    {
                        DirMove(lastCardName, lastCardStatus, inputPath, ouputPath);
                    }

                    SEDMigrationCardsToSign c = new SEDMigrationCardsToSign();

                    //DirectoryInfo directoryInfo = new DirectoryInfo(dir);

                    c.SPUID = dir.Name;
                    c.files = RBSEDMigration_ParseFileSignatures.GetFileToSign(dir.FullName);

                    lastCardName = c.SPUID;
                    lastCardStatus = 0;

                    // Получаем карточку по extGUID
                    var cardId = await GetCardIDAsync(c.SPUID, dbScope);

                    if (cardId == null)
                    {
                        logger.Info("Card with UID: " + c.SPUID + " not exist");
                        continue;
                    }

                    cardsCount++;

                    logger.Info($"Processing {cardsCount} of {cardsNum} {c.SPUID}");

                    var cardGetRequest = new CardGetRequest
                    {
                        CardID = cardId
                    };

                    permissionsProvider.SetFullPermissions(cardGetRequest);
                    var cardGetResponse = await cardRepository.GetAsync(cardGetRequest, cancellationToken);

                    if (!cardGetResponse.ValidationResult.IsSuccessful())
                    {
                        logger.Error("GetCardError\n" + cardGetResponse.ValidationResult.Build());
                        continue;
                    }

                    var card = cardGetResponse.Card;

                    // Получаем файлы прикрепленные к карточке
                    var cardFiles = card.TryGetFiles();

                    // Если файлов нет, переход к следующему
                    if (cardFiles == null)
                    {
                        logger.Info($"No files in card UID: {card.ID} external UID: {c.SPUID}");
                        continue;
                    }

                    logger.Info($"Card found {card.ID}");

                    await using (var fileContainer = await fileManager.CreateContainerAsync(card))
                    {
                        if (fileContainer == null)
                        {
                            logger.Info($"FileConatiner is null in card: {card.ID}");
                            continue;
                        }
                        if (fileContainer.FileContainer.Files.Count < 1)
                        {
                            logger.Info($"No files in card: {card.ID}");
                            continue;
                        }

                        var filesCard = fileContainer.FileContainer.Files;

                        foreach (var f in c.files)
                        {
                            var searchFileInCard = filesCard.AsParallel().Select(x => x).Where(x => x.Name == f.name).FirstOrDefault();

                            logger.Info($"Found file {searchFileInCard.Name}");

                            if (searchFileInCard != null)
                            {
                                int count = 1;
                                foreach (var s in f.signs)
                                {
                                    logger.Info($"Sign {count} of {f.signs.Count}");

                                    string signFileName = Path.Combine(inputPath, c.SPUID, Guid.NewGuid() + f.name + ".sig");

                                    var contentElements = s.content.Split(' ');

                                    byte[] content = new byte[contentElements.Length];

                                    for (int i = 0; i < contentElements.Length; i++)
                                    {
                                        content[i] = byte.Parse(contentElements[i]);
                                    }

                                    await System.IO.File.WriteAllBytesAsync(signFileName, content, cancellationToken);

                                    bool resSign = await SignFile(signFileName, s.signType, s.signed, searchFileInCard.TryGetActualVersion(), cadesManager, cancellationToken);

                                    if (resSign)
                                    {
                                        lastCardStatus = 1;
                                    }
                                    count++;
                                }
                            }
                        }

                        // сохраняем карточку с файлами
                        var storeResponse = await fileContainer.StoreAsync();
                        if (!storeResponse.ValidationResult.IsSuccessful())
                        {
                            ValidationResult result = storeResponse.ValidationResult.Build();
                            logger.LogResult(result); // пишем ошибку или другие сообщения в лог
                            continue;
                        }
                        else
                        {
                            //logger.Info("Signature attached succesfully");
                        }
                    }

                    lastCardStatus = 1;

                    logger.Info($"Files signs in card {card.ID} number {card.Sections["DocumentCommonInfo"].Fields["FullNumber"]} updated successfully");
                }
                catch (Exception ex)
                {
                    await System.IO.File.AppendAllTextAsync(Path.Combine(ouputPath, "failure.txt"), $"\n{DateTime.Now} \n{dir.Name} \n{ex.Message}\n{ex.StackTrace}");

                    //lastCardName = "";
                    //lastCardStatus = 0;
                    logger.LogException(ex);

                    continue;
                }
            }

            if (cardsCount > 0)
            {
                DirMove(lastCardName, lastCardStatus, inputPath, ouputPath);
            }

            logger.Info("Shutting down RBSEDMigration_attachSignatures");
        }

        private bool DirMove(string name, int status, string inputS, string outputS)
        {
            try
            {
                string target = string.Empty;
                switch(status)
                {
                    case 0:
                        target = Path.Combine(outputS, "ERROR", name);
                        break;
                    case 1:
                        target = Path.Combine(outputS, "OK", name);
                        break;
                    default:
                        return false;
                }

                Directory.Move(Path.Combine(inputS, name), Path.Combine(outputS, "OK", name));
                return true;
            }
            catch
            {
                logger.Error($"{name} not moved to output");
                return false;
            }
        }

        private async Task<bool> SignFile(
            string path, 
            FileSignType signType, 
            DateTime? signed, 
            IFileVersion toSignVersion, 
            ICAdESManager cadesManager, 
            CancellationToken cancellationToken)
        {
            logger.Info($"beforeSign {path}");

            byte[] signatureBytes = await cadesManager.GetSignatureBytesFromFileAsync(path, cancellationToken);

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
            signatureToken.Comment = signType.GetDescription();
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

        private async Task<Guid?> GetCardIDAsync(string externalGuid, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("dci", "ID")
                            .From("DocumentCommonInfo", "dci").NoLock()
                            .Where().C("dci", "ExternalGuid").Equals().P("externalGuid")
                            .Limit(1).Build(),
                        db.Parameter("externalGuid", externalGuid))
                    .LogCommand()
                    .ExecuteAsync<Guid?>();

                return result;
            }
        }

        #endregion
    }
}
