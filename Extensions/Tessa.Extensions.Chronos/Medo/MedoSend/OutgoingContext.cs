using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using LinqToDB.Common;
using Microsoft.Identity.Client;
using NLog;
using Tessa.Cards;
using Tessa.Cards.Caching;
using Tessa.Extensions.Chronos.Medo.MedoSend.Helpers;
using Tessa.Extensions.Shared.Helpers.Medo;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using Unity;

namespace Tessa.Extensions.Chronos.Medo.MedoSend
{
    public abstract class OutgoingContext
    {
        #region Constructor

        protected OutgoingContext(
            IUnityContainer container,
            IDbScope dbScope,
            string globalPath,
            NewMessageInfo newMessageInfo)
        {
            this.Container = container;
            this.DbScope = dbScope;
            this.ValidationResult = new ValidationResultBuilder();
            this.CardID = newMessageInfo.ID;
            this.RowID = newMessageInfo.RowID;
            this.MainMesID = newMessageInfo.MessageID == Guid.Empty ? Guid.NewGuid() : newMessageInfo.MessageID;
            this.Type = (NoticeType)newMessageInfo.MedoTypeID;
            this.GlobalPath = this.GetOutDirectory(globalPath);
            this.ArchivePath = this.GetOutDirectory("/opt/medo/ArchiveOUT/");
            this.MedoPartnerID = newMessageInfo.MedoPartnerID.GetValueOrDefault();
            this.MedoPartnerName = newMessageInfo.MedoPartnerName;
            this.XsdVersion = (XsdVersion) newMessageInfo.MedoXsdVersion;
            this.Ns = this.XsdVersion == XsdVersion.OldVersion ? MedoConst.Ns27 : MedoConst.Ns271;
            this.Xdms = this.XsdVersion == XsdVersion.OldVersion ? MedoConst.Xdms27 : MedoConst.Xdms271;
            this.Check = this.CheckInfo();
            this.CardCache = container.Resolve<ICardCache>();
        }

        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private string GetOutDirectory(string path)
        {
            //var partnerName = this.MedoPartnerName;

            //if (Regex.Match(partnerName, "[А-Яа-яЁё]").Success)
            //{
            //    partnerName = Transliteration.Translit(partnerName);
            //}

            //Regex.Replace(partnerName, @"[^0-9a-zA-Z_]+", "_");
            //Regex.Replace(partnerName, @"[.-]", "_");


            //StringBuilder sb = new StringBuilder();

            //sb.Append(this.CardID.ToString().Replace("-", ""));
            //sb.Append('_');
            //sb.Append(partnerName);

            string str = "";

            var tempType = this.Type;

            switch (this.Type)
            {
                case NoticeType.Registered:
                    str = "NotificationRegistered_" +
                        this.MainMesID.ToString().Replace("-", "") + "_" +
                        DateTime.UtcNow.AddHours(8).ToString("dd_MM_yyyy__HH_mm_ss");
                    break;
                case NoticeType.RegDenied:
                    str = "NotificationRefused_" +
                        this.MainMesID.ToString().Replace("-", "") + "_" +
                        DateTime.UtcNow.AddHours(8).ToString("dd_MM_yyyy__HH_mm_ss");
                    break;
                case NoticeType.Document or _:
                    str = this.CardID.ToString().Replace("-", "") + "_" +
                        this.MainMesID.ToString().Replace("-", "") + "_" +
                        DateTime.UtcNow.AddHours(8).ToString("dd_MM_yyyy__HH_mm_ss");
                    break;
            }

            //var str = this.CardID.ToString().Replace("-", "") + "_" + 
            //    this.MainMesID.ToString().Replace("-", "") + "_" + 
            //    DateTime.UtcNow.AddHours(8).ToString("dd_MM_yyyy__hh_mm_ss");
            return Path.Combine(path, str);
        }

        #endregion
        
        #region Fields

        protected IUnityContainer Container { get; }

        protected IDbScope DbScope { get; }

