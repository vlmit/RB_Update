using System.Linq;
using System.Threading.Tasks;
using Tessa.BusinessCalendar;
using Tessa.Cards;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Extensions.Default.Shared.Workflow.WorkflowEngine;
using Tessa.Localization;
using Tessa.Platform.Validation;
using Tessa.Workflow;
using Tessa.Workflow.Compilation;
using Unity;

namespace Tessa.Extensions.Default.Server.Workflow.WorkflowEngine
{
    public sealed class KrChangeStateAction : KrWorkflowActionBase
    {
        #region Consts

        public const string MainActionSection = "KrChangeStateAction";

        #endregion

        #region Fields

        private readonly IKrTypesCache typesCache;
        private readonly ICardMetadata cardMetadata;

        #endregion

        #region Constructors

        public KrChangeStateAction(
            [Dependency(CardRepositoryNames.ExtendedWithoutTransaction)]ICardRepository cardRepository,
            IKrTypesCache typesCache,
            ICardMetadata cardMetadata,
            IWorkflowEngineCardRequestExtender requestExtender,
            IBusinessCalendarService calendarService)
            :base(KrDescriptors.KrChangeStateDescriptor, cardRepository, requestExtender, calendarService)
        {
            this.typesCache = typesCache;
            this.cardMetadata = cardMetadata;
        }

        #endregion

        #region Base Overrides

        protected override async Task ExecuteAsync(
            IWorkflowEngineContext context,
            IWorkflowEngineCompiled scriptObject)
        {
            await base.ExecuteAsync(context, scriptObject);

            var typeID =
                context.StoreCard?.ID == context.ProcessInstance.CardID
                ? context.StoreCard.TypeID
                : (await context.GetMainCardAsync(context.CancellationToken))?.TypeID;

            if (!typeID.HasValue)
            {
                return;
            }

            var cardType = (await typesCache.GetCardTypesAsync(context.CancellationToken))
                .FirstOrDefault(x => x.ID == typeID);

            if(cardType == null)
            {
                var typeCaption = (await cardMetadata.GetCardTypesAsync(context.CancellationToken))[typeID.Value].Caption;
                context.ValidationResult.AddError(
                    this,
                    "$KrActions_ChangeState_TypeNotAllowed",
                    LocalizationManager.Localize(typeCaption));
                return;
            }

            var stateID = await context.GetAsync<int?>(MainActionSection, "State", "ID");
            var stateName = await context.GetAsync<string>(MainActionSection, "State", "Name");
            if (stateID.HasValue)
            {
                await this.SetStateIDAsync(
                    context,
                    stateID.Value,
                    stateName,
                    cancellationToken: context.CancellationToken);
            }
        }

        #endregion
    }
}
