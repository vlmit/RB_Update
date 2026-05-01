using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.ComponentModel;
using Tessa.Platform.Data;

namespace Tessa.Extensions.Default.Chronos.Notices
{
    /// <summary>
    /// Тестовый сендер, который сохраняет обработанные сообщения в свойстве SendedMessages
    /// </summary>
    public sealed class TestSender : MailSender
    {
        #region Constructors

        public TestSender(
            ICardServerPermissionsProvider permissionsProvider,
            ICardContentStrategy contentStrategy,
            ICardStreamClientRepository clientRepository,
            ICardRepository extendedRepository,
            IDbScope dbScope,
            IOutboxManager outboxManager)
            : base(permissionsProvider, contentStrategy, clientRepository, extendedRepository, dbScope, outboxManager)
        {
            this.SentMessages = new List<MailSenderMessage>();
        }

        #endregion

        #region Properties

        public List<MailSenderMessage> SentMessages { get; }

        #endregion

        #region Public Methods

        public Task<bool> SendTestEmailAsync(MailSenderMessage mailSenderMessage, CancellationToken cancellationToken = default) =>
            this.SendMessageAsync(mailSenderMessage, cancellationToken);

        #endregion

        #region MailSender Methods

        protected override async Task<bool> SendMessageAsync(MailSenderMessage message, CancellationToken cancellationToken = default)
        {
            this.SentMessages.Add(message);
            return true;
        }

        #endregion
    }
}