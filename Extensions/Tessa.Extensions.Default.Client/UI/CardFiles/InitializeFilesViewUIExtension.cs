using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Extensions.Default.Shared.Cards;
using Tessa.Extensions.Default.Shared.Views;
using Tessa.Platform;
using Tessa.Platform.Runtime;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using Tessa.UI;
using Tessa.UI.Cards;
using Tessa.UI.Cards.Controls;
using Tessa.UI.Files;
using Tessa.UI.Views.Content;
using Tessa.Views;

namespace Tessa.Extensions.Default.Client.UI.CardFiles
{
    /// <summary>
    /// Реализация расширения типа карточки для преобразования представления в файловый контрол
    /// </summary>
    public sealed class InitializeFilesViewUIExtension : FilesViewGeneratorBaseUIExtension
    {
        #region Private fields

        private readonly ICardMetadata cardMetadata;

        private readonly FilesViewCardControlInitializationStrategy initializationStrategy;

        #endregion

        #region Constructor

        public InitializeFilesViewUIExtension(
            ISession session,
            IExtensionContainer extensionContainer,
            IViewService viewService,
            ICardMetadata cardMetadata,
            IProcessNameResolver processNameResolver,
            FilesViewCardControlInitializationStrategy initializationStrategy)
            : base(session, extensionContainer, viewService, processNameResolver)
        {
            this.initializationStrategy = initializationStrategy ?? throw new ArgumentNullException(nameof(initializationStrategy));
            this.cardMetadata = cardMetadata;
        }

        #endregion

        #region Private methods

        private async Task ExecuteInitializingActionAsync(ITypeExtensionContext context)
        {
            var settings = context.Settings;
            var filesViewAlias = settings.TryGet<string>(DefaultCardTypeExtensionSettings.FilesViewAlias);

            var options = new FileControlCreationParams
            {
                CategoriesViewAlias = settings.TryGet<string>(DefaultCardTypeExtensionSettings.CategoriesViewAlias),
                PreviewControlName = settings.TryGet<string>(DefaultCardTypeExtensionSettings.PreviewControlName),
                IsCategoriesEnabled = settings.TryGet<bool>(DefaultCardTypeExtensionSettings.IsCategoriesEnabled),
                IsIgnoreExistingCategories = settings.TryGet<bool>(DefaultCardTypeExtensionSettings.IsIgnoreExistingCategories),
                IsManualCategoriesCreationDisabled = settings.TryGet<bool>(DefaultCardTypeExtensionSettings.IsManualCategoriesCreationDisabled),
                IsNullCategoryCreationDisabled = settings.TryGet<bool>(DefaultCardTypeExtensionSettings.IsNullCategoryCreationDisabled),
                CategoriesViewMapping = settings.TryGet<IList>(DefaultCardTypeExtensionSettings.CategoriesViewMapping)
            };
            var uiContext = (ICardUIExtensionContext) context.ExternalContext;

            if (context.CardTask is null)
            {
                this.AddCardModelInitializers(uiContext.Model, filesViewAlias, options);
            }
            else
            {
                uiContext.Model.TaskInitializers.Add(async (taskCardModel, ct) =>
                {
                    if (taskCardModel.CardTask == context.CardTask)
                    {
                        this.AddCardModelInitializers(taskCardModel, filesViewAlias, options);
                    }
                });
            }
        }

        private async Task ExecuteInitializedActionAsync(ITypeExtensionContext context)
        {
            var uiContext = (ICardUIExtensionContext) context.ExternalContext;

            if (context.CardTask is null)
            {
                var isAttached = await AttachViewToFileControlAsync(
                    uiContext.Model,
                    context.Settings,
                    CreateRowFunc,
                    this.initializationStrategy,
                    cancellationToken: context.CancellationToken);
                if (!isAttached)
                {
                    var tables = uiContext.Model.ControlBag.OfType<GridViewModel>();
                    foreach (var table in tables)
                    {
                        table.RowInvoked += (s, e) =>
                        {
                            if (e.Action == GridRowAction.Inserted || e.Action == GridRowAction.Opening)
                            {
                                this.InitializeExtensionForTableForm(context, e);
                            }
                        };
                    }
                }
            }
            else
            {
                await uiContext.Model.ModifyTasksAsync(async (task, model) =>
                {
                    if (task.TaskModel.CardTask == context.CardTask)
                    {
                        await task.ModifyWorkspaceAsync(async (t, subscribeToTaskModel) =>
                            {
                                var isAttached = await AttachViewToFileControlAsync(
                                    task.TaskModel,
                                    context.Settings,
                                    CreateRowFunc,
                                    this.initializationStrategy);
                                if (!isAttached)
                                {
                                    var tables = task.TaskModel.ControlBag.OfType<GridViewModel>();
                                    foreach (var table in tables)
                                    {
                                        table.RowInvoked += (s, e) =>
                                        {
                                            if (e.Action == GridRowAction.Inserted || e.Action == GridRowAction.Opening)
                                            {
                                                this.InitializeExtensionForTableForm(context, e);
                                            }
                                        };
                                    }
                                }
                            });
                    }
                });
            }
        }

        private void InitializeExtensionForTableForm(ITypeExtensionContext context, GridRowEventArgs e)
        {
            var uiContext = (ICardUIExtensionContext) context.ExternalContext;
            var cardViewControlViewModels = e.RowModel.ControlBag.OfType<CardViewControlViewModel>();
            if (cardViewControlViewModels.Any())
            {
                DispatcherHelper.InvokeInUI(async () =>
                {
                    var isAttached = await AttachViewToFileControlAsync(
                        e.RowModel,
                        context.Settings,
                        CreateRowFunc,
                        this.initializationStrategy,
                        cancellationToken: CancellationToken.None);
                    if (!isAttached)
                    {
                        var tables = e.RowModel.ControlBag.OfType<GridViewModel>();
                        foreach (var table in tables)
                        {
                            table.RowInvoked += (s, e) =>
                            {
                                this.InitializeExtensionForTableForm(context, e);
                            };
                        }
                    }
                });
            }
        }

        private static ViewControlRowViewModel CreateRowFunc(TableRowCreationOptions options)
        {
            var fileViewModel = (IFileViewModel) options.Data[ColumnsConst.FileViewModel];
            options.Data.Remove(ColumnsConst.FileViewModel);

            return new TableFileRowViewModel(fileViewModel, options)
            {
                AutomationId = $"{fileViewModel.Model.ID}",
                AutomationName = fileViewModel.Model.Name,
            };
        }

        #endregion

        #region Base overrides

        public override async Task Initializing(ICardUIExtensionContext context)
        {
            ValidationResult result = await CardHelper
                .ExecuteTypeExtensionsAsync(
                    DefaultCardTypeExtensionTypes.InitializeFilesView,
                    context.Card,
                    this.cardMetadata,
                    ExecuteInitializingActionAsync,
                    context,
                    cancellationToken: context.CancellationToken);

            context.ValidationResult.Add(result);
        }

        public override async Task Initialized(ICardUIExtensionContext context)
        {
            // Вот здесь проходит основная инициализация. Она не запускается при открытии мелкой карточки.
            ValidationResult result = await CardHelper
                .ExecuteTypeExtensionsAsync(
                    DefaultCardTypeExtensionTypes.InitializeFilesView,
                    context.Card,
                    this.cardMetadata,
                    ExecuteInitializedActionAsync,
                    context);

            context.ValidationResult.Add(result);
        }

        #endregion
    }
}
