using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Extensions.Default.Server.Workflow.KrCompilers;
using Tessa.Extensions.Default.Server.Workflow.KrProcess;
using Tessa.Extensions.Default.Server.Workflow.KrProcess.Scope;
using Tessa.Extensions.Default.Server.Workflow.KrProcess.Serialization;
using Tessa.Extensions.Default.Shared;
using Tessa.Extensions.Default.Shared.Workflow.KrCompilers;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform;
using Tessa.Platform.Collections;
using Tessa.Platform.Storage;
using static Tessa.Extensions.Default.Shared.Workflow.KrProcess.KrConstants;

namespace Tessa.Extensions.Default.Server.Workflow.KrObjectModel
{
    /// <summary>
    /// Предоставляет методы для работы с хранилищами Kr процесса и объектной моделью процесса.
    /// </summary>
    public sealed class ObjectModelMapper : IObjectModelMapper
    {
        #region nested types

        private sealed class ObjectModelToCardRowContext
        {
            /// <summary>
            /// Способ сохранения карточки, в которую необходимо перенести процесс.
            /// </summary>
            public CardStoreMode CardStoreMode { get; init; }

            /// <summary>
            /// Возвращает коллекцию этапов процесса.
            /// </summary>
            public IList<Stage> Stages { get; init; }

            /// <summary>
            /// Возвращает коллекцию этапов процесса до выполнения обработчиков этапов.
            /// </summary>
            public IList<Stage> InitialStages { get; init; }

            /// <summary>
            /// Возвращает коллекцию строк этапов процесса, расположенные в карточке, в которую необходимо перенести процесс.
            /// </summary>
            public ListStorage<CardRow> StageRows { get; init; }

            /// <summary>
            /// Возвращает хэш-таблицу построенную по <see cref="StageRows"/>.<para/>
            /// Ключ - <see cref="CardRow.RowID"/>.
            /// </summary>
            public HashSet<Guid, CardRow> StageRowsTable { get; init; }

            /// <summary>
            /// Возвращает идентификатор дочернего процесса.
            /// </summary>
            /// <remarks>Используется только, если обрабатываемый процесс является вложенным.</remarks>
            public Guid? NestedProcessID { get; init; }

            /// <summary>
            /// Возвращает идентификатор родительского этапа.
            /// </summary>
            /// <remarks>Используется только, если обрабатываемый процесс является вложенным.</remarks>
            public Guid? ParentStageRowID { get; init; }

            /// <summary>
            /// Возвращает порядковый номер дочернего процесса.
            /// </summary>
            /// <remarks>Используется только, если обрабатываемый процесс является вложенным.</remarks>
            public int? NestedOrder { get; init; }
        }

        #endregion

        #region fields

        private readonly IKrStageSerializer serializer;

        private readonly ICardMetadata cardMetadata;

        private readonly IKrScope krScope;

        #endregion

        #region constructor

        public ObjectModelMapper(
            IKrStageSerializer serializer,
            ICardMetadata cardMetadata,
            IKrScope krScope)
        {
            this.serializer = serializer;
            this.cardMetadata = cardMetadata;
            this.krScope = krScope;
        }

        #endregion

        #region implementation

        /// <inheritdoc />
        public MainProcessCommonInfo GetMainProcessCommonInfo(
            Card processHolderSatellite,
            bool withInfo = true)
        {
            Check.ArgumentNotNull(processHolderSatellite, nameof(processHolderSatellite));

            if (!processHolderSatellite.TryGetKrApprovalCommonInfoSection(out var commonInfo))
            {
                return null;
            }

            // Берем значение инфо процесса, которое могло быть ранее помещено в кэш
            // Таким образом, из объектной модели и кэша будет ссылка на один объект
            // далее при сохранении кэш будет сброшен и единственное актуальное значение будет в секции
            var info = withInfo
                ? ProcessInfoCacheHelper.Get(this.serializer, processHolderSatellite)
                : null;

            return new MainProcessCommonInfo(
                commonInfo.Fields.TryGet<Guid?>(KrProcessCommonInfo.CurrentApprovalStageRowID),
                info,
                commonInfo.Fields.TryGet<Guid?>(KrSecondaryProcessCommonInfo.SecondaryProcessID),
                commonInfo.Fields.TryGet<Guid?>(KrApprovalCommonInfo.AuthorID),
                commonInfo.Fields.TryGet<string>(KrApprovalCommonInfo.AuthorName),
                commonInfo.Fields.TryGet<string>(KrApprovalCommonInfo.AuthorComment),
                commonInfo.Fields.TryGet<int>(StateID),
                commonInfo.Fields.TryGet<Guid?>(KrApprovalCommonInfo.ProcessOwnerID),
                commonInfo.Fields.TryGet<string>(KrApprovalCommonInfo.ProcessOwnerName));
        }

