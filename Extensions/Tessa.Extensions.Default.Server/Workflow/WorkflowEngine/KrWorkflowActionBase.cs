using System;
using System.Threading;
using System.Threading.Tasks;

using Tessa.BusinessCalendar;
using Tessa.Cards;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Extensions.Default.Shared.Workflow.WorkflowEngine;
using Tessa.Platform;
using Tessa.Platform.Storage;
using Tessa.Roles;
using Tessa.Workflow;
using Tessa.Workflow.Actions;
using Tessa.Workflow.Actions.Descriptors;
using Tessa.Workflow.Compilation;
using Tessa.Workflow.Helpful;

namespace Tessa.Extensions.Default.Server.Workflow.WorkflowEngine
{
    /// <summary>
    /// Базовый класс обработчиков действий взаимодействующих с подсистемой маршрутов.
    /// </summary>
    public abstract class KrWorkflowActionBase : WorkflowActionBase
    {
        #region Fields And Constants

        /// <summary>
        /// Имя ключа, по которому в параметрах текущего действия содержится значение флага управляющего принудительным увеличением версии основной карточки, даже если других изменений в карточке не было. Если не найден в параметрах действия, то считается равным <see langword="true"/>. Тип значения: <see cref="bool"/>.
        /// </summary>
        protected const string AffectMainCardVersionWhenStateChangedKey = "AffectMainCardVersionWhenStateChanged";

        /// <summary>
        /// Имя ключа, по которому в параметрах текущего процесса содержится идентификатор предыдущего состояния карточки. Тип значения: <see cref="int"/>.
        /// </summary>
        protected const string PreviousStateKey = "KrPreviousState";

        protected readonly ICardRepository cardRepository;
        protected readonly IWorkflowEngineCardRequestExtender requestExtender;
        protected readonly IBusinessCalendarService calendarService;

        #endregion

        #region Constructors

