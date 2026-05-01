using System;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Extensions.Default.Shared.Workflow.KrPermissions;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Server.Workflow.KrPermissions
{
    /// <summary>
    /// Данный класс является примером расширений <see cref="IKrPermissionsRuleExtension"/> и <see cref="ICardPermissionsExtension"/>
    /// Чтобы включить данные расширения необходимо из зарегистрировать в <see cref="IExtensionContainer"/>
    /// </summary>
    public sealed class KrPermissionTestExtension : IKrPermissionsRuleExtension, ICardPermissionsExtension
    {
        #region Private Methods

        /// <summary>
        /// Делает select к базе запрашивая сумму документа
        /// </summary>
        /// <param name="dbScope">IDbScope</param>
        /// <param name="cardID">ИД карточки</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Сумму документа</returns>
        private static async Task<decimal?> SelectAmountFromDbAsync(IDbScope dbScope, Guid cardID, CancellationToken cancellationToken = default)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                return await db
                    .SetCommand(
                        dbScope.BuilderFactory
                            .Select().C("Amount")
                            .From("DocumentCommonInfo").NoLock()
                            .Where().C("ID").Equals().P("CardID")
                            .Build(),
                        db.Parameter("CardID", cardID))
                    .LogCommand()
                    .ExecuteAsync<decimal?>(cancellationToken);
            }
        }

        /// <summary>
        /// Проверяет, что указанная сумма находится в диапазоне, указанном в карточке правил доступа
        /// с указанным ИД.
        /// </summary>
        /// <param name="dbScope">IDbScope</param>
        /// <param name="ruleID">ИД проверяемого правила доступа</param>
        /// <param name="amount">Проверяемая на вхождение в диапазон сумма</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Входит ли указанная сумма в диапазон указанной карточки правил доступа</returns>
        private static async Task<bool> CheckOnDbAsync(IDbScope dbScope, Guid ruleID, decimal amount, CancellationToken cancellationToken = default)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                return await db
                    .SetCommand(
                        dbScope.BuilderFactory
                            .Select().AsBit(b => b
                                .Exists(b1 => b1
                                    .Select().V(null)
                                    .From("KrSamplePermissionsExtension").NoLock()
                                    .Where().C("ID").Equals().P("RuleID")
                                    .And(b2 => b2.C("MinAmount").IsNull().Or().C("MinAmount").LessOrEquals().P("Amount"))
                                    .And(b2 => b2.C("MaxAmount").IsNull().Or().C("MaxAmount").GreaterOrEquals().P("Amount"))))
                            .Build(),
                        db.Parameter("RuleID", ruleID),
                        db.Parameter("Amount", amount))
                    .LogCommand()
                    .ExecuteAsync<bool>(cancellationToken);
            }
        }

        #endregion

        #region IKrPermissionsRuleExtension Implementation

        public async Task CheckRuleAsync(IKrPermissionsRuleExtensionContext context)
        {
            switch(context.Mode)
            {
                case KrPermissionsCheckMode.WithCard:
                case KrPermissionsCheckMode.WithStoreCard:
                    {
                        // Если карточка есть, то пытаемся получить ее сумму из самой карточки
                        decimal? amount = context.Card.Sections["DocumentCommonInfo"].Fields.Get<decimal?>("Amount");
                        if (amount.HasValue
                            && !await CheckOnDbAsync(context.DbScope, context.RuleID, amount.Value, context.CancellationToken))
                        {
                            context.Cancel = true;
                        }
                        else if (context.Mode == KrPermissionsCheckMode.WithStoreCard)
                        {
                            goto case KrPermissionsCheckMode.WithCardID;
                        }
                    }
                    break;
                case KrPermissionsCheckMode.WithCardID:
                    {
                        decimal? amount = await SelectAmountFromDbAsync(context.DbScope, context.CardID.Value, context.CancellationToken);
                        if (amount.HasValue
                            && !await CheckOnDbAsync(context.DbScope, context.RuleID, amount.Value, context.CancellationToken))
                        {
                            context.Cancel = true;
                        }
                    }
                    break;

                    // При отсутствии карточки проверку не выполняем
                case KrPermissionsCheckMode.WithoutCard:
                    return;
            }
        }

        #endregion

        #region ICardPermissionsExtension Implementation

        public async Task ExtendPermissionsAsync(IKrPermissionsManagerContext context)
        {
            // При чтении карточки с суммой требуем дополнительно расчета прав на добавление файлов
            if (context.Mode == KrPermissionsCheckMode.WithCard)
            {
                Card card = context.Card;
                if (card.Sections.ContainsKey("DocumentCommonInfo")
                    && card.Sections["DocumentCommonInfo"].Fields.ContainsKey("Amount")
                    && !context.Descriptor.Permissions.Contains(KrPermissionFlagDescriptors.AddFiles))
                {
                    context.Descriptor.StillRequired.Add(KrPermissionFlagDescriptors.AddFiles);
                }
            }
        }

        public async Task IsPermissionsRecalcRequired(IKrPermissionsRecalcContext context)
        {
            if (context.Mode == KrPermissionsCheckMode.WithStoreCard)
            {
                //Если изменилась сумма - требуем перепроверки прав
                Card card = context.Card;
                if (card.Sections.ContainsKey("DocumentCommonInfo")
                    && card.Sections["DocumentCommonInfo"].Fields.ContainsKey("Amount"))
                {
                    context.IsRecalcRequired = true;
                }
            }
        }

        #endregion
    }
}