        /// <inheritdoc />
        public async ValueTask SetMainProcessCommonInfoAsync(
            Guid mainCardID,
            Card processHolderSatellite,
            MainProcessCommonInfo processCommonInfo,
            CancellationToken cancellationToken = default)
        {
            if (!processHolderSatellite.TryGetKrApprovalCommonInfoSection(out var aci))
            {
                return;
            }

            aci.SetIfDiffer(KrProcessCommonInfo.CurrentApprovalStageRowID, processCommonInfo.CurrentStageRowID);
            ProcessInfoCacheHelper.Update(this.serializer, processHolderSatellite);

            aci.SetIfDiffer(KrProcessCommonInfo.AuthorID, processCommonInfo.AuthorID);
            aci.SetIfDiffer(KrProcessCommonInfo.AuthorName, processCommonInfo.AuthorName);

            aci.SetIfDiffer(KrProcessCommonInfo.ProcessOwnerID, processCommonInfo.ProcessOwnerID);
            aci.SetIfDiffer(KrProcessCommonInfo.ProcessOwnerName, processCommonInfo.ProcessOwnerName);

            if (processHolderSatellite.TypeID == DefaultCardTypes.KrSecondarySatelliteTypeID)
            {
                aci.SetIfDiffer(KrSecondaryProcessCommonInfo.SecondaryProcessID, processCommonInfo.SecondaryProcessID);
                return;
            }

            aci.SetIfDiffer(KrApprovalCommonInfo.AuthorComment, processCommonInfo.AuthorComment);

            if (aci.SetIfDiffer(StateID, processCommonInfo.State))
            {
                aci.Fields[KrApprovalCommonInfo.StateChangedDateTimeUTC] = DateTime.UtcNow;
                aci.SetIfDiffer(StateName, await this.cardMetadata.GetDocumentStateNameAsync((KrState) processCommonInfo.State, cancellationToken));

                if (processCommonInfo.AffectMainCardVersionWhenStateChanged)
                {
                    this.krScope.ForceIncrementMainCardVersion(mainCardID);
                }
            }
        }

        /// <inheritdoc />
        public List<NestedProcessCommonInfo> GetNestedProcessCommonInfos(
            Card processHolderSatellite)
        {
            Check.ArgumentNotNull(processHolderSatellite, nameof(processHolderSatellite));

            if (processHolderSatellite.TryGetKrApprovalCommonInfoSection(out var aci)
                && aci.Fields.TryGetValue(KrProcessCommonInfo.NestedWorkflowProcesses, out var nwpObj))
            {
                if (nwpObj is string nwp
                    && !string.IsNullOrWhiteSpace(nwp))
                {
                    return this.serializer.Deserialize<List<object>>(nwp)
                        .Select(static p => new NestedProcessCommonInfo((Dictionary<string, object>) p))
                        .ToList();
                }

                return new List<NestedProcessCommonInfo>();
            }

            return null;
        }

        /// <inheritdoc />
        public void SetNestedProcessCommonInfos(
            Card processHolderSatellite,
            IReadOnlyCollection<NestedProcessCommonInfo> nestedProcessCommonInfos)
        {
            if (nestedProcessCommonInfos is null)
            {
                return;
            }

            // Выпиливаем завершенные процессы
            var activeInfos = new List<object>(nestedProcessCommonInfos.Count);

            foreach (var info in nestedProcessCommonInfos)
            {
                if (info.CurrentStageRowID != null)
                {
                    activeInfos.Add(info.GetStorage());
                }
            }

            var nwpJson = this.serializer.Serialize(activeInfos);

            processHolderSatellite
                .GetApprovalInfoSection()
                .Fields[KrProcessCommonInfo.NestedWorkflowProcesses] = nwpJson;
        }

