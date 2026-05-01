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
using Tessa.Extensions.Default.Shared.Workflow.KrProcess.Formatters;
using Unity.Injection;
using Tessa.Extensions.Chronos.SSTUIntegration.Models;
using System.Text.Json;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Tessa.Extensions.Chronos.Plugins
{
    /// <summary>
    /// Пример плагина, который может работать через серверное API.
    /// </summary>
    [Plugin(
        Name = "RBOG_SSTUIntegrationPlugin",
        Description = "Плагин создание карточкек и загрузку данных из СЭД EOS4SP",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class RBOG_SSTUIntegrationPlugin :
        Plugin
    {
        #region Constants

        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RBOG_SSTUIntegrationPlugin.xml";
        private const string OutputMainFolder = "/home/tessa/tessa/share/SSTUOutput/";

        //введено
        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Base Overrides

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("Starting plugin RBOG_SSTUIntegrationPlugin");

            // конфигурируем контейнер Unity для использования стандартных серверных API (в т.ч. API карточек)
            // а также для получения прямого доступа к базе данных через IDbScope по строке подключения из app.config;
            // предполагаем, что все действия, совершаемые плагином, будут выполняться от имени пользователя System
            //logger.Trace("Configuring container");

            // настраиваем контейнер Unity для работы с карточками
            await TessaPlatform.InitializeFromConfigurationAsync();

            IUnityContainer container = await new UnityContainer()
                //.RegisterType<IStageTypeFormatterContainer>(new InjectionConstructor())
                .AddExtension(new Diagnostic())
                .RegisterServerForPluginAsync()
                ;

            ICardRepository cardRepository = container.Resolve<ICardRepository>();
            ICardFileManager manager = container.Resolve<ICardFileManager>();
            IKrTypesCache typesCache = container.Resolve<IKrTypesCache>();
            ICardServerPermissionsProvider permissionsProvider = container.Resolve<ICardServerPermissionsProvider>();

            IDbScope dbScope = container.Resolve<IDbScope>();

            DateTime startDate = DateTime.Parse("01.02.2023 00:00:00");

            DateTime finishDate = DateTime.Parse("28.02.2023 23:59:59");

            string outputFolder = string.Empty;

            if(!Directory.Exists(Path.Combine(OutputMainFolder, DateTime.Now.ToString())))
            {
                outputFolder = Path.Combine(OutputMainFolder, DateTime.Now.ToString());
            }

            List<Guid> listID = await GetCardID(startDate, finishDate, dbScope);

            logger.Info($"Founded {listID.Count} cards");

            //DateTime tempValue = (DateTime)card.Sections["DocumentCommonInfo"].Fields["CreationDate"];

            foreach (var item in listID)
            {
                var cardGetRequest = new CardGetRequest
                {
                    CardID = item
                };

                permissionsProvider.SetFullPermissions(cardGetRequest);
                var cardGetResponse = await cardRepository.GetAsync(cardGetRequest, cancellationToken);

                if (!cardGetResponse.ValidationResult.IsSuccessful())
                {
                    logger.Error(cardGetResponse.ValidationResult.Build());
                    return;
                }

                var card = cardGetResponse.Card;

                QNotRecieved qNotRecieved = new QNotRecieved("0001.0003.0029.0201.0074");

                List<QNotRecieved> QList = new List<QNotRecieved>();

                QList.Add(qNotRecieved);

                DateTime creationDate = (DateTime)card.Sections["DocumentCommonInfo"].Fields["CreationDate"];

                string fullNumber = await GetFullNumberAsync(item, dbScope);

                logger.Info($"{fullNumber}");

                //var strBytes = Encoding.ASCII.GetBytes(fullNumber);

                //var strRes = Encoding.GetEncoding(1251).GetString(strBytes);
                
                SSTURequest<QNotRecieved> request = new SSTURequest<QNotRecieved>()
                {
                    //departmentId = (Guid)card.Sections["DocumentCommonInfo"].Fields["DepartmentID"],
                    departmentId = Guid.Parse("5a80a53d-dbcf-4bfb-9b49-1a5295029d95"),
                    isDirect = true,
                    format = "Other",
                    number = fullNumber,
                    createDate = creationDate.ToString("yyyy-MM-dd"),
                    Name = "",
                    Address = "",
                    Email = "",
                    Questions = QList
                };

                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(request, options);

                logger.Info("\n" + json);

                break;
            }

            //logger.Info($"{tempID}");
            logger.Info($"{startDate}");

            logger.Info("Shutting down plugin RBOG_SSTUIntegrationPlugin");
         }


        private async Task<List<Guid>> GetCardID(DateTime startDate, DateTime finishDate, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = new List<Guid>();

                result = await db.SetCommand(
                        builderFactory
                            .Select().C("dci", "ID")
                            .From("DocumentCommonInfo", "dci").NoLock()
                            .Where().C("dci", "CreationDate").Greater().P("startDate")
                            .And().C("dci", "CreationDate").Less().P("finishDate")
                            .And().C("dci", "CardTypeID").Equals().P("cardTypeID")
                            .Build(),
                        db.Parameter("startDate", startDate),
                        db.Parameter("finishDate", finishDate),
                        db.Parameter("cardTypeID", Guid.Parse("ed6d5ee1-9075-4ce0-bdae-76afe87544f2")))
                    .LogCommand()
                    .ExecuteListAsync<Guid>();

                return result;
            }
        }

        private async Task<string> GetFullNumberAsync(Guid cardID, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                string result;

                result = await db.SetCommand(
                        builderFactory
                            .Select().C("dci", "FullNumber")
                            .From("DocumentCommonInfo", "dci").NoLock()
                            .Where().C("dci", "ID").Equals().P("cardID")
                            .Build(),
                        db.Parameter("cardID", cardID))
                    .LogCommand()
                    .ExecuteAsync<string>();

                return result;
            }
        }

        private async Task<string> GetExternalGuidAsync(string externalGuid, string commissionId, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("dci", "ExternalGuidForTask")
                            .From("DocumentCommonInfo", "dci").NoLock()
                            .Where().C("dci", "ExternalGuidForTask").Equals().P("externalGuid")
                            .And().C("dci", "FullNumber").Equals().P("commissionId")
                            .Limit(1).Build(),
                        db.Parameter("externalGuid", externalGuid),
                        db.Parameter("commissionId", commissionId))
                    .LogCommand()
                    .ExecuteAsync<string>();

                return result;
            }
        }

        private async Task<RBDocInfo> GetCardIDAsync(string externalGuid, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = new RBDocInfo();

                result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("dci", "ID", "FullNumber", "Subject")
                            .From("DocumentCommonInfo", "dci").NoLock()
                            .Where().C("dci", "ExternalGuid").Equals().P("externalGuid")
                            .Limit(1).Build(),
                        db.Parameter("externalGuid", externalGuid))
                    .LogCommand()
                    .ExecuteAsync<RBDocInfo>();

                return result;
            }
        }

        private async Task<Guid?> GetUserCreatedByAsync(string userName, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("ID")
                            .From("PersonalRoles").NoLock()
                            .Where().C("FullName").Equals().P("userFIO")
                            .Limit(1).Build(),
                        db.Parameter("userFIO", userName))
                    .LogCommand()
                    .ExecuteAsync<Guid?>();

                return result;
            }
        }

        private async Task<RBExecutorRow> GetExecutorInfoAsync(string login, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = new RBExecutorRow();

                result = await db.SetCommand(
                        builderFactory
                            .Select().Top(1).C("ro", "ID", "Name")
                            .From("PersonalRoles", "ro").NoLock()
                            .Where().C("ro", "Login").Equals().P("login")
                            .Limit(1).Build(),
                        db.Parameter("login", login))
                    .LogCommand()
                    .ExecuteAsync<RBExecutorRow>();

                return result;
            }
        }

        public class RBDocInfo
        {
            public Guid ID { get; set; }
            public string FullNumber { get; set; }
            public string Subject { get; set; }
            //public string Login { get; set; }
        }

        public class RBExecutorRow
        {
            public Guid ID { get; set; }
            public string Name { get; set; }
            //public string Login { get; set; }
        }


        #endregion
    }
}
