using System;
using System.IO;
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

namespace Tessa.Extensions.Chronos.Medo.MedoRecieve
{
    public abstract class IncomingContext
    {
        #region Constructor

        protected IncomingContext(
            IUnityContainer container,
            IDbScope dbScope,
            string globalPath,
            NewMessageInfo newMessageInfo
            )
        {
            this.Container = container;
            this.DbScope = dbScope;
            this.ValidationResult = new ValidationResultBuilder();
            this.CardID = newMessageInfo.ID;
            this.RowID = newMessageInfo.RowID;
            this.MainMesID = newMessageInfo.MessageID == Guid.Empty ? Guid.NewGuid() : newMessageInfo.MessageID;
            this.GlobalPath = this.GetOutDirectory(globalPath);
            this.Type = (NoticeType)newMessageInfo.MedoTypeID;
            this.MedoPartnerID = newMessageInfo.MedoPartnerID.GetValueOrDefault();
            this.XsdVersion = (XsdVersion)newMessageInfo.MedoXsdVersion;
            this.Ns = this.XsdVersion == XsdVersion.OldVersion ? MedoConst.Ns27 : MedoConst.Ns271;
            this.Xdms = this.XsdVersion == XsdVersion.OldVersion ? MedoConst.Xdms27 : MedoConst.Xdms271;
            this.Check = this.CheckInfo();
            this.CardCache = container.Resolve<ICardCache>();
        }

        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private string GetOutDirectory(string path)
        {
            var str = this.CardID.ToString().Replace("-", "") + " " + this.MainMesID + " " + DateTime.Now;
            return Path.Combine(path, str);
        }

        #endregion

        #region Fields

        protected IUnityContainer Container { get; }

        protected IDbScope DbScope { get; }

        protected IValidationResultBuilder ValidationResult { get; }

        protected ICardCache CardCache { get; }

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