        /// <inheritdoc />
        public void FillWorkflowProcessFromPci(
            WorkflowProcess workflowProcess,
            ProcessCommonInfo commonInfo,
            MainProcessCommonInfo primaryProcessCommonInfo)
        {
            Check.ArgumentNotNull(workflowProcess, nameof(workflowProcess));

            if (primaryProcessCommonInfo is null)
            {
                workflowProcess.SetState(KrState.Draft, false);
            }
            else
            {
                if (primaryProcessCommonInfo.AuthorID.HasValue)
                {
                    workflowProcess.SetAuthor(
                        new Author(
                            primaryProcessCommonInfo.AuthorID.Value,
                            primaryProcessCommonInfo.AuthorName),
                        false);
                }

                if (primaryProcessCommonInfo.ProcessOwnerID.HasValue)
                {
                    workflowProcess.SetProcessOwner(
                        new Author(
                            primaryProcessCommonInfo.ProcessOwnerID.Value,
                            primaryProcessCommonInfo.ProcessOwnerName),
                        false);
                }

                workflowProcess.SetAuthorComment(primaryProcessCommonInfo.AuthorComment, false);
                workflowProcess.SetState((KrState) primaryProcessCommonInfo.State, false);
            }

            if (commonInfo != null)
            {
                if (commonInfo.AuthorID.HasValue)
                {
                    workflowProcess.SetAuthorCurrentProcess(
                        new Author(
                            commonInfo.AuthorID.Value,
                            commonInfo.AuthorName),
                        false);
                }

                if (commonInfo.ProcessOwnerID.HasValue)
                {
                    workflowProcess.SetProcessOwnerCurrentProcess(
                        new Author(
                            commonInfo.ProcessOwnerID.Value,
                            commonInfo.ProcessOwnerName),
                        false);
                }

                workflowProcess.CurrentApprovalStageRowID = commonInfo.CurrentStageRowID;
            }
        }

        /// <inheritdoc />
        public async ValueTask<WorkflowProcess> CardRowsToObjectModelAsync(
            IKrStageTemplate stageTemplate,
            IReadOnlyCollection<IKrRuntimeStage> runtimeStages,
            MainProcessCommonInfo primaryPci = null,
            bool initialStage = true,
            bool saveInitialStages = false,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(stageTemplate, nameof(stageTemplate));
            Check.ArgumentNotNull(runtimeStages, nameof(runtimeStages));

            var krStages = new SealableObjectList<Stage>();

            foreach (var stageRow in runtimeStages.OrderBy(static p => p.Order))
            {
                var stage = await Stage.InitializeAsync(stageRow, stageTemplate);
                krStages.Add(stage);
            }

            var primaryPciInfo = primaryPci?.Info;

            return new WorkflowProcess(
                primaryPciInfo,
                primaryPciInfo,
                krStages,
                saveInitialStages: saveInitialStages,
                nestedProcessID: null,
                isMainProcess: true);
        }

