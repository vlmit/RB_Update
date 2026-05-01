using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Tessa.Cards;
using Tessa.Extensions.Default.Shared;
using Tessa.Extensions.Default.Shared.Settings;
using Tessa.Extensions.Default.Shared.Workflow.Wf;
using Tessa.Localization;
using Tessa.Platform;
using Tessa.Platform.Storage;
using Tessa.UI.WorkflowViewer;
using Tessa.UI.WorkflowViewer.Helpful;
using Tessa.UI.WorkflowViewer.Shapes;

namespace Tessa.Extensions.Default.Client.Workflow.Wf
{
    /// <summary>
    /// Объект, создающий узлы визуализации резолюций по истории заданий.
    /// </summary>
    /// <returns>
    /// Может запрашиваться из Unity по интерфейсу <see cref="IWfResolutionVisualizationGenerator"/>.
    /// </returns>
    public sealed class WfResolutionVisualizationGenerator :
        IWfResolutionVisualizationGenerator
    {
        #region Constructors

        public WfResolutionVisualizationGenerator(KrSettingsLazy settingsLazy) =>
            this.settingsLazy = settingsLazy ?? throw new ArgumentNullException(nameof(settingsLazy));

        #endregion

        #region Constants

        private const double LargeFontSize = 14;

        private const double NormalFontSize = 12;

        #endregion

        #region Fields

        private readonly KrSettingsLazy settingsLazy;

        #endregion

        #region Private Methods

        private async ValueTask SetupResolutionNodeAsync(
           IWfResolutionVisualizationContext context,
           INode node)
        {
            var mainContent = new TextBlock { MaxWidth = 200.0, MaxHeight = 70.0, TextWrapping = TextWrapping.Wrap };
            var mainContentToolTip = new TextBlock { MaxWidth = 600.0, TextWrapping = TextWrapping.Wrap };
            var expandedContent = new TextBlock { MaxWidth = 400.0, TextWrapping = TextWrapping.Wrap };

            string kindCaption = LocalizationManager.Format(context.HistoryItem.KindCaption);

            string typeCaption = LocalizationManager.Localize(context.HistoryItem.TypeCaption);
            string effectiveTypeCaption = string.IsNullOrEmpty(kindCaption) ? typeCaption : kindCaption;

            mainContent.Inlines.Add(new Run(effectiveTypeCaption) { FontSize = LargeFontSize });
            mainContentToolTip.Inlines.Add(new Run(typeCaption));
            expandedContent.Inlines.Add(new Run(effectiveTypeCaption) { FontSize = LargeFontSize });

            // состояние резолюции
            string expandedResolutionState = await this.GetResolutionStateAsync(context);
            string mainResolutionState = expandedResolutionState.Limit(
                (await this.settingsLazy.GetValueAsync(context.CancellationToken).ConfigureAwait(false))
                .ChildResolutionColumnStateMaxLength);

            mainContent.Inlines.Add(new Run(Environment.NewLine + mainResolutionState) { FontSize = NormalFontSize });
            expandedContent.Inlines.Add(new Run(Environment.NewLine + expandedResolutionState) { FontSize = NormalFontSize });

            Guid typeID = context.HistoryItem.TypeID;
            bool isInProgress = context.HistoryItem.InProgress.HasValue;
            bool isCompleted = context.HistoryItem.Completed.HasValue;

            // тип резолюции
            Dictionary<string, object> historyItemInfo = context.HistoryItem.TryGetInfo();
            int overdueQuants = historyItemInfo != null
                ? historyItemInfo.TryGet<int>(WfHelper.OverdueQuantsKey)
                : 0;

            bool isOverdue = overdueQuants > 0 && !WfHelper.TaskTypeIsResolutionWithoutOverdue(typeID);

            // информация о просроченности задания (как завершённых, так и ещё не завершённых)
            // не отображается для проекта резолюции и контроля исполнения, т.к. их срок задаётся системой автоматически
            if (isOverdue)
            {
                // для отрицательного числа квантов -overdueQuants автоматически добавляется предлог "на"
                string overdue =
                    Environment.NewLine
                    + string.Format(
                        LocalizationManager.GetString("WfResolution_Visualization_Overdue"),
                        FormattingHelper.FormatDateDiff(-overdueQuants));

                mainContent.Inlines.Add(new Run(overdue) { FontSize = NormalFontSize });
                expandedContent.Inlines.Add(new Run(overdue) { FontSize = NormalFontSize });
            }

            bool hasLineBreakAfterHeader = false;

            if (isCompleted)
            {
                // фактическая дата завершения
                string completed =
                    Environment.NewLine
                    + Environment.NewLine
                    + string.Format(
                        LocalizationManager.GetString("WfResolution_Visualization_Completed"),
                        FormattingHelper.FormatDateTimeWithoutSeconds(context.HistoryItem.Completed));

                mainContentToolTip.Inlines.Add(new Run(completed));
                expandedContent.Inlines.Add(new Run(completed) { FontSize = NormalFontSize });
            }
            else
            {
                // запланированная дата завершения с информацией по тому, сколько времени осталось или насколько просрочено
                int untilCompletionQuants = historyItemInfo != null
                    ? historyItemInfo.TryGet<int>(WfHelper.UntilCompletionQuantsKey)
                    : 0;

                // если до завершения осталось меньше суток, но задание ещё не просрочено,
                // то выводим надпись прямо в шапке узла визуализации
                string untilCompletionText = null;
                if (untilCompletionQuants >= 0 && untilCompletionQuants < 32)
                {
                    untilCompletionText = FormattingHelper.FormatDateDiff(untilCompletionQuants);

                    string timeLeft = string.Format(
                        Environment.NewLine
                        + LocalizationManager.GetString("WfResolution_Visualization_TimeLeft_TooLittle"),
                        untilCompletionText);

                    mainContent.Inlines.Add(new Run(timeLeft) { FontSize = NormalFontSize });
                    expandedContent.Inlines.Add(new Run(timeLeft) { FontSize = NormalFontSize });
                }

                string plannedDateTime = FormattingHelper.FormatDateTimeWithoutSeconds(context.HistoryItem.Planned);
                string planned =
                    Environment.NewLine
                    + (untilCompletionQuants >= 0
                        ? string.Format(
                            LocalizationManager.GetString("WfResolution_Visualization_TimeLeft"),
                            untilCompletionText ?? FormattingHelper.FormatDateDiff(untilCompletionQuants),
                            plannedDateTime)
                        : string.Format(
                            LocalizationManager.GetString("WfResolution_Visualization_CompleteUntilForOverdue"),
                            plannedDateTime));

                string displayPlanned = Environment.NewLine + planned;
                mainContentToolTip.Inlines.Add(new Run(planned));
                expandedContent.Inlines.Add(new Run(displayPlanned) { FontSize = NormalFontSize });
                hasLineBreakAfterHeader = true;

                // дата взятия в работу, если задание в работе и ещё не завершено
                if (isInProgress)
                {
                    int inProgressQuants = historyItemInfo != null
                        ? historyItemInfo.TryGet<int>(WfHelper.InProgressQuantsKey)
                        : 0;

                    string inProgress =
                        Environment.NewLine
                        + string.Format(
                            LocalizationManager.GetString("WfResolution_Visualization_InProgress"),
                            FormattingHelper.FormatDateDiff(inProgressQuants),
                            FormattingHelper.FormatDateTimeWithoutSeconds(context.HistoryItem.InProgress.GetValueOrDefault()));

                    mainContentToolTip.Inlines.Add(new Run(inProgress));
                    expandedContent.Inlines.Add(new Run(inProgress) { FontSize = NormalFontSize });
                }
            }

            // дата постановки задачи
            string created =
                Environment.NewLine
                + string.Format(
                    LocalizationManager.GetString("WfResolution_Visualization_Created"),
                    FormattingHelper.FormatDateTimeWithoutSeconds(context.HistoryItem.Created));

            mainContentToolTip.Inlines.Add(new Run(created));
            expandedContent.Inlines.Add(new Run(created) { FontSize = NormalFontSize });

            // запланированная дата завершения для уже завершённой задачи
            if (isCompleted)
            {
                string planned =
                    Environment.NewLine
                    + string.Format(
                        LocalizationManager.GetString("WfResolution_Visualization_Planned"),
                        FormattingHelper.FormatDateTimeWithoutSeconds(context.HistoryItem.Planned));

                mainContentToolTip.Inlines.Add(new Run(planned));
                expandedContent.Inlines.Add(new Run(planned) { FontSize = NormalFontSize });
            }

            // роль, на которую назначена резолюция
            string roleName =
                Environment.NewLine
                + string.Format(
                    LocalizationManager.GetString("WfResolution_Visualization_AssigneeRole"),
                    LocalizationManager.Format(context.HistoryItem.RoleName));

            string displayRoleName = hasLineBreakAfterHeader ? roleName : Environment.NewLine + roleName;
            mainContentToolTip.Inlines.Add(new Run(roleName));
            expandedContent.Inlines.Add(new Run(displayRoleName) { FontSize = NormalFontSize });

            // здесь и ниже между шапкой и прочим текст уже гарантированно есть пустая строка

            // контролёр резолюции
            string controllerName = historyItemInfo?.TryGet<string>(WfHelper.HistoryControllerNameKey);

            if (!string.IsNullOrEmpty(controllerName))
            {
                string controllerText =
                    Environment.NewLine
                    + string.Format(
                        LocalizationManager.GetString("WfResolution_Visualization_ControllerRole"),
                        LocalizationManager.Localize(controllerName));

                mainContentToolTip.Inlines.Add(new Run(controllerText));
                expandedContent.Inlines.Add(new Run(controllerText) { FontSize = NormalFontSize });
            }

            // комментарий автора или результат завершения резолюции
            string historyResult = context.HistoryItem.Result;
            string comment = LocalizationManager.Format(
                string.IsNullOrEmpty(historyResult)
                    ? TryGetCommentFromTask(context)
                    : historyResult);

            if (!string.IsNullOrWhiteSpace(comment))
            {
                string emptyLineAndComment = Environment.NewLine + Environment.NewLine + comment;
                mainContentToolTip.Inlines.Add(new Run(emptyLineAndComment));
                expandedContent.Inlines.Add(new Run(emptyLineAndComment) { FontSize = NormalFontSize });
            }

            // завершаем настройку узла
            mainContent.ToolTip = mainContentToolTip;
            ToolTipService.SetInitialShowDelay(mainContent, 700);
            ToolTipService.SetShowDuration(mainContent, 30000);

            node.SetExpandedContent(expandedContent, nodeExpansionManager);

            node.ContentControl = mainContent;
            node.Background = GetBackground(context, isOverdue);
            node.IsChildNode = context.ParentNode != null
                && typeID == DefaultTaskTypes.WfResolutionChildTypeID;
        }



        private static string TryGetCommentFromTask(IWfResolutionVisualizationContext context)
        {
            if (context.Task == null)
            {
                return null;
            }

            Card taskCard = context.Task.TryGetCard();
            StringDictionaryStorage<CardSection> taskSections;
            if (taskCard == null
                || (taskSections = taskCard.TryGetSections()) == null
                || !taskSections.TryGetValue(WfHelper.ResolutionSection, out CardSection resolutionSection))
            {
                return null;
            }

            string parentComment = resolutionSection.RawFields.TryGet<string>(WfHelper.ResolutionParentCommentField).NormalizeCommentWithLineBreaks();
            return string.IsNullOrEmpty(parentComment)
                ? null
                : string.Format(LocalizationManager.LocalizeAndEscapeFormat("$WfResolution_Result_ParentComment"), parentComment);
        }


        private static Brush GetBackground(IWfResolutionVisualizationContext context, bool overdue)
        {
            return context.HistoryItem.OptionID.HasValue
                ? (context.HistoryItem.OptionID == DefaultCompletionOptions.Revoke
                    ? WorkflowBrushes.LightGray
                    : WorkflowBrushes.Gray)
                : (overdue
                    ? WorkflowBrushes.Red
                    : context.HistoryItem.UserID.HasValue
                        ? WorkflowBrushes.Yellow
                        : WorkflowBrushes.Blue);
        }


        private async ValueTask<string> GetResolutionStateAsync(IWfResolutionVisualizationContext context)
        {
            CardTaskHistoryItem item = context.HistoryItem;
            return WfHelper.GetResolutionState(
                await this.settingsLazy.GetValueAsync(context.CancellationToken),
                item.RoleName,
                item.UserID.HasValue ? item.UserName : null,
                item.OptionID,
                limitLength: false);
        }


        private static string TryGetArrowCaption(IWfResolutionVisualizationContext context)
        {
            switch (context.ParentAction)
            {
                case WfResolutionParentAction.CreateChild:
                case WfResolutionParentAction.SendToPerformer:
                    Dictionary<string, object> historyItemInfo = context.HistoryItem.Info;
                    return historyItemInfo != null
                           && historyItemInfo.TryGet<object>(WfHelper.HistoryControlledKey) != null
                        ? LocalizationManager.GetString("WfResolution_Visualization_WithControl")
                        : null;

                case WfResolutionParentAction.SendControl:
                    return LocalizationManager.GetString("WfResolution_Visualization_ToControl");

                default:
                    return null;
            }
        }


        private static readonly BorderNodeExpansionManager nodeExpansionManager = null;
            //new BorderNodeExpansionManager();

        #endregion

        #region IWfResolutionVisualizationGenerator Members

        /// <summary>
        /// Создаёт узел со стрелкой от родительского узла по записи в истории заданий резолюций.
        /// Возвращает созданный узел для этой записи или <c>null</c>,
        /// если узел не был создан. Возвращённый узел не добавляется в макет визуализации.
        /// </summary>
        /// <param name="context">Контекст с информацией по записи, для которой создаётся узел.</param>
        /// <returns>
        /// Созданный узел для этой записи или <c>null</c>,
        /// если узел не был создан. Возвращённый узел не добавляется в макет визуализации.
        /// </returns>
        public async ValueTask<INode> GenerateAsync(IWfResolutionVisualizationContext context)
        {
            var node = context.NodeFactory.CreateBorderedWithForeground<RoundedRectangleNode>();
            await this.SetupResolutionNodeAsync(context, node);

            context.NodeLayout.Nodes.Add(node);

            if (context.ParentNode != null)
            {
                string caption = TryGetArrowCaption(context);
                Connection arrow = Connection.CreateArrow(context.ParentNode, node, caption);
                context.NodeLayout.Connections.Add(arrow);
            }

            return node;
        }

        #endregion
    }
}
