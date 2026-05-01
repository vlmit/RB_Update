using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.ComponentModel;
using Tessa.Extensions.Default.Server.Workflow.KrPermissions;
using Tessa.Extensions.Default.Shared.Workflow.KrPermissions;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform;
using Tessa.Platform.Runtime;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.Workflow.KrProcess
{
    /// <summary>
    /// Объект, обеспечивающий создание и валидацию токена безопасности для типового решения.
    /// </summary>
    /// <remarks>
    /// Наследники класса могут переопределить методы и изменить свойства,
    /// в т.ч. срок жизни выписанного токена <see cref="TokenExpirationTimeSpan"/>.
    /// 
    /// Зарегистрируйте наследник по интерфейсу <see cref="IKrTokenProvider"/>, указав в атрибуте
    /// <c>[Registrator(Order = 1)]</c>, чтобы переопределить стандартную регистрацию.
    /// </remarks>
    public class KrTokenProvider :
        IKrTokenProvider
    {
        #region Fields

        private readonly IKrPermissionsCacheContainer permissionsCacheContainer;

        #endregion

        #region Constructors

        public KrTokenProvider(
            ISignatureProvider signatureProvider,
            IKrPermissionsCacheContainer permissionsCacheContainer)
        {
            this.SignatureProvider = signatureProvider ?? throw new ArgumentNullException(nameof(signatureProvider));
            this.permissionsCacheContainer = permissionsCacheContainer;
        }

        #endregion

        #region Protected Properties

        protected ISignatureProvider SignatureProvider { get; set; }

        /// <summary>
        /// Срок жизни выписанного токена при условии, что он не будет пересчитываться при изменении версии карточки
        /// и расширение на права доступа не укажет, что токен требуется пересчитать.
        /// </summary>
        protected TimeSpan TokenExpirationTimeSpan { get; set; } = TimeSpan.FromHours(2.0);

        #endregion

        #region IKrTokenValidator Members

        /// <summary>
        /// Создаёт подписанный токен безопасности для заданной информации по карточке
        /// с указанием прав для процесса согласования.
        /// </summary>
        /// <param name="cardID">Идентификатор карточки, для которой требуется создать токен безопасности.</param>
        /// <param name="cardVersion">
        /// Номер версии карточки, для которой требуется создать токен безопасности.<para/>
        /// 
        /// При выписывании токена на сервере можно указать <see cref="CardComponentHelper.DoNotCheckVersion"/>,
        /// чтобы не проверять номер версии карточки (т.е. чтобы токен подходил для любой версии).<para/>
        /// 
        /// Не допускайте передачу такого токена до клиента!
        /// </param>
        /// <param name="permissionsVersion">
        /// Номер версии правил доступа для которой создается токен безопасности. 
        /// Если при проверке правил доступа номер версии в токене будет отличаться от текущей, то токен не будет учитываться при проверке прав
        /// </param>
        /// <param name="permissions">
        /// Права на карточку, сохраняемые в токене безопасности. 
        /// Если не задана, устанавливаются права <see cref="KrPermissionFlagDescriptors.Full"/>
        /// </param>
        /// <param name="extendedCardSettings">
        /// Расширенные настройки прав по карточке
        /// </param>
        /// <returns>Токен безопасности, полученный для заданной информации по карточке.</returns>
        public virtual KrToken CreateToken(
            Guid cardID,
            int cardVersion = CardComponentHelper.DoNotCheckVersion,
            long permissionsVersion = CardComponentHelper.DoNotCheckVersion,
            ICollection<KrPermissionFlagDescriptor> permissions = null,
            IKrPermissionExtendedCardSettings extendedCardSettings = null,
            Action<KrToken> modifyTokenAction = null)
        {
            if (cardVersion < 0 && cardVersion != CardComponentHelper.DoNotCheckVersion)
            {
                throw new ArgumentOutOfRangeException(nameof(cardVersion));
            }

            var token = new KrToken
            {
                PermissionsVersion = permissionsVersion,
                CardID = cardID,
                CardVersion = cardVersion,
                ExpiryDate = DateTime.UtcNow.Add(this.TokenExpirationTimeSpan),
                Permissions = permissions ?? KrPermissionFlagDescriptors.Full.IncludedPermissions,
            };

            if (extendedCardSettings != null)
            {
                token.ExtendedCardSettings = extendedCardSettings;
            }

            modifyTokenAction?.Invoke(token);

            byte[] serializedToken = Encoding.UTF8.GetBytes(token.ToTypedJson());
            byte[] signature = this.SignatureProvider.Sign(serializedToken);

            token.Signature = RuntimeHelper.ConvertKeyToString(signature);
            return token;
        }


        /// <summary>
        /// Создаёт подписанный токен безопасности для заданной карточки
        /// с указанием прав для процесса согласования.
        /// </summary>
        /// <param name="card">Карточка, для которой требуется создать токен безопасности.</param>
        /// <param name="permissionsVersion">
        /// Номер версии правил доступа для которой создается токен безопасности. 
        /// Если при проверке правил доступа номер версии в токене будет отличаться от текущей, то токен не будет учитываться при проверке прав
        /// </param>
        /// <param name="permissions">
        /// Права на карточку, сохраняемые в токене безопасности. 
        /// Если не задана, устанавливаются права <see cref="KrPermissionFlagDescriptors.Full"/>
        /// </param>
        /// <param name="extendedCardSettings">
        /// Расширенные настройки прав по карточке
        /// </param>
        /// <returns>Токен безопасности, полученный для заданной карточки.</returns>
        public virtual KrToken CreateToken(
            Card card,
            long permissionsVersion = CardComponentHelper.DoNotCheckVersion,
            ICollection<KrPermissionFlagDescriptor> permissions = null,
            IKrPermissionExtendedCardSettings extendedCardSettings = null,
            Action<KrToken> modifyTokenAction = null) =>
                this.CreateToken(
                    (card ?? throw new ArgumentNullException(nameof(card))).ID, 
                    card.Version,
                    permissionsVersion,
                    permissions, 
                    extendedCardSettings, 
                    modifyTokenAction);


        /// <summary>
        /// Выполняет проверку валидности токена безопасности, что гарантирует его неизменность с момента подписания.
        /// Возвращает признак того, что токен успешно прошёл все проверки.
        /// </summary>
        /// <param name="card">Карточка, для которой был получен токен.</param>
        /// <param name="token">Токен, полученный для карточки.</param>
        /// <param name="validationResult">
        /// Результат валидации, в который будет записано сообщение об ошибке,
        /// или <c>null</c>, если не требуется получать результат в виде сообщений,
        /// достаточно признака успешности, возвращаемого методом.
        /// </param>
        /// <param name="cancellationToken">Токен для отмены асинхронной операции</param>
        /// <returns>
        /// <c>true</c>, если токен валиден и не был изменён с момента его подписания;
        /// <c>false</c> в противном случае.
        /// </returns>
        public async virtual ValueTask<KrTokenValidationResult> ValidateTokenAsync(
            Card card,
            KrToken token,
            IValidationResultBuilder validationResult = null,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(card, nameof(card));
            Check.ArgumentNotNull(token, nameof(token));

            if (validationResult == null)
            {
                validationResult = new FakeValidationResultBuilder();
            }

            token.Validate(validationResult);

            if (!validationResult.IsSuccessful())
            {
                return KrTokenValidationResult.Fail;
            }

            Guid tokenCardID = token.CardID;
            if (tokenCardID != Guid.Empty && tokenCardID != card.ID)
            {
                validationResult.AddError(this, "$KrMessages_TokenForOtherCard", tokenCardID, card.ID);
                return KrTokenValidationResult.Fail;
            }

            KrToken tokenClone = token.Clone();
            tokenClone.Signature = null;

            byte[] serializedToken = Encoding.UTF8.GetBytes(tokenClone.ToTypedJson());
            byte[] signature = RuntimeHelper.ConvertKeyFromString(token.Signature);

            if (!this.SignatureProvider.Verify(serializedToken, signature))
            {
                validationResult.AddError(this, "$KrMessages_TokenIncorrectlySigned");
                return KrTokenValidationResult.Fail;
            }

            // подпись валидна и выписана для той же карточки, но может отличаться версия или время жизни токена
            // в этом случае мы указываем NeedRecreating, что при загрузке карточки даёт права только на чтение,
            // в остальных случаях означает пересоздание независимо от прав
            int tokenVersion = token.CardVersion;
            if (tokenVersion != CardComponentHelper.DoNotCheckVersion
                && tokenVersion != card.Version)
            {
                // молча пересчитываем токен без сообщений
                return KrTokenValidationResult.NeedRecreating;
            }

            if (token.ExpiryDate.ToUniversalTime() <= DateTime.UtcNow)
            {
                return KrTokenValidationResult.NeedRecreating;
            }

            long permissionsversion = token.PermissionsVersion;
            if (permissionsversion != CardComponentHelper.DoNotCheckVersion
                && permissionsversion != await permissionsCacheContainer.GetVersionAsync(cancellationToken))
            {
                return KrTokenValidationResult.NeedRecreating;
            }

            return KrTokenValidationResult.Success;
        }

        #endregion
    }
}