        /// <inheritdoc />
        public async ValueTask<WorkflowProcess> CardRowsToObjectModelAsync(
            Card processHolder,
            ProcessCommonInfo pci,
            MainProcessCommonInfo mainPci,
            IReadOnlyDictionary<Guid, IKrStageTemplate> templates,
            IReadOnlyDictionary<Guid, IReadOnlyCollection<IKrRuntimeStage>> runtimeStages,
            string processTypeName,
            bool initialStage = true,
            CancellationToken cancellationToken = default)
        {
            var pciNested = pci as NestedProcessCommonInfo;
            var nestedProcessID = pciNested?.NestedProcessID;
            var stagesRows = processHolder.GetStagesSection().Rows;
            var krStages = new SealableObjectList<Stage>(stagesRows.Count);

            foreach (var stageRow in stagesRows
                .Where(p => p.State != CardRowState.Deleted && p.TryGet<Guid?>(KrStages.NestedProcessID) == nestedProcessID)
                .OrderBy(static row => row.Get<int>(KrStages.Order)))
            {
                var basedOnID = stageRow.Fields.TryGet<Guid?>(KrStages.BasedOnStageTemplateID);
                IKrStageTemplate templateCard = null;
                IReadOnlyCollection<IKrRuntimeStage> stages = null;

                if (basedOnID.HasValue)
                {
                    templates.TryGetValue(basedOnID.Value, out templateCard);
                    runtimeStages.TryGetValue(basedOnID.Value, out stages);
                }

                if (stages is null)
                {
                    stages = EmptyHolder<IKrRuntimeStage>.Collection;
                }

                var settings = await this.serializer.DeserializeSettingsStorageAsync(stageRow, cancellationToken: cancellationToken);
                var stageInfoStorage = this.ParseInfoStorage(stageRow);

                var stage = new Stage(
                    stageRow,
                    settings,
                    stageInfoStorage,
                    templateCard,
                    stages,
                    initialStage);

                krStages.Add(stage);
            }

            return new WorkflowProcess(
                pci.Info,
                mainPci.Info,
                krStages,
                saveInitialStages: true,
                nestedProcessID: nestedProcessID,
                isMainProcess: string.Equals(processTypeName, KrConstants.KrProcessName, StringComparison.Ordinal));
        }

        /// <inheritdoc />
        public void ObjectModelToPci(
            WorkflowProcess process,
            ProcessCommonInfo pci,
            MainProcessCommonInfo mainPci,
            MainProcessCommonInfo primaryPci)
        {
            pci.CurrentStageRowID = process.CurrentApprovalStageRowID;

            if (primaryPci != null)
            {
                if (primaryPci.AuthorTimestamp < process.AuthorTimestamp)
                {
                    primaryPci.AuthorID = process.Author?.AuthorID;
                    primaryPci.AuthorName = process.Author?.AuthorName;
                    primaryPci.AuthorTimestamp = process.AuthorTimestamp;
                }

                if (primaryPci.AuthorCommentTimestamp < process.AuthorCommentTimestamp)
                {
                    primaryPci.AuthorComment = process.AuthorComment;
                    primaryPci.AuthorCommentTimestamp = process.AuthorCommentTimestamp;
                }

                if (primaryPci.StateTimestamp < process.StateTimestamp)
                {
                    primaryPci.State = process.State.ID;
                    primaryPci.StateTimestamp = process.StateTimestamp;
                }

                if (primaryPci.AffectMainCardVersionWhenStateChangedTimestamp < process.AffectMainCardVersionWhenStateChangedTimestamp)
                {
                    primaryPci.AffectMainCardVersionWhenStateChanged = process.AffectMainCardVersionWhenStateChanged;
                    primaryPci.AffectMainCardVersionWhenStateChangedTimestamp = process.AffectMainCardVersionWhenStateChangedTimestamp;
                }

                if (primaryPci.ProcessOwnerTimestamp < process.ProcessOwnerTimestamp)
                {
                    primaryPci.ProcessOwnerID = process.ProcessOwner?.AuthorID;
                    primaryPci.ProcessOwnerName = process.ProcessOwner?.AuthorName;
                    primaryPci.ProcessOwnerTimestamp = process.ProcessOwnerTimestamp;
                }
            }

            pci.Info = process.InfoStorage;

            if (!ReferenceEquals(pci, mainPci))
            {
                StorageHelper.Merge(process.MainProcessInfoStorage, mainPci.Info);
            }

            if (pci.AuthorTimestamp < process.AuthorCurrentProcessTimestamp)
            {
                pci.AuthorID = process.AuthorCurrentProcess?.AuthorID;
                pci.AuthorName = process.AuthorCurrentProcess?.AuthorName;
                pci.AuthorTimestamp = process.AuthorCurrentProcessTimestamp;
            }

            if (pci.ProcessOwnerTimestamp < process.ProcessOwnerCurrentProcessTimestamp)
            {
                pci.ProcessOwnerID = process.ProcessOwnerCurrentProcess?.AuthorID;
                pci.ProcessOwnerName = process.ProcessOwnerCurrentProcess?.AuthorName;
                pci.ProcessOwnerTimestamp = process.ProcessOwnerCurrentProcessTimestamp;
            }
        }

