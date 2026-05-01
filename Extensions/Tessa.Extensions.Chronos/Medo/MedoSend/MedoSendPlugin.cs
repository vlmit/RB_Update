using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chronos.Contracts;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using LinqToDB.Common;
using LinqToDB.Data;
using NLog;
using Tessa.Extensions.Chronos.Medo.MedoSend.Helpers;
using Tessa.Extensions.Chronos.Medo.MedoSend.OutgoingTypes;
using Tessa.Extensions.Shared.Helpers.Medo;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;
using Unity;

namespace Tessa.Extensions.Chronos.Medo.MedoSend
{
    [Plugin(
        Name = "MedoSendPlugin",
        Description = "Plugin send medo message",
        Version = 1,
        ConfigFile = ConfigFilePath)]
    public sealed class MedoSendPlugin : Plugin
    {
        #region Const

        /// <summary>
        ///     Относительный путь к конфигурационному файлу плагина.
        /// </summary>
        private const string ConfigFilePath = "configuration/MedoPlugins.xml";

        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region EntryPoint

        public override async Task EntryPointAsync(CancellationToken cancellationToken = default)
        {
            await TessaPlatform.InitializeFromConfigurationAsync(cancellationToken: cancellationToken);

            var container = await new UnityContainer().RegisterServerForPluginAsync();
            var dbScope = container.Resolve<IDbScope>();

            //путь к папке входящих
            var outPath = ConfigurationManager.Settings.TryGet<string>(MedoConst.GlobalOutPathSettingName);
            if (outPath.IsNullOrEmpty())
            {
                logger.Error("Cant get GlobalOutPathSettingName from app.json");
                return;
            }

            logger.Info("Start send MEDO message");

            var newMessages = await NewMedoMessageAsync(dbScope, cancellationToken);

            logger.Info($"MEDO message to send: {newMessages.Count}");

            foreach (var newMessageInfo in newMessages)
            {
                var noticeType = (NoticeType) newMessageInfo.MedoTypeID;
                logger.Info($"Start send message type {noticeType.GetDescription()} by card {newMessageInfo.ID} partner name {newMessageInfo.MedoPartnerName}");

                if (newMessageInfo.MedoPartnerID == null || newMessageInfo.MedoPartnerID == Guid.Empty)
                {
                    logger.Error($"Missing MedoPartner {newMessageInfo.ID}");
                    continue;
                }

                var medoPartnerInfo = await GetPartnerMedoFormat((Guid)newMessageInfo.MedoPartnerID, dbScope, cancellationToken);

                if (string.IsNullOrWhiteSpace(medoPartnerInfo.MedoID))
                {
                    logger.Error($"Missing MedoID {newMessageInfo.MedoPartnerName}");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(medoPartnerInfo.MedoAddress))
                {
                    logger.Error($"Missing MedoAddress {newMessageInfo.MedoPartnerName}");
                    continue;
                }

                switch(medoPartnerInfo.MEDOFormatID)
                {
                    case 1:
                        newMessageInfo.MedoXsdVersion = 0;
                        await UpdateMedoXsdVersionAsync(0, newMessageInfo.RowID, dbScope);
                        break;
                    case 2:
                        newMessageInfo.MedoXsdVersion = 1;
                        await UpdateMedoXsdVersionAsync(1, newMessageInfo.RowID, dbScope);
                        break;
                    default:
                        logger.Error($"Unknown MEDO XSD Version {medoPartnerInfo.MEDOFormatID}");
                        continue;
                }

                OutgoingContext context;
                if (noticeType == NoticeType.Document && newMessageInfo.ResponseMesID == null)
                {
                    logger.Info($"Create NewOutShipContainer by card {newMessageInfo.ID}");
                    context = new NewOutShipContainer(
                        container,
                        dbScope,
                        outPath,
                        newMessageInfo);
                    //if (newMessageInfo.MedoStatusID == 4)
                    //{
                    //    logger.Info($"Create ResendOutShipContainer by card {newMessageInfo.ID}");
                    //    context = new ResendOutShipContainer(
                    //        container,
                    //        dbScope,
                    //        outPath,
                    //        newMessageInfo);
                    //}
                    //else
                    //{
                    //    logger.Info($"Create NewOutShipContainer by card {newMessageInfo.ID}");
                    //    context = new NewOutShipContainer(
                    //        container,
                    //        dbScope,
                    //        outPath,
                    //        newMessageInfo);
                    //}
                }
                else if (noticeType == NoticeType.RegDenied)
                {
                    logger.Info($"Create OutNotification by card {newMessageInfo.ID}");

                    context = new OutNotification(
                        container,
                        dbScope,
                        outPath,
                        newMessageInfo);
                }
                //else if(noticeType == NoticeType.Registered && newMessageInfo.ID == Guid.Parse("00000"))
                //{
                //    logger.Info($"Create OutNotification Registered by card {newMessageInfo.ID}");

                //    context = new OutNotification(
                //        container,
                //        dbScope,
                //        outPath,
                //        newMessageInfo);
                //}
                else
                {
                    continue;
                }
                try
                {
                    await context.InitializeAsync(cancellationToken).ConfigureAwait(false);

                }
                catch(Exception e) 
                {
                    logger.Error(e, "MedoSend Exception");
                }

                if (context.Check)
                {
                    try
                    {
                        logger.Info("MedoSendPlugin context check " + context.Check.ToString());
                        logger.Info("MedoSendPlugin context check CreateMedoContainerAsync run");

                        await context.CreateMedoContainerAsync(cancellationToken).ConfigureAwait(false);
                        logger.Info("MedoSendPlugin AuxiliaryActionAsync run");
                        await context.AuxiliaryActionAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch(Exception e)
                    {
                        logger.Error($"MedoSend Exception\n{e.Message}\n{e.StackTrace}");
                    }
                }
                //else
                //{

                //    logger.Info("MedoSendPlugin context check " + context.Check.ToString());
                //    logger.Info("MedoSendPlugin context check CreateMedoContainerAsync run");

                //    await context.CreateMedoContainerAsync(cancellationToken).ConfigureAwait(false);
                //    logger.Info("MedoSendPlugin AuxiliaryActionAsync run");

                //    await context.AuxiliaryActionAsync(cancellationToken).ConfigureAwait(false);
                //}

                logger.Info($"End send message type {noticeType.GetDescription()} by card {newMessageInfo.ID}");
            }

            logger.Info("End send MEDO message");
        }

        #endregion

        #region Private

        /// <summary>
        ///     получение id карточек входящих и типа уведомлений для отправки
        ///     уведомления МЭДО
        /// </summary>
        /// <returns>id карточки, тип сообщения, id контрагента</returns>
        private static async Task<List<NewMessageInfo>> NewMedoMessageAsync(
            IDbScope dbScope,
            CancellationToken cancellationToken = default)
        {
            await using (dbScope.Create())
            {
                return await dbScope.Db.SetCommand
                    ("SELECT \"ID\", \"RowID\", \"MedoTypeID\", \"MedoPartnerID\", \"MedoXsdVersion\", \"MessageID\", \"ResponseMesID\", \"MedoPartnerName\", \"MedoStatusID\" " +
                        "FROM \"MedoCommonInfo\" " +
                        "WHERE \"MedoStatusID\" = 0  OR \"MedoStatusID\" = 0 OR \"MedoStatusID\" = 0")
                        .LogCommand()
                        .ExecuteListAsync<NewMessageInfo>(cancellationToken);
            }
        }

        private static async Task<MedoPartnerInfo> GetPartnerMedoFormat(
            Guid partnerID,
            IDbScope dbScope,
            CancellationToken cancellationToken = default)
        {
            await using (dbScope.Create())
            {
                DataParameter[] param = new DataParameter[]
                {
                    new DataParameter("@partnerID", partnerID)
                };

                return await dbScope.Db.SetCommand
                    ("SELECT \"MEDOFormatID\", \"MedoID\", \"MedoAddress\" " +
                        "FROM \"Partners\" " +
                        "WHERE \"ID\" = @partnerID", param)
                        .LogCommand()
                        .ExecuteAsync<MedoPartnerInfo>(cancellationToken);
            }
        }

        private async Task<int> UpdateMedoXsdVersionAsync(int version, Guid rowID, IDbScope dbScope)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                var param = new[]
                {
                    new DataParameter("@version", version),
                    new DataParameter("@rowID", rowID)
                };

                return await db.SetCommand
                    ("UPDATE \"MedoCommonInfo\" " +
                            "SET \"MedoXsdVersion\" = @version " +
                            "WHERE \"RowID\" = @rowID", param)
                        .LogCommand()
                        .ExecuteNonQueryAsync();
            }
        }

        #endregion
    }
}