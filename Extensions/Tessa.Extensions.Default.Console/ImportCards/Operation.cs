using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Applications.Package;
using Tessa.Cards;
using Tessa.Cards.SmartMerge;
using Tessa.Platform;
using Tessa.Platform.ConsoleApps;
using Tessa.Platform.IO;
using Tessa.Platform.SourceProviders;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Console.ImportCards
{
    public sealed class Operation : ConsoleOperation<OperationContext>
    {
        #region FailedImportingResult Private Class

        #endregion

        #region Constructors

        public Operation(
            ConsoleSessionManager sessionManager,
            IConsoleLogger logger,
            ICardManager cardManager,
            ICardRepository cardRepository)
            : base(logger, sessionManager, extendedInitialization: true)
        {
            this.cardManager = cardManager;
            this.cardRepository = cardRepository;
        }

        #endregion

        #region Fields

        private readonly ICardManager cardManager;

        private readonly ICardRepository cardRepository;

        #endregion

        #region Private Constants

        private const string ImportingAddOperationPrefix = "UI_Cards_ImportingAddOperationPrefix";

        #endregion

        #region Private Methods

        private async Task<bool> ImportFilesAsync(
            bool ignoreExistentCards,
            bool ignoreRepairMessages,
            string generalMergeOptionsPath,
            string ignoredFilesPath,
            CancellationToken cancellationToken,
            params (string fileName, string optionsName)[] files)
        {
            IValidationResultBuilder validationResult = new ValidationResultBuilder();
            var successfulCardNames = new List<string>(files.Length);

            // если опции указаны через параметр /options:fileName, то используем указанные для всего импорта
            ICardMergeOptions cardMergeOptions = null;
            if (!string.IsNullOrEmpty(generalMergeOptionsPath))
            {
                string[] fileNames = DefaultConsoleHelper.GetSourceFiles(generalMergeOptionsPath, null,
                    throwIfNotFound: true, checkPatternMatch: false).ToArray();

                switch (fileNames.Length)
                {
                    case 0:
                        await this.Logger.ErrorAsync(
                            $"Can't find merge options file using path \"{generalMergeOptionsPath}\".");
                        return false;

                    case > 1:
                        await this.Logger.ErrorAsync(
                            $"Expected to find single file for merge options \"{generalMergeOptionsPath}\"" +
                            $" but found multiple ({fileNames.Length}): {string.Join(", ", fileNames)}");
                        return false;
                }

                string optionsFileName = Path.GetFullPath(fileNames[0]);
                ISourceContentProvider optionsFileProvider = new FileSourceContentProvider(optionsFileName);

                cardMergeOptions = await CardHelper.ParseMergeOptionsAsync(optionsFileProvider, cancellationToken);
            }

            try
            {
                // Файл с паттернами игноров
                if (!string.IsNullOrEmpty(ignoredFilesPath))
                {
                    ignoredFilesPath = Path.GetFullPath(ignoredFilesPath);

                    if (!File.Exists(ignoredFilesPath))
                    {
                        await this.Logger.ErrorAsync(
                            $"File with ignored patterns \"{ignoredFilesPath}\" not found.");
                        return false;
                    }
                }

                foreach ((string fileName, string optionsName) in files)
                {
                    string cardName = GetImportingCardName(fileName);

                    // Функция игнорирования лишних файлов
                    Func<string, bool> ignoredFilesFunc = null;
                    if (!string.IsNullOrEmpty(ignoredFilesPath))
                    {
                        var ignoredProvider = new FileSystemIgnoredFilesProvider();
                        var filesFolder = Path.Combine(
                            Path.GetDirectoryName(fileName) ?? string.Empty,
                            Path.GetFileNameWithoutExtension(fileName) ?? string.Empty);

                        if (Directory.Exists(filesFolder))
                        {
                            var ignoredPathList =
                                new HashSet<string>(
                                    ignoredProvider.GetIgnoredFileNames(filesFolder, ignoredFilesPath)
                                        .Select(Path.GetFullPath))
                                {
                                    ignoredFilesPath
                                };
                            ignoredFilesFunc = nameToCheck => ignoredPathList.Contains(nameToCheck);
                        }
                    }

                    // если общие опции слияния не указаны
                    ICardMergeOptions fileCardMergeOptions;
                    string mergeOptionsPath;
                    if (cardMergeOptions is null)
                    {
                        fileCardMergeOptions = new CardMergeOptions();

                        // Сначала смотрим не указаны ли опции в атрибуте библиотеки
                        if (!string.IsNullOrEmpty(optionsName))
                        {
                            // На данном этапе если файла по пути не найдется, должно бросаться исключение,
                            // т.к. либо этот файл не должен быть указан, либо его случайно удалили из папки/репозитория

                            string optionsFileName = Path.GetFullPath(optionsName);
                            ISourceContentProvider optionsFileProvider = new FileSourceContentProvider(optionsFileName);

                            fileCardMergeOptions = await CardHelper.ParseMergeOptionsAsync(optionsFileProvider, cancellationToken);
                            mergeOptionsPath = optionsFileName;
                        }
                        // Затем пытаемся найти их в подпапке, имя подпапки = имя файла без расширения
                        else
                        {
                            mergeOptionsPath = Path.Combine(
                                Path.GetDirectoryName(Path.GetFullPath(fileName)) ?? string.Empty,
                                Path.GetFileNameWithoutExtension(fileName),
                                CardHelper.DefaultMergeOptionsFileName);

                            // На данном этапе если файла по пути не найдется, НЕ должно бросаться исключение,
                            // т.к. такой вариант загрузки опций слияния не является обязательным
                            // поэтому производим проверку на доступность файла
                            if (File.Exists(mergeOptionsPath))
                            {
                                ISourceContentProvider optionsFileProvider = new FileSourceContentProvider(mergeOptionsPath);

                                fileCardMergeOptions = await CardHelper.ParseMergeOptionsAsync(optionsFileProvider, cancellationToken);
                            }
                            else
                            {
                                mergeOptionsPath = null;
                            }
                        }
                    }
                    else
                    {
                        fileCardMergeOptions = cardMergeOptions;
                        mergeOptionsPath = generalMergeOptionsPath;
                    }

                    // Если задано через параметр, то всегда переопределяем в опциях слияния
                    if (ignoreExistentCards)
                    {
                        fileCardMergeOptions.SkipIfCardExists = true;
                    }

                    await this.Logger.DebugAsync(
                        $"Importing card \"{cardName}\"" + (string.IsNullOrEmpty(mergeOptionsPath)
                            ? null
                            : $", using merge options \"{mergeOptionsPath}\""));

                    string sourceName = Path.GetFullPath(fileName);
                    ISourceContentProvider fileSourceContentProvider = new FileSourceContentProvider(sourceName, true);

                    CardStoreResponse response = await this.ImportCardCoreAsync(
                        fileSourceContentProvider,
                        fileCardMergeOptions,
                        ignoredFilesFunc,
                        cancellationToken);

                    // Проверка на Info-ключ CardIsSkippedDuringImport
                    bool skipped = false;
                    if (response.ValidationResult.Items.Any(x => x.Key == CardValidationKeys.CardIsSkippedDuringImport))
                    {
                        // Удаляем CardIsSkippedDuringImport чтобы не выводилось лишних сообщений
                        response.ValidationResult.RemoveAll(CardValidationKeys.CardIsSkippedDuringImport);
                        skipped = true;
                    }
                    ValidationResult result = response.ValidationResult.Build();

                    DefaultConsoleHelper.AddOperationToValidationResult(
                        ImportingAddOperationPrefix,
                        cardName,
                        result,
                        validationResult,
                        ignoreExistentCards,
                        ignoreRepairMessages);

                    if (result.IsSuccessful)
                    {
                        successfulCardNames.Add(cardName);
                        if (skipped)
                        {
                            await this.Logger.InfoAsync("Card is skipped: {0}", cardName);
                        }
                        else
                        {
                            await this.Logger.InfoAsync("Card is imported: {0}", cardName);
                        }
                    }
                    else
                    {
                        await this.Logger.InfoAsync("Card is failed to import: {0}", cardName);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                validationResult.AddException(this, ex);
            }

            if (successfulCardNames.Count != 0)
            {
                validationResult = GetImportingResultWithPreamble(validationResult, successfulCardNames);
            }

            ValidationResult totalResult = validationResult.Build();
            await this.Logger.LogResultAsync(totalResult);

            return totalResult.IsSuccessful;
        }


        private static string GetImportingCardName(string fileName)
        {
            // в качестве имени выводим полностью относительный путь к файлу, чтобы было подробно залогировано
            // return Path.GetFileNameWithoutExtension(fileName);

            return fileName;
        }


        private static IValidationResultBuilder GetImportingResultWithPreamble(
            IValidationResultBuilder validationResult,
            ICollection<string> successfulCardNames)
        {
            if (successfulCardNames.Count == 0)
            {
                return validationResult;
            }

            string infoText =
                DefaultConsoleHelper.GetQuotedItemsText(
                        new StringBuilder(),
                        "UI_Cards_CardImported",
                        "UI_Cards_MultipleCardsImported",
                        successfulCardNames)
                    .ToString();

            return new ValidationResultBuilder()
                .AddInfo(typeof(Operation), infoText)
                .Add(validationResult);
        }

        private async Task<CardStoreResponse> ImportCardCoreAsync(
            ISourceContentProvider sourceContentProvider,
            ICardMergeOptions cardMergeOptions,
            Func<string, bool> ignoredFilesFunc,
            CancellationToken cancellationToken = default)
        {
            CardStoreResponse response;

            try
            {
                string extension = Path.GetExtension(sourceContentProvider.ToString());
                CardFileFormat format = CardHelper.TryParseCardFileFormatFromExtension(extension) ??
                    CardFileFormat.Json;

                response = await this.cardManager.ImportAsync(
                    sourceContentProvider,
                    format: format,
                    mergeOptions: cardMergeOptions,
                    ignoredFilesFunc: ignoredFilesFunc,
                    wipeDeleted: true,
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                response = new CardStoreResponse();
                response.ValidationResult.AddException(this, ex);
            }

            return response;
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc />
        public override async Task<int> ExecuteAsync(OperationContext context,
            CancellationToken cancellationToken = default)
        {
            if (!this.SessionManager.IsOpened)
            {
                return -1;
            }

            try
            {
                // если указана папка, то находим первый файл с подходящим расширением
                HashSet<int> libraries = new HashSet<int>();
                foreach (string source in context.Sources.SelectMany((x, i) =>
                {
                    var sourceFiles = DefaultConsoleHelper.GetSourceFiles(
                        x,
                        "*.cardlib",
                        throwIfNotFound: false,
                        checkPatternMatch: true);
                    if (sourceFiles.Count > 0)
                    {
                        libraries.Add(i);
                    }

                    return sourceFiles;
                }))
                {
                    await this.Logger.InfoAsync("Reading card library from: \"{0}\"", source);

                    var library = new CardLibrary();
                    await using (FileStream fileStream = FileHelper.OpenRead(source, synchronousOnly: true))
                    {
                        library.DeserializeFromXml(fileStream);
                    }

                    if (library.Items.Count == 0)
                    {
                        await this.Logger.InfoAsync("There are no files in the card library");
                        continue;
                    }

                    string folder = Path.GetDirectoryName(source);
                    if (string.IsNullOrEmpty(folder))
                    {
                        folder = Directory.GetCurrentDirectory();
                    }

                    var files = library.Items
                        .Select(libraryItem => (
                            Path.Combine(folder, libraryItem.Path).NormalizePathOnCurrentPlatform(),
                            string.IsNullOrEmpty(libraryItem.Options)
                                ? string.Empty
                                : Path.Combine(folder, libraryItem.Options).NormalizePathOnCurrentPlatform()))
                        .ToArray();

                    await this.Logger.InfoAsync(
                        "Importing cards ({0}) in the card library:{1}{2}",
                        files.Length,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, files.Select(x => $"\"{x.Item1}\"")));

                    if (!await this.ImportFilesAsync(
                        context.IgnoreExistentCards,
                        context.IgnoreRepairMessages,
                        context.MergeOptionsPath,
                        context.IgnoredFilesPath,
                        cancellationToken,
                        files))
                    {
                        return -1;
                    }
                }


                bool foundFiles = false;
                var sourcesWithoutLibraries = context.Sources
                    .Where((x, i) => !libraries.Contains(i))
                    .SelectMany(x => DefaultConsoleHelper.GetSourceFiles(x, "*.jcard", throwIfNotFound: false, checkPatternMatch: true)).ToList();
                sourcesWithoutLibraries.AddRange(context.Sources
                    .Where((x, i) => !libraries.Contains(i))
                    .SelectMany(x => DefaultConsoleHelper.GetSourceFiles(x, "*.card", throwIfNotFound: false, checkPatternMatch: true)));
                foreach (string source in sourcesWithoutLibraries)
                {
                    string extension = Path.GetExtension(source);
                    CardFileFormat? format = CardHelper.TryParseCardFileFormatFromExtension(extension);

                    if (format.HasValue)
                    {
                        foundFiles = true;

                        await this.Logger.InfoAsync("Importing card from: \"{0}\"", source);

                        if (!await this.ImportFilesAsync(
                            context.IgnoreExistentCards,
                            context.IgnoreRepairMessages,
                            context.MergeOptionsPath,
                            context.IgnoredFilesPath,
                            cancellationToken,
                            (source, string.Empty)))
                        {
                            return -1;
                        }
                    }
                }

                if (sourcesWithoutLibraries.Count > 0 && !foundFiles)
                {
                    var sourcesString = string.Join(',', sourcesWithoutLibraries.Select(x => $"\"{x}\""));
                    throw new FileNotFoundException(
                        $"Couldn't locate *.cardlib, *.jcard or *.card files in {sourcesString}.");
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                await this.Logger.LogExceptionAsync("Error importing cards", e);
                return -1;
            }

            await this.Logger.InfoAsync("Cards are imported successfully");
            return 0;
        }

        #endregion
    }
}