        /// <inheritdoc />
        public async ValueTask<List<RouteDiff>> ObjectModelToCardRowsAsync(
            WorkflowProcess process,
            Card baseCard,
            NestedProcessCommonInfo npci = null,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(process, nameof(process));
            Check.ArgumentNotNull(process.Stages, nameof(process) + "." + nameof(process.Stages));
            Check.ArgumentNotNull(process.InitialWorkflowProcess, nameof(process) + "." + nameof(process.InitialWorkflowProcess));
            Check.ArgumentNotNull(process.InitialWorkflowProcess.Stages, nameof(process) + "." + nameof(process.InitialWorkflowProcess) + "." + nameof(process.InitialWorkflowProcess.Stages));
            Check.ArgumentNotNull(baseCard, nameof(baseCard));

            var stageRows = baseCard.GetStagesSection().Rows;
            var ctx = new ObjectModelToCardRowContext
            {
                CardStoreMode = baseCard.StoreMode,
                Stages = process.Stages,
                InitialStages = process.InitialWorkflowProcess.Stages,

                NestedProcessID = npci?.NestedProcessID,
                ParentStageRowID = npci?.ParentStageRowID,
                NestedOrder = npci?.NestedOrder,

                // Будут использоваться только от текущего нестеда, но отдаем всю коллекцию, чтобы была
                // возможность модифицировать ее по ссылке.
                StageRows = stageRows,

                // Здесь строится хеш-таблица только по текущему нестеду.
                StageRowsTable = new HashSet<Guid, CardRow>(
                    static p => p.RowID,
                    stageRows.Where(p => p.Get<Guid?>(KrStages.NestedProcessID) == npci?.NestedProcessID)),
            };

            return await this.MoveStagesIntoCardRowsAsync(ctx, cancellationToken);
        }

        #endregion

        #region CardRowsToObjectModel

        /// <summary>
        /// Разобрать хранилище info из JSON, который хранится в fields по ключу Info.
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        private IDictionary<string, object> ParseInfoStorage(IDictionary<string, object> fields)
        {
            if (fields.TryGetValue(Info, out var stateObj)
                && stateObj is string state
                && !string.IsNullOrWhiteSpace(state))
            {
                return this.serializer.Deserialize<Dictionary<string, object>>(state);
            }

            return new Dictionary<string, object>(StringComparer.Ordinal);
        }

        #endregion

        #region ObjectModelToCardRows

        /// <summary>
        /// Исправляет ссылки StageRowID в подставленных настройках, а также выставляет порядок сортировки.
        /// </summary>
        /// <param name="stageRowID">Идентификатор строки этапа.</param>
        /// <param name="stage">Этап, настройки которого необходимо исправить.</param>
        private void RepairSettings(Guid stageRowID, Stage stage)
        {
            var storage = stage.SettingsStorage;
            foreach (var referenceToStages in this.serializer.ReferencesToStages)
            {
                if (storage.TryGetValue(referenceToStages.SectionName, out var rowsObj)
                    && rowsObj is IList rows
                    && rows is not byte[])
                {
                    foreach (var row in rows)
                    {
                        if (row is IDictionary<string, object> rowStorage)
                        {
                            Guid? oldStageRowID;
                            if ((oldStageRowID = rowStorage.TryGet<Guid?>(referenceToStages.RowIDFieldName)) != null
                                && oldStageRowID != stageRowID)
                            {
                                rowStorage[referenceToStages.RowIDFieldName] = stageRowID;
                                break;
                            }
                        }
                    }
                }
            }

            if (stage.Performers != null)
            {
                for (var i = 0; i < stage.Performers.Count; i++)
                {
                    var performer = stage.Performers[i];
                    performer.GetStorage()[Order] = Int32Boxes.Box(i);
                }
            }
        }

