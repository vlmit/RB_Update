using System.Threading.Tasks;
using Tessa.Properties.Resharper;
using Tessa.UI.Cards.Controls;
using Tessa.UI.Menu;
using Tessa.Views;

namespace Tessa.Extensions.Default.Client.UI.CardFiles
{
    /// <summary>
    /// Стратегия инициализации модели представления
    /// </summary>
    public class FilesViewCardControlInitializationStrategy : ViewCardControlInitializationStrategy
    {

        #region Constructor

        public FilesViewCardControlInitializationStrategy([NotNull] IViewService viewService,
            [NotNull] CreateMenuContextFunc createMenuContextFunc,
            [NotNull] IViewCardControlContentItemsFactory contentItemsFactory)
            : base(viewService, createMenuContextFunc, contentItemsFactory)
        { }

        #endregion

        #region Base overrides
        
        public override ValueTask InitializeMetadataAsync(CardViewControlInitializationContext context)
        {
            context.ControlViewModel.ViewMetadata = FilesViewMetadata.Create();

            return new ValueTask();
        }
        public override ValueTask InitializeDataProviderAsync(CardViewControlInitializationContext context)
        {
            context.ControlViewModel.DataProvider =
                new CardFilesDataProvider(
                    context.ControlViewModel.ViewMetadata,
                    FilesViewGeneratorBaseUIExtension.TryGetFileControl(context.Model.Info, context.ControlViewModel.Name));

            return new ValueTask();
        }

        #endregion
    }
}