        protected IValidationResultBuilder ValidationResult { get; }

        protected ICardCache CardCache { get; }

        protected string ArchivePath { get; set; }

        /// <summary>
        ///     первичная проверка сообщения
        /// </summary>
        public bool Check { get; }

        /// <summary>
        ///     путь к глобальной папке мэдо
        /// </summary>
        protected string GlobalPath { get; set; }

        /// <summary>
        ///     путь к xsd схеме паспорта
        /// </summary>
        protected string ContainerPath { get; private set; }

        /// <summary>
        ///     путь к xsd схеме паспорта
        /// </summary>
        protected string CommunicationPath { get; private set; }

        /// <summary>
        ///     ID сообщения
        /// </summary>
        protected Guid MainMesID { get; }
        
        /// <summary>
        ///     ID контрагента в МЭДО
        /// </summary>
        protected Guid MedoPartnerID { get; }

        /// <summary>
        ///     Имя контрагента МЭДО
        /// </summary>
        protected string MedoPartnerName { get; }

        /// <summary>
        ///     ID карточки
        /// </summary>
        protected Guid CardID { get; }
        
        /// <summary>
        ///     ID строки
        /// </summary>
        protected Guid RowID { get; }

        /// <summary>
        ///     Тип уведомления
        /// </summary>
        protected NoticeType Type { get; }
        
        /// <summary>
        ///     Версия схемы
        /// </summary>
        protected XsdVersion XsdVersion { get; }
        
        protected XNamespace Ns { get; }

        protected XNamespace Xdms { get; }

        #endregion
        
        #region Protected
        
        protected abstract Task EnvelopeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     получаем карточку
        /// </summary>
        /// <param name="permissionsProvider">ICardServerPermissionsProvider</param>
        /// <param name="cardRepository">ICardRepository</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<Card> GetCardAsync(
            ICardServerPermissionsProvider permissionsProvider,
            ICardRepository cardRepository,
            CancellationToken cancellationToken = default)
        {
            var request = new CardGetRequest { CardID = this.CardID };
            permissionsProvider.SetFullPermissions(request);

            var response = await cardRepository.GetAsync(request, cancellationToken);
            if (!response.ValidationResult.IsSuccessful())
            {
                logger.Error($"validationresult: {response.ValidationResult}");
                this.ValidationResult.Add(response.ValidationResult);
                return null;
            }

            return response.Card;
        }

        #endregion
        
        #region Private

        /// <summary>
        ///     проверяем xml и получаем пути к файлам из app.json
        /// </summary>
        /// <returns></returns>
        private bool CheckInfo()
        {
            logger.Info("OutgoingContext Check info, xsd version: " + this.XsdVersion.ToString());
            switch (this.XsdVersion)
            {
                case XsdVersion.OldVersion:
                    this.ContainerPath = ConfigurationManager.Settings.TryGet<string>(MedoConst.ContainerSettingName);
                    this.CommunicationPath = ConfigurationManager.Settings.TryGet<string>(MedoConst.CommunicationSettingName);
                    break;
                case XsdVersion.NewVersion:
                    this.ContainerPath = ConfigurationManager.Settings.TryGet<string>(MedoConst.ContainerSettingName271);
                    this.CommunicationPath = ConfigurationManager.Settings.TryGet<string>(MedoConst.CommunicationSettingName271);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(this.XsdVersion));
            }

            if (this.ContainerPath.IsNullOrEmpty()
                || this.CommunicationPath.IsNullOrEmpty())
            {
                this.ValidationResult.AddError(this, "Cant get xsd scheme path from app.json");
                return false;
            }

            return true;
        }

        #endregion

        #region Abstract

        public abstract Task CreateMedoContainerAsync(CancellationToken cancellationToken = default);

        public abstract Task AuxiliaryActionAsync(CancellationToken cancellationToken = default);
        
        public virtual ValueTask InitializeAsync(CancellationToken cancellationToken = default)
        {
            return new ValueTask();
        }

        #endregion
    }
}