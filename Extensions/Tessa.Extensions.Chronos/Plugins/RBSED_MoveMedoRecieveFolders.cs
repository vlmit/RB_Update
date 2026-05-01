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
using NLog.Time;

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
    public sealed class RBSED_MoveMedoRecieveFolders :
        Plugin
    {
        #region Constants

        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RBSED_MoveMedoRecieveFolders.xml";

        //введено
        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        #endregion

        #region Base Overrides

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("Starting plugin RBSED_MoveMedoRecieveFolders");

            // конфигурируем контейнер Unity для использования стандартных серверных API (в т.ч. API карточек)
            // а также для получения прямого доступа к базе данных через IDbScope по строке подключения из app.config;
            // предполагаем, что все действия, совершаемые плагином, будут выполняться от имени пользователя System
            //logger.Trace("Configuring container");

            // настраиваем контейнер Unity для работы с карточками

            string pathToLogFile = "/home/tessa/tessa/share/MEDO/first.txt";

            List<string> paths = new List<string>();

            using (StreamReader sr = new StreamReader(pathToLogFile))
            {
                string s;

                while ((s = sr.ReadLine()) != null)
                {
                    string[] sSplit = s.Split("---------------");

                    //logger.Info("-----------");

                    int numericValue = 0;
                    
                    foreach (string tempS in sSplit) 
                    {
                        //logger.Info(tempS + " ");

                        bool isNumber = int.TryParse(tempS, out numericValue);

                        if(isNumber && numericValue != 0)
                        {
                            logger.Info(tempS);
                            paths.Add(tempS);
                        }

                    }

                    if (numericValue!= 0)
                    {
                        logger.Info(numericValue);
                        //paths.Add(numericValue.ToString());
                    }

                    //logger.Info(numericValue);

                    //logger.Info("-----------");
                }
            }

            string dirPath = "/home/tessa/tessa/share/MEDO/first_attempt/";

            string srcDirPath = "/home/tessa/tessa/share/MEDO/IN_TEST/";

            var srcDir = new DirectoryInfo(srcDirPath);

            foreach (var inDir in srcDir.GetDirectories())
            {
                if (paths.Contains(inDir.Name))
                {
                    if (Directory.Exists(Path.Combine(dirPath, inDir.Name)))
                    {
                        continue;
                    }
                    Directory.Move(inDir.FullName, Path.Combine(dirPath, inDir.Name));


                    //logger.Info($"exist {inDir.Name}");
                }
            }




            

            var dir = new DirectoryInfo(dirPath);

            if (dir.Exists)
            {

            }



            // Обходим директорию с файлами, добавляем файлы в карточку
            //var dir = new DirectoryInfo($@"/home/tessa/tessa/share/Test_ImportFilesToTask");



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
