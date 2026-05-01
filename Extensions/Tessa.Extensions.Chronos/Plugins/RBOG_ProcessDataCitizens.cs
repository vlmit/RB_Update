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
        Name = "RBOG_ProcessDataCitizens",
        Description = "Плагин обновления карточек для добавление граждан обращения",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class RBOG_ProcessDataCitizens :
        Plugin
    {
        #region Constants

        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RBOG_ProcessDataCitizens.xml";

        //введено
        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Base Overrides

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("Starting plugin RBOG_ProcessDataCitizens");

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

            FileParseCitizens fileParseCategories = new FileParseCitizens();

            // прописать путь
            //var dictCategories = fileParseCategories.GetCategoriesFromFile("/home/tessa/tessa_og/sync/testtt.txt");

            var listCategories = fileParseCategories.GetCategories("/home/tessa/tessa_og/sync/testtt.txt");
            logger.Info("---List---");
            foreach (var d in listCategories)
            {
                logger.Info(d.UID + " " + d.citizen);
            }

            var dictCitizens = fileParseCategories.GetCitizensFromFile("/home/tessa/tessa_og/sync/testtt.txt");
            logger.Info("---Dict---");
            foreach (var dd in dictCitizens)
            {
                logger.Info(dd.Value + " " + dd.Value);
            }

            foreach (var c in listCategories)
            {
                IDbScope dbScope = container.Resolve<IDbScope>();

                var cardId = await GetCardIDAsync(c.UID.ToLower(), dbScope);

                if (cardId == null)
                {
                    logger.Info("Card with UID: " + c.UID.ToLower() + " not exist");
                    continue;
                }

                /*    var cardGetRequest = new CardGetRequest
                    {
                        CardID = cardId
                    };*/

                //   permissionsProvider.SetFullPermissions(cardGetRequest);
                //    var cardGetResponse = await cardRepository.GetAsync(cardGetRequest, cancellationToken);

                ///     if (!cardGetResponse.ValidationResult.IsSuccessful())
                //    {
                //        logger.Error(cardGetResponse.ValidationResult.Build());
                //        return;
                //    }

                //     var card = cardGetResponse.Card;

                RBCitizen TCitizen = await GetCitizensAsync(c.citizen, dbScope);


                if (TCitizen == null)
                {
                    logger.Error("Гражданин " + c.citizen + " не найдена. Карточка: " + c.UID.ToLower() + " CitNotFound");
                    continue;
                }

                await UpdateCitizenAsync((Guid)cardId, TCitizen, dbScope);
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

            logger.Info("Shutting down RBOG_ProcessDataCategories");
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

        private async Task UpdateCitizenAsync(Guid ID, RBCitizen rbCitizen, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var insertQuery11 = builderFactory
                        .Update("DocumentCommonInfo")
                        .C("CitizensID").Equals().P("citizensID")
                        .C("CitizensFIO").Equals().P("citizensFIO")
                        .C("CitizensAddress").Equals().P("citizensAddress")
                        .C("CitizensCiti").Equals().P("citizensCiti")
                        .C("CitizensIndex").Equals().P("citizensIndex")
                        .C("CitizensEmail").Equals().P("citizensEmail")
                        .Where()
                        .C("ID").Equals().P("ID")
                        .And()
                        .C("CitizensID").IsNull()
                        .Build();
                await db
                .SetCommand(
                        insertQuery11,
                         db.Parameter("ID", ID),
                    db.Parameter("citizensID", rbCitizen.ID),
                    db.Parameter("citizensFIO", rbCitizen.FIO),
                    db.Parameter("citizensAddress", rbCitizen.Address),
                    db.Parameter("citizensCiti", rbCitizen.Citi),
                    db.Parameter("citizensIndex", rbCitizen.Index),
                    db.Parameter("citizensEmail", rbCitizen.Email)
                    )
                    .LogCommand()
                    .ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }


        private async Task<RBCitizen> GetCitizensAsync(string citizensFIO, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = new RBCitizen();

                result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("cit", "ID", "FIO", "Address", "Citi", "Index", "Email")
                            .From("Citizens", "cit").NoLock()
                            .Where().C("cit", "FIO").Equals().P("citizensFIO")
                            .Limit(1).Build(),
                        db.Parameter("citizensFIO", citizensFIO))
                    .LogCommand()
                    .ExecuteAsync<RBCitizen>();

                return result;
            }
        }

  

        /*&    private async Task<RBCategory> GetCategoryAsync(string catKod, IDbScope dbScope)
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
            }*/

        public class RBCitizen
        {
            public Guid ID { get; set; }
            public string FIO { get; set; }
            public string Address { get; set; }
            public string Citi { get; set; }
            public string Index { get; set; }
            public string Email { get; set; }
        }

        //public class RBCategory
        //{
        //    public Guid ID { get; set; }
        //    public string KodRub { get; set; }
        //    public string Name { get; set; }
        //}

        //public class RBCategoryQuery
        //{
        //  //  public Guid ID { get; set; }
        //  //  public Guid rowId { get; set; }
        //    public Guid rubID { get; set; }
        //    public string KodRub { get; set; }
        //    public string Name { get; set; }
        //}

        #endregion
    }
}
