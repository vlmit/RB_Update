using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Localization;
using Tessa.Platform;
using Tessa.Platform.ConsoleApps;
using Tessa.Platform.Data;
using Tessa.Platform.IO;
using Tessa.Platform.Json;
using Tessa.Platform.Storage;
using Tessa.Views;
using Tessa.Views.Json;
using Tessa.Views.Json.Converters;
using Tessa.Views.Metadata;
using Tessa.Views.Parser;
using Tessa.Views.Parser.Serialization;
using Tessa.Views.Parser.SyntaxTree.ExchangeFormat;
using Tessa.Views.Parser.SyntaxTree.ViewMetadata;
using Tessa.Views.SearchQueries;
using Tessa.Views.Workplaces;
using Tessa.Views.Workplaces.Json;
using Tessa.Views.Workplaces.Json.Converters;
using Tessa.Views.Workplaces.Json.Metadata;

namespace Tessa.Extensions.Default.Console.ConvertConfiguration
{
    public sealed class Operation :
        ConsoleOperation<OperationContext>
    {
        #region Constructors

        public Operation(
            ConsoleSessionManager sessionManager,
            IConsoleLogger logger,
            ViewFilePersistent viewFilePersistent,
            IJsonViewModelExporter jsonViewModelExporter,
            WorkplaceFilePersistent workplaceFilePersistent,
            IConverter<IJsonSearchQueryMetadata, ISearchQueryMetadata> searchQueryConverter,
            IJsonViewModelAdapter viewModelAdapter,
            IViewServiceImplementer viewServiceImplementer,
            ViewMetadataEvaluationContextFactory evaluationContextFactory,
            IViewMetadataInterpreter viewMetadataInterpreter,
            IExchangeFormatInterpreter exchangeFormatInterpreter,
            IIndentationStrategy indentationStrategy)
            : base(logger, sessionManager)
        {
            this.viewFilePersistent = viewFilePersistent;
            this.jsonViewModelExporter = jsonViewModelExporter;
            this.viewModelAdapter = viewModelAdapter;
            this.workplaceFilePersistent = workplaceFilePersistent;
            this.searchQueryConverter = searchQueryConverter;
            this.viewServiceImplementer = viewServiceImplementer;
            this.evaluationContextFactory = evaluationContextFactory;
            this.viewMetadataInterpreter = viewMetadataInterpreter;
            this.exchangeFormatInterpreter = exchangeFormatInterpreter;
            this.indentationStrategy = indentationStrategy;
        }

        #endregion

        #region Private Fields

        private readonly ViewFilePersistent viewFilePersistent;

        private readonly IJsonViewModelExporter jsonViewModelExporter;

        private readonly IJsonViewModelAdapter viewModelAdapter;

        private readonly WorkplaceFilePersistent workplaceFilePersistent;

        private readonly IConverter<IJsonSearchQueryMetadata, ISearchQueryMetadata> searchQueryConverter;

        private readonly IViewServiceImplementer viewServiceImplementer;

        private readonly ViewMetadataEvaluationContextFactory evaluationContextFactory;

        private readonly IViewMetadataInterpreter viewMetadataInterpreter;

        private readonly IExchangeFormatInterpreter exchangeFormatInterpreter;

        private readonly IIndentationStrategy indentationStrategy;

        #endregion

        #region Private Methods

        private async ValueTask<string> ConvertItemsAsync(
            string sourcePath,
            string targetPath,
            bool doNotDelete,
            ConversionMode conversionMode,
            CancellationToken cancellationToken = default)
        {
            // Подменяем источных данных в поставщике представлений, чтоб он не пытался вытягивать их через сессию.
            if (viewServiceImplementer is IViewServiceInitializer viewServiceInitializer)
            {
                viewServiceInitializer.Initialize(new List<IViewMetadata>());
            }

            var items = new List<ConversionItem>();

            FileAttributes attr = File.GetAttributes(sourcePath);
            bool sourcePathIsDirectory = (attr & FileAttributes.Directory) == FileAttributes.Directory;
            foreach (string sourceFilePath in DefaultConsoleHelper.GetSourceFiles(sourcePath, "*.*", false))
            {
                // Имя директории может быть с точкой, тогда Path.GetDirectoryName() даст ошибочный результат, отбросив
                // часть пути, на самом деле являющегося директорией, поэтому выше проверяется через файловую систему
                // является ли sourcePath директорией.
                string relativeFilePath =
                    sourcePathIsDirectory
                        ? Path.GetRelativePath(sourcePath, sourceFilePath)
                        : Path.GetRelativePath(Path.GetDirectoryName(sourcePath) ?? string.Empty, sourceFilePath);

                string targetFilePath = Path.Combine(targetPath, relativeFilePath);
                string extension = Path.GetExtension(sourceFilePath).ToLowerInvariant();
                ConversionItem item;

                try
                {
                    switch (conversionMode)
                    {
                        case ConversionMode.Upgrade:
                            item = await this.ReadForConvertAsync(extension, sourceFilePath, targetFilePath,
                                cancellationToken);
                            break;

                        case ConversionMode.Downgrade:
                            item = await this.ReadForDowngradeAsync(extension, sourceFilePath, targetFilePath,
                                cancellationToken);
                            break;

                        case ConversionMode.LF:
                        case ConversionMode.CRLF:
                            switch (extension)
                            {
                                case ".cardlib": // библиотека карточек в xml
                                case ".jcard": // карточка в json
                                case ".json": // произвольный текстовый json, например, app.json
                                case ".jlocalization": // библиотека локализации в json
                                case ".jtype": // тип карточки в json
                                case ".jview": // представление в json
                                case ".jworkplace": // рабочее место в json
                                case ".jquery": // поисковый запрос в json
                                case ".sql": // sql-скрипт с процедурой, функцией или миграцией
                                case ".tct": // тип карточки в xml
                                case ".tll": // библиотека локализации в xml
                                case ".tpf": // функция схемы в xml
                                case ".tpm": // миграция схемы в xml
                                case ".tpp": // процедура схемы в xml
                                case ".tsd": // база данных схемы в xml
                                case ".tsp": // библиотека схемы в xml
                                case ".tst": // таблица схемы в xml
                                case ".txt": // текстовые файлы вида readme.txt
                                case ".view": // представление в exchange format
                                case ".workplace": // рабочее место в exchange format
                                case ".query": // поисковый в exchange format
                                case ".xml": // произвольный текстовый xml, например, extensions.xml
                                    // только наши текстовые файлы, не трогаем бинарные .card, и другие файлы (например, файлы реестра .reg)
                                    item = new ConversionItem(sourceFilePath, targetFilePath, null);
                                    break;

                                default:
                                    item = null;
                                    break;
                            }

                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(ConversionMode), conversionMode, null);
                    }
                }
                catch (Exception ex)
                {
                    await this.Logger.LogExceptionAsync($"Error when loading file \"{sourceFilePath}\"", ex);
                    item = null;
                }

                if (item != null)
                {
                    items.Add(item);
                }
            }

            var errorCount = 0;

            if (items.Count > 0)
            {
                await this.Logger.InfoAsync("Converting configuration files ({0})", items.Count);

                foreach (ConversionItem item in items)
                {
                    var itemWasConverted = true;
                    string targetDirectoryName = Path.GetDirectoryName(item.NewPath);
                    try
                    {
                        // Подготовить директорию для target
                        FileHelper.CreateDirectoryIfNotExists(targetDirectoryName, true);

                        switch (item.Object)
                        {
                            case null:
                                // преобразование переводов строк
                                string text =
                                    await File.ReadAllTextAsync(item.OldPath, Encoding.UTF8, cancellationToken);
                                string newText = conversionMode == ConversionMode.LF
                                    ? text.NormalizeLineEndingsUnixStyle()
                                    : text.NormalizeLineEndingsWindowsStyle();

                                if (!string.Equals(text, newText, StringComparison.Ordinal))
                                {
                                    await File.WriteAllTextAsync(item.NewPath, newText, Encoding.UTF8,
                                        cancellationToken);
                                    await this.Logger.InfoAsync("Line endings are converted: \"{0}\"", item.NewPath);
                                }

                                break;

                            case CardType cardType:
                                string typeText = conversionMode == ConversionMode.Downgrade
                                    ? cardType.SerializeToXml()
                                    : cardType.SerializeToJson(indented: true);
                                await File.WriteAllTextAsync(item.NewPath, typeText, Encoding.UTF8, cancellationToken);
                                await this.Logger.InfoAsync("Type is converted: \"{0}\"", item.NewPath);
                                break;

                            case LocalizationLibrary localizationLibrary:
                                if (conversionMode == ConversionMode.Downgrade)
                                {
                                    var localizationService =
                                        new FileLocalizationService(Path.GetDirectoryName(item.NewPath));
                                    await localizationService.SaveLibraryAsync(localizationLibrary, item.NewPath,
                                        cancellationToken);
                                }
                                else
                                {
                                    var localizationService =
                                        new JsonFileLocalizationService(Path.GetDirectoryName(item.NewPath));
                                    await localizationService.SaveLibraryAsync(localizationLibrary, item.NewPath,
                                        cancellationToken);
                                }

                                await this.Logger.InfoAsync("Localization library is converted: \"{0}\"", item.NewPath);
                                break;

                            case TessaViewModel _:
                                var viewFileName =
                                    await this.ConvertViewToJsonAsync(item.OldPath, item.NewPath, cancellationToken);
                                if (!string.IsNullOrWhiteSpace(viewFileName))
                                {
                                    await this.Logger.InfoAsync("View is converted: \"{0}\"", viewFileName);
                                }
                                else
                                {
                                    itemWasConverted = false;
                                    errorCount++;
                                    await this.Logger.InfoAsync("Cannot convert view: \"{0}\"", item.OldPath);
                                }

                                break;

                            case WorkplaceModel _:
                                var workplaceFileName =
                                    await this.ConvertWorkplaceToJsonAsync(item.OldPath, item.NewPath,
                                        cancellationToken);
                                if (!string.IsNullOrWhiteSpace(workplaceFileName))
                                {
                                    await this.Logger.InfoAsync("Workplace is converted: \"{0}\"", workplaceFileName);
                                }
                                else
                                {
                                    itemWasConverted = false;
                                    errorCount++;
                                    await this.Logger.InfoAsync("Cannot convert workplace: \"{0}\"", item.OldPath);
                                }

                                break;

                            case SearchQueryMetadata _:
                                var searchQueryName =
                                    await this.ConvertSearchQueryToJsonAsync(item.OldPath, item.NewPath,
                                        cancellationToken);
                                if (!string.IsNullOrWhiteSpace(searchQueryName))
                                {
                                    await this.Logger.InfoAsync("Search query is converted: \"{0}\"", searchQueryName);
                                }
                                else
                                {
                                    itemWasConverted = false;
                                    errorCount++;
                                    await this.Logger.InfoAsync("Cannot convert search query: \"{0}\"", item.OldPath);
                                }

                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        itemWasConverted = false;
                        errorCount++;
                        await this.Logger.LogExceptionAsync($"Error when converting file \"{item.OldPath}\"", ex);

                        // Удалить созданную target directory если в ней ничего нет.
                        if (Directory.Exists(targetDirectoryName)
                            && !Directory.EnumerateFileSystemEntries(targetDirectoryName).Any())
                        {
                            Directory.Delete(targetDirectoryName);
                        }
                    }

                    if (!doNotDelete
                        && itemWasConverted
                        && item.NewPath != item.OldPath)
                    {
                        FileHelper.DeleteFileSafe(item.OldPath);
                    }
                }
            }

            return $"{items.Count - errorCount} of {items.Count} files were converted successfully.";
        }

        private async ValueTask<string> ConvertViewToJsonAsync(string oldPath, string newPath,
            CancellationToken cancellationToken = default)
        {
            var viewModels = await this.viewFilePersistent.ReadAsync(oldPath, async (fileName, exception, ct) =>
            {
                await this.Logger.LogExceptionAsync($"Error while reading {fileName}", exception);
                return true;
            }, cancellationToken);

            var viewModel = viewModels.FirstOrDefault();
            if (viewModel is null)
            {
                return null;
            }

            var jsonViewModel = this.viewModelAdapter.AdaptToJsonViewModel(viewModel);
            var context = this.evaluationContextFactory(Dbms.Unknown, jsonViewModel.Alias, jsonViewModel.Caption);

            jsonViewModel.JsonMetadataSource =
                (await this.viewMetadataInterpreter.EvaluateAsync(viewModel.MetadataSource, context, cancellationToken))
                .ToJsonString();
            jsonViewModel.MetadataSource = null;

            var fullName = Path.Combine(newPath, $"{jsonViewModel.Alias}.jview");

            await using var file = FileHelper.Create(fullName);
            await this.jsonViewModelExporter.ExportAsync(jsonViewModel, file, CancellationToken.None);

            return fullName;
        }

        private async ValueTask<string> ConvertWorkplaceToJsonAsync(string oldPath, string newPath,
            CancellationToken cancellationToken = default)
        {
            var workplaceModels = await this.workplaceFilePersistent.ReadAsync(oldPath,
                async (fileName, exception, ct) =>
                {
                    await this.Logger.LogExceptionAsync($"Error while reading {fileName}", exception);
                    return true;
                }, cancellationToken);

            var model = workplaceModels.FirstOrDefault();
            if (model is null)
            {
                return null;
            }

            var context = new TessaJsonSerializationContext();
            await using var scope = TessaJsonSerializationContext.Create(context);

            var workplace = model.Workplace;

            var jsonWorkplace = new JsonWorkplace()
            {
                Metadata = workplace.Metadata.FromJsonString<JsonWorkplaceMetadata>()
            };
            if (workplace.Roles != null)
            {
                jsonWorkplace.Roles.AddRange(workplace.Roles);
            }

            var jsonWorkplaceModel = new JsonWorkplaceModel()
            {
                Content = jsonWorkplace
            };

            var fileName = await LocalizationManager.LocalizeOrGetNameAsync(
                workplace.Name,
                CultureInfo.GetCultureInfo(LocalizationManager.EnglishLanguageCode),
                cancellationToken);

            var fullName = Path.Combine(newPath, $"{fileName}.jworkplace");

            await using var file = new FileStream(fullName, FileMode.Create);

            var jsonString = jsonWorkplaceModel.ToJsonString();

            var bytes = Encoding.UTF8.GetBytes(jsonString);
            await file.WriteAsync(bytes.AsMemory(0, bytes.Length), cancellationToken);
            var textpartWriter = new TextPartWriter(file);
            await textpartWriter.WriteAsync(cancellationToken);

            return fullName;
        }

        private async ValueTask<string> ConvertSearchQueryToJsonAsync(string oldPath, string newPath,
            CancellationToken cancellationToken = default)
        {
            await using var stream = FileHelper.OpenRead(oldPath);
            var context = await this.exchangeFormatInterpreter.InterpretAsync(stream, this.indentationStrategy,
                cancellationToken: cancellationToken);
            var searchQueries = context.GetSearchQueries();

            var searchQuery = searchQueries.FirstOrDefault();
            if (searchQuery is null)
            {
                return null;
            }

            var alias = await LocalizationManager.LocalizeOrGetNameAsync(searchQuery.Alias, cancellationToken);
            var fullName = this.GetSearchQueryConvertedFileName(alias, newPath);

            await using var file = FileHelper.Create(fullName);

            var jsonMetadata = await this.searchQueryConverter.ConvertBackAsync(searchQuery);
            var jsonString = jsonMetadata.ToJsonString();
            byte[] bytes = Encoding.UTF8.GetBytesWithPreamble(jsonString);
            await file.WriteAsync(bytes, 0, bytes.Length, cancellationToken);

            return fullName;
        }


        // Метод определяет имя файла для сконвертированного поискового запроса. По умолчанию это <alias>.jquery, но если такой файл уже существует, 
        // то метод вернет <alias>_1.jquery. Если существует и <alias>_1.jquery, то возвращено будет <alias>_2.jquery. И т.д.
        private string GetSearchQueryConvertedFileName(string alias, string path)
        {
            var fileName = Path.Combine(path, $"{alias}.jquery");
            var counter = 0;
            while (File.Exists(fileName))
            {
                fileName = Path.Combine(path, $"{alias}_{++counter}.jquery");
            }

            return fileName;
        }

        private async Task<ConversionItem> ReadForConvertAsync(string extension, string sourceFilePath,
            string targetFilePath, CancellationToken cancellationToken = default)
        {
            switch (extension)
            {
                case ".tct":
                    await this.Logger.InfoAsync("Reading type from: \"{0}\"", sourceFilePath);

                    var cardType = new CardType();
                    await using (FileStream fileStream = FileHelper.OpenRead(sourceFilePath, synchronousOnly: true))
                    {
                        cardType.DeserializeFromXml(fileStream);
                    }

                    string newTypeFilePath = DefaultConsoleHelper.ChangeExtension(targetFilePath, ".tct", ".jtype");
                    return new ConversionItem(sourceFilePath, newTypeFilePath, cardType);

                case ".tll":
                    await this.Logger.InfoAsync("Reading localization library from: \"{0}\"", sourceFilePath);

                    var localizationService = new FileLocalizationService(new[] { sourceFilePath });
                    var localizationLibrary =
                        (await localizationService.GetLibrariesAsync(returnComments: true,
                            cancellationToken: cancellationToken)).First();

                    string newLibraryFilePath =
                        DefaultConsoleHelper.ChangeExtension(targetFilePath, ".tll", ".jlocalization");
                    return new ConversionItem(sourceFilePath, newLibraryFilePath, localizationLibrary);

                case ".view":
                    await this.Logger.InfoAsync("View is pending to convert: \"{0}\"", sourceFilePath);
                    // string newViewTypePath = ChangeExtension(filePath, ".view", ".jview");
                    return new ConversionItem(sourceFilePath, Path.GetDirectoryName(targetFilePath),
                        new TessaViewModel());

                case ".workplace":
                    await this.Logger.InfoAsync("Workplace is pending to convert: \"{0}\"", sourceFilePath);
                    return new ConversionItem(sourceFilePath, Path.GetDirectoryName(targetFilePath),
                        new WorkplaceModel());

                case ".query":
                    await this.Logger.InfoAsync("Search query is pending to convert: \"{0}\"", sourceFilePath);
                    return new ConversionItem(sourceFilePath, Path.GetDirectoryName(targetFilePath),
                        new SearchQueryMetadata());

                default:
                    return null;
            }
        }

        private async Task<ConversionItem> ReadForDowngradeAsync(string extension, string sourceFilePath,
            string targetFilePath, CancellationToken cancellationToken = default)
        {
            switch (extension)
            {
                case ".jtype":
                    await this.Logger.InfoAsync("Reading type from: \"{0}\"", sourceFilePath);

                    string text = await File.ReadAllTextAsync(sourceFilePath, cancellationToken);
                    var cardType = CardSerializableObject.DeserializeFromJson<CardType>(text);

                    string newTypeFilePath = DefaultConsoleHelper.ChangeExtension(targetFilePath, ".jtype", ".tct");
                    return new ConversionItem(sourceFilePath, newTypeFilePath, cardType);

                case ".jlocalization":
                    await this.Logger.InfoAsync("Reading localization library from: \"{0}\"", sourceFilePath);

                    var localizationService = new JsonFileLocalizationService(new[] { sourceFilePath });
                    var localizationLibrary =
                        (await localizationService.GetLibrariesAsync(returnComments: true,
                            cancellationToken: cancellationToken)).First();

                    string newLibraryFilePath =
                        DefaultConsoleHelper.ChangeExtension(targetFilePath, ".jlocalization", ".tll");
                    return new ConversionItem(sourceFilePath, newLibraryFilePath, localizationLibrary);

                default:
                    return null;
            }
        }

        #endregion

        #region Base Overrides

        public override async Task<int> ExecuteAsync(
            OperationContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await this.Logger.InfoAsync("Converting configuration from: \"{0}\"", context.Source);
                var resultMessage = await this.ConvertItemsAsync(
                    context.Source,
                    context.Target,
                    context.DoNotDelete,
                    context.ConversionMode,
                    cancellationToken);
                await this.Logger.InfoAsync(resultMessage);
                return 0;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                await this.Logger.LogExceptionAsync("Error converting configuration", e);
                return -1;
            }
        }

        #endregion
    }
}
