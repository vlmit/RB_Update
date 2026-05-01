using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Extensions.Default.Shared;
using Tessa.Extensions.Shared;
using Tessa.FileConverters;
using Tessa.Files;
using Tessa.Platform.Collections;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Server.Chronos
{
    public sealed class FileConvertAdditionAction : FileConverterExtension
    {
        #region Fields

        private readonly ICardRepository cardRepository;
        private readonly ICardFileManager fileManager;
        private readonly ICardServerPermissionsProvider permissionsProvider;
        private readonly IDbScope dbScope;

        #endregion

        #region Constructor

        public FileConvertAdditionAction(
            ICardRepository cardRepository,
            ICardFileManager fileManager,
            ICardServerPermissionsProvider permissionsProvider,
            IDbScope dbScope)
        {
            this.cardRepository = cardRepository;
            this.fileManager = fileManager;
            this.permissionsProvider = permissionsProvider;
            this.dbScope = dbScope;
        }

        #endregion

        #region Base Override

        public override async Task AfterRequest(IFileConverterContext context)
        {
            var info = context.Request.Info;

            if (!info.ContainsKey("ConvertBeforeSign"))
            {
                return;
            }

            var cardID = info.Get<Guid>("ConvertBeforeSign");

            try
            {
                var fileAdded = await this.AddFinalVersionFileAsync(context, cardID);
                if (fileAdded)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                context.ValidationResult.AddException(this, ex);
            }

            // Файл не получилось добавить, пробуем хотя бы завершить задание, чтобы отправить процесс дальше

            var result = await this.GetCardAndCloseTask(cardID);

            if (!result.IsSuccessful())
            {
                context.ValidationResult.Add(result);
            }
        }

        #endregion

        private async Task<bool> AddFinalVersionFileAsync(IFileConverterContext context, Guid cardID)
        {
            if (!context.RequestIsSuccessful)
            {
                return false;
            }

            var request = new CardGetRequest { CardID = cardID };
            this.permissionsProvider.SetFullPermissions(request);
            var response = await this.cardRepository.GetAsync(request);
            var responseResult = response.ValidationResult.Build();
            context.ValidationResult.Add(responseResult);
            if (!responseResult.IsSuccessful)
            {
                return false;
            }

            var card = response.Card;

            await using (var container = await this.fileManager.CreateContainerRemoteAsync(card))
            {
                var fileID = context.Request.FileID;
                IFile additionalFile = null;
                var file = container.FileContainer.Files[fileID];
                if (file.Options.TryGetValue("MainFile", out var mainFileId))
                {
                    additionalFile = container.FileContainer.Files.First(f => f.ID == Guid.Parse((string) mainFileId));
                }

                var versionsResult = additionalFile != null
                    ? await additionalFile.EnsureVersionsLoadedAsync()
                    : await file.EnsureVersionsLoadedAsync();

                context.ValidationResult.Add(versionsResult);
                if (!versionsResult.IsSuccessful)
                {
                    return false;
                }

                using (var pdfContent = System.IO.File.OpenRead(context.OutputFilePath))
                {
                    var fileName = Path.GetFileNameWithoutExtension((additionalFile??file).Name);
                    //var replaceResult = await file.ReplaceAsync(pdfContent);
                    //if (!replaceResult.IsSuccessful)
                    //{
                    //    return false;
                    //}
                    var (addedFile, buildFileResult) = await container.FileContainer
                        .BuildFile(fileName.Replace("_temp_", "") + ".pdf")
                        //set isLocal if bytes
                        .SetContent(pdfContent)
                        .AddWithNotificationAsync();

                    //if (additionalFile != null)
                    //{
                        await container.FileContainer.Files
                            .RemoveWithNotificationAsync(file);
                    //}

                    context.ValidationResult.Add(buildFileResult);

                    if (!buildFileResult.IsSuccessful)
                    {
                        return false;
                    }

                    addedFile.Options.Add("FinalVersion", true);
                    addedFile.Versions.Last.Options.Add("FinalVersion", true);

                    var addedCardFile = card.Files.First(x => x.RowID == addedFile.ID);
                    addedCardFile.Options = addedFile.Options.SerializeJson();
                    addedCardFile.Versions.Last().Options = addedFile.Versions.Last.Options.SerializeJson();
                    addedCardFile.Flags |= CardFileFlags.UpdateOptions;

                    //foreach (var mainFile in container.FileContainer.Files.Where(x =>
                    //    x.Category?.ID == FilesInfo.Category.Osnovnie.ID &&
                    //    x.ID != addedFile.ID))
                    //{
                    //    await mainFile.ChangeCategoryAsync(newCategory: null);
                    //}

                    //foreach (var finalVersionCandidate in container.FileContainer.Files.Where(x => x.ID != addedFile.ID)
                    //)
                    //{
                    //    finalVersionCandidate.Options.Remove("FinalVersion");
                    //    finalVersionCandidate.Versions.Last.Options.Remove("FinalVersion");

                    //    var cardFile = card.Files.FirstOrDefault(x => x.RowID == finalVersionCandidate.ID);
                    //    cardFile.Options = finalVersionCandidate.Options.SerializeJson();
                    //    cardFile.Versions.Last().Options = finalVersionCandidate.Versions.Last.Options.SerializeJson();
                    //    cardFile.Flags |= CardFileFlags.UpdateOptions;
                    //}

                    //if ((additionalFile ?? file).Category != null && (additionalFile ?? file).Category.Equals(FilesInfo.Category.Osnovnie))
                    //{
                    //    await (additionalFile ?? file).ChangeCategoryAsync(newCategory: null);
                    //}

                    var storeResponse = await this.fileManager.StoreAsync(container,
                        async (cont, req, t) =>
                        {
                            CloseTask(cont.Card);
                            this.permissionsProvider.SetFullPermissions(req);

                            req.AffectVersion = false;
                            req.DoesNotAffectVersion = true;
                        });
                    var storeResult = storeResponse.ValidationResult.Build();
                    if (!storeResult.IsSuccessful)
                    {
                        context.ValidationResult.Add(storeResult);
                        return false;
                    }
                }

                var result = await this.GetCardAndCloseTask(card.ID);

                if (!result.IsSuccessful())
                {
                    context.ValidationResult.Add(result);
                    return false;
                }

                return true;
            }
        }

        private static void CloseTask(Tessa.Cards.Card card)
        {
            var task = card.Tasks.First(x => x.TypeID == GipTaskTypes.GipConvertToPdfTypeID);

            task.OptionID = DefaultCompletionOptions.Complete;
            task.Action = CardTaskAction.Complete;
            task.State = CardRowState.Deleted;
        }

        private async Task<IValidationResultBuilder> CloseTaskAndSaveCard(Tessa.Cards.Card card)
        {
            var validationResultBuilder = new ValidationResultBuilder();
            
            CloseTask(card);

            card.RemoveAllButChanged();

            var storeRequest = new CardStoreRequest
            {
                Card = card,
                AffectVersion = false,
                DoesNotAffectVersion = true
            };
            this.permissionsProvider.SetFullPermissions(storeRequest);
            var storeResponse = await this.cardRepository.StoreAsync(storeRequest);
            var storeResult = storeResponse.ValidationResult.Build();

            if (!storeResult.IsSuccessful)
            {
                return validationResultBuilder.Add(storeResult);
            }

            return validationResultBuilder;
        }
        private async Task<IValidationResultBuilder> GetCardAndCloseTask(Guid cardId)
        {
            var validationResultBuilder = new ValidationResultBuilder();
            var cardGetRequest = new CardGetRequest
            {
                CardID = cardId
            };

            this.permissionsProvider.SetFullPermissions(cardGetRequest);
            var cardGetResponse = await this.cardRepository.GetAsync(cardGetRequest);

            if (!cardGetResponse.ValidationResult.IsSuccessful())
            {
                return validationResultBuilder.Add(cardGetResponse.ValidationResult.Build());
            }

            var card = cardGetResponse.Card;

            CloseTask(card);

            card.RemoveAllButChanged();

            var storeRequest = new CardStoreRequest
            {
                Card = card,
                AffectVersion = false,
                DoesNotAffectVersion = true
            };
            this.permissionsProvider.SetFullPermissions(storeRequest);
            var storeResponse = await this.cardRepository.StoreAsync(storeRequest);
            var storeResult = storeResponse.ValidationResult.Build();

            if (!storeResult.IsSuccessful)
            {
                return validationResultBuilder.Add(storeResult);
            }

            return validationResultBuilder;
        }
    }
}