using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using Tessa.Extensions.Default.Shared.Workplaces;
using Tessa.Json;
using Tessa.Json.Bson;
using Tessa.Platform.Storage;
using Tessa.Properties.Resharper;
using Tessa.UI;
using Tessa.UI.Views;
using Tessa.UI.Views.Charting.Charts;
using Tessa.UI.Views.Extensions;
using Tessa.UI.Views.Workplaces.Tree;
using Tessa.Views;

namespace Tessa.Extensions.Default.Client.Workplaces
{
    /// <summary>
    ///     Расширение предоставляющее возможность автоматического обновления узлов рабочего места
    /// </summary>
    /// <remarks>
    /// У расширения есть конфигуратор <see cref="AutomaticNodeRefreshExtensionConfigurator"/>.
    /// </remarks>
    public class AutomaticNodeRefreshExtension : ViewModel<EmptyModel>, ITreeItemExtension, IWorkplaceExtensionSettingsRestore
    {
        /// <summary>
        ///     The refresh pending.
        /// </summary>
        private bool refreshPending = false;

        /// <summary>
        ///     The settings.
        /// </summary>
        [CanBeNull]
        private AutomaticNodeRefreshSettings settings;

        /// <summary>
        ///     The timer.
        /// </summary>
        [NotNull]
        private DispatcherTimer timer;

        /// <summary>
        ///     The tree item.
        /// </summary>
        private ITreeItem treeItem;

        /// <inheritdoc />
        public AutomaticNodeRefreshExtension()
        {
            this.settings = new AutomaticNodeRefreshSettings();
        }

        /// <inheritdoc />
        public void Clone(ITreeItem source, ITreeItem cloned, ICloneableContext context)
        {
        }

        /// <inheritdoc />
        public void Initialize(ITreeItem model)
        {
            this.treeItem = model;
        }

        /// <inheritdoc />
        public void Initialized(ITreeItem model)
        {
            // таймер тикает каждую секунду, за каждый тик отправляется запрос на сервер
            this.timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle) { Interval = TimeSpan.FromSeconds(this.settings.RefreshInterval) };

            this.timer.Tick += async (s, e) => await this.UpdateByTimerAsync(false);

            this.SubscribeToEvents(model);
            if (model.Workplace != null && model.Workplace.IsActive)
            {
                // вкладка с рабочим местом активна на момент запуска приложения
                this.StartTimer();
            }
        }

        /// <inheritdoc />
        public void Restore(Dictionary<string, object> metadata) =>
            this.settings = (AutomaticNodeRefreshSettings) ExtensionSettingsSerializationHelper.DeserializeDictionary<AutomaticNodeRefreshSettings>(metadata);

