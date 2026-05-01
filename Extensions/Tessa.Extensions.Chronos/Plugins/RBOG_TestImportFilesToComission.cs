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
using Tessa.Platform.Operations;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Topshelf.Runtime.Windows;

namespace Tessa.Extensions.Chronos.Plugins
{
    /// <summary>
    /// Пример плагина, который может работать через серверное API.
    /// </summary>
    [Plugin(
        Name = "RBOG_TestImportFilesToComission",
        Description = "Плагин для загрузку файлов в карточки поручений",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class RBOG_TestImportFilesToComission :
        Plugin
    {
        #region Constants

        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RBOG_TestImportFilesToComission.xml";

        //введено
        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        #endregion

        #region Base Overrides

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("Starting plugin RBOG_TestImportFilesToComission");

            // конфигурируем контейнер Unity для использования стандартных серверных API (в т.ч. API карточек)
            // а также для получения прямого доступа к базе данных через IDbScope по строке подключения из app.config;
            // предполагаем, что все действия, совершаемые плагином, будут выполняться от имени пользователя System
            //logger.Trace("Configuring container");

            // настраиваем контейнер Unity для работы с карточками
            await TessaPlatform.InitializeFromConfigurationAsync();

            IUnityContainer container = await new UnityContainer()
                .RegisterServerForPluginAsync()
                ;

            ICardServerPermissionsProvider permissionsProvider = container.Resolve<ICardServerPermissionsProvider>();
            ICardRepository cardRepository = container.Resolve<ICardRepository>();
            ICardFileManager manager = container.Resolve<ICardFileManager>();
            IKrTypesCache typesCache = container.Resolve<IKrTypesCache>();

            var cardsFolders = Directory.GetDirectories(@"/home/tessa/tessa/share/CommissionFiles2022");
            // logger.Info($"длина массива {commissionsFolders.Length}");
            foreach (var c in cardsFolders)
            {
                // logger.Info($"путь {f}");
                IDbScope dbScope = container.Resolve<IDbScope>();

                var cSplit = c.Split('/');
                string cardExtGuid = cSplit.Last();

                //проверка наличия карточки
                var cardId = await GetCardIDAsync(cardExtGuid, dbScope);

                if (cardId == null)
                {
                    logger.Info("Card with UID: " + cardExtGuid + " not exist");
                    continue;
                }
                //logger.Info("Get folder: " + cardExtGuid + " not exist");
                //получения списка папок с поручениями для карточки
                var comissionFolders = Directory.GetDirectories(c);
                

                foreach (var comFolder in comissionFolders)
                {
                    var comSplit = comFolder.Split('/');
                    string comExtGuid = comSplit.Last();

                    //проверка наличия поручения
                    var comId = await GetComissionExternalGuidAsync(cardExtGuid, comExtGuid, dbScope);

                    if (comId == null)
                    {
                        logger.Info("Comission with UID: " + comExtGuid + " for card with UID: " + cardExtGuid + " not exist");
                        continue;
                    }

                    var cardGetRequest = new CardGetRequest
                    {
                        CardID = comId
                    };

                    permissionsProvider.SetFullPermissions(cardGetRequest);
                    var cardGetResponse = await cardRepository.GetAsync(cardGetRequest, cancellationToken);

                    var card = cardGetResponse.Card;

                    if (!cardGetResponse.ValidationResult.IsSuccessful())
                    {
                        logger.Error(cardGetResponse.ValidationResult.Build());
                        return;
                    }

                    var dir = new DirectoryInfo(comFolder);

                    var cardFiles = card.TryGetFiles();

                    var cardFilesName = cardFiles.Select(f => f.Name);

                    await using (var fileContainer = await manager.CreateContainerAsync(card))
                    {
                        foreach (FileInfo file in dir.GetFiles())
                        {
                            if (cardFilesName.Contains(file.Name))
                            {
                                logger.Info("File " + file.Name + " for comission with SP UID: " + comExtGuid + " already attached 1110101");
                                continue;
                            }

                            await fileContainer
                            .FileContainer
                            .BuildFile(file.Name)
                            .SetContent(file.FullName)
                            .AddWithNotificationAsync();

                            // file.Delete();
                        }

                        var storeResponse = await fileContainer.StoreAsync();
                        if (!storeResponse.ValidationResult.IsSuccessful())
                        {
                            ValidationResult result = storeResponse.ValidationResult.Build();
                            logger.LogResult(result); // пишем ошибку или другие сообщения в лог
                            logger.Info("Files for comission with SP UID: "+ comExtGuid + " attached with error 1100111");
                        }
                        else
                        {
                            logger.Info("Files for comission with SP UID: " + comExtGuid + " attached succesful");
                        }
                    }
                }
            }

            logger.Info("Shutdown plugin RBOG_TestImportFilesToComission");
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

        private async Task<Guid?> GetComissionExternalGuidAsync(string externalGuid, string commissionId, IDbScope dbScope)
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
                            .Where().C("dci", "ExternalGuidForTask").Equals().P("externalGuid")
                            .And().C("dci", "FullNumber").Equals().P("commissionId")
                            .Limit(1).Build(),
                        db.Parameter("externalGuid", externalGuid),
                        db.Parameter("commissionId", commissionId))
                    .LogCommand()
                    .ExecuteAsync<Guid?>();

                return result;
            }
        }



        #endregion


    }
}
