using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Extensions.Default.Server.Files.VirtualFiles.Compilation;
using Tessa.Extensions.Default.Shared;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Files;
using Tessa.Localization;
using Tessa.Platform.Conditions;
using Tessa.Platform.Data;
using Tessa.Platform.Placeholders;
using Tessa.Platform.Runtime;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using Unity;

namespace Tessa.Extensions.Default.Server.Files.VirtualFiles
{
    /// <inheritdoc cref="IKrVirtualFileManager" />
    public sealed class KrVirtualFileManager : IKrVirtualFileManager
    {
        #region Fields

        private readonly ISession session;
        private readonly IDbScope dbScope;
        private readonly ICardRepository cardRepository;
        private readonly IConfigurationInfoProvider configurationInfoProvider;
        private readonly ICardServerPermissionsProvider permissionProvider;
        private readonly ICardFileManager fileManager;
        private readonly IKrVirtualFileCache virtualFileCache;
        private readonly IKrVirtualFileCompilationCache compilationCache;
        private readonly Func<IPlaceholderManager> placeholderManagerFactory;
        private readonly IConditionCompilationCache conditionCache;
        private readonly IConditionExecutor conditionExecutor;
        private readonly IUnityContainer container;

        #endregion

        #region Constructors

        public KrVirtualFileManager(
            ISession session,
            IDbScope dbScope,
            ICardRepository cardRepository,
            IConfigurationInfoProvider configurationInfoProvider,
            ICardServerPermissionsProvider permissionsProvider,
            ICardFileManager fileManager,
            IKrVirtualFileCache virtualFileCache,
            IKrVirtualFileCompilationCache compilationCache,
            Func<IPlaceholderManager> placeholderManagerFactory,
            IConditionCompilationCache conditionCache,
            IConditionExecutor conditionExecutor,
            IUnityContainer container)
        {
            this.session = session;
            this.dbScope = dbScope;
            this.cardRepository = cardRepository;
            this.configurationInfoProvider = configurationInfoProvider ?? throw new ArgumentNullException(nameof(configurationInfoProvider));
            this.permissionProvider = permissionsProvider;
            this.fileManager = fileManager;
            this.virtualFileCache = virtualFileCache;
            this.compilationCache = compilationCache;
            this.placeholderManagerFactory = placeholderManagerFactory;
            this.conditionCache = conditionCache;
            this.conditionExecutor = conditionExecutor;
            this.container = container;
        }

        #endregion

        #region IKrVirtualFileManager Implementation

        /// <inheritdoc />
        public ValueTask<string> GetSuggestedFileNameAsync(
            string templateFileName,
            Card card = null,
            Guid? cardID = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(templateFileName))
            {
                return new(templateFileName);
            }

            return new(this.ReplacePlaceholderAsync(card, cardID, templateFileName, cancellationToken));
        }

        /// <inheritdoc />
        public async Task<ValidationResult> FillCardWithFilesAsync(
            Card card,
            CancellationToken cancellationToken = default)
        {
            // Если для данного типа
            if (!(await virtualFileCache.GetAllowedTypesAsync(cancellationToken))
                .Contains(card.TypeID))
            {
                return ValidationResult.Empty;
            }

            var validationResult = new ValidationResultBuilder();
            var virtualFiles = await GetVirtualFilesAsync(card, validationResult, cancellationToken);
            if (virtualFiles.Count == 0)
            {
                return validationResult.Build();
            }

            var compilationResult = await compilationCache.GetAsync(cancellationToken);
            validationResult.Add(compilationResult.ValidationResult.ConvertToSuccessful());
            if (!compilationResult.ValidationResult.IsSuccessful)
            {
                return validationResult.Build();
            }

            await using var fileContainer = await this.fileManager.CreateContainerAsync(card, cancellationToken: cancellationToken);
            validationResult.Add(fileContainer.CreationResult);

            foreach (var virtualFile in virtualFiles)
            {
                VirtualFileVersion[] versions = new VirtualFileVersion[virtualFile.Versions.Count];
                for (int i = 0; i < virtualFile.Versions.Count; i++)
                {
                    var version = virtualFile.Versions[i];
                    versions[i] =
                        new VirtualFileVersion(
                            version.ID,
                            await this.ReplacePlaceholderAsync(card, null, version.Name, cancellationToken));
                }

                var newFile = await fileContainer.FileContainer.AddVirtualAsync(
                    new VirtualFile(
                        DefaultFileTypes.KrVirtualFile,
                        virtualFile.ID,
                        await this.ReplacePlaceholderAsync(card, null, virtualFile.Name, cancellationToken),
                        async (token, ct) => token.Category = virtualFile.FileCategory),
                    versions: versions,
                    cancellationToken: cancellationToken);

                var cardFile = GetCardFile(newFile.ID, card);

                IKrVirtualFileScript script;
                if (cardFile != null
                    && (script = compilationResult.GetScript(newFile.ID)) != null)
                {
                    var context = new KrVirtualFileScriptContext
                    {
                        CardFile = cardFile,
                        File = newFile,
                        DbScope = this.dbScope,
                        Session = this.session,
                        Card = card,
                        Container = this.container,
                        CancellationToken = cancellationToken,
                    };

                    await script.InitializationScenarioAsync(context);
                }
            }

            return validationResult.Build();
        }


