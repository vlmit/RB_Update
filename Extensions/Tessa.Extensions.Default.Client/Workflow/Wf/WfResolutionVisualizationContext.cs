using System;
using System.Threading;
using Tessa.Cards;
using Tessa.Platform.Storage;
using Tessa.UI.WorkflowViewer.Factories;
using Tessa.UI.WorkflowViewer.Layouts;
using Tessa.UI.WorkflowViewer.Shapes;

namespace Tessa.Extensions.Default.Client.Workflow.Wf
{
    /// <summary>
    /// Контекст расширений на посещение записей в истории резолюций
    /// для визуализации посредством <see cref="IWfResolutionVisualizationGenerator"/>.
    /// </summary>
    public sealed class WfResolutionVisualizationContext :
        IWfResolutionVisualizationContext
    {
        #region Constructors

        /// <summary>
        /// Создаёт экземпляр класса с указанием значений его свойств.
        /// </summary>
        /// <param name="nodeLayout">Макет визуализатора.</param>
        /// <param name="nodeFactory">Фабрика узлов визуализатора.</param>
        /// <param name="card">Карточка, обход резолюций которой выполняется.</param>
        /// <param name="rootHistoryItem">
        /// Корневая запись в истории заданий среди посещаемых резолюций.
        /// Не может быть равна <c>null</c>.
        /// </param>
        /// <param name="rootTask">
        /// Корневое задание среди посещаемых резолюций
        /// или <c>null</c>, если для записи в истории заданий отсутствует задание.
        /// </param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        public WfResolutionVisualizationContext(
            INodeLayout nodeLayout,
            INodeFactory nodeFactory,
            Card card,
            CardTaskHistoryItem rootHistoryItem,
            CardTask rootTask,
            CancellationToken cancellationToken = default)
        {
            this.NodeLayout = nodeLayout ?? throw new ArgumentNullException(nameof(nodeLayout));
            this.NodeFactory = nodeFactory ?? throw new ArgumentNullException(nameof(nodeFactory));
            this.Card = card ?? throw new ArgumentNullException(nameof(card));
            this.RootHistoryItem = rootHistoryItem ?? throw new ArgumentNullException(nameof(rootHistoryItem));
            this.RootTask = rootTask;
            this.CancellationToken = cancellationToken;
        }

        #endregion

        #region IExtensionContext Members

        /// <doc path='info[@type="IExtensionContext" and @item="CancellationToken"]'/>
        public CancellationToken CancellationToken { get; set; }

        #endregion

        #region IWfResolutionVisualizationContext Members

        /// <summary>
        /// Макет визуализатора.
        /// </summary>
        public INodeLayout NodeLayout { get; }

        /// <summary>
        /// Фабрика узлов визуализатора.
        /// </summary>
        public INodeFactory NodeFactory { get; }

        /// <summary>
        /// Карточка, обход резолюций которой выполняется.
        /// </summary>
        public Card Card { get; }

        /// <summary>
        /// Корневая запись в истории заданий среди посещаемых резолюций.
        /// Не может быть равна <c>null</c>.
        /// </summary>
        public CardTaskHistoryItem RootHistoryItem { get; }

        /// <summary>
        /// Корневое задание среди посещаемых резолюций
        /// или <c>null</c>, если для записи в истории заданий отсутствует задание.
        /// </summary>
        public CardTask RootTask { get; }

        /// <summary>
        /// Дополнительная информация, которая может пригодиться между посещениями узлов.
        /// </summary>
        public ISerializableObject Info { get; } = new SerializableObject();

        /// <summary>
        /// Посещаемая запись в истории заданий.
        /// </summary>
        /// <remarks>
        /// Значение изменяется при посещении очередного узла.
        /// </remarks>
        public CardTaskHistoryItem HistoryItem { get; set; }

        /// <summary>
        /// Задание, которое соответствует записи <see cref="HistoryItem"/>,
        /// или <c>null</c>, если задание уже завершено или не было загружено.
        /// </summary>
        /// <remarks>
        /// Значение изменяется при посещении очередного узла.
        /// </remarks>
        public CardTask Task { get; set; }

        /// <summary>
        /// Узел, соответствующий текущей резолюции,
        /// или <c>null</c>, если узел для текущей резолюции ещё не был создан.
        ///
        /// При установке узла в методе расширений <see cref="IWfResolutionVisualizationExtension.OnNodeGenerating"/>
        /// генерация узла стандартными средствами не будет выполнена.
        ///
        /// В методе расширений <see cref="IWfResolutionVisualizationExtension.OnNodeGenerated"/>
        /// узел должен быть установлен как текущий сгенерированный узел.
        /// </summary>
        /// <remarks>
        /// Значение изменяется при посещении очередного узла.
        /// </remarks>
        public INode Node { get; set; }

        /// <summary>
        /// Узел, соответствующий родительской резолюции,
        /// или <c>null</c>, если текущая резолюция не имеет родительской.
        /// </summary>
        /// <remarks>
        /// Значение изменяется при посещении очередного узла.
        /// </remarks>
        public INode ParentNode { get; set; }

        /// <summary>
        /// Действие родительской резолюции по отношению к текущей,
        /// в результате которого текущая резолюция была создана.
        /// </summary>
        /// <remarks>
        /// Значение изменяется при посещении очередного узла.
        /// </remarks>
        public WfResolutionParentAction ParentAction { get; set; }

        #endregion
    }
}
