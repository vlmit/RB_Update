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
        Name = "RBOG_TestImportFilesToTask",
        Description = "Плагин для загрузку файлов в карточки поручений",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class RBOG_TestImportFilesToTask :
        Plugin
    {
        #region Constants

        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RBOG_TestImportFilesToTask.xml";

        //введено
        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        #endregion

        #region Base Overrides

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("Starting plugin RBOG_TestImportFilesToTask");

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


            //получаем ID карточки для обработки
            var cardId = Guid.Parse("7ad47346-bf7f-4cc4-a9e3-45f358a7966d");

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

            // Обходим директорию с файлами, добавляем файлы в карточку
            var dir = new DirectoryInfo($@"/home/tessa/tessa/share/Test_ImportFilesToTask");

            await using (var fileContainer = await manager.CreateContainerAsync(card))
            {
                foreach (FileInfo file in dir.GetFiles())
                {
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
                    logger.Info("Files for card with SP UID:  attached with error 1000111");
                }
                else
                {
                    logger.Info("Files for card with SP UID:  attached succesful");
                }
            }

         /*       var storeRequest = await cardRepository.StoreAsync(new CardStoreRequest { Card = card }, cancellationToken);
                if (!storeRequest.ValidationResult.IsSuccessful())
            {
                    ValidationResult result = storeRequest.ValidationResult.Build();
                    logger.LogResult(result);
                    return;
            }*/
            
        }
            

        #endregion


    }
}
