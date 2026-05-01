using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Azure;
using LinqToDB;
using LinqToDB.Tools;
using Tessa.Cards;
using Tessa.Extensions.Client.Helpers;
//using Tessa.Extensions.Client.Helpers.AsyncHelper;
using Tessa.Extensions.Client.Helpers.Dialogs;
using Tessa.Extensions.Default.Client.EDS;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Extensions.Shared;
//using Tessa.Extensions.Shared.Extensions;
using Tessa.Extensions.Shared.Info;
using Tessa.Files;
using Tessa.Platform.Collections;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using Tessa.UI;
using Tessa.UI.Cards;
using Tessa.UI.Cards.Controls;
using Tessa.UI.Controls.FilePreview;
using Tessa.UI.Controls.Zooming;
using Tessa.UI.Views.Charting.Charts;
using Tessa.UI.Windows;
using StampType = Tessa.Extensions.Shared.Helpers.Medo.StampType;

namespace Tessa.Extensions.Client.UI
{
    public sealed class MedoLogbookCardUIExtension : CardUIExtension
    {
        private readonly ICardRepository cardRepository;

        private readonly IDialogHelper dialogHelper;

        private readonly IUIHost uIHost;
        private readonly CreateDialogFormFuncAsync createDialogFormFuncAsync;
        private readonly ICardFileManager cardFileManager;
        private IUIContext uiContext;
        private ICardModel mainCardModel;
        private readonly IFileCategory
        //previewCategory = new FileCategory(new Guid(0xAD65B77D, 0x6933, 0x4A03, 0xAA, 0x76, 0x9C, 0x07, 0x59, 0xF3, 0x36, 0x22), "Предпросмотр"); // AD65B77D-6933-4A03-AA76-9C0759F33622
        previewCategory = new FileCategory(new Guid("3133e616-81d8-4a82-937f-4a8636bfa1de"), "Предпросмотр"); // AD65B77D-6933-4A03-AA76-9C0759F33622
        private readonly IFileCategory hiddenFilesCategory = new FileCategory(Guid.Parse("1d7170fc-9f28-4060-bdda-e60eb5c3b37d"), "Скрытые файлы");

        public MedoLogbookCardUIExtension(ICardRepository cardRepository,
            IDialogHelper dialogHelper,
            IUIHost uIHost,
            CreateDialogFormFuncAsync createDialogFormFuncAsync,
            ICardFileManager cardFileManager)
        {
            this.cardRepository = cardRepository;
            this.dialogHelper = dialogHelper;
            this.uIHost = uIHost;
            this.createDialogFormFuncAsync = createDialogFormFuncAsync;
            this.cardFileManager = cardFileManager;
        }

        #region Base Overrides

