using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Tessa.Cards;
//using Tessa.Extensions.Client.Extensions;
using Tessa.Extensions.Shared;
using Tessa.Extensions.Shared.Info;
using Tessa.Localization;
using Tessa.Platform.Runtime;
using Tessa.Platform.Storage;
using Tessa.UI;
using Tessa.UI.Cards;
using Tessa.UI.Cards.Controls.AutoComplete;
using Tessa.UI.Controls.AutoCompleteCtrl;
using Tessa.UI.Files;
using Tessa.UI.Views;
//using User = Tessa.Extensions.Shared.Info.User;

namespace Tessa.Extensions.Client.Helpers.Dialogs
{
    public sealed class DialogHelper : IDialogHelper
    {
        #region Constructor

        public DialogHelper(
            ICardRepository cardRepository,
            IUIHost uiHost,
            ICardMetadata cardMetadata,
            CreateCardModelFuncAsync createCardModelFunc,
            ICardDialogManager dialogManager,
            CreateFileSourceForCardModelFuncAsync createFileSourceForCardModelFunc,
            CreateFileUIContainerFuncAsync createFileContainerFunc,
            ISession session)
        {
            this.cardRepository = cardRepository;
            this.uiHost = uiHost;
            this.cardMetadata = cardMetadata;
            this.createCardModelFunc = createCardModelFunc;
            this.dialogManager = dialogManager;
            this.createFileSourceForCardModelFunc = createFileSourceForCardModelFunc;
            this.createFileContainerFunc = createFileContainerFunc;
            this.session = session;
        }

        #endregion

        #region Private Fields

        private readonly ICardRepository cardRepository;
        private readonly IUIHost uiHost;
        private readonly ICardMetadata cardMetadata;
        private readonly CreateCardModelFuncAsync createCardModelFunc;
        private readonly ICardDialogManager dialogManager;
        private readonly CreateFileSourceForCardModelFuncAsync createFileSourceForCardModelFunc;
        private readonly CreateFileUIContainerFuncAsync createFileContainerFunc;
        private readonly ISession session;

        #endregion

        #region Public Methods

        public async Task ShowDialogAsync(
            string cardTypeName,
            string name,
            string caption = null,
            Func<IFormViewModel, CancellationToken, ValueTask> initFormAction = null,
            Action<Card> initCardAction = null,
            Func<Window, CancellationToken, ValueTask> initWindowAction = null,
            params DialogButton[] dialogButtons)
        {
            var context = UIContext.Current;
            var cardEditor = context.CardEditor;
            var cardID = cardEditor?.CardModel?.Card?.ID ?? Guid.NewGuid();

            CardTypeNamedForm dialogForm;
            if (!(await this.cardMetadata.GetCardTypesAsync()).TryGetValue(cardTypeName, out var dialogType)
                || (dialogForm = dialogType.Forms.FirstOrDefault(x => x.Name == name)) == null)
            {
                TessaDialog.ShowError($"Не удалось получить карточку с типом {cardTypeName} или ее вкладку {name}");
                return;
            }

            var request = new CardNewRequest {CardTypeID = dialogType.ID};
            var response = await this.cardRepository.NewAsync(request);
            if (!response.ValidationResult.IsSuccessful())
            {
                TessaDialog.ShowNotEmpty(response.ValidationResult.Build());
                return;
            }

            var windowCard = response.Card;
            windowCard.ID = cardID;

            initCardAction?.Invoke(windowCard);

            var windowCardModel = await this.createCardModelFunc(
                windowCard,
                response.SectionRows,
                async (x, ct) => await this.dialogManager.ShowRowAsync(x, ct));

            var fileSource = await this.createFileSourceForCardModelFunc(windowCardModel);
            var fileContainer = await this.createFileContainerFunc(fileSource);
            await fileContainer.Permissions.SetCanAddAsync(true);
            windowCardModel.FileContainer = fileContainer;

            var formCaption = string.IsNullOrEmpty(caption) ? dialogForm.TabCaption : caption;

            var uiButtons = new List<UIButton>();

            foreach (var button in dialogButtons)
            {
                var uiButton = new UIButton(button.Caption, async b => button.ButtonFunc(windowCardModel, b), button.IsEnabled);
                uiButton.Visibility = button.Visibility;
                uiButtons.Add(uiButton);
            }

            await this.uiHost.ShowFormDialogAsync(
                LocalizationManager.Localize(formCaption),
                dialogForm,
                windowCardModel,
                initFormAction,
                initWindowAction,
                buttons: uiButtons.ToArray());
        }

