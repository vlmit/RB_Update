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

namespace Tessa.Extensions.Chronos.Plugins
{
    /// <summary>
    /// Пример плагина, который может работать через серверное API.
    /// </summary>
    [Plugin(
        Name = "RBOG_ProcessDataOrganizationsExtNumDate",
        Description = "Плагин обновления карточек для добавление категорий (рубрик) обращения",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class RBOG_ProcessDataOrganizationsExtNumDate :
        Plugin
    {
        #region Constants

        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RBOG_ProcessDataOrganizationsExtNumDate.xml";

        //введено
        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Base Overrides

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("Starting plugin RBOG_ProcessDataOrganizationsExtNumDate");

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

            FileParseOGOrganizationExtNumDate fileParse = new FileParseOGOrganizationExtNumDate();

            // прописать путь
            //var dictCategories = fileParseCategories.GetCategoriesFromFile("/home/tessa/tessa_og/sync/testtt.txt");

            var listData = fileParse.GetData("/", logger);

            foreach(var item in listData )
            {
                logger.Info("UID: " + item.UID + " Org: " + item.organization + " ExtNum: " + item.extNum + " ExtDate:" + item.extDate);
            }

            foreach (var c in listData)
            {
                IDbScope dbScope = container.Resolve<IDbScope>();

                var cardId = await GetCardIDAsync(c.UID.ToLower(), dbScope);

                if (cardId == null)
                {
                    logger.Info("Card with UID: " + c.UID.ToLower() + " not exist");
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

                if (c.organization != "NULL"
                    && !string.IsNullOrWhiteSpace(c.organization)
                    && card.Sections["DocumentCommonInfo"].Fields[""] != null)
                {
                    card.Sections["DocumentCommonInfo"].Fields[""] = c.organization;
                }

                if (c.extNum != "NULL"
                    && !string.IsNullOrWhiteSpace(c.extNum)
                    && card.Sections["DocumentCommonInfo"].Fields[""] != null)
                {
                    card.Sections["DocumentCommonInfo"].Fields[""] = c.extNum;
                }

                if (c.extDate != "NULL"
                    && !string.IsNullOrWhiteSpace(c.extDate)
                    && card.Sections["DocumentCommonInfo"].Fields[""] != null)
                {
                    card.Sections["DocumentCommonInfo"].Fields[""] = DateTime.ParseExact(c.extDate, "M/d/yyyy h:mm:ss", CultureInfo.InvariantCulture).AddHours(8);
                }



                //RBCategory TCategory = await GetCategoryAsync(c.category, dbScope);


                //if (TCategory == null)
                //{
                //    logger.Error("Рубрика " + c.category + " не найдена. Карточка: " + c.UID.ToLower() + " CatNotFound");
                //    continue;
                //}

                //await InsertSSTUInfRKRow((Guid)cardId, Guid.NewGuid(), TCategory, dbScope);

                /*  CardSection newOutgoingRefDocs = card.Sections["SSTUInfRK"];

                  CardRow row = newOutgoingRefDocs.Rows.Add();
                  row.RowID = Guid.NewGuid();
                  row["OTKGID"] = TCategory.ID;
                  row["OTKGKodRub"] = TCategory.KodRub;
                  row["OTKGName"] = TCategory.Name;
                  row.State = CardRowState.Inserted;*/

                /*    var storeRequest = await cardRepository.StoreAsync(new CardStoreRequest { Card = card }, cancellationToken);
                    if (!storeRequest.ValidationResult.IsSuccessful())
                    {
                        ValidationResult result = storeRequest.ValidationResult.Build();
                        logger.LogResult(result);
                        return;
                    }*/

                logger.Info("Card with external GUID " + c.UID + " updated successfully");
            }

            logger.Info("Shutting down RBOG_ProcessDataOrganizationsExtNumDate");
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
