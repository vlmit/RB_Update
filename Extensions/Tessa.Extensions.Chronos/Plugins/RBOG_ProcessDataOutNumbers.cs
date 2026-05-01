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
using DocumentFormat.OpenXml.Bibliography;

namespace Tessa.Extensions.Chronos.Plugins
{
    /// <summary>
    /// Пример плагина, который может работать через серверное API.
    /// </summary>
    [Plugin(
        Name = "RBOG_ProcessDataOutNumbers",
        Description = "Плагин обновления карточек для добавление категорий (рубрик) обращения",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class RBOG_ProcessDataOutNumbers :
        Plugin
    {
        #region Constants

        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RBOG_ProcessDataOutNumbers.xml";

        //введено
        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Base Overrides

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("Starting plugin RBOG_ProcessDataOutNumbers");

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

            FileParseOutNumbersOG fileParse = new FileParseOutNumbersOG();

            // прописать путь
            var outNumbers = fileParse.GetSPCardsFromDir("");

            foreach (var c in outNumbers)
            {
                IDbScope dbScope = container.Resolve<IDbScope>();
                var cardId = await GetCardIDAsync(c["DocUID"], dbScope);

                if (cardId == null)
                {
                    logger.Info("Card with UID: " + c["DocUID"] + " not exist");
                    continue;
                }

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

                if(card.Sections["DocumentCommonInfo"].Fields["OutgoingDate"] == null)
                {
                    card.Sections["DocumentCommonInfo"].Fields["OutgoingDate"] = DateTime.ParseExact(c["OutgoingDate"], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                    logger.Info("OutgoingDate for Card with external GUID " + c["DocUID"] + " updated successfully");
                }
                else
                {
                    logger.Info("OutgoingDate for Card with external GUID " + c["DocUID"] + " already exist");
                }

                if (card.Sections["DocumentCommonInfo"].Fields["OutgoingNumber"] == null)
                {
                    card.Sections["DocumentCommonInfo"].Fields["OutgoingNumber"] = c["OutgoingNumber"];
                    logger.Info("OutgoingNumber for Card with external GUID " + c["DocUID"] + " updated successfully");
                }
                else
                {
                    logger.Info("OutgoingNumber for Card with external GUID " + c["DocUID"] + " already exist");
                }

                //logger.Info("Card with external GUID " + c["DocUID"] + " updated successfully");
            }

            logger.Info("Shutting down RBOG_ProcessDataOutNumbers");
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
