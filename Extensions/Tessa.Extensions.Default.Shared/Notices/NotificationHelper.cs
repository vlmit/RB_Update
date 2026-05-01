using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Tessa.Cards;
using Tessa.Cards.Caching;
using Tessa.Extensions.Default.Shared.Workflow.Wf;
using Tessa.Localization;
using Tessa.Notices;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Placeholders;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Shared.Notices
{
    /// <summary>
    /// Вспомогательные средства для использования в уведомлениях.
    /// </summary>
    public static class NotificationHelper
    {
        #region Private Constants

        private const string NotificationsKey = "Notifications";

        #endregion

        #region Public Constants

        public const string DefaultCss =
@"		body
		{
			font-family: tahoma, verdana, arial, sans-serif;
		}

		div.head_link
		{
			border-bottom: #b9c4da 1px solid;
		}

		#head_link
		{
			display: inline-block;
			font-weight: bold;
			font-size: 14px;
			margin: 3px 0px 5px;
			overflow: hidden;
			color: #365a7b;
		}

		a
		{
			text-decoration: none;
		}

		div.task_content
		{
			display: inline-block;
			overflow: hidden;
			font-size: 15px;
			margin: 5px 0px 3px;
			padding-top: 5px;
			border-top: #b9c4da 1px solid;
		}

		span.data_description
		{
			display: inline-block;
			margin: 11px;
			font-size: 13px;
			overflow: hidden;
			color: #808080;
		}

		span.data
		{
			display: inline-block;
			margin: 11px;
			font-size: 13px;
			overflow: hidden;
		}";

        #endregion

        #region Public Methods

        /// <summary>
        /// Возвращает текст со ссылкой на карточку в desktop-клиенте или в web-клиенте в формате html,
        /// который обычно добавляется снизу от основного содержимого уведомления.
        ///
        /// Перед ссылкой может выводиться разделитель от выше расположенного контента.
        /// Ссылка локализуется на текущий или заданный язык <paramref name="cultureInfo"/>.
        /// </summary>
        /// <param name="cardLink">
        /// Ссылка на карточку, доступную в desktop-клиенте. Может быть равна <c>null</c> или пустой строке.
        /// </param>
        /// <param name="webCardLink">
        /// Ссылка на карточку, доступную в web-клиенте. Может быть равен <c>null</c> или пустой строке.
        /// </param>
        /// <param name="cultureInfo">
        /// Культура, используемая для локализации уведомлений, или <c>null</c>, если используется текущая культура.
        /// </param>
        /// <returns>
        /// Текст со ссылкой на карточку и разделителем, или пустую строку, если ссылка не задана.
        /// </returns>
        public static string GetCardLinkHtmlFooter(string cardLink, string webCardLink, CultureInfo cultureInfo = null)
        {
            var builder = StringBuilderHelper.Acquire(capacity: 128);

            if (!string.IsNullOrEmpty(cardLink))
            {
                builder.AppendFormat(
                    "<br/><a href=\"{0}\" style=\"text-decoration:underline;font-size:14px;color:#808080\" target=\"_blank\">{1}</a>",
                    cardLink,
                    LocalizationManager.GetString(
                        "Notification_CardLinkCaption",
                        cultureInfo ?? LocalizationManager.CurrentUICulture));
            }

            if (!string.IsNullOrEmpty(webCardLink))
            {
                if (builder.Length > 0)
                {
                    builder.Append("<br/>");
                }

                builder.AppendFormat(
                    "<br/><a href=\"{0}\" style=\"text-decoration:underline;font-size:14px;color:#808080\" target=\"_blank\">{1}</a>",
                    webCardLink,
                    LocalizationManager.GetString(
                        "Notification_WebCardLinkCaption",
                        cultureInfo ?? LocalizationManager.CurrentUICulture));
            }

            return builder.ToStringAndRelease();
        }


        /// <summary>
        /// Формирует строку для параметра Name для использования в ссылках
        /// </summary>
        /// <param name="digest">Дайджест</param>
        /// <param name="fullNumber">Номер карточки</param>
        /// <param name="typeCaption">Тип Карточки</param>
        /// <returns></returns>
        public static string GetNameForLink(string digest, string fullNumber, string typeCaption)
        {
            if (!string.IsNullOrEmpty(digest))
            {
                return digest;
            }
            if (!string.IsNullOrEmpty(fullNumber))
            {
                return fullNumber;
            }
            if (!string.IsNullOrEmpty(typeCaption))
            {
                return typeCaption;
            }
            return LocalizationManager.GetString("UI_Common_DefaultDigest_Card");
        }


        public static void AddNotification<T>(
            IDictionary<string, object> info,
            params T[] notifications)
            where T : INotification
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            if (notifications == null)
            {
                throw new ArgumentNullException("notifications");
            }
            if (typeof(T) == typeof(INotification))
            {
                throw new ArgumentException(
                    string.Format(
                        "Method call AddMessage<{0}> has to use a concrete class as generic parameter, not an interface {1}.",
                        typeof(T).Name,
                        nameof(INotification)),
                    "notifications");
            }

            if (notifications.Length == 0)
            {
                return;
            }

            var notificationInfo = info.TryGet<Dictionary<string, object>>(NotificationsKey);
            if (notificationInfo == null)
            {
                notificationInfo = new Dictionary<string, object>(StringComparer.Ordinal);
                info.Add(NotificationsKey, notificationInfo);
            }

            object[] attributes = typeof(T).GetCustomAttributes(typeof(NotificationAttribute), inherit: false);
            if (attributes.Length != 1)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Type {0} doesn't contain attribute {1}.",
                        typeof(T).Name,
                        nameof(NotificationAttribute)));
            }

            var attribute = (NotificationAttribute)attributes[0];
            string notificationTypeKey = attribute.Key;

            var notificationList = notificationInfo.TryGet<IList>(notificationTypeKey);
            if (notificationList == null)
            {
                notificationList = new List<object>();
                notificationInfo.Add(notificationTypeKey, notificationList);
            }

            foreach (T notification in notifications)
            {
                var storage = new Dictionary<string, object>(StringComparer.Ordinal);
                notification.SerializeTo(storage);

                notificationList.Add(storage);
            }
        }

        public static void ModifyTaskCaption(NotificationEmail email, CardTask task)
        {
            if (task.Card.Sections.TryGetValue("TaskCommonInfo", out var section))
            {
                var kindCaption = section.RawFields.TryGet<string>("KindCaption");

                if (!string.IsNullOrWhiteSpace(kindCaption))
                {
                    email.PlaceholderAliases.SetReplacement("taskType", "f:TaskCommonInfo.KindCaption task");
                }
            }
        }

        public static void ModifyEmailForMobileApprovers(
            NotificationEmail email,
            CardTask task,
            string mobileApprovalEmail)
        {
            var taskTypeID = task.TypeID;
            if (WfHelper.TaskTypeIsResolution(taskTypeID))
            {
                email.BodyTemplate = email.BodyTemplate
                       .Replace(
                           "{optionsForMobileApproval}",
                           $@"
                        <!--.MA-->
                        <br/>
		                    <p>{GetCompleteRef(task.RowID, mobileApprovalEmail)}{{$KrMessages_TaskCompleteLink}}</a></p>
		                <br/>
                        <!--.MAE-->", StringComparison.Ordinal);
            }
            else if (taskTypeID == DefaultTaskTypes.KrApproveTypeID || taskTypeID == DefaultTaskTypes.KrAdditionalApprovalTypeID)
            {
                email.BodyTemplate = email.BodyTemplate
                    .Replace(
                        "{optionsForMobileApproval}",
                        $@"
                        <!--.MA-->
                        <br/>
                            <p>{GetApprovalRef(true, task.RowID, mobileApprovalEmail)}{{$KrMessages_ApproveLink}}</a></p>
                            <p>{GetApprovalRef(false, task.RowID, mobileApprovalEmail)}{{$KrMessages_DisapproveLink}}</a></p>
                        <br/>
                        <!--.MAE-->", StringComparison.Ordinal);
            }
            else if (taskTypeID == DefaultTaskTypes.KrSigningTypeID)
            {
                email.BodyTemplate = email.BodyTemplate
                    .Replace(
                        "{optionsForMobileApproval}",
                        $@"
                        <!--.MA-->
                        <br/>
                            <p>{GetSigningRef(true, task.RowID, mobileApprovalEmail)}{{$KrMessages_SignLink}}</a></p>
                            <p>{GetSigningRef(false, task.RowID, mobileApprovalEmail)}{{$KrMessages_DeclineLink}}</a></p>
                        <br/>
                        <!--.MAE-->", StringComparison.Ordinal);
            }

            email.BodyTemplate = email.BodyTemplate.Replace(
                "{filesForMobileApproval}",
                @"
                <!--.MA-->
                <br/>
	            <!-- .MISSED_FILES -->
	            <!-- .OVERSIZED_FILES -->
                <!--.MAE-->", StringComparison.Ordinal);

            List<MailFile> mailFiles = null;

            email.GetMailInfoFuncAsync = async (user, card, ct) =>
            {
                if (user.HasMobileApproval)
                {
                    if (mailFiles == null)
                    {
                        mailFiles = new List<MailFile>();
                        foreach (var file in card.Files)
                        {
                            mailFiles.Add(
                                file.ToMailFile());
                        }
                    }

                    var mailInfo = new MailInfo()
                    {
                        CardID = card.ID,
                        CardTypeID = card.TypeID,
                        CardTypeName = card.TypeName,
                        LanguageCode = user.LanguageCode,
                    };
                    mailInfo.Files.AddItems(mailFiles);

                    return mailInfo;
                }
                return null;
            };

            email.ModifyBodyFuncAsync = async (body, user, ct) =>
            {
                if (!user.HasMobileApproval)
                {
                    // Удаляем блок с вариантами завершения
                    var startIndex = body.IndexOf("<!--.MA-->", StringComparison.Ordinal);
                    var endIndex = body.IndexOf("<!--.MAE-->", StringComparison.Ordinal);
                    body = body.Remove(startIndex, endIndex - startIndex + 11);
                }

                return body;
            };
        }

        public static Dictionary<string, object> GetInfoWithTask(CardTask task) =>
            new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [PlaceholderHelper.TaskKey] = task
            };

        public static async ValueTask<string> GetMobileApprovalEmailAsync(ICardCache cardCache, CancellationToken cancellationToken = default)
        {
            var serverInstance = await cardCache.Cards.GetAsync(CardHelper.ServerInstanceTypeName, cancellationToken).ConfigureAwait(false);

            return serverInstance.IsSuccess
                ? serverInstance.GetValue().Sections["ServerInstances"].RawFields.Get<string>("MobileApprovalEmail")
                : null;
        }

        public static void AddNotification<T>(
            CardInfoStorageObject infoObject,
            params T[] notifications)
            where T : INotification
        {
            if (infoObject == null)
            {
                throw new ArgumentNullException("infoObject");
            }

            AddNotification(infoObject.Info, notifications);
        }


        public static bool HasNotifications(IDictionary<string, object> info) => info?.ContainsKey(NotificationsKey) == true;


        public static async Task SendNotificationsAsync(
            IDictionary<string, object> info,
            Guid cardID,
            Guid cardTypeID,
            string cardDigest,
            IValidationResultBuilder validationResult,
            INotificationResolver notificationResolver,
            bool withoutTransaction = false,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(info, "info");
            Check.ArgumentNotNull(validationResult, "validationResult");
            Check.ArgumentNotNull(notificationResolver, "notificationResolver");

            var notificationInfo = info.TryGet<Dictionary<string, object>>(NotificationsKey);
            if (notificationInfo == null || notificationInfo.Count == 0)
            {
                return;
            }

            INotificationContext context = null;
            IDbScopeInstance dbScopeInstance = null;

            try
            {
                foreach (KeyValuePair<string, object> pair in notificationInfo)
                {
                    var list = pair.Value as IList;
                    INotificationSender sender;
                    if (list == null
                        || list.Count == 0
                        || (sender = notificationResolver.TryResolve(pair.Key)) == null)
                    {
                        continue;
                    }

                    INotification[] notifications = list
                        .Cast<Dictionary<string, object>>()
                        .Select(x => sender.DeserializeFrom(x))
                        .ToArray();

                    if (context == null)
                    {
                        context = await notificationResolver
                            .CreateContextAsync(
                                cardID,
                                cardTypeID,
                                cardDigest,
                                validationResult,
                                withoutTransaction,
                                cancellationToken).ConfigureAwait(false);

                        dbScopeInstance = context.DbScope.Create();
                    }

                    await sender.SendMessageAsync(context, notifications, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                if (dbScopeInstance != null)
                {
                    await dbScopeInstance.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        public static string GetCompleteRef(
            Guid taskID,
            string mobileApprovalEmail)
        {
            StringBuilder approvalRef = StringBuilderHelper.Acquire(128);

            const string subject = "{$KrMessages_CompleteTaskResultMessage}: {f:DocumentCommonInfo.FullNumber}{f:DocumentCommonInfo.Subject format as (, [0])}";

            // это текст HTML, поэтому должен использоваться метод HtmlEncode, а не UrlEncode
            approvalRef
                .Append("<a href=\"mailto:")
                .Append(mobileApprovalEmail)
                .Append("?subject=[tsk-1-")
                .Append(taskID.ToString("N"))
                .Append("] ")
                .Append(HttpUtility.HtmlEncode(subject))
                .Append("&body=%0A%0A%0A%3C")
                .Append(HttpUtility.HtmlEncode(LocalizationManager.GetString("KrMessages_CommentTextMessage")))
                .Append("%3E");

            // 940 - максимальная длина; минус 2 символа - кавычка и скобка ниже
            if (approvalRef.Length > 938)
            {
                approvalRef.Remove(938, approvalRef.Length - 938);
            }

            return approvalRef
                .Append("\">")
                .ToStringAndRelease();
        }


        public static string GetRef(
            bool isApprove,
            Guid taskID,
            string mobileApprovalEmail,
            string locApproveSubject,
            string locDisapproveSubject)
        {
            StringBuilder approvalRef = StringBuilderHelper.Acquire(128);

            int approve;
            string subject;

            if (isApprove)
            {
                approve = 1;
                subject = $"{{{locApproveSubject}}}";
            }
            else
            {
                approve = 0;
                subject = $"{{{locDisapproveSubject}}}";
            }

            subject += ": {f:DocumentCommonInfo.FullNumber}{f:DocumentCommonInfo.Subject format as (, [0])}";

            // это текст HTML, поэтому должен использоваться метод HtmlEncode, а не UrlEncode
            approvalRef
                .Append("<a href=\"mailto:")
                .Append(mobileApprovalEmail)
                .Append("?subject=[apr-")
                .Append(approve)
                .Append('-')
                .Append(taskID.ToString("N"))
                .Append("] ")
                .Append(HttpUtility.HtmlEncode(subject))
                .Append("&body=%0A%0A%0A%3C")
                .Append(HttpUtility.HtmlEncode(LocalizationManager.GetString("KrMessages_CommentTextMessage")))
                .Append("%3E");

            // 940 - максимальная длина; минус 2 символа - кавычка и скобка ниже
            if (approvalRef.Length > 938)
            {
                approvalRef.Remove(938, approvalRef.Length - 938);
            }

            return approvalRef
                .Append("\">")
                .ToStringAndRelease();
        }

        public static string GetApprovalRef(bool isApprove, Guid taskID, string mobileApprovalEmail) =>
            GetRef(isApprove, taskID, mobileApprovalEmail, "$KrMessages_ApprovalResultMessage", "$KrMessages_DisapprovalResultMessage");

        public static string GetSigningRef(bool isApprove, Guid taskID, string mobileApprovalEmail) =>
            GetRef(isApprove, taskID, mobileApprovalEmail, "$KrMessages_SigningResultMessage", "$KrMessages_DecliningResultMessage");

        #endregion
    }
}