        /// <summary>
        /// Заполняет поля строки содержащей информацию по этапу.
        /// </summary>
        /// <param name="stage">Информация о новом этапе (источник данных).</param>
        /// <param name="initialStage">Информация о заменяемом этапе (этапе до пересчёта). Может быть не задана.</param>
        /// <param name="stageRow">Строка этапа (куда записать данные).</param>
        /// <param name="ctx"></param>
        private void FillStageSections(
            Stage stage,
            Stage initialStage,
            CardRow stageRow,
            ObjectModelToCardRowContext ctx)
        {
            if (initialStage is null
                || !StorageHelper.Equals(stage.SettingsStorage, initialStage.SettingsStorage))
            {
                this.serializer.SerializeSettingsStorage(stageRow, stage.SettingsStorage);
            }

            stageRow.SetIfDiffer(Name, stage.Name);
            stageRow.SetIfDiffer(KrStages.TimeLimit, stage.TimeLimit);
            stageRow.SetIfDiffer(KrStages.Planned, stage.Planned);
            stageRow.SetIfDiffer(KrStages.Hidden, stage.Hidden);
            stageRow.SetIfDiffer(KrStages.Skip, stage.Skip);
            stageRow.SetIfDiffer(KrStages.CanBeSkipped, stage.CanBeSkipped);

            stageRow.SetIfDiffer(StageGroupID, stage.StageGroupID);
            stageRow.SetIfDiffer(StageGroupName, stage.StageGroupName);
            stageRow.SetIfDiffer(StageGroupOrder, stage.StageGroupOrder);

            stageRow.SetIfDiffer(KrStages.RowChanged, stage.RowChanged);
            stageRow.SetIfDiffer(KrStages.OrderChanged, stage.OrderChanged);

            stageRow.SetIfDiffer(KrStages.StageTypeID, stage.StageTypeID);
            stageRow.SetIfDiffer(KrStages.StageTypeCaption, stage.StageTypeCaption);

            stageRow.SetIfDiffer(KrStages.NestedProcessID, ctx.NestedProcessID);
            stageRow.SetIfDiffer(KrStages.ParentStageRowID, ctx.ParentStageRowID);
            stageRow.SetIfDiffer(KrStages.NestedOrder, ctx.NestedOrder);

            // Этап может быть не привязанным вообще, в случае для этапа, добавленного пользователем
            // Этап может быть привязан только к шаблону этапов, это этап, добавленный в пользовательских скриптах
            // Этап может быть привязан шаблону этапов и к конкретному этапу внутри шаблона.
            if (stage.BasedOnTemplate)
            {
                stageRow.Fields[KrStages.BasedOnStageTemplateID] = stage.TemplateID;
                stageRow.Fields[KrStages.BasedOnStageTemplateName] = stage.TemplateName;
                stageRow.Fields[KrStages.BasedOnStageTemplateOrder] = Int32Boxes.Box(stage.TemplateOrder);
                stageRow.Fields[KrStages.BasedOnStageTemplateGroupPositionID] = Int32Boxes.Box(stage.GroupPosition.ID);
            }

            if (stage.BasedOnTemplateStage)
            {
                stageRow.Fields[KrStages.BasedOnStageRowID] = stage.ID;
            }

        }

        private async ValueTask<List<RouteDiff>> MoveNewStagesIntoCardRowsAsync(
            ObjectModelToCardRowContext ctx,
            CancellationToken cancellationToken = default)
        {
            var diffs = new List<RouteDiff>(ctx.Stages.Count);
            for (var stageIndex = 0; stageIndex < ctx.Stages.Count; stageIndex++)
            {
                var newStage = ctx.Stages[stageIndex];
                var cardRow = AddRow(ctx.StageRows, newStage.RowID);

                this.RepairSettings(cardRow.RowID, newStage);
                this.FillStageSections(newStage, null, cardRow, ctx);
                cardRow.Fields[Info] = this.serializer.Serialize(newStage.InfoStorage);
                var stateChanged = cardRow.SetIfDiffer(KrStages.StageStateID, (int) newStage.State);

                if (stateChanged)
                {
                    cardRow.SetIfDiffer(KrStages.StageStateName, await this.cardMetadata.GetStageStateNameAsync(newStage.State, cancellationToken));
                }

                UpdateRowOrder(cardRow, stageIndex, Order, onlyIfNeeded: false);
                diffs.Add(RouteDiff.NewStage(cardRow.RowID, newStage.Name, newStage.Hidden));
            }
            return diffs;
        }

