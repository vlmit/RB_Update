using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tessa.BusinessCalendar;
using Tessa.Cards;
using Tessa.Platform.Operations;
using Tessa.Platform.Validation;
using Tessa.UI;
using Tessa.UI.Cards;
using Tessa.UI.Cards.Controls;
using Tessa.UI.Notifications;

namespace Tessa.Extensions.Default.Client.UI
{
    public sealed class CalendarUIExtension :
        CardUIExtension
    {
        #region Constructors

        public CalendarUIExtension(
            IBusinessCalendarService businessCalendarService,
            IOperationRepository operationRepository,
            INotificationUIManager notificationUIManager)
        {
            this.businessCalendarService = businessCalendarService;
            this.operationRepository = operationRepository;
            this.notificationUIManager = notificationUIManager;
        }

        #endregion

        #region Fields

        private readonly IBusinessCalendarService businessCalendarService;

        private readonly IOperationRepository operationRepository;

        private readonly INotificationUIManager notificationUIManager;

        #endregion

        #region Command Actions

        private async void ValidateCalendarButtonActionAsync(object parameter)
        {
            ValidationResult result = await this.businessCalendarService.ValidateCalendarAsync();
            TessaDialog.ShowNotEmpty(result, "$UI_BusinessCalendar_ValidatedTitle");
        }


        private async void RebuildCalendarButtonActionAsync(object parameter)
        {
            var context = UIContext.Current;
            ICardEditorModel editor = context.CardEditor;

            if (editor != null && !editor.OperationInProgress)
            {
                using (editor.SetOperationInProgress())
                {
                    Guid operationID = await this.operationRepository.CreateAsync(OperationTypes.CalendarRebuild);

                    bool result = await editor.SaveCardAsync(
                        context,
                        new Dictionary<string, object>(StringComparer.Ordinal)
                        {
                            { BusinessCalendarHelper.RebuildOperationGuidKey, operationID },
                        });

                    if (!result)
                    {
                        return;
                    }

                    using (TessaSplash.Create(TessaSplashMessage.CalculatingCalendar))
                    {
                        do
                        {
                            await Task.Delay(500).ConfigureAwait(false);
                        } while (await this.operationRepository.IsAliveAsync(operationID).ConfigureAwait(false));
                    }
                }

                await this.notificationUIManager.ShowTextOrMessageBoxAsync("$UI_BusinessCalendar_CalendarIsRebuiltNotification").ConfigureAwait(false);
            }
        }

        #endregion

        #region PrivateMethods

        private static void AttachCommandToButton(ICardUIExtensionContext context, string buttonAlias, Action<object> action)
        {
            if (!context.Model.Controls.TryGet(buttonAlias, out IControlViewModel control))
            {
                return;
            }

            var button = (ButtonViewModel)control;
            if (!context.Model.Card.Permissions.Resolver.GetCardPermissions().Has(CardPermissionFlags.AllowModify))
            {
                button.IsReadOnly = true;
                return;
            }

            button.CommandClosure.Execute = action;
        }

        #endregion

        #region Base Overrides

        public override async Task Initialized(ICardUIExtensionContext context)
        {
            AttachCommandToButton(context, "ValidateCalendar", this.ValidateCalendarButtonActionAsync);
            AttachCommandToButton(context, "RebuildCalendar", this.RebuildCalendarButtonActionAsync);
        }

        #endregion
    }
}
