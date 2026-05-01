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
        Name = "RB_ProcessDataFromTessa",
        Description = "Плагин создание карточкек и загрузку данных из СЭД Tessa",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class RB_ProcessDataFromTessa :
        Plugin
    {
        #region Constants

        /// <summary>
        /// Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/RB_ProcessDataFromTessa.xml";

        //введено
        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        #endregion

        #region Base Overrides

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("Starting plugin RB_ProcessDataFromTessa");

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
            var cardId = Guid.Parse("8988be2e-97b7-4624-892a-9ced4eb5b577");

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

            using (NetworkManager network = new NetworkManager(@"\\172.18.16.209\TessaOut"))
            {

                var card = cardGetResponse.Card;
                //string pathBase = $@"/home/tessa/tessa/sync/out/{card.ID}";
                string pathBase = $@"\\172.18.16.209\TessaOut\{card.ID}";
                
                DirectoryInfo dirInfo = new DirectoryInfo(pathBase);
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }

                // путь к файлу
                //string pathSGN = $@"/home/tessa/tessa/sync/out/{card.ID}/ТЕСТ.sig";   // путь к файлу
                string pathSGN = $@"\\172.18.16.209\TessaOut\{card.ID}\ТЕСТ.sig";
                //string pathMetaFile = $@"/home/tessa/tessa/sync/out/{card.ID}.txt";
                string pathMetaFile = $@"\\172.18.16.209\TessaOut\{card.ID}.txt";

                using (StreamWriter w = new StreamWriter(pathMetaFile, false, Encoding.GetEncoding(1251)))
                {
                    w.WriteLine("ID           : " + card.ID);
                    w.WriteLine("Тема         : " + card.Sections["DocumentCommonInfo"].Fields["Subject"]);
                    w.WriteLine("Номер        : " + card.Sections["DocumentCommonInfo"].Fields["FullNumber"]);
                    w.WriteLine("Номенклатура : " + card.Sections["DocumentCommonInfo"].Fields["NomenclatureDescription"]);
                    w.WriteLine("Подразделение: " + card.Sections["DocumentCommonInfo"].Fields["DepartmentName"]);
                    w.WriteLine("Адресат      : " + card.Sections["DocumentCommonInfo"].Fields["PartnerName"]);
                    w.WriteLine("Тип доставки : " + card.Sections["DocumentCommonInfo"].Fields["DeliveryTypeName"]);
                }

                Guid? versionRowID = null;
                await using (ICardFileContainer fileContainer = await manager.CreateContainerAsync(card))
                {
                    fileContainer.FileContainer.Files.Clear();
                    foreach (var file in fileContainer.FileContainer.Files)
                    {
                       // await fileContainer.FileContainer.Files.RemoveWithNotificationAsync(file);
                        versionRowID = file.Versions.Last.ID;
                        if (file != null)
                        {
                            if (!file.Content.HasData)
                            {
                                
                                var contentResult = await file.EnsureContentDownloadedAsync(); //file.TryGetActualVersion().Content;
                                versionRowID = file.Versions.Last.ID;
                                if (contentResult.IsSuccessful)
                                {
                                    using (var stream = await file.Content.GetAsync())
                                    {
                                        //string path = $@"/home/tessa/tessa/sync/out/{card.ID}/{file.Name}";
                                        string path = $@"\\172.18.16.209\TessaOut\{card.ID}\{file.Name}";
                                        using (FileStream fstream = new FileStream(path, FileMode.Create))
                                        {
                                            // преобразуем строку в байты

                                            // запись массива байтов в файл
                                            await stream.CopyToAsync(fstream);

                                        }
                                    }
                                }
                                else
                                {
                                    logger.Trace($"Failed to load last file version");
                                }
                            }
                        }
                    }
                    //var file = fileContainer.FileContainer.Files.FirstOrDefault(x => x.Name == "ТЕСТ.docx");

                }

                //  IDbScope dbScope = container.Resolve<IDbScope>();

                /*  await using (dbScope.Create())
                  {
                      // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                      if (this.StopRequested)
                      {
                          // была запрошена асинхронная остановка, можно периодически проверять значение этого свойства,
                          // и консистентно завершать выполнение (закрыть транзакцию, если была открыта, и др.)
                          return;
                      }

                      var db = dbScope.Db;

                      var builderFactory = dbScope.BuilderFactory; // введено

                      if(versionRowID!= null)
                      {
                          var result = await db.SetCommand(
                              builderFactory
                                  .Select().Top(1).C("fs", "Data")
                                  .From("FileSignatures", "fs").NoLock()
                                  .Where().C("fs", "VersionRowID").Equals().P("versionRowID")
                                  .Limit(1).Build(),
                              db.Parameter("versionRowID", (Guid)versionRowID))
                          .LogCommand()
                          .ExecuteAsync<byte[]>();

                          using (FileStream fstream = new FileStream(pathSGN, FileMode.OpenOrCreate))
                          {
                              // запись массива байтов в файл
                              await fstream.WriteAsync(result, 0, result.Length);
                          }
                      }


                  }*/

                card.Sections["DocumentCommonInfo"].Fields["LastUploadOut"] = DateTime.Now;
                var storeRequest = await cardRepository.StoreAsync(new CardStoreRequest { Card = card }, cancellationToken);
                if (!storeRequest.ValidationResult.IsSuccessful())
                {
                    ValidationResult result = storeRequest.ValidationResult.Build();
                    logger.LogResult(result);
                    return;
                }
            }
        }
            

        #endregion





            #region NetworkManager
        public class NetworkManager : IDisposable
        {
            private readonly string _networkName;

            public NetworkManager(string networkName)
            {
                _networkName = networkName;

                NetResource netResource = new NetResource
                {
                    Scope = ResourceScope.GlobalNetwork,
                    ResourceType = ResourceType.Any,
                    DisplayType = ResourceDisplayType.Directory,
                    RemoteName = networkName
                };

                int result = WNetAddConnection2(netResource, "1uNCoFuGGA", "power\\sed_sps_farm", 0);

                if (result != 0)
                {
                    throw new Win32Exception(result);
                }
            }

            ~NetworkManager()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                WNetCancelConnection2(_networkName, 0, true);
            }

            [DllImport("mpr.dll")]
            private static extern int WNetAddConnection2(NetResource netResource, string password, string username, int flags);

            [DllImport("mpr.dll")]
            private static extern int WNetCancelConnection2(string name, int flags, bool force);
        }

        [StructLayout(LayoutKind.Sequential)]
        public class NetResource
        {
            public ResourceScope Scope;
            public ResourceType ResourceType;
            public ResourceDisplayType DisplayType;
            public int Usage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }

        public enum ResourceScope
        {
            Connected = 1,
            GlobalNetwork,
            Remembered,
            Recent,
            Context
        }

        public enum ResourceType
        {
            Any = 0,
            Disk = 1,
            Print = 2,
            Reserved = 8,
        }

        public enum ResourceDisplayType
        {
            Generic = 0x0,
            Domain = 0x01,
            Server = 0x02,
            Share = 0x03,
            File = 0x04,
            Group = 0x05,
            Network = 0x06,
            Root = 0x07,
            Shareadmin = 0x08,
            Directory = 0x09,
            Tree = 0x0a,
            Ndscontainer = 0x0b
        }
        #endregion
    }
}