        /// <inheritdoc />
        protected override bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(PropertyChangedEventManager))
            {
                var eventArgs = (PropertyChangedEventArgs) e;
                if (eventArgs.PropertyName == "Parent")
                {
                    var treeItem = (ITreeItem) sender;
                    if (treeItem.Parent == null)
                    {
                        this.UnSubscribeFromEvents(treeItem);
                        this.StopTimer();
                    }
                    else
                    {
                        this.SubscribeToEvents(treeItem);
                        this.StartTimer();
                    }
                }

                if (eventArgs.PropertyName == "IsActive")
                {
                    var workplace = (IWorkplaceViewModel) sender;
                    if (workplace.IsActive)
                    {
                        if (this.refreshPending)
                        {
                            var _ = this.UpdateByTimerAsync(true);
                        }

                        this.StartTimer();
                    }
                }

                if (eventArgs.PropertyName == "IsExpanded")
                {
                    var isExpanded = ((ITreeItem) sender).IsExpanded;
                    if (isExpanded && this.treeItem.IsVisibleInPath())
                    {
                        if (this.refreshPending)
                        {
                            var _ = this.UpdateByTimerAsync(false);
                        }

                        this.StartTimer();
                    }
                }

                if (eventArgs.PropertyName == "LastUpdateTime")
                {
                    if (this.timer.IsEnabled)
                    {
                        this.StopTimer();
                        this.StartTimer();
                        this.refreshPending = false;
                    }
                }

                return true;
            }

            return base.OnReceiveWeakEvent(managerType, sender, e);
        }

        /// <summary>
        /// Вызывает обновление содержимого табличной части
        /// </summary>
        /// <param name="workplaceViewModel">
        /// Модель рабочего места
        /// </param>
        private static async Task RefreshContentAsync([CanBeNull] IWorkplaceViewModel workplaceViewModel)
        {
            if (workplaceViewModel == null)
            {
                return;
            }

            // обновляем содержимое (таблицы)
            var viewContext = workplaceViewModel.Context.ViewContext;
            if (viewContext != null)
            {
                // получаем верхнюю вью (от которой зависят остальные)
                var rootContext = viewContext.GetRoot();
                var viewComponent = rootContext.TryGetViewContainer();

                if (viewComponent == null || viewComponent.CurrentPage == 1)
                {
                    // либо вью не поддерживает пейджинг, либо страница и так первая, либо это какой-то кастом
                    // если кастом, то надеемся, что он поддерживает RefreshCommand
                    await rootContext.RefreshViewAsync();
                }
                else
                {
                    // Refresh будет автоматом при изменении номера страницы на первую
                    viewComponent.CurrentPage = 1;
                }
            }
        }

        /// <summary>
        /// The refresh table content.
        /// </summary>
        /// <param name="skipUpdateTable">
        /// The skip update table.
        /// </param>
        private Task RefreshTableContentAsync(bool skipUpdateTable)
        {
            this.refreshPending = false;

            return this.settings.WithContentDataRefreshing && !skipUpdateTable
                ? RefreshContentAsync(this.treeItem.Workplace)
                : Task.CompletedTask;
        }

        /// <summary>
        ///     Запускает таймер
        /// </summary>
        private void StartTimer()
        {
            if (!this.timer.IsEnabled)
            {
                this.timer.Start();
            }
        }

        /// <summary>
        ///     Останавливает таймер
        /// </summary>
        private void StopTimer()
        {
            this.timer.Stop();
        }

        /// <summary>
        /// Осуществляет подписку на события изменения рабочего места и активности узла
        /// </summary>
        /// <param name="treeItem">
        /// Узел дерева рабочего места
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// treeItem is null
        /// </exception>
        private void SubscribeToEvents([NotNull] ITreeItem treeItem)
        {
            if (treeItem == null)
            {
                throw new ArgumentNullException("treeItem");
            }

            PropertyChangedEventManager.AddListener(treeItem, this, "Parent");
            if (treeItem.Workplace != null)
            {
                PropertyChangedEventManager.AddListener(treeItem.Workplace, this, "IsActive");
            }

            PropertyChangedEventManager.AddListener(treeItem, this, "LastUpdateTime");

            var currentNode = treeItem.Parent;
            while (currentNode != null)
            {
                PropertyChangedEventManager.AddListener(currentNode, this, "IsExpanded");
                currentNode = currentNode.Parent;
            }
        }

        /// <summary>
        /// Отписывается от событий влияющих на запуск автоматического обновления
        /// </summary>
        /// <param name="treeItem">
        /// Узел дерева
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// treeItem is null
        /// </exception>
        private void UnSubscribeFromEvents([NotNull] ITreeItem treeItem)
        {
            if (treeItem == null)
            {
                throw new ArgumentNullException("treeItem");
            }

            PropertyChangedEventManager.RemoveListener(treeItem, this, "Parent");
            PropertyChangedEventManager.RemoveListener(treeItem, this, "LastUpdateTime");
            if (treeItem.Workplace != null)
            {
                PropertyChangedEventManager.RemoveListener(treeItem.Workplace, this, "IsActive");
            }

            var currentNode = treeItem.Parent;
            while (currentNode != null)
            {
                PropertyChangedEventManager.RemoveListener(currentNode, this, "IsExpanded");
                currentNode = currentNode.Parent;
            }
        }

        /// <summary>
        /// Вызывается при срабатывании таймера
        /// </summary>
        /// <param name="skipUpdateTable">
        /// Признак необходимости отменить обновление таблицы
        /// </param>
        private async Task UpdateByTimerAsync(bool skipUpdateTable)
        {
            // Если задача не успела отработать или узел находится в процессе обновления,
            // то просто выходим из задачи
            if (this.treeItem.InUpdate || (DateTime.UtcNow - this.treeItem.LastUpdateTime).TotalSeconds < this.settings.RefreshInterval)
            {
                return;
            }

            if (!this.treeItem.Workplace.IsActive || !this.treeItem.IsVisibleInPath())
            {
                this.refreshPending = true;
                this.StopTimer();
                return;
            }

            await this.treeItem.RefreshNodeAsync(onCompletedAsync: async t =>
            {
                if (this.treeItem.HasSelection())
                {
                    await this.RefreshTableContentAsync(skipUpdateTable);
                }
            });
        }
    }
}