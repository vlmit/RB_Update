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

namespace Tessa.Extensions.Chronos.Plugins
{
    /// <summary>
    /// Пример плагина, который может работать через серверное API.
    /// </summary>
    [Plugin(
        Name = "RBSED_AttachSignatures",
        Description = "Плагин добавления подписей к файлам",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class RBSED_AttachSignatures :
        Plugin
    {
        #region Constants

        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RBSED_AttachSignatures.xml";

        //введено
        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Base Overrides

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("Starting plugin RBSED_AttachSignatures");

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

            var mainDir = new DirectoryInfo("/home/tessa/tessa/tessa/share/AttachSignaturesTest");

            if (!mainDir.Exists)
            {
                logger.Error("Main dir for input not exists by path: " + mainDir.FullName);
                return;
            }

            var directories = mainDir.GetDirectories();            

            if (directories.Length == 0)
            {
                logger.Info("The folder is empty by path: " + mainDir.FullName);
                return;
            }

            foreach (var dir in directories)
            {
                IDbScope dbScope = container.Resolve<IDbScope>();

                // Получаем карточку по extGUID
                //var cardId = await GetCardIDAsync(dir.Name, dbScope);

                //if (cardId == null)
                //{
                //    logger.Info("Card with UID: " + dir.Name + " not exist");
                //    continue;
                //}

                //Guid cardId = Guid.Parse("d1bcb863-9f6e-46d8-b64b-7fc181bc3c1b");

                Guid cardId = Guid.Parse(dir.Name);

                var cardGetRequest = new CardGetRequest
                {
                    CardID = cardId
                };

                permissionsProvider.SetFullPermissions(cardGetRequest);
                var cardGetResponse = await cardRepository.GetAsync(cardGetRequest, cancellationToken);

                if (!cardGetResponse.ValidationResult.IsSuccessful())
                {
                    logger.Error(cardGetResponse.ValidationResult.Build());
                    return;
                }

                var card = cardGetResponse.Card;

                // Получаем файлы прикрепленные к карточке
                var cardFiles = card.TryGetFiles();
                
                // Если файлов нет, переход к следующему
                if (cardFiles == null)
                {
                    logger.Info($"No files in card UID: {card.ID} external UID: {dir.Name}");
                    continue;
                }

                //var dirFiles = dir.GetFiles();

                //var temp = cardFiles.Select(x => x.Name);


                //string fileExtension = dirFile.Extension;
                //string nameToFind = dirFile.Name;
                //nameToFind = nameToFind.Remove(nameToFind.Length - fileExtension.Length);

                //logger.Info($"nameToFind: {nameToFind}");

                //var tempFile = cardFiles.FirstOrDefault(x => x.Name == nameToFind);
                //// Добавление подписи файла
                //byte[] signatureBytes = await cadesManager.GetSignatureBytesFromFileAsync(dirFile.FullName, cancellationToken);

                //var (certificate, errorText) = cadesManager.DecodeCertificateFromSignature(signatureBytes);

                //if (certificate is null)
                //{
                //    logger.Error($"Certificate is null, errorText: {errorText}");
                //    continue;
                //}

                //SignatureType signatureType = SignatureType.CAdES;
                //SignatureProfile signatureProfile = SignatureProfile.BES;

                //try
                //{
                //    var validationInfos = await cadesManager.CheckExtendedSignatureAsync(new SignedData
                //    {
                //        Signature = signatureBytes,
                //        SignatureType = signatureType,
                //        SignatureProfile = signatureProfile,
                //    }, cancellationToken);
                //    var firstValInfo = validationInfos?.FirstOrDefault();
                //    signatureType = firstValInfo?.ReachedSignatureType ?? SignatureType.CAdES;
                //    signatureProfile = firstValInfo?.ReachedSignatureProfile ?? SignatureProfile.BES;
                //}
                //catch (OperationCanceledException)
                //{
                //    logger.Error($"OperationCanceledException");
                //    throw;
                //}
                //catch (Exception ex)
                //{
                //    //logger.Error($"Can't parse certificate from file {dirFile.FullName}, {ex}");
                //    throw new InvalidOperationException($"Can't parse certificate from file \"{dirFile.FullName}\" stacktrace {ex.StackTrace} message {ex.Message}", ex);
                //}

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

                    foreach (var dirFile in dir.GetFiles())
                    {
                        string fileExtension = dirFile.Extension;
                        string nameToFind = dirFile.Name;
                        nameToFind = nameToFind.Remove(nameToFind.Length - fileExtension.Length);

                        logger.Info($"nameToFind: {nameToFind}");

                        // Добавление подписи файла
                        byte[] signatureBytes = await cadesManager.GetSignatureBytesFromFileAsync(dirFile.FullName, cancellationToken);

                        var (certificate, errorText) = cadesManager.DecodeCertificateFromSignature(signatureBytes);

                        if (certificate is null)
                        {
                            logger.Error($"Certificate is null, errorText: {errorText}");
                            continue;
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
                            //logger.Error($"Can't parse certificate from file {dirFile.FullName}, {ex}");
                            throw new InvalidOperationException($"Can't parse certificate from file \"{dirFile.FullName}\" stacktrace {ex.StackTrace} message {ex.Message}", ex);
                        }

                        var file = fileContainer.FileContainer.Files.FirstOrDefault(x => x.Name == nameToFind);

                        logger.Info($"fileID: {file.ID} {file.Name}");

                        IFileSignatureCreationToken signatureToken = await file.TryGetActualVersion().Source.GetSignatureCreationTokenAsync(cancellationToken).ConfigureAwait(false);
                        signatureToken.Comment = string.Empty;
                        signatureToken.EventType = FileSignatureEventType.Imported;
                        signatureToken.Company = certificate.Company;
                        signatureToken.SubjectName = certificate.SubjectName;
                        signatureToken.SerialNumber = certificate.SerialNumber;
                        signatureToken.IssuerName = certificate.IssuerName;
                        signatureToken.Data = signatureBytes;
                        signatureToken.SignatureType = signatureType;
                        signatureToken.SignatureProfile = signatureProfile;
                        IFileSignature signature = await file.TryGetActualVersion().Source.CreateSignatureAsync(signatureToken, file.TryGetActualVersion(), cancellationToken).ConfigureAwait(false);
                        await file.TryGetActualVersion().Signatures.AddWithNotificationAsync(signature, cancellationToken);
                    }

                    // сохраняем карточку с файлами


                    var storeResponse = await fileContainer.StoreAsync();
                    if (!storeResponse.ValidationResult.IsSuccessful())
                    {
                        ValidationResult result = storeResponse.ValidationResult.Build();
                        logger.LogResult(result); // пишем ошибку или другие сообщения в лог
                    }
                    else
                    {
                        logger.Info("Signature attached succesfully");
                    }
                }
                                      
            }

            //FileParseCategories fileParseCategories = new FileParseCategories();

            //// прописать путь
            ////var dictCategories = fileParseCategories.GetCategoriesFromFile("/home/tessa/tessa_og/sync/testtt.txt");

            //var listCategories = fileParseCategories.GetCategories("/home/tessa/tessa_og/sync/testtt.txt");

            //foreach (var c in listCategories)
            //{
            //    IDbScope dbScope = container.Resolve<IDbScope>();

            //    var cardId = await GetCardIDAsync(c.UID.ToLower(), dbScope);

            //    if (cardId == null)
            //    {
            //        logger.Info("Card with UID: " + c.UID.ToLower() + " not exist");
            //        continue;
            //    }

            //    var cardGetRequest = new CardGetRequest
            //    {
            //        CardID = cardId
            //    };

            //    permissionsProvider.SetFullPermissions(cardGetRequest);
            //    var cardGetResponse = await cardRepository.GetAsync(cardGetRequest, cancellationToken);

            //    if (!cardGetResponse.ValidationResult.IsSuccessful())
            //    {
            //        logger.Error(cardGetResponse.ValidationResult.Build());
            //        return;
            //    }

            //    var card = cardGetResponse.Card;

            //    //RBCategory TCategory = await GetCategoryAsync(c.category, dbScope);


            //    //if (TCategory == null)
            //    //{
            //    //    logger.Error("Рубрика " + c.category + " не найдена. Карточка: " + c.UID.ToLower() + " CatNotFound");
            //    //    continue;
            //    //}

            //    //await InsertSSTUInfRKRow((Guid)cardId, Guid.NewGuid(), TCategory, dbScope);

            //  /*  CardSection newOutgoingRefDocs = card.Sections["SSTUInfRK"];

            //    CardRow row = newOutgoingRefDocs.Rows.Add();
            //    row.RowID = Guid.NewGuid();
            //    row["OTKGID"] = TCategory.ID;
            //    row["OTKGKodRub"] = TCategory.KodRub;
            //    row["OTKGName"] = TCategory.Name;
            //    row.State = CardRowState.Inserted;*/

            //    /*    var storeRequest = await cardRepository.StoreAsync(new CardStoreRequest { Card = card }, cancellationToken);
            //        if (!storeRequest.ValidationResult.IsSuccessful())
            //        {
            //            ValidationResult result = storeRequest.ValidationResult.Build();
            //            logger.LogResult(result);
            //            return;
            //        }*/

            //    logger.Info("Card with external GUID " + c.UID + " updated successfully");
            //}

            logger.Info("Shutting down RBSED_AttachSignatures");
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

        private async Task InsertSSTUInfRKRow(Guid ID, Guid RowID, RBCategory rbCategoryQuery, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var insertQuery11 = builderFactory
                            .InsertInto("SSTUInfRK", "ID", "RowID", "OTKGID", "OTKGKodRub", "OTKGName")
                            .Values(b => b.P("ID", "RowID", "OTKOG_RubID", "OTKOG_RubKodRub", "OTKOG_RubName"))
                            .Build();
                await db
                .SetCommand(
                        insertQuery11,
                        db.Parameter("ID", ID),
                        db.Parameter("RowID", RowID),
                        db.Parameter("OTKOG_RubID", rbCategoryQuery.ID),
                        db.Parameter("OTKOG_RubKodRub", rbCategoryQuery.KodRub),
                        db.Parameter("OTKOG_RubName", rbCategoryQuery.Name))
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private async Task<RBCategory> GetCategoryAsync(string catKod, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = new RBCategory();

                result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("rub", "ID", "KodRub", "Name")
                            .From("OTKOG_Rub", "rub").NoLock()
                            .Where().C("rub", "KodRub").Equals().P("catKod")
                            .Limit(1).Build(),
                        db.Parameter("catKod", catKod))
                    .LogCommand()
                    .ExecuteAsync<RBCategory>();

                return result;
            }
        }

        public class RBCategory
        {
            public Guid ID { get; set; }
            public string KodRub { get; set; }
            public string Name { get; set; }
        }

        public class RBCategoryQuery
        {
          //  public Guid ID { get; set; }
          //  public Guid rowId { get; set; }
            public Guid rubID { get; set; }
            public string KodRub { get; set; }
            public string Name { get; set; }
        }

        #endregion
    }
}
