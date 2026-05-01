using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.ComponentModel;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared;
using Tessa.Extensions.Default.Shared.Workflow.KrPermissions;
using Tessa.Platform;
using Tessa.Platform.Collections;
using Tessa.Platform.Conditions;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using Tessa.Roles;
using Unity;

namespace Tessa.Extensions.Default.Server.Workflow.KrPermissions
{
    /// <summary>
    /// При сохранении карточки "Правила доступа" прописывает флаг IsContext для всех ролей,
    /// а также выполняет изменение строковых полей для представления во вложенном сохранении.
    /// </summary>
    public sealed class KrPermissionsRulesStoreExtension :
        CardStoreExtension
    {
        #region Constructors

        public KrPermissionsRulesStoreExtension(
            [Dependency(CardRepositoryNames.ExtendedWithoutTransaction)]
            ICardRepository extendedWithoutTransaction,
            ICardGetStrategy getStrategy,
            IKrPermissionsCacheContainer permissionsCache,
            IKrPermissionsLockStrategy lockStrategy,
            ICardMetadata cardMetadata,
            IConditionTypesProvider conditionTypesProvider)
        {
            this.extendedWithoutTransaction = extendedWithoutTransaction ?? throw new ArgumentNullException(nameof(extendedWithoutTransaction));
            this.getStrategy = getStrategy ?? throw new ArgumentNullException(nameof(getStrategy));
            this.permissionsCache = permissionsCache ?? throw new ArgumentNullException(nameof(permissionsCache));
            this.lockStrategy = lockStrategy ?? throw new ArgumentNullException(nameof(lockStrategy));
            this.cardMetadata = cardMetadata ?? throw new ArgumentNullException(nameof(cardMetadata));
            this.conditionTypesProvider = conditionTypesProvider ?? throw new ArgumentNullException(nameof(conditionTypesProvider));
        }

        #endregion

        #region Fields

        private readonly ICardRepository extendedWithoutTransaction;
        private readonly ICardGetStrategy getStrategy;
        private readonly IKrPermissionsCacheContainer permissionsCache;
        private readonly IKrPermissionsLockStrategy lockStrategy;
        private readonly ICardMetadata cardMetadata;
        private readonly IConditionTypesProvider conditionTypesProvider;

        private const string LockKey = CardHelper.SystemKeyPrefix + "KrPermissionsLock";

        #endregion

        #region Constants

        private const string SkipPermissionsStoreKey = "SkipPermissionsStore";

        #endregion

        #region Private Methods

        private void UpdateTextFields(Card card, IValidationResultBuilder validationResult)
        {
            StringBuilder text = StringBuilderHelper.Acquire(128);

            StringDictionaryStorage<CardSection> sections = card.Sections;
            CardSection permissionsSection = sections["KrPermissions"];
            Dictionary<string, object> permissionsFields = permissionsSection.RawFields;

            bool hasCreating = permissionsFields.Get<bool>(KrPermissionFlagDescriptors.CreateCard.SqlName);

            foreach (var permission in KrPermissionFlagDescriptors.Full.IncludedPermissions.OrderBy(x => x.Order))
            {
                if (!permission.IsVirtual)
                {
                    if (permissionsFields.Get<bool>(permission.SqlName))
                    {
                        text.Append("{").Append(permission.Description).AppendLine("}");
                    }
                }
            }

            //Достаточно, наверно, просто проверить что текстовое поле не было заполнено
            if (text.Length == 0)
            {
                validationResult.AddWarning(this, "$KrMessages_PermissionsNotSpecified");
            }

            permissionsSection.Fields["Permissions"] = text.ToString().TrimEnd();

            //забить текстовые поля для представлений
            //Типы
            text.Clear();

            //Если тип был удален и добавлен снова
            CardSection permissionTypes = sections["KrPermissionTypes"];

            foreach (CardRow row in permissionTypes.Rows)
            {
                if (row.State != CardRowState.Deleted)
                {
                    string typeCaption = row.Get<string>("TypeCaption");
                    if (typeCaption.StartsWith("$", StringComparison.Ordinal))
                    {
                        typeCaption = "{" + typeCaption + "}";
                    }

                    text.AppendLine(typeCaption);
                }
            }

            permissionsSection.Fields["Types"] = text.ToString().TrimEnd();

            //Состояния
            text.Clear();

            CardSection permissionsStates = sections["KrPermissionStates"];

            foreach (CardRow row in permissionsStates.Rows)
            {
                if (row.State != CardRowState.Deleted)
                {
                    string stateName = row.Get<string>("StateName");
                    if (stateName.StartsWith("$", StringComparison.Ordinal))
                    {
                        stateName = "{" + stateName + "}";
                    }

                    text.AppendLine(stateName);
                }
            }

            //Достаточно, наверно, просто проверить что текстовое поле не было заполнено
            if (!hasCreating && string.IsNullOrWhiteSpace(text.ToString()))
            {
                validationResult.AddWarning(this, "$KrMessages_StatesNotSpecified");
            }

            permissionsSection.Fields["States"] = text.ToString().TrimEnd();

            //Роли
            text.Clear();

            CardSection permissionsRoles = sections["KrPermissionRoles"];

            foreach (CardRow row in permissionsRoles.Rows)
            {
                if (row.State != CardRowState.Deleted)
                {
                    string roleName = row.Get<string>("RoleName");
                    if (roleName.StartsWith("$", StringComparison.Ordinal))
                    {
                        roleName = "{" + roleName + "}";
                    }

                    text.AppendLine(roleName);
                }
            }

            permissionsSection.Fields["Roles"] = text.ToString().TrimEnd();

            text.Release();
        }

        /// <summary>
        /// Устанавливает признак контекстные ли роли на секцию из списка с определенным названием
        /// </summary>
        /// <param name="sectionName">Имя табличной секции, в которой находятся ссылочные колонки
        /// ролей, для каждой из которых нужно проставить признак контекстная ли роль</param>
        /// <param name="sections">Секции карточки</param>
        /// <param name="dbScope"></param>
        /// <param name="cancellationToken">Токен отмены асинхронной операции</param>
        public static async Task SetIsContextForAllRoles(
            string sectionName,
            StringDictionaryStorage<CardSection> sections,
            IDbScope dbScope,
            CancellationToken cancellationToken = default)
        {
            ListStorage<CardRow> rows;
            if (!sections.TryGetValue(sectionName, out CardSection rolesSection)
                || (rows = rolesSection.TryGetRows()) == null
                || rows.Count == 0)
            {
                return;
            }

            Dictionary<Guid, bool> isContextByRoleID = null;
            foreach (CardRow row in rows)
            {
                if (row.State == CardRowState.Inserted
                    || row.State == CardRowState.Modified)
                {
                    isContextByRoleID ??= new Dictionary<Guid, bool>();

                    Guid? roleID = row.TryGet<Guid?>("RoleID");
                    if (roleID.HasValue)
                    {
                        if (!isContextByRoleID.TryGetValue(roleID.Value, out bool roleIsContext))
                        {
                            DbManager db = dbScope.Db;
                            var builderFactory = dbScope.BuilderFactory;

                            Guid roleTypeID = await db
                                .SetCommand(
                                    builderFactory
                                        .Select().C("TypeID")
                                        .From("Instances").NoLock()
                                        .Where().C("ID").Equals().P("RoleID")
                                        .Build(),
                                    db.Parameter("RoleID", roleID.Value))
                                .LogCommand()
                                .ExecuteAsync<Guid>(cancellationToken);

                            roleIsContext = roleTypeID == RoleHelper.ContextRoleTypeID;
                            isContextByRoleID.Add(roleID.Value, roleIsContext);
                        }

                        row["IsContext"] = BooleanBoxes.Box(roleIsContext);
                    }
                }
            }
        }

        private async Task UpdateConditionsAsync(ICardStoreExtensionContext context)
        {
            // Алгортим сохранения
            // 1. Проверяем наличие изменений секций с условиями. Если есть, продолжаем
            // 2. Загружаем текущие настройки и десериализуем
            // 3. Мержим изменения
            // 4. Сериализуем настройки и записываем в поле карточки
            var mainCard = context.Request.Card;
            HashSet<string> checkSections = new HashSet<string>() { ConditionHelper.ConditionSectionName };

            var conditionBaseType = await this.cardMetadata.GetMetadataForTypeAsync(ConditionHelper.ConditionsBaseTypeID, context.CancellationToken);
            var sections = await conditionBaseType.GetSectionsAsync(context.CancellationToken);
            checkSections.AddRange(sections.Select(x => x.Name));

            if (mainCard.Sections.Any(x => checkSections.Contains(x.Key)))
            {
                await using (context.DbScope.Create())
                {
                    var db = context.DbScope.Db;
                    var oldSettings =
                        await db.SetCommand(
                                context.DbScope.BuilderFactory
                                    .Select().Top(1).C("Conditions")
                                    .From("KrPermissions").NoLock()
                                    .Where().C("ID").Equals().P("CardID")
                                    .Limit(1)
                                    .Build(),
                                db.Parameter("CardID", mainCard.ID))
                            .LogCommand()
                            .ExecuteAsync<string>(context.CancellationToken);

                    var oldCard = new Card();
                    oldCard.Sections.GetOrAdd("KrPermissions").RawFields["Conditions"] = oldSettings;
                    await ConditionHelper.DeserializeConditionsToEntrySectionAsync(
                        oldCard,
                        cardMetadata,
                        this.conditionTypesProvider,
                        "KrPermissions",
                        "Conditions",
                        false,
                        context.CancellationToken);

                    foreach (var section in mainCard.Sections.Values)
                    {
                        if (checkSections.Contains(section.Name))
                        {
                            var oldSection = oldCard.Sections.GetOrAdd(section.Name);
                            oldSection.Type = section.Type;

                            CardHelper.MergeSection(section, oldSection);
                            mainCard.Sections.Remove(section.Name);
                        }
                    }

                    var conditionsSection = oldCard.Sections.GetOrAddTable(ConditionHelper.ConditionSectionName);

                    foreach (var conditionRow in conditionsSection.Rows)
                    {
                        await ConditionHelper.SerializeConditionRowAsync(
                            conditionRow,
                            oldCard,
                            DefaultCardTypes.KrPermissionsTypeID,
                            this.cardMetadata,
                            this.conditionTypesProvider,
                            true,
                            context.CancellationToken);
                    }

                    mainCard.Sections.GetOrAdd("KrPermissions").RawFields["Conditions"] =
                        StorageHelper.SerializeToTypedJson((List<object>) conditionsSection.Rows.GetStorage(), false);
                }
            }
        }

        #endregion

        #region Base Overrides

        public override async Task BeforeRequest(ICardStoreExtensionContext context)
        {
            if (context.Request.Info.TryGet<bool>(KrPermissionsHelper.DropPermissionsCacheMark))
            {
                context.Request.ForceTransaction = true;
            }

            if (context.Method == CardStoreMethod.Default)
            {
                await UpdateConditionsAsync(context);
            }

            Card card;
            StringDictionaryStorage<CardSection> sections;
            if (context.Request.Info.TryGet<bool>(SkipPermissionsStoreKey)
                || context.Request.Method != CardStoreMethod.Default
                && context.Request.Method != CardStoreMethod.Import
                || !context.ValidationResult.IsSuccessful()
                || (card = context.Request.TryGetCard()) == null
                || (sections = card.TryGetSections()) == null)
            {
                return;
            }

            // заполняем строковые поля при первом сохранении карточки или при любом импорте
            if (card.StoreMode == CardStoreMode.Insert)
            {
                this.UpdateTextFields(card, context.ValidationResult);
            }

            if (context.Request.Method != CardStoreMethod.Default)
            {
                return;
            }

            // определяем, являются ли роли контекстными
            await using (context.DbScope.Create())
            {
                await SetIsContextForAllRoles(
                    "KrPermissionRoles",
                    sections,
                    context.DbScope,
                    context.CancellationToken);
            }
        }

        public override async Task AfterBeginTransaction(ICardStoreExtensionContext context)
        {
            if (context.Request.Info.TryGet<bool>(KrPermissionsHelper.DropPermissionsCacheMark))
            {
                await lockStrategy.ClearLocksAsync(context.CancellationToken);
            }

            var lockObject = await lockStrategy.TryObtainWriterLockAsync(context.CancellationToken);
            if (lockObject is null)
            {
                context.ValidationResult.AddError(
                    this,
                    "$KrPermissions_PermissionsStoreErrorMessage");
            }
            else
            {
                context.Info[LockKey] = lockObject;
                await permissionsCache.UpdateVersionAsync(context.CancellationToken);
            }
        }

        public override async Task BeforeCommitTransaction(ICardStoreExtensionContext context)
        {
            // если поменялась любая секция при изменении уже созданной карточки,
            // то выполняем расчёт текстовых свойств и повторное сохранение
            Card card;
            StringDictionaryStorage<CardSection> sections;
            if (context.Request.Info.TryGet<bool>(SkipPermissionsStoreKey)
                || !context.ValidationResult.IsSuccessful()
                || (card = context.Request.TryGetCard()) == null
                || (sections = card.TryGetSections()) == null
                || sections.Count == 0)
            {
                return;
            }

            // Вся последующая логика выполняется только при базовом сохранении уже сохраненной карточки
            if (context.Method is not CardStoreMethod.Default and not CardStoreMethod.Import
                || card.StoreMode == CardStoreMode.Insert)
            {
                return;
            }

            CardGetContext getContext = await this.getStrategy
                .TryLoadCardInstanceAsync(
                    card.ID,
                    context.DbScope.Db,
                    context.CardMetadata,
                    context.ValidationResult,
                    cancellationToken: context.CancellationToken);

            if (getContext == null)
            {
                return;
            }

            if (!await this.getStrategy.LoadSectionsAsync(getContext, context.CancellationToken))
            {
                return;
            }

            Card updatedCard = getContext.Card;
            this.UpdateTextFields(updatedCard, context.ValidationResult);

            if (updatedCard.HasChanges())
            {
                updatedCard.RemoveAllButChanged();

                var storeRequest = new CardStoreRequest { Card = updatedCard, Info = { [SkipPermissionsStoreKey] = BooleanBoxes.True } };
                CardStoreResponse storeResponse = await this.extendedWithoutTransaction.StoreAsync(storeRequest, context.CancellationToken);
                context.ValidationResult.Add(storeResponse.ValidationResult);
            }
        }

        public override async Task AfterRequestFinally(ICardStoreExtensionContext context)
        {
            if (context.Info.TryGetValue(LockKey, out var lockObject)
                && lockObject is IAsyncDisposable lockSlim)
            {
                await lockSlim.DisposeAsync();
            }
        }

        #endregion
    }
}
