using System;
using System.Threading;
using System.Threading.Tasks;
using Tessa.BusinessCalendar;
using Tessa.Cards;
using Tessa.Platform.ConsoleApps;
using Tessa.Platform.Operations;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Console.RebuildCalendar
{
    public sealed class Operation : ConsoleOperation
    {
        #region Fields

        private readonly IBusinessCalendarService businessCalendarService;

        private readonly ICardRepository cardRepository;

        private readonly IOperationRepository operationRepository;

        #endregion

        #region Constructors

        public Operation(
            ConsoleSessionManager sessionManager,
            IConsoleLogger logger,
            IBusinessCalendarService businessCalendarService,
            ICardRepository cardRepository,
            IOperationRepository operationRepository)
            : base(logger, sessionManager, extendedInitialization: true)
        {
            this.businessCalendarService = businessCalendarService;
            this.cardRepository = cardRepository;
            this.operationRepository = operationRepository;
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc />
        public override async Task<int> ExecuteAsync(ConsoleOperationContext context, CancellationToken cancellationToken = default)
        {
            if (!this.SessionManager.IsOpened)
            {
                return -1;
            }

            try
            {
                await this.Logger.InfoAsync("Rebuilding calendar started, reading calendar card");

                var getRequest = new CardGetRequest
                {
                    CardTypeID = CardHelper.CalendarTypeID,
                    GetMode = CardGetMode.ReadOnly,
                    CompressionMode = CardCompressionMode.Full,
                };

                var getResponse = await this.cardRepository.GetAsync(getRequest, cancellationToken);
                var getResult = getResponse.ValidationResult.Build();

                await this.Logger.LogResultAsync(getResult);
                if (!getResult.IsSuccessful)
                {
                    return -1;
                }

                await this.Logger.InfoAsync("Creating calendar operation");
                Guid operationID = await this.operationRepository.CreateAsync(OperationTypes.CalendarRebuild, cancellationToken: cancellationToken);

                await this.Logger.InfoAsync("Starting calendar rebuilding process");
                var storeRequest = new CardStoreRequest { Card = getResponse.Card, Info = { [BusinessCalendarHelper.RebuildOperationGuidKey] = operationID } };

                var storeResponse = await this.cardRepository.StoreAsync(storeRequest, cancellationToken);
                var storeResult = storeResponse.ValidationResult.Build();

                await this.Logger.LogResultAsync(storeResult);
                if (!storeResult.IsSuccessful)
                {
                    return -1;
                }

                await this.Logger.InfoAsync("Waiting for rebuilding to complete");

                do
                {
                    await Task.Delay(500, cancellationToken);
                } while (await this.operationRepository.IsAliveAsync(operationID, cancellationToken));

                await this.Logger.InfoAsync("Rebuilding was completed, validating the result");

                ValidationResult result = await this.businessCalendarService.ValidateCalendarAsync(cancellationToken);
                await this.Logger.LogResultAsync(result);

                if (!result.IsSuccessful)
                {
                    return -1;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                await this.Logger.LogExceptionAsync("Error rebuilding calendar", e);
                return -1;
            }

            await this.Logger.InfoAsync("Calendar has been rebuilt successfully");
            return 0;
        }

        #endregion
    }
}