       /* public async Task<string> GetCommentaryAsync(string acceptButtonCaption)
        {
            string commentary = null;

            await this.ShowDialogAsync(
                TypeInfo.RostehDialogs,
                TypeInfo.RostehDialogs.FormCommentary,
                caption: null,
                initFormAction: null,
                initCardAction: null,
                initWindowAction: null,
                new DialogButton(acceptButtonCaption, async (x, b) =>
                {
                    var dialogSection = x.Card.Sections[SchemeInfo.RostehDialogs];
                    commentary = dialogSection.Fields.Get<string>(SchemeInfo.RostehDialogs.Commentary);
                    if (string.IsNullOrEmpty(commentary))
                    {
                        TessaDialog.ShowError("Необходимо заполнить комментарий");
                        return;
                    }

                    await b.CloseAsync();
                }),
                DialogButton.CancelButton);

            return commentary;
        }

        public async Task<ResultWithCommentary<List<User>>> GetUserAsync(
            string acceptButtonCaption,
            bool hasCommentary,
            bool commentaryRequired = false,
            string forwardComment = null,
            string userFieldCaption = null,
            string formCaption = null,
            bool isMultiPerformer = false)
        {
            ResultWithCommentary<List<User>> result = null;

            await this.ShowDialogAsync(
                TypeInfo.RostehDialogs,
                isMultiPerformer ? TypeInfo.RostehDialogs.FormNewRoles : TypeInfo.RostehDialogs.FormNewRole,
                formCaption,
                async (x, _) =>
                {
                    if (!string.IsNullOrEmpty(userFieldCaption))
                    {
                        x.Blocks[0].Controls[0].Caption = userFieldCaption;
                    }

                    x.Blocks[1].BlockVisibility = hasCommentary
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    x.Blocks[1].Controls[0].IsRequired = commentaryRequired;
                    x.RearrangeSelf();
                },
                card =>
                {
                    if (!string.IsNullOrEmpty(forwardComment))
                    {
                        card.Sections[SchemeInfo.RostehDialogs].Fields[SchemeInfo.RostehDialogs.Commentary] = forwardComment;
                    }
                },
                initWindowAction: null,
                new DialogButton(acceptButtonCaption, async (cardModel, button) =>
                {
                    var users = isMultiPerformer
                        ? cardModel.Card.Sections[SchemeInfo.RostehDialogsRoles].Rows.Select(r => User.Get(r, "Role")).ToList()
                        : new List<User> {User.Get(cardModel.Card.Sections[SchemeInfo.RostehDialogs], "Role")};
                    var comment = cardModel.Card.Sections[SchemeInfo.RostehDialogs].Fields.Get<string>(SchemeInfo.RostehDialogs.Commentary);

                    if (users.Count == 0)
                    {
                        if (!isMultiPerformer)
                        {
                            TessaDialog.ShowError("Необходимо заполнить исполнителя" + (commentaryRequired ? " и комментарий" : string.Empty));
                        }
                        else
                        {
                            TessaDialog.ShowError("Необходимо указать исполнителей" + (commentaryRequired ? " и комментарий" : string.Empty));
                        }
                    }

                    result = new ResultWithCommentary<List<User>>(users, comment);

                    await button.CloseAsync();
                }),
                DialogButton.CancelButton);

            return result;
        }

        public async Task<ResultsWithCommentary<User>> GetDistributionUsersAsync(
            string acceptButtonCaption,
            bool hasCommentary,
            bool commentaryRequired = false,
            string forwardComment = null,
            string userFieldCaption = null,
            string formCaption = null)
        {
            ResultsWithCommentary<User> results = null;

            await this.ShowDialogAsync(
                TypeInfo.RostehDialogs,
                TypeInfo.RostehDialogs.FormNewDistributionRoles,
                formCaption,
                async (form, cToken) =>
                {
                    var controls = form.Blocks[0].Controls;
                    var performerControl = controls.First(i => i.Name == TypeInfo.RostehDialogs.PerformersControl) as AutoCompleteTableViewModel;
                    var commentaryControl = controls.First(i => i.Name == TypeInfo.RostehDialogs.CommentaryControl);

                    if (!string.IsNullOrEmpty(userFieldCaption))
                    {
                        performerControl.Caption = userFieldCaption;
                    }

                    performerControl.SetChangeFieldAsyncHandler(async _ =>
                    {
                        var value = await this.uiHost.ShowViewsDialogAsync(ViewInfo.RefSections.DistributionLists, selectAction: async viewContext =>
                        {
                            using (TessaSplash.Create("Добавление пользователей..."))
                            {
                                foreach (var selectedRow in viewContext.Selected)
                                {
                                    await this.ConvertSelectedRow(selectedRow, performerControl);
                                }
                            }
                        }, cancellationToken: cToken);

                        await this.ConvertSelectedRow(value, performerControl);
                    });

                    commentaryControl.ControlVisibility = hasCommentary
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    commentaryControl.IsRequired = commentaryRequired;

                    form.RearrangeSelf();
                },
                card =>
                {
                    if (!string.IsNullOrEmpty(forwardComment))
                    {
                        card.Sections[SchemeInfo.RostehDialogs].Fields[SchemeInfo.RostehDialogs.Commentary] = forwardComment;
                    }

                    card.Sections[SchemeInfo.RostehDialogs].Fields[SchemeInfo.RostehDialogs.RoleID] = this.session.User.ID;
                    card.Sections[SchemeInfo.RostehDialogs].Fields[SchemeInfo.RostehDialogs.RoleName] = this.session.User.Name;
                },
                null,
                new DialogButton(acceptButtonCaption, async (cardModel, button) =>
                {
                    var rows = cardModel.Card.Sections[SchemeInfo.RostehDialogsRoles].Rows;
                    var section = cardModel.Card.Sections[SchemeInfo.RostehDialogs];
                    var comment = section.Fields.Get<string>(SchemeInfo.RostehDialogs.Commentary);
                    var users = rows.Select(row => User.Get(row, "Role")).ToList();
                    var fromUser = new User(cardModel.Card.Sections[SchemeInfo.RostehDialogs].Fields.Get<Guid>(SchemeInfo.RostehDialogs.RoleID),
                        cardModel.Card.Sections[SchemeInfo.RostehDialogs].Fields.Get<string>(SchemeInfo.RostehDialogs.RoleName));

                    if (users.Count == 0 || commentaryRequired && string.IsNullOrEmpty(comment))
                    {
                        TessaDialog.ShowError("Необходимо заполнить исполнителей" + (commentaryRequired ? " и комментарий" : string.Empty));
                        return;
                    }

                    results = new ResultsWithCommentary<User>(users, comment, fromUser);
                    await button.CloseAsync();
                }),
                DialogButton.CancelButton);

            return results;
        }
       */
        #endregion

