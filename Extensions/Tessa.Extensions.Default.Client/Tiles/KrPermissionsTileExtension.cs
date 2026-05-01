using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Tessa.Extensions.Default.Shared;
using Tessa.Extensions.Default.Shared.Workflow.KrPermissions;
using Tessa.UI;
using Tessa.UI.Cards;
using Tessa.UI.Tiles;
using Tessa.UI.Tiles.Extensions;

namespace Tessa.Extensions.Default.Client.Tiles
{
    public sealed class KrPermissionsTileExtension : TileExtension
    {
        #region Fields

        private readonly ICommand tileCommand = new DelegateCommand(TileAction);

        #endregion
        #region Base Overrides

        public override Task InitializingGlobal(ITileGlobalExtensionContext context)
        {
            var panel = context.Workspace.LeftPanel;

            panel.Tiles.Add(
                new Tile(
                    DefaultTileNames.KrPermissionsDropCache,
                    TileHelper.SplitCaption("$KrTiles_DropPermissionsCache"),
                    context.Icons.Get("Thin57"),
                    panel,
                    tileCommand,
                    order: 10,
                    evaluating: KrPermissionsTileEvaluating));

            return Task.CompletedTask;
        }

        #endregion

        #region Private Methods

        private void KrPermissionsTileEvaluating(object sender, TileEvaluationEventArgs e)
        {
            ICardEditorModel editor = e.CurrentTile.Context.CardEditor;
            ICardModel model;
            
            e.SetIsEnabledWithCollapsing(
                e.CurrentTile,
                editor != null
                && (model = editor.CardModel) != null
                && model.CardType.ID == DefaultCardTypes.KrPermissionsTypeID);
        }

        private static async void TileAction(object obj)
        {
            IUIContext context = UIContext.Current;
            ICardEditorModel editor = UIContext.Current.CardEditor;

            if (editor == null)
            {
                return;
            }

            await editor.SaveCardAsync(
                context,
                new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    [KrPermissionsHelper.DropPermissionsCacheMark] = true,
                });
        }

        #endregion
    }
}