        /// <inheritdoc />
        public async Task<ValidationResult> CheckAccessForFileAsync(
            Guid cardID,
            Guid fileID,
            CancellationToken cancellationToken = default)
        {
            var virtualFile = await virtualFileCache.TryGetAsync(fileID, cancellationToken);
            if (virtualFile == null)
            {
                // Не даем доступ к файлам, о которых не знаем
                return ValidationResult.FromText(
                    LocalizationManager.Format(
                        "$KrVirtualFiles_UnknownVirtualFile",
                        fileID),
                    ValidationResultType.Error);
            }

            await using (dbScope.Create())
            {
                var (stateID, typeID) = await GetCardInfoAsync(cardID, cancellationToken);

                // Неизвестная карточка
                if (typeID == Guid.Empty)
                {
                    return new ValidationResultBuilder()
                        .AddInstanceNotFoundError(
                            this,
                            this.configurationInfoProvider.GetFlags(),
                            cardID)
                        .Build();
                }

                var db = dbScope.Db;
                var builder = dbScope.BuilderFactory
                    .Select().Top(1)
                    .V(1)
                    .From("KrVirtualFiles", "vf").NoLock();
                AddJoinsToBuilder(builder);
                builder
                    .Where().C("vf", "ID").Equals().P("FileID")
                    .Limit(1);

                var validationResult = new ValidationResultBuilder();
                var hasAccess =
                    (await db.SetCommand(
                            builder.Build(),
                            db.Parameter("UserID", session.User.ID),
                            db.Parameter("TypeID", typeID),
                            db.Parameter("StateID", stateID),
                            db.Parameter("FileID", fileID))
                        .LogCommand()
                        .ExecuteAsync<int?>(cancellationToken)).HasValue;

                if (hasAccess)
                {
                    var conditionContext = new ConditionContext(
                        cardID,
                        async (ct) => await LoadCardAsync(cardID, validationResult, ct),
                        dbScope,
                        session,
                        validationResult,
                        container)
                    {
                        CancellationToken = cancellationToken,
                    };

                    var conditionCompilationResult = await conditionCache.GetAsync(cancellationToken);
                    if (!conditionCompilationResult.ValidationResult.IsSuccessful)
                    {
                        return conditionCompilationResult.ValidationResult;
                    }

                    hasAccess &= await CheckConditionsAsync(
                        conditionContext,
                        conditionCompilationResult,
                        virtualFile);
                }

                if (validationResult.IsSuccessful())
                {
                    if (!hasAccess)
                    {
                        validationResult.AddError(this, "$KrVirtualFiles_NoAccessToFile");
                    }
                }

                return validationResult.Build();
            }
        }

        #endregion

        #region Private Methods

        private static CardFile GetCardFile(Guid fileID, Card card)
        {
            foreach (var file in card.Files)
            {
                if (file.RowID == fileID)
                {
                    return file;
                }
            }

            return null;
        }

        private Task<string> ReplacePlaceholderAsync(
            Card card,
            Guid? cardID,
            string name,
            CancellationToken cancellationToken = default)
        {
            var placeholderManager = placeholderManagerFactory();

            return
                placeholderManager.ReplaceTextAsync(
                    name,
                    session,
                    container,
                    dbScope,
                    card: card,
                    cardID: cardID,
                    cancellationToken: cancellationToken);
        }

        private async Task<Card> LoadCardAsync(
            Guid cardID,
            IValidationResultBuilder validationResult,
            CancellationToken cancellationToken = default)
        {
            var getRequest = new CardGetRequest { CardID = cardID };

            // Устаналиваем все права на карточку.
            // Базовая проверка доступа на чтение файлов карточки производится в KrFileAccessHelper.CheckAccess
            permissionProvider.SetFullPermissions(getRequest);

            var response = await cardRepository.GetAsync(getRequest, cancellationToken);
            validationResult.Add(response.ValidationResult);

            return response.Card;
        }

        private async Task<(int, Guid)> GetCardInfoAsync(Guid cardID, CancellationToken cancellationToken = default)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                var builder = dbScope.BuilderFactory;