        #region Private Methods

      /*  private async Task ConvertSelectedRow(SelectedValue value, AutoCompleteTableViewModel distributionListControl)
        {
            if (value?.SelectedRow == null)
            {
                return;
            }

            if (value.SelectedRow.TryGet<int?>(ViewInfo.DistributionLists.ColumnListAddedFromView) != 1)
            {
                if (distributionListControl.ItemsSource is IAutoCompleteControlDataSource source)
                {
                    source.InsertSelectedValue(value);
                }

                return;
            }

            var listID = value.SelectedRow.Get<Guid>(ViewInfo.DistributionLists.ColumnListID);

            var request = new CardRequest
            {
                RequestType = RequestTypes.DistributionListTypeID,
                Info = new Dictionary<string, object> {{"CardID", listID}}
            };
            var response = await this.cardRepository.RequestAsync(request);

            var recievers = response.Info.Get<List<object>>("Recievers").Cast<IDictionary<string, object>>();

            foreach (var reciever in recievers)
            {
                var selectedValue = new SelectedValue((string) reciever["UserName"], reciever["UserID"],
                    value.Metadata,
                    new Dictionary<string, object>
                    {
                        {"ListID", reciever["UserID"]},
                        {"ListName", reciever["UserName"]},
                        {"ListAddedFromView", 0}
                    }, value.ViewMetadata);

                if (distributionListControl.ItemsSource is IAutoCompleteControlDataSource source)
                {
                    source.InsertSelectedValue(selectedValue);
                }
            }
        }*/

        #endregion
    }
}