        public override async Task Initialized(ICardUIExtensionContext context)
        {
            //System.Diagnostics.Debugger.Launch();

            this.uiContext = context.UIContext;
            mainCardModel = context.Model;
            var card = mainCardModel.Card;

            bool isMedoCard = false;

            if (card.Sections.ContainsKey("DocumentCommonInfo"))
            {
                isMedoCard = card.Sections["DocumentCommonInfo"].RawFields.ContainsKey("GriffMedoIndex");
            }

            //.TryGetFieldIgnoreCaseAsync<string?>("GriffMedoIndex");            

            if (isMedoCard)// card.TypeID == TypeInfo.RB_OutgoingTypeID) //if (card.TypeID == TypeInfo.RB_OutgoingTypeID)
            {
                var stateID = mainCardModel.Card.Sections["KrApprovalCommonInfoVirtual"].RawFields.Get<int?>("StateID");

                if (mainCardModel.Forms.TryGet("FormLogbookMedo", out var tab)
                    && false
                    )
                {
                    mainCardModel.Forms.Remove(tab);
                }
                else
                {
                    var stampTable = mainCardModel.Controls.TryGet<GridViewModel>("StampInfoTable");
                    stampTable.RightButtons.Add(new UIButton("Предпросмотр", async _ => await this.StampPreviewAsync(mainCardModel, uiContext)));

                    var pageNumberControl = mainCardModel.Controls.TryGet<IntegerBoxViewModel>("PageNumberForSignStamp");
                    if (pageNumberControl != null)
                    {
                        pageNumberControl.PropertyChanged += (obj, e) =>
                        {
                            if (e.PropertyName == "Text" 
                                && obj is IntegerBoxViewModel vm 
                                && int.TryParse(vm.Text, out int pageNumber))
                            {
                                var signStamps = card.Sections["MedoStampInfo"].Rows
                                    ;
                                foreach (var stamp in signStamps)
                                {
                                    stamp.Fields["Page"] = pageNumber;
                                }
                            }
                        };
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        private async Task StampPreviewAsync(ICardModel cardModel, IUIContext uiContext)
        {
            System.Diagnostics.Debugger.Launch();

            var result = await cardModel.SaveAsync();
            if (TessaDialog.ShowNotEmpty(result.Result))
            {
                return;
            }

            mainCardModel = uiContext.CardEditor.CardModel;

            var card = mainCardModel.Card;
            var file = card.Files.First(x => x.CategoryID == FileCategories.MainDoc.ID && x.Name.ToLower().EndsWith(".pdf"));
            IFile updatedFile;

            using (TessaSplash.Create("Обновление данных __"))
            {
                updatedFile = await UpdateFile(cardModel, uiContext, true, true);
                // Обновляем CardModel
                mainCardModel = uiContext.CardEditor.CardModel;
            }

            var accept = false;

            #region NewDialog
            CancellationTokenSource cancellationTokenSource = new();
            var ct = cancellationTokenSource.Token;

            var (dialogForm, dialogCardModel) = await this.createDialogFormFuncAsync(
                "DialogMedoTest",
                "Tab",
                modifyResponseAsync: (response, ct) =>
                {
                    return new ValueTask();
                },
                modifyModelAsync: (model, ct) =>
                {
                    return new ValueTask();
                },
                cancellationToken: ct
                );

            if (dialogForm is null)
            {
                TessaDialog.ShowError("Тип диалога \"Название типа диалога\" не найден.");
                return;
            }

            await ModifyDialogModel(mainCardModel, dialogCardModel, updatedFile, ct);

            #region Table handlers
            var stampBlock = dialogForm
                .Blocks
                .FirstOrDefault(x => x.Name.Equals("StampInfo", StringComparison.Ordinal));
            var stampTable = stampBlock.Controls.FirstOrDefault(x => x.Name.Equals("StampTable", StringComparison.Ordinal)) as GridViewModel;
            stampTable.RowEditorClosed += StampTable_RowEditorClosed;
            #endregion

            #region Buttons handlers

            ButtonViewModel acceptButton = await GetButton(dialogForm, "Accept");
            ButtonViewModel cancelButton = await GetButton(dialogForm, "Cancel");

            acceptButton.CommandClosure.Execute = async e =>
            {
                await FillMainTable(dialogCardModel);
                await UpdateFile(dialogCardModel, uiContext, false, false);
                await cardModel.SaveAsync();
                await dialogForm.CloseAsync();
                await uiContext.CardEditor.RefreshCardAsync(uiContext);
            };

            cancelButton.CommandClosure.Execute = async e =>
            {
                mainCardModel = uiContext.CardEditor.CardModel;
                await UpdateFile(mainCardModel, uiContext, true, false);
                await mainCardModel.SaveAsync();
                await dialogForm.CloseAsync();
            };
            #endregion

            await this.uIHost.ShowDialogAsync(
                "Превью диалог",
                dialogForm,
                initializeWindowActionAsync: (window, ct) =>
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.WindowState = WindowState.Maximized;
                    return new ValueTask();
                },
                modalDialog: true,
                closeOnEscapeKey: false,
                cancellationToken: ct
                );
            #endregion

            if (accept)
            {
                card.Info[".ignorePermission"] = true;
                result = await mainCardModel.SaveAsync();
                TessaDialog.ShowNotEmpty(result.Result);
                var fileToDelete = mainCardModel.FileContainer.Files.First(f => f.Category?.ID == this.previewCategory.ID);
                await mainCardModel.FileContainer.Files.RemoveWithNotificationAsync(fileToDelete);
                return;
            }
            await uiContext.CardEditor.RefreshCardAsync(uiContext);
        }

        private async Task FillMainTable(ICardModel dialogCardModel)
        {
            var dialogTable = dialogCardModel.Card.Sections.Get<CardSection>("StampTable");
            var mainCardTable = mainCardModel.Card.Sections.Get<CardSection>("MedoStampInfo");
            dialogTable.Rows.ForEach(r =>
            {
                CardRow row = new CardRow();
                row.RowID = r.RowID;
                row.State = r.State;
                row.Fields["Page"] = r.Fields["Page"];
                row.Fields["X"] = r.Fields["X"];
                row.Fields["Y"] = r.Fields["Y"];
                row.Fields["Width"] = r.Fields["Width"];
                row.Fields["Height"] = r.Fields["Height"];
                row.Fields["StampTypeID"] = r.Fields["StampTypeID"];
                row.Fields["StampTypeName"] = r.Fields["StampTypeName"];
                mainCardTable.Rows.Add(row);
            });
            mainCardModel.Card.Sections[mainCardTable.Name].Set(mainCardTable);
        }

        private async void StampTable_RowEditorClosed(object sender, GridRowEventArgs e)
        {
            var updatedFile = await UpdateFile(e.CardModel.EntryModel, uiContext, false, true);
            await SetFilePreview(mainCardModel, e.CardModel, updatedFile, e.CancellationToken);
        }

        private async Task<ButtonViewModel> GetButton(IFormViewModel dialogForm, string buttonName)
        {
            var stampBlock = dialogForm
                .Blocks
                .FirstOrDefault(x => x.Name.Equals("StampInfo", StringComparison.Ordinal));

            var buttonsContainer = stampBlock
                .Controls
                .FirstOrDefault(x => x.Caption.Equals("MedoStampButtons", StringComparison.Ordinal)) as ContainerViewModel;

            var buttonsBlock = buttonsContainer
                .Form
                .Blocks
                .FirstOrDefault(x => x.Name.Equals("MedoStampButtons", StringComparison.Ordinal));

            var acceptButton = buttonsBlock.Controls.FirstOrDefault(x => x.Name.Equals(buttonName, StringComparison.Ordinal)) as ButtonViewModel;
            return acceptButton;
        }

        private async ValueTask ModifyDialogModel(
            ICardModel cardModel
            ,ICardModel dialogCardModel
            ,IFile updatedFile
            ,CancellationToken cancellationToken = default)
        {
            await SetFilePreview(cardModel, dialogCardModel, updatedFile, cancellationToken);
            await FillVirtualTable(cardModel, dialogCardModel);
        }

        private async Task FillVirtualTable(ICardModel cardModel, ICardModel dialogCardModel)
        {
            var mainCard = cardModel.Card;
            var stampSettingsTableMain = mainCard.Sections.Get<CardSection>("MedoStampInfo");
            var dialogStampSettingsTable = dialogCardModel.Card.Sections.Get<CardSection>("StampTable");
            stampSettingsTableMain.Rows.ForEach(x =>
            {
                CardRow row = new CardRow();
                row.RowID = x.RowID;
                row.Fields["Page"] = x.Fields["Page"];
                row.Fields["X"] = x.Fields["X"];
                row.Fields["Y"] = x.Fields["Y"];
                row.Fields["Width"] = x.Fields["Width"];
                row.Fields["Height"] = x.Fields["Height"];
                row.Fields["StampTypeID"] = x.Fields["StampTypeID"];
                row.Fields["StampTypeName"] = x.Fields["StampTypeName"];
                dialogStampSettingsTable.Rows.Add(row);
            });

        }

        /// <summary>
        /// Получаем данные координат из таблицы. </br>
        /// Если надо получить данные из основной таблицы - передаём cardModel основной карточки
        /// Иначе - cardModel диалога.
        /// </summary>
        /// <param name="cardModel">Модель карточки</param>
        /// <param name="fromMainCard">Признак - получаем данные из основной карточки или диалога</param>
        /// <returns></returns>
        private async Task<Dictionary<string, object>> GetStampData(ICardModel cardModel, bool fromMainCard)
        {
            CardSection stampTable;
            if (fromMainCard)
            {
                var mainCard = cardModel.Card;
                stampTable = mainCard.Sections.Get<CardSection>("MedoStampInfo");
            }
            else
            {
                stampTable = cardModel.Card.Sections.Get<CardSection>("StampTable");
            }
            Dictionary<string, object> stampDataRows = new();
            int index = 0;
            stampTable.Rows.ForEach(x =>
            {                
                Dictionary<string, object> row = new()
                {
                    ["Page"] = x.Fields["Page"],
                    ["X"] = x.Fields["X"],
                    ["Y"] = x.Fields["Y"],
                    ["Width"] = x.Fields["Width"],
                    ["Height"] = x.Fields["Height"],
                    ["StampTypeID"] = x.Fields["StampTypeID"]
                    //["FileSignaturesRowID"] = x.Fields["FileSignaturesRowID"] != null ? x.Fields["FileSignaturesRowID"] : null
                };
                stampDataRows.Add(index.ToString(), row);
                index++;
            });
            return stampDataRows;
        }

        private async ValueTask SetFilePreview(ICardModel cardModel, ICardModel dialogCardModel, IFile updatedFile, CancellationToken cancellationToken)
        {
            // Устанавливаем связь файла из основной карточки и диалогом

            var filePreviewBlock = dialogCardModel.Blocks["PreviewFileBlock"];
            var filePreview = filePreviewBlock?.Controls.FirstOrDefault(c => c.Caption == "FilePreview") as FilePreviewViewModel;

            if (filePreview == null)
            {
                return;
            }

            //foreach (var f in cardModel.FileContainer.Files)
            //{
            //    Console.WriteLine(f.Name);
            //    //Console.WriteLine(f.Category.ID);
            //}

            var temp = cardModel.FileContainer.Files;

            var iFile = cardModel.FileContainer.Files
                .Where(f => f.Category!= null && f.Category.ID == this.previewCategory.ID && f.ID == updatedFile.ID)
                .FirstOrDefault();
                //.FirstOrDefault(f => f.Category.ID == this.previewCategory.ID && f.ID == updatedFile.ID);

            var loadResult = await iFile.EnsureContentDownloadedInUIAsync(cancellationToken: cancellationToken);
            TessaDialog.ShowNotEmpty(loadResult);
            if (loadResult.IsSuccessful)
            {
                if (iFile.Content.IsDisposed || !iFile.Content.IsLocal)
                {
                    await filePreview.FileControlManager.SetPreviewTextAsync("Произошла ошибка при загрузке файла", cancellationToken: cancellationToken);
                    return;
                }
                var filePath = iFile.Content.GetLocalFilePath();
                await DispatcherHelper.InvokeInUIAsync(async () =>
                {
                    await filePreview.FileControlManager.ShowPreviewAsync(null, filePath, cancellationToken: cancellationToken);
                });

                filePreview.FilePreview.PagingControlPropertyChanged += async (p, e) =>
                {
                    var pagingControl = p as PagingPreviewViewModel;
                    if (pagingControl != null)
                    {
                        pagingControl.ImageScalingType = PreviewScalingType.WidthAndHeight;
                    }
                };
            }
            return;
        }

        private async Task<IFile> UpdateFile(ICardModel cardModel
            ,IUIContext uiContext
            ,bool fromMainCard
            ,bool withGrid)
        {
            var result = await mainCardModel.SaveAsync();
            if (TessaDialog.ShowNotEmpty(result.Result))
            {
                return null;
            }

            mainCardModel = uiContext.CardEditor.CardModel;

            var card = mainCardModel.Card;
            var file = card.Files.First(x => x.CategoryID == FileCategories.MainDoc.ID && x.Name.ToLower().EndsWith(".pdf"));

            using (TessaSplash.Create("Обновление данных"))
            {
                var stampData = await GetStampData(cardModel, fromMainCard);

                CardResponse response = null;

                for (int i = 0; i < 15; i++)
                {
                    response = await this.cardRepository.RequestAsync(
                    new CardRequest
                    {
                        RequestType = RequestTypes.ShowStampPreviewTypeID,
                        Info = new Dictionary<string, object>
                        {
                            {"cardID", card.ID},
                            {"fileID", file.Card.ID},
                            {"fileName", file.Name},
                            {"fileVersion", file.VersionRowID},
                            {"stampInfo", stampData},
                            {"withGrid", withGrid }
                        },
                    });

                    if (response.Info.ContainsKey("Content"))
                    {
                        break;
                    }
                }

                if (response == null || TessaDialog.ShowNotEmpty(response.ValidationResult.Build()))
                {
                    return null;
                }

                var content = response.Info.Get<byte[]>("Content");

                if (content == null || content.Length == 0)
                {
                    TessaDialog.ShowError("$UI_Cards_FilePreview_Fail");
                    return null;
                }

                foreach (var cardFile in card.Files)
                {
                    if (cardFile.CategoryID == this.previewCategory.ID && cardFile.Name == "(Штамп)" + file.Name)
                    {
                        var tempFile =  mainCardModel.FileContainer.Files
                            .Where(c => c.ID == cardFile.Card.ID).FirstOrDefault();

                        cardFile.CategoryCaption = this.hiddenFilesCategory.Caption;
                        cardFile.CategoryID = this.hiddenFilesCategory.ID;

                        await tempFile.SetCategoryAsync(this.hiddenFilesCategory);

                        StringBuilder sb = new StringBuilder();

                        foreach(var tag in tempFile.NewVersionTags)
                        {
                            sb.AppendLine($"{tag.Key} {tag.Info}");
                        }

                        TessaDialog.ShowMessage(sb.ToString());
                        //cardFile.CategoryCaption = "Скрытые файлы";
                        //cardFile.CategoryID = Guid.Parse("1d7170fc-9f28-4060-bdda-e60eb5c3b37d");

                        cardFile.State = CardFileState.Modified;
                        //cardFile.State = CardFileState.Deleted;
                    }
                }

                IFile newFile = null;
                Tessa.Platform.Validation.ValidationResult buildFileResult;
                if (withGrid)
                {
                    (newFile, buildFileResult) = await mainCardModel.FileContainer
                                            .BuildFile("(Штамп)" + file.Name)
                                          //.BuildFile("Предпросмотр расположения штампов.pdf")
                                          .SetCategory(this.previewCategory)
                                          .SetContent(content, true)
                                          .AddWithNotificationAsync();
                }
                else
                {
                    (newFile, buildFileResult) = await mainCardModel.FileContainer
                                          .BuildFile("(Штамп)" + file.Name)
                                          .SetCategory(this.previewCategory)
                                          .SetContent(content, true)
                                          .AddWithNotificationAsync();
                }

                if (TessaDialog.ShowNotEmpty(buildFileResult))
                {
                    return null;
                }
                return newFile;
            }
        }
        #endregion
    }
}