        private static List<RouteDiff> DeleteAllStageRows(
            ObjectModelToCardRowContext ctx)
        {
            if (ctx.InitialStages != null)
            {
                var diffs = new List<RouteDiff>(ctx.InitialStages.Count);
                foreach (var oldStage in ctx.InitialStages)
                {
                    var cardRow = ctx.StageRowsTable[oldStage.RowID];
                    cardRow.State = CardRowState.Deleted;
                    diffs.Add(RouteDiff.DeleteStage(cardRow.RowID, oldStage.Name, oldStage.Hidden));
                }
                return diffs;
            }

            return new List<RouteDiff>(0);
        }

        private static void DeleteRedundantStageRows(
            ObjectModelToCardRowContext ctx,
            HashSet<Guid, Stage> initialStagesTable,
            ICollection<RouteDiff> diffs)
        {
            var redundantIDs = ctx.InitialStages
                .Select(static x => (x.ID, x.RowID))
                .Except(ctx.Stages.Select(static x => (x.ID, x.RowID)))
                .Select(static x => x.RowID);

            foreach (var redundantRowID in redundantIDs)
            {
                var redundantCardRowStage = ctx.StageRowsTable[redundantRowID];
                redundantCardRowStage.State = CardRowState.Deleted;

                var initialStage = initialStagesTable[redundantRowID];
                diffs.Add(
                    RouteDiff.DeleteStage(
                        redundantCardRowStage.RowID,
                        initialStage.Name,
                        initialStage.Hidden));
            }
        }

        private static void UnbindUnconfirmedOrDeletedStage(
            CardRow stageRow)
        {
            stageRow.Fields[KrStages.BasedOnStageTemplateID] = null;
            stageRow.Fields[KrStages.BasedOnStageTemplateName] = null;
            stageRow.Fields[KrStages.BasedOnStageTemplateOrder] = null;
            stageRow.Fields[KrStages.BasedOnStageTemplateGroupPositionID] = null;
            stageRow.Fields[KrStages.BasedOnStageRowID] = null;
        }

