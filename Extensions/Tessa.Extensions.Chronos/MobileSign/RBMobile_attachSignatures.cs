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
using Tessa.Extensions.Chronos.Medo.MedoSend.Helpers;
using Tessa.Extensions.Chronos.MobileSign.Helpers;
using Tessa.Extensions.Shared.Info;
using LinqToDB.Data;
using DocumentFormat.OpenXml.Drawing;

namespace Tessa.Extensions.Chronos.MobileSign
{
    /// <summary>
    /// Пример плагина, который может работать через серверное API.
    /// </summary>
    [Plugin(
        Name = "RBMobile_attachSignatures",
        Description = "Плагин добавления подписей, созданных на моб устройствах, к файлам в карточках",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class RBMobile_attachSignatures :
        Plugin
    {
        #region Constants

        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RBMobile_attachSignatures.xml";

        //private const string inputPath = "/home/tessa/tessa/share/Migration/EDSFiles/";

        //введено
        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Base Overrides

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("Starting plugin RBMobile_attachSignatures");

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
            IDbScope dbScope = container.Resolve<IDbScope>();

            var newSignatures = await NewMobileSignaturesAsync(dbScope, cancellationToken);

            logger.Info($"To sign from mobile: {newSignatures.Count} files");

            foreach (var signature in newSignatures)
            {
                //Получаем карточку
                var card = await GetCard(signature.ID, cardRepository, permissionsProvider, dbScope, cancellationToken);

                if (card == null)
                {
                    logger.Error($"Card {signature.ID} not found");
                    continue;
                }

                var cardID = card.ID;

                if (!card.Sections.ContainsKey(SchemeInfo.DocumentCommonInfo))
                {
                    logger.Error($"Section {SchemeInfo.DocumentCommonInfo} in Card {cardID} not found");
                    continue;
                }

                if (!card.Sections[SchemeInfo.DocumentCommonInfo].Fields.ContainsKey(SchemeInfo.DocumentCommonInfo.FullNumber))
                {
                    logger.Error($"Field {SchemeInfo.DocumentCommonInfo.FullNumber} in Card {cardID} not found");
                    continue;
                }

                string cardFullNumber = string.Empty;

                if (card.Sections[SchemeInfo.DocumentCommonInfo].Fields[SchemeInfo.DocumentCommonInfo.FullNumber] != null)
                {
                    cardFullNumber = card.Sections[SchemeInfo.DocumentCommonInfo].Fields[SchemeInfo.DocumentCommonInfo.FullNumber].ToString();
                }

                logger.Info($"Card was found FullNumber: {cardFullNumber} ID: {cardID}");

                // Поиск файла
                // Получаем файлы прикрепленные к карточке
                var cardFiles = card.TryGetFiles();

                // Если файлов нет, переход к следующему
                if (cardFiles == null)
                {
                    logger.Error($"No files in card UID: {cardID} RegNum: {cardFullNumber}");
                    continue;
                }

                Guid fileRowID = Guid.Empty;

                if (!Guid.TryParse(signature.SignedFileName, out fileRowID))
                {
                    logger.Error($"File {signature.SignedFileName} not found in card ");
                    continue;
                }

                var fileCard = cardFiles.Select(x => x).Where(x => x.RowID == fileRowID).FirstOrDefault();

                if (fileCard == null)
                {
                    logger.Error($"File {signature.SignedFileName} not found in card ");
                    continue;
                }

                var fileContainer = await fileManager.CreateContainerAsync(card);

                var fileToSign = fileContainer.FileContainer.Files.Select(x => x).Where(x => x.Name == fileCard.Name).FirstOrDefault();
                //cardFiles.Select(x => x).Where(x => x.Name == signature.SignedFileName).FirstOrDefault();

                if (fileToSign == null)
                {
                    logger.Error($"File {signature.SignedFileName} not found in card ");
                    continue;
                }

                var signContent = Convert.FromBase64String(signature.SignContent);

                DateTime signDateTime;
                
                var isDateTimeValid = DateTime.TryParseExact(signature.SignDate, "dd.MM.yyyy H:m", CultureInfo.InvariantCulture, DateTimeStyles.None, out signDateTime);

                DateTime signDate;

                var isDateValid = DateTime.TryParseExact(signature.SignDate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out signDate);

                if (!isDateTimeValid && !isDateValid)
                {
                    logger.Error($"Incorrect date value: {signature.SignDate}");
                    continue;
                }
                else if (!isDateTimeValid)
                {
                    signDateTime = signDate;
                }

                signDateTime.AddHours(5);

                logger.Info($"\nSign info\nsignDate {signDateTime}\nsignature.SignType {signature.SignType}");

                //Прикрепление подписи
                bool resSign = await Chronos.Helpers.Sign.SignFile.SignFileAsync(
                    signContent,
                    signDateTime,
                    signature.SignType,
                    fileToSign.TryGetActualVersion(),
                    cadesManager,
                    cancellationToken);

                if (!resSign)
                {
                    logger.Error($"File {signature.SignedFileName} sign fail");
                    continue;
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
                    logger.Info($"File {fileToSign.Name} in card {cardFullNumber} signed successfully");
                }

                //Изменение статуса обработки
                await UpdateMobileSignStatusAsync(signature.RowID, dbScope);
            }

            logger.Info("Shutting down RBMobile_attachSignatures");
        }

        /// <summary>
        ///     получение id карточек входящих и типа уведомлений для отправки
        ///     уведомления МЭДО
        /// </summary>
        /// <returns>id карточки, тип сообщения, id контрагента</returns>
        private static async Task<List<NewMobileSignInfo>> NewMobileSignaturesAsync(
            IDbScope dbScope,
            CancellationToken cancellationToken = default)
        {
            await using (dbScope.Create())
            {
                return await dbScope.Db.SetCommand
                    ("SELECT \"ID\", \"RowID\", \"SignedFileName\", \"SignContent\", \"SignType\", \"SignDate\", \"IsProcessed\" " +
                        "FROM \"SignFilesMobile\" " +
                        "WHERE \"IsProcessed\" = false")
                        .LogCommand()
                        .ExecuteListAsync<NewMobileSignInfo>(cancellationToken);
            }
        }

        private async Task<Card> GetCard(
            Guid cardID, 
            ICardRepository cardRepository, 
            ICardServerPermissionsProvider permissionsProvider, 
            IDbScope dbScope,
            CancellationToken cancellationToken = default)
        {
            var cardGetRequest = new CardGetRequest
            {
                CardID = cardID
            };

            permissionsProvider.SetFullPermissions(cardGetRequest);
            var cardGetResponse = await cardRepository.GetAsync(cardGetRequest, cancellationToken);

            if (!cardGetResponse.ValidationResult.IsSuccessful())
            {
                logger.Error("GetCardError\n" + cardGetResponse.ValidationResult.Build());
                return null;
            }

            return cardGetResponse.Card;
        }

        private async Task<int> UpdateMobileSignStatusAsync(Guid rowID, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                var param = new[]
                {
                    new DataParameter("@rowID", rowID)
                };

                return await db.SetCommand
                    ("UPDATE \"SignFilesMobile\" " +
                            "SET \"IsProcessed\" = true " +
                            "WHERE \"RowID\" = @rowID", param)
                        .LogCommand()
                        .ExecuteNonQueryAsync();
            }
        }

        #endregion
    }
}