        protected KrWorkflowActionBase(
            WorkflowActionDescriptor actionDescriptor,
            ICardRepository cardRepository,
            IWorkflowEngineCardRequestExtender requestExtender,
            IBusinessCalendarService calendarService)
            :base(actionDescriptor)
        {
            this.cardRepository = cardRepository;
            this.requestExtender = requestExtender;
            this.calendarService = calendarService;
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        protected override Task ExecuteAsync(
            IWorkflowEngineContext context,
            IWorkflowEngineCompiled scriptObject)
        {
            context.CardsScope.CardStorePriorityComparer = WorkflowConstants.KrCardStorePriorityComparerDefault;

            return Task.CompletedTask;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Устанавливает состояние карточки.
        /// </summary>
        /// <param name="context">Контекст обработки процесса в WorkflowEngine.</param>
        /// <param name="state">Состояние.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        /// <remarks>Предыдущее состояние сохраняется в параметрах процесса (см. <see cref="StorePreviousState(IWorkflowEngineContext, int)"/>).</remarks>
        protected Task SetStateIDAsync(
            IWorkflowEngineContext context,
            KrState state,
            CancellationToken cancellationToken = default)
        {
            return this.SetStateIDAsync(context, state.ID, state.TryGetDefaultName(), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Устанавливает состояние карточки.
        /// </summary>
        /// <param name="context">Контекст обработки процесса в WorkflowEngine.</param>
        /// <param name="stateID">Идентификатор состояния.</param>
        /// <param name="stateName">Отображаемое имя состояния.</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Асинхронная задача.</returns>
        /// <remarks>Предыдущее состояние сохраняется в параметрах процесса (см. <see cref="StorePreviousState(IWorkflowEngineContext, int)"/>).</remarks>
        protected async Task SetStateIDAsync(
            IWorkflowEngineContext context,
            int stateID,
            string stateName,
            CancellationToken cancellationToken = default)
        {
            var sCard = await context.GetKrSatelliteAsync();

            if (sCard is null)
            {
                return;
            }

            var approvalCommonInfoFields = sCard.GetApprovalInfoSection().Fields;
            var oldStateID = approvalCommonInfoFields.TryGet<int>(KrConstants.StateID);
            if (oldStateID == stateID)
            {
                // Если состояния равны, то смену состояния не производим.
                return;
            }

            this.StorePreviousState(context, oldStateID);
            var stateIDObj = Int32Boxes.Box(stateID);
            approvalCommonInfoFields[KrConstants.StateID] = stateIDObj;
            approvalCommonInfoFields[KrConstants.StateName] = stateName;
            approvalCommonInfoFields[KrConstants.KrApprovalCommonInfo.StateChangedDateTimeUTC] = DateTime.UtcNow;

            approvalCommonInfoFields = (await context.GetMainCardAsync(context.CancellationToken)).GetApprovalInfoSection().Fields;
            approvalCommonInfoFields[KrConstants.StateID] = stateIDObj;
            approvalCommonInfoFields[KrConstants.StateName] = stateName;

            if (await context.GetAsync<bool?>(AffectMainCardVersionWhenStateChangedKey) ?? true)
            {
                context.ModifyStoreRequest(ModifyRequest);
            }
        }

        protected async Task AddTaskHistoryByTaskAsync(
            IWorkflowEngineContext context,
            Guid taskTypeID,
            Guid optionID,
            string result,
            Action<CardTaskHistoryItem> modifyAction = default)
        {
            if (!(await context.CardMetadata.GetCardTypesAsync(context.CancellationToken)).TryGetValue(taskTypeID, out var taskType))
            {
                return;
            }

            await this.AddTaskHistoryAsync(
                context,
                taskTypeID,
                taskType.Name,
                taskType.Caption,
                optionID,
                result,
                modifyAction);
        }

        protected async Task AddTaskHistoryAsync(
            IWorkflowEngineContext context,
            Guid taskTypeID,
            string taskTypeName,
            string taskTypeCaption,
            Guid optionID,
            string result,
            Action<CardTaskHistoryItem> modifyAction = default)
        {
            var userID = context.Session.User.ID;
            var userName = context.Session.User.Name;

            var groupID = context.ProcessInstance.GetHistoryGroup();
            // Временная зона текущего сотрудника, для записи в историю заданий
            var userZoneInfo = await this.calendarService.GetRoleTimeZoneInfoAsync(userID, context.CancellationToken);
            var option = (await context.CardMetadata.GetEnumerationsAsync(context.CancellationToken)).CompletionOptions[optionID];
            var newItem = new CardTaskHistoryItem
            {
                State = CardTaskHistoryState.Inserted,
                RowID = Guid.NewGuid(),
                TypeID = taskTypeID,
                TypeName = taskTypeName,
                TypeCaption = taskTypeCaption,
                Created = context.StoreDateTime,
                Planned = context.StoreDateTime,
                InProgress = context.StoreDateTime,
                Completed = context.StoreDateTime,
                AuthorID = userID,
                UserID = userID,
                RoleID = userID,
                AuthorName = userName,
                UserName = userName,
                RoleName = userName,
                RoleTypeID = RoleHelper.PersonalRoleTypeID,
                Result = result,
                OptionID = optionID,
                OptionCaption = option.Caption,
                OptionName = option.Name,
                ParentRowID = null,
                CompletedByID = userID,
                CompletedByName = userName,
                GroupRowID = groupID,
                TimeZoneID = userZoneInfo.TimeZoneID,
                TimeZoneUtcOffsetMinutes = (int?)userZoneInfo.TimeZoneUtcOffset.TotalMinutes
            };

            modifyAction?.Invoke(newItem);

            var mainCard = await context.GetMainCardAsync(context.CancellationToken);
            if (mainCard is not null)
            {
                mainCard.TaskHistory.Add(newItem);
            }
        }

        /// <summary>
        /// Сохраняет идентификатор предыдущего состояния карточки в параметрах процесса.
        /// </summary>
        /// <param name="context">Контекст обработки процесса в WorkflowEngine.</param>
        /// <param name="previousState">Идентификатор сохраняемого состояния.</param>
        /// <seealso cref="PreviousStateKey"/>
        protected void StorePreviousState(IWorkflowEngineContext context, int previousState)
        {
            context.ProcessInstance.Hash[PreviousStateKey] = Int32Boxes.Box(previousState);
        }

        /// <summary>
        /// Возвращает идентификатор предыдущего состояния карточки из параметров процесса.
        /// </summary>
        /// <param name="context">Контекст обработки процесса в WorkflowEngine.</param>
        /// <returns>Идентификатор предыдущего состояния карточки из параметров процесса. Если не найден в параметрах действия, то считается равным идентификатору состояния <see cref="KrState.Draft"/>.</returns>
        /// <seealso cref="PreviousStateKey"/>
        protected int TryGetPreviousState(IWorkflowEngineContext context)
        {
            return context.ProcessInstance.Hash.TryGet<int?>(PreviousStateKey) ?? KrState.Draft.ID;
        }

        #endregion

        #region Private Methods

        private void ModifyRequest(CardStoreRequest request)
        {
            request.AffectVersion = true;
        }

        #endregion
    }
}