                db.SetCommand(
                        builder
                            .Select().Top(1).C("i", "TypeID").C("kaci", KrConstants.StateID)
                            .From("Instances", "i").NoLock()
                            .LeftJoin(KrConstants.KrApprovalCommonInfo.Name, "kaci").NoLock()
                            .On().C("i", "ID").Equals().C("kaci", "MainCardID")
                            .Where().C("i", "ID").Equals().P("CardID")
                            .Limit(1)
                            .Build(),
                        db.Parameter("CardID", cardID))
                    .LogCommand();

                await using var reader = await db.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    return
                        (reader.GetValue<short?>(1) ?? KrState.Draft.ID, reader.GetValue<Guid>(0));
                }
            }

            return default;
        }

        private async Task<IList<IKrVirtualFile>> GetVirtualFilesAsync(
            Card card,
            IValidationResultBuilder validationResult,
            CancellationToken cancellationToken = default)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                var builder = dbScope.BuilderFactory
                    .Select()
                    .C("vf", "ID")
                    .From("KrVirtualFiles", "vf").NoLock();

                AddJoinsToBuilder(builder);

                var ids = await db.SetCommand(
                        builder.Build(),
                        db.Parameter("UserID", session.User.ID),
                        db.Parameter("TypeID", card.TypeID),
                        db.Parameter("StateID", card.Sections.GetOrAdd(KrConstants.KrApprovalCommonInfo.Virtual).RawFields.TryGet<int?>(KrConstants.StateID) ?? KrState.Draft.ID))
                    .LogCommand()
                    .ExecuteListAsync<Guid>(cancellationToken);

                if (ids.Count == 0)
                {
                    return Array.Empty<IKrVirtualFile>();
                }

                var result = new List<IKrVirtualFile>(ids.Count);
                var innerValidationResult = new ValidationResultBuilder();
                var conditionContext = new ConditionContext(
                    card,
                    dbScope,
                    session,
                    innerValidationResult,
                    container)
                {
                    CancellationToken = cancellationToken,
                };

                var conditionCompilationResult = await conditionCache.GetAsync(cancellationToken);
                if (!conditionCompilationResult.ValidationResult.IsSuccessful)
                {
                    innerValidationResult.Add(conditionCompilationResult.ValidationResult.ConvertToSuccessful());
                    return Array.Empty<IKrVirtualFile>();
                }

                foreach (var id in ids)
                {
                    var virtualFile = await virtualFileCache.TryGetAsync(id, cancellationToken);
                    if (virtualFile != null
                        && await CheckConditionsAsync(conditionContext, conditionCompilationResult, virtualFile))
                    {
                        result.Add(virtualFile);
                    }
                }

                if (innerValidationResult.Count > 0)
                {
                    validationResult.Add(innerValidationResult.Build().ConvertToSuccessful());
                }

                return result;
            }
        }

        private ValueTask<bool> CheckConditionsAsync(
            IConditionContext context,
            IConditionCompilationResult conditionCompilationResult,
            IKrVirtualFile virtualFile)
        {
            if (virtualFile.Conditions == null)
            {
                return new ValueTask<bool>(true);
            }

            return conditionExecutor.CheckConditionAsync(
                virtualFile.Conditions,
                context,
                conditionCompilationResult);
        }

        private static void AddJoinsToBuilder(IQueryBuilder builder)
        {
            builder
                .InnerJoinLateral(
                    b => b.Select().Top(1).V(true).As("tmp")
                        .From("KrVirtualFileStates", "vfs").NoLock()
                        .Where().C("vfs", "ID").Equals().C("vf", "ID")
                        .And().C("vfs", "StateID").Equals().P("StateID")
                        .Limit(1),
                    "vfs")
                .InnerJoinLateral(
                    b => b.Select().Top(1).V(true).As("tmp")
                        .From("KrVirtualFileCardTypes", "vft").NoLock()
                        .Where().C("vft", "ID").Equals().C("vf", "ID")
                        .And().C("vft", "TypeID").Equals().P("TypeID")
                        .Limit(1),
                    "vft")
                .InnerJoinLateral(
                    b => b.Select().Top(1).V(true).As("tmp")
                        .From("KrVirtualFileRoles", "vfr").NoLock()
                        .InnerJoin("RoleUsers", "ru").NoLock().On().C("ru", "ID").Equals().C("vfr", "RoleID")
                        .Where().C("vfr", "ID").Equals().C("vf", "ID")
                        .And().C("ru", "UserID").Equals().P("UserID")
                        .Limit(1),
                    "vfr")
                ;
        }

        #endregion
    }
}