        private async ValueTask<List<RouteDiff>> MoveStagesIntoCardRowsAsync(
            ObjectModelToCardRowContext ctx,
            CancellationToken cancellationToken = default)
        {
            // Новых этапов нет, маршрут пустой, нужно удалить все старые этапы
            if (ctx.Stages.Count == 0)
            {
                return DeleteAllStageRows(ctx);
            }

            // Старых этапов нет, нужно просто создать новые
            if (ctx.InitialStages is null
                || ctx.InitialStages.Count == 0)
            {
                return await this.MoveNewStagesIntoCardRowsAsync(ctx, cancellationToken);
            }

            var initialStagesTable = new HashSet<Guid, Stage>(static x => x.RowID, ctx.InitialStages);
            var diffs = new List<RouteDiff>(ctx.Stages.Count + ctx.InitialStages.Count);

            for (var stageIndex = 0; stageIndex < ctx.Stages.Count; stageIndex++)
            {
                var stage = ctx.Stages[stageIndex];
                _ = initialStagesTable.TryGetItem(stage.RowID, out var oldStage);
                _ = ctx.StageRowsTable.TryGetItem(stage.RowID, out var cardRow);

                // Для корректного восстановления и сравнения с существующими нужно восстановить
                // StageRowID конкретной карточки.
                if (cardRow != null)
                {
                    this.RepairSettings(cardRow.RowID, stage);
                }

                // Если текущий этап не совпадает с этапом до пересчета по значению или порядку
                // то этап можно считать добавленным или измененным,
                // в зависимости от того, был ли уже этап с таким ID
                if (ctx.InitialStages.Count <= stageIndex
                    || stage != ctx.InitialStages[stageIndex])
                {
                    if (oldStage != null
                        && cardRow != null
                        && stage.RowID == cardRow.RowID)
                    {
                        // изменен этап oldStage -> stage
                        if (cardRow.State == CardRowState.None)
                        {
                            cardRow.State = CardRowState.Modified;
                        }

                        diffs.Add(RouteDiff.ModifyStage(
                            cardRow.RowID,
                            stage.Name,
                            oldStage.Name,
                            stage.Hidden));
                    }
                    else
                    {
                        // добавлен этап stage
                        cardRow = AddRow(ctx.StageRows, stage.RowID);
                        oldStage = default;

                        diffs.Add(RouteDiff.NewStage(
                            cardRow.RowID,
                            stage.Name,
                            stage.Hidden));
                    }

                    this.FillStageSections(stage, oldStage, cardRow, ctx);
                }

                if (cardRow != null)
                {
                    // Шаблон с таким флагом может дойти в двух случаях:
                    // 1. Шаблон удален, но этап изменился пользователем, поэтому остался
                    // 2. Шаблон не подтвержден, но этап изменился пользователем, поэтому остался
                    // В обоих случаях нужно забыть, что этап связан с шаблоном.
                    if (stage.UnbindTemplate)
                    {
                        UnbindUnconfirmedOrDeletedStage(
                            cardRow);
                    }

                    if (oldStage is null
                        || stage.IsInfoChanged(oldStage))
                    {
                        cardRow.Fields[Info] = this.serializer.Serialize(stage.InfoStorage);
                    }

                    cardRow.Fields[KrStages.StateID] = Int32Boxes.Box(stage.State.ID);
                    cardRow.Fields[KrStages.StateName] = await this.cardMetadata.GetStageStateNameAsync(
                        stage.State,
                        cancellationToken);

                    UpdateRowOrder(cardRow, stageIndex, KrStages.Order);
                }
            }

            // Пометка лишних на удаление.
            // Лишние определяются как разность множеств начальных и текущих этапов.
            DeleteRedundantStageRows(ctx, initialStagesTable, diffs);

            // Если возникает кейс, когда операция производится над еще не сохраненной карточкой,
            // Но в ней уже был какой-то маршрут, то после пересчета этапы могут пропасть.
            // Их нужно удалить из коллекции, а не отправить на сохранение с состоянием Deleted.
            // Возникает, например, при создании копии
            if (ctx.CardStoreMode == CardStoreMode.Insert)
            {
                ctx.StageRows.RemoveAll(static p => p.State == CardRowState.Deleted);
            }

            return diffs;
        }

        /// <summary>
        /// Задаёт порядковый номер строки этапа.
        /// </summary>
        /// <param name="row">Строка содержащая информацию по этапу.</param>
        /// <param name="order">Порядковый номер строки этапа.</param>
        /// <param name="alias">Алиас поля содержащего порядковый номер строки этапа.</param>
        /// <param name="onlyIfNeeded">Значение <see langword="true"/>, если необходимо установить новое значение порядкового номера строки этапа только если оно отличается от старого значение, иначе - <see langword="false"/> - задать новое значение без проверки.</param>
        private static void UpdateRowOrder(
            CardRow row,
            int order,
            string alias,
            bool onlyIfNeeded = true)
        {
            if (!onlyIfNeeded)
            {
                row.Fields[alias] = Int32Boxes.Box(order);
                return;
            }

            if (!row.Fields.TryGetValue(alias, out var oldOrderObj)
                || !(oldOrderObj is int oldOrder)
                || oldOrder != order)
            {
                row.Fields[alias] = Int32Boxes.Box(order);

                if (row.State == CardRowState.None)
                {
                    row.State = CardRowState.Modified;
                }
            }
        }

        private static CardRow AddRow(ListStorage<CardRow> rows, Guid rowID)
        {
            var row = rows.Add();
            row.State = CardRowState.Inserted;
            row.RowID = rowID;
            return row;
        }

        #endregion
    }
}
