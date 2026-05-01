using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Extensions.Default.Shared.Notices;
using Tessa.Localization;
using Tessa.Platform.Data;

namespace Tessa.Extensions.Default.Server.Notices
{
    public static class NotificationExtensionHelper
    {
        #region Public Methods

        public static string GetCardName<TNotification>(INotificationData<TNotification> data)
            where TNotification : INotification
        {
            string cardDigest = data.CardDigest ?? data.CardNumber;

            return !string.IsNullOrEmpty(cardDigest) &&
                   !string.IsNullOrEmpty(data.CardSubject)
                ? cardDigest + ", " + data.CardSubject
                : !string.IsNullOrEmpty(cardDigest)
                    ? cardDigest
                    : !string.IsNullOrEmpty(data.CardSubject)
                        ? data.CardSubject
                        : null;
        }


        public static string GetCardLink<TNotification>(INotificationContext context, INotificationData<TNotification> data)
            where TNotification : INotification
        {
            return CardHelper.GetLink(context.Session, context.CardID);
        }


        public static string GetWebCardLink(INotificationContext context)
        {
            return CardHelper.GetWebLink(context.WebAddress, context.CardID, normalize: false);
        }


        public static async Task SendNotificationsAsync<TNotification, TNotificationData>(
            INotificationContext context,
            ICollection<TNotification> notifications,
            IEnumerable<TNotificationData> dataList,
            GetNotificationSubjectFuncAsync<TNotification, TNotificationData> getSubjectFuncAsync,
            GetNotificationBodyFuncAsync<TNotification, TNotificationData> getBodyFuncAsync,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
            where TNotificationData : INotificationData<TNotification>
        {
            string lastLanguageCode = null;
            CultureInfo initialUICulture = LocalizationManager.CurrentUICulture;
            IDbScopeInstance dbScopeInstance = context.DbScope.Create();

            try
            {
                foreach (TNotificationData data in dataList)
                {
                    string languageCode = data.LanguageCode;
                    if (!string.Equals(languageCode, lastLanguageCode, StringComparison.Ordinal))
                    {
                        if (languageCode != null)
                        {
                            LocalizationManager.CurrentUICulture = CultureInfo.GetCultureInfo(languageCode);
                            lastLanguageCode = languageCode;
                        }
                        else
                        {
                            LocalizationManager.CurrentUICulture = initialUICulture;
                            lastLanguageCode = null;
                        }
                    }

                    TNotification notification = notifications.FirstOrDefault(x => data.IsApplicable(x));
                    if (notification == null)
                    {
                        continue;
                    }

                    data.Initialize(notification);

                    string subject = await getSubjectFuncAsync(context, notification, data, cancellationToken);
                    string body = notification.Body;

                    string actualBody = string.IsNullOrEmpty(body)
                        ? await getBodyFuncAsync(
                            context, notification, data, subject,
                            GetCardLink(context, data),
                            GetWebCardLink(context),
                            cancellationToken)
                        : body;

                    await context.MailService.PostMessageAsync(
                        data.Email,
                        subject,
                        actualBody,
                        context.ValidationResult,
                        data.Info,
                        cancellationToken);
                }
            }
            finally
            {
                await dbScopeInstance.DisposeAsync();
                LocalizationManager.CurrentUICulture = initialUICulture;
            }
        }

        #endregion
    }
}
