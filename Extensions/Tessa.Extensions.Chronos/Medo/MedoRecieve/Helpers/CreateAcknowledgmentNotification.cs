using DocumentFormat.OpenXml.EMMA;
using LinqToDB.Data;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Tessa.Extensions.Shared.Helpers.Medo;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Chronos.Medo.MedoRecieve.Helpers
{
    public class CreateAcknowledgmentNotification
    {
        #region Properties

        private IDbScope DbScope { get; set; }
        private Guid CardID { get; set; }
        private Guid MainMesID { get; set; }
        private Guid MedoPartnerID { get; set; }
        private Guid ResponseID { get; set; }
        private string MedoPartnerName { get; set; }
        private string PartnerMedoID { get; set; }
        private string GlobalPath { get; set; }
        private XsdVersion XsdVersion { get; set; }

        #endregion

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        public CreateAcknowledgmentNotification(AcknowledgmentInfo info)
        {
            this.DbScope = info.DbScope;
            this.CardID = info.CardID;
            this.MainMesID = info.MainMesID;
            this.MedoPartnerID = info.MedoPartnerID;
            this.MedoPartnerName = info.MedoPartnerName;
            //this.XsdVersion = info.XsdVersion == XsdVersion.NewVersion ? XsdVersion.NewVersion : XsdVersion.OldVersion;
        }

        public async Task InitializeAsync()
        {
            logger.Info("InitializeAsync");
            
            var outPath = ConfigurationManager.Settings.TryGet<string>(MedoConst.GlobalOutPathSettingName);
            if (string.IsNullOrEmpty(outPath))
            {
                logger.Error("Cant get GlobalOutPathSettingName from app.json");
                return;
            }

            this.GlobalPath = this.GetOutDirectory(outPath);

            Directory.CreateDirectory(this.GlobalPath);

            this.PartnerMedoID = await GetPartnerMedoID(this.MedoPartnerID, this.DbScope);
            this.XsdVersion = await GetPartnerMedoFormat() == 1 ? XsdVersion.OldVersion : XsdVersion.NewVersion;

            (var notification, var respID) = MedoHelper.CreateAcknowledgMes(
                this.XsdVersion, 
                true, 
                this.MainMesID,
                this.PartnerMedoID,
                this.MedoPartnerName);

            this.ResponseID = respID;

            //Добавление xml в папку
            using (var writer = new XmlTextWriter(Path.Combine(this.GlobalPath, "notification.xml"), new UTF8Encoding(false)))
            {
                notification.Save(writer);
            }

            //Добавление envelope.ini в папку
            await EnvelopeAsync();

            //Создание записи в MedoCommonInfo
            await InsertNewMedoEntryAsync();
        }

        /// <summary>
        /// Создание файла envelope
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task EnvelopeAsync(CancellationToken cancellationToken = default)
        {
            logger.Info("OutshipContainer EnvelopeAsync");

            string partnerMedoAddress = await GetPartnerMedoAddress(this.MedoPartnerID, this.DbScope, cancellationToken);

            var sb = StringBuilderHelper
                     .Acquire()
                     .AppendLine("[ПИСЬМО КП ПС СЗИ]")
                     .AppendLine("АВТООТПРАВКА=1")
                     .AppendLine("ШИФРОВАНИЕ=0")
                     .AppendLine("ЭЦП=0")
                     .AppendLine("ДОСТАВЛЕНО=1")
                     .AppendLine("ПРОЧТЕНО=0")
                     .AppendLine($"ДАТА={DateTime.Now:dd.MM.yyyy HH:mm:ss}")
                     .AppendLine()
                     .AppendLine("[АДРЕСАТЫ]")
                     .AppendLine($"0={partnerMedoAddress}");

            //var i = 0;
            //foreach (var mail in mails)
            //{
            //    sb.AppendLine($"{i++}={mail["PartnerMedoAddress"]}");
            //}

            sb.AppendLine()
              .AppendLine("[ФАЙЛЫ]")
              //.AppendLine("0=document.edc.zip")
              .AppendLine("0=notification.xml");

            await File.WriteAllTextAsync(Path.Combine(this.GlobalPath, "envelope.ini"), sb.ToStringAndRelease(), /*Encoding.UTF8*/ Encoding.GetEncoding(1251), cancellationToken);
        }

        /// <summary>
        /// Получение адреса корреспондента МЭДО
        /// </summary>
        /// <param name="partnerID"></param>
        /// <param name="dbScope"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task<string> GetPartnerMedoAddress(
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
                    ("SELECT \"MedoAddress\" " +
                        "FROM \"Partners\" " +
                        "WHERE \"ID\" = @partnerID", param)
                        .LogCommand()
                        .ExecuteAsync<string>(cancellationToken);
            }
        }

        /// <summary>
        /// Получение GUID корреспондента МЭДО
        /// </summary>
        /// <param name="partnerID"></param>
        /// <param name="dbScope"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task<string> GetPartnerMedoID(
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
                    ("SELECT \"MedoID\" " +
                        "FROM \"Partners\" " +
                        "WHERE \"ID\" = @partnerID", param)
                        .LogCommand()
                        .ExecuteAsync<string>(cancellationToken);
            }
        }

        /// <summary>
        /// Получение имени папки
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GetOutDirectory(string path)
        {
            var str = 
                //this.CardID.ToString().Replace("-", "") + "_" +
                "NotificationAck_" +
                this.MainMesID.ToString().Replace("-", "") + "_" +
                DateTime.Now.ToString("dd_MM_yyyy__hh_mm_ss");
            return Path.Combine(path, str);
        }

        /// <summary>
        /// Добавление новой записи об отправке уведомления
        /// </summary>
        /// <returns></returns>
        private async Task<int> InsertNewMedoEntryAsync()
            //Guid CardID, Guid? MainMesID, MedoPartner partner, XsdVersion xsdVersion, IDbScope dbScope)
        {
            await using (this.DbScope.Create())
            {
                var db = this.DbScope.Db;

                string InsertMedoInfo = "INSERT INTO \"MedoCommonInfo\" " +
                    "(\"ID\", \"RowID\", \"MessageID\", \"DateMessage\", " +
                    "\"MedoStatusID\", \"MedoStatusName\", " +
                    "\"MedoTypeID\", " +
                    "\"MedoTypeName\", " +
                    //"\"MedoComment\", \"MedoError\", " +
                    "\"MedoPartnerID\", \"MedoPartnerFullName\" , \"MedoXsdVersion\", \"ResponseMesID\" )" +
                    "VALUES(@cID, @rowID, @mID, @date, " +
                    "@state, @stName, " +
                    "@typeID, @typeName, " +
                    //"@comment, @error, " +
                    "@partnerID, @partnerName, @medoVersion, @respID)";

                DataParameter[] param = new DataParameter[]
                {
                    new DataParameter("@cID", this.CardID),
                    new DataParameter("@mID", this.MainMesID),
                    new DataParameter("@rowID", Guid.NewGuid()),
                    new DataParameter("@date", DateTime.UtcNow),
                    new DataParameter("@state", 1),
                    new DataParameter("@stName", "Отправлено"),
                    new DataParameter("@typeID", 9),
                    new DataParameter("@typeName", "Квитанция о приеме"),
                    //new DataParameter("@comment", ""),
                    //new DataParameter("@error", ""),
                    new DataParameter("@partnerID", this.MedoPartnerID),
                    new DataParameter("@partnerName", this.MedoPartnerName),
                    new DataParameter("@medoVersion", this.XsdVersion == XsdVersion.NewVersion ? 1 : 0),
                    new DataParameter("@respID", this.ResponseID)
                };

                foreach (var p in param)
                {
                    logger.Info(p.Name + " " + p.Value);
                }
                return await db.SetCommand(InsertMedoInfo, param)
                        .LogCommand()
                        .ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Получение формата МЭДО корреспондента
        /// </summary>
        /// <returns></returns>
        private async Task<int> GetPartnerMedoFormat()
        {
            await using (this.DbScope.Create())
            {
                DataParameter[] param = new DataParameter[]
                {
                    new DataParameter("@partnerID", this.MedoPartnerID)
                };

                return await this.DbScope.Db.SetCommand
                    ("SELECT \"MEDOFormatID\" " +
                        "FROM \"Partners\" " +
                        "WHERE \"ID\" = @partnerID", param)
                        .LogCommand()
                        .ExecuteAsync<int>();
                //.ExecuteListAsync
            }
        }
    }
}
