using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Extensions.Default.Shared.Views;
using Tessa.Localization;
using Tessa.Platform;
using Tessa.Platform.Collections;
using Tessa.Platform.Validation;
using Tessa.Properties.Resharper;
using Tessa.Scheme;
using Tessa.UI.Cards.Controls;
using Tessa.UI.Files;
using Tessa.Views.Metadata;
using Tessa.Views.Metadata.Criteria;

namespace Tessa.Extensions.Default.Client.UI.CardFiles
{
    public class CardFilesDataProvider : IDataProvider
    {
        private readonly IViewMetadata viewMetadata;
        [NotNull]
        private readonly IFileControl fileControl;

        public CardFilesDataProvider(IViewMetadata viewMetadata, IFileControl fileControl)
        {
            this.viewMetadata = viewMetadata;
            this.fileControl = fileControl ?? throw new ArgumentNullException(nameof(fileControl));
        }

        /// <inheritdoc />
        public async Task<IGetDataResponse> GetDataAsync(
            IGetDataRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = new GetDataResponse(new ValidationResultBuilder());
            AddFieldDescriptions(result);
            await this.PopulateDataRowsAsync(request, result, cancellationToken);
            return result;
        }

        private async ValueTask PopulateDataRowsAsync(
            IGetDataRequest request,
            [NotNull] IGetDataResponse result,
            CancellationToken cancellationToken = default)
        {
            var filter = this.GetFilter(request);
            IEnumerable<IFileViewModel> files = this.fileControl.Items;

            var requestParameters = BuildParametersCollectionFromRequest(request);

            // если фильтры сбросили через кнопку в представлении, то нужно сбросить фильтрацию в файловом контроле.
            if (fileControl.SelectedFiltering != null && !requestParameters.Any(x => x.Name == ColumnsConst.FilterParameter))
            {
                await this.fileControl.SelectFilteringAsync(null, cancellationToken);
            }

            if (fileControl.SelectedFiltering != null)
            {
                files = files.Where(x => this.fileControl.SelectedFiltering.IsVisible(x));
            }

            var filteredFiles = files.Where(filter).ToArray();
            var rows = new List<IDictionary<string, object>>(filteredFiles.Length);

            foreach (var file in filteredFiles)
            {
                var row = MapFileToRow(file);
                rows.Add(row);
            }

            rows.Sort(new FilesSorter(this.viewMetadata, request));
            result.Rows.AddRange(rows);
        }

        public static Dictionary<string, object> MapFileToRow(IFileViewModel file)
        {
            string size = FormattingHelper.FormatSize(file.Model.Size, SizeUnit.Kilobytes)
                + FormattingHelper.FormatUnit(SizeUnit.Kilobytes);

            return new Dictionary<string, object>
            {
                [ColumnsConst.FileViewModel] = file,
                [ColumnsConst.GroupCaption] = file.GroupCaption,

                // если в "( Без категории )" не заменять обычные пробелы на неделимые, то автосайз начинает колбасить
                // при наличии нескольких таких строк, и расчёт ширины происходит заметно длительнее
                [ColumnsConst.CategoryCaption] = file.Model.Category?.Caption
                    ?? LocalizationManager.Localize("$UI_Cards_FileNoCategory").ReplaceSpacesToNonBreakable(),

                [ColumnsConst.Caption] = file.Caption,
                [ColumnsConst.SizeAbsolute] = file.Model.Size,
                [ColumnsConst.Size] = size,
            };
        }

        private Func<IFileViewModel, bool> GetFilter(IGetDataRequest request)
        {
            return FileFilter.Create(request).Filter;
        }

        private static void AddFieldDescriptions([NotNull] IGetDataResponse result)
        {
            result.Columns.Add(new KeyValuePair<string, SchemeType>(ColumnsConst.GroupCaption, SchemeType.NullableString));
            result.Columns.Add(new KeyValuePair<string, SchemeType>(ColumnsConst.Caption, SchemeType.NullableString));
            result.Columns.Add(new KeyValuePair<string, SchemeType>(ColumnsConst.CategoryCaption, SchemeType.NullableString));
            result.Columns.Add(new KeyValuePair<string, SchemeType>(ColumnsConst.Size, SchemeType.String));
            result.Columns.Add(new KeyValuePair<string, SchemeType>(ColumnsConst.SizeAbsolute, SchemeType.Int64));
        }

        private static IEnumerable<RequestParameter> BuildParametersCollectionFromRequest(IGetDataRequest request)
        {
            var parametersCollection = new List<RequestParameter>();
            foreach (var action in request.ParametersActions)
            {
                action(parametersCollection);
            }

            return parametersCollection;
        }

        private sealed class FileFilter
        {
            private readonly List<List<Func<IFileViewModel, bool>>> filter;

            [NotNull]
            private readonly IGetDataRequest request;

            private FileFilter([NotNull] IGetDataRequest request)
            {
                this.request = request ?? throw new ArgumentNullException(nameof(request));
                this.filter = new List<List<Func<IFileViewModel, bool>>>();
            }

            public bool Filter([NotNull] IFileViewModel fileObject)
            {
                return this.filter
                    .Select(block => block.Aggregate(false, (current, func) => current | func(fileObject)))
                    .Aggregate(true, (filterResult, blockResult) => filterResult & blockResult);
            }

            private void BuildFilter()
            {
                var alwaysTrueBlock = new List<Func<IFileViewModel, bool>> { AlwaysTrue };
                this.filter.Clear();
                this.filter.Add(alwaysTrueBlock);
                var requestParameters = BuildParametersCollectionFromRequest(this.request);
                foreach (var parameter in requestParameters)
                {
                    if (parameter.Name == ColumnsConst.Caption)
                    {
                        var filterBlock = new List<Func<IFileViewModel, bool>>();

                        foreach (var criteria in parameter.CriteriaValues)
                        {
                            AppendCriteriaToCaptionFilterFunc(criteria, filterBlock);
                        }
                        this.filter.Add(filterBlock);

                    }
                }
            }

            private static void AppendCriteriaToCaptionFilterFunc([NotNull] RequestCriteria criteria, [NotNull] ICollection<Func<IFileViewModel, bool>> filterBlock)
            {
                switch (criteria.CriteriaName)
                {
                    case CriteriaOperatorConst.Contains:
                        filterBlock.Add(f => f.Caption.Contains(
                            ((string)criteria.Values[0].Value) ?? string.Empty,
                            StringComparison.OrdinalIgnoreCase));
                        break;

                    case CriteriaOperatorConst.Equality:
                        filterBlock.Add(f => string.Equals(f.Caption, (string)criteria.Values[0].Value,
                            StringComparison.OrdinalIgnoreCase));
                        break;

                    case CriteriaOperatorConst.StartWith:
                        filterBlock.Add(f => f.Caption.StartsWith((string) criteria.Values[0].Value ?? string.Empty,
                            StringComparison.OrdinalIgnoreCase));
                        break;

                    case CriteriaOperatorConst.EndWith:
                        filterBlock.Add(f => f.Caption.EndsWith((string)criteria.Values[0].Value ?? string.Empty,
                            StringComparison.OrdinalIgnoreCase));
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(criteria.CriteriaName), criteria.CriteriaName,
                            $"Unsupported criteria: '{criteria.CriteriaName}'");
                }
            }

            private static bool AlwaysTrue([CanBeNull] IFileViewModel fileObject)
            {
                return true;
            }

            public static FileFilter Create(IGetDataRequest request)
            {
                var filter = new FileFilter(request);
                filter.BuildFilter();
                return filter;
            }
        }
    }

    internal class FilesSorter : IComparer<IDictionary<string, object>>
    {
        private readonly List<SortingColumn> sortingColumns;

        public FilesSorter(IViewMetadata viewMetadata, IGetDataRequest request)
        {
            this.sortingColumns = new List<SortingColumn>();
            foreach (var column in request.SortingColumns)
            {
                var sortingColumn = new SortingColumn()
                {
                    SortDirection = column.SortDirection,
                    Alias = viewMetadata.Columns.First(x => x.Alias == column.Alias).SortBy,
                };
                this.sortingColumns.Add(sortingColumn);
            }
        }

        /// <inheritdoc />
        public int Compare(IDictionary<string, object> lhv, IDictionary<string, object> rhv)
        {
            if (!this.sortingColumns.Any())
            {
                return 0;
            }
            var lhvFileVM = (IFileViewModel)lhv[ColumnsConst.FileViewModel];
            var rhvFileVM = (IFileViewModel)rhv[ColumnsConst.FileViewModel];
            // оргининал всегда "выше" чем копия файла независимо от направления сортировки
            if(lhvFileVM.Model.Origin == rhvFileVM.Model)
            {
                return 1;
            }

            if (rhvFileVM.Model.Origin == lhvFileVM.Model)
            {
                return -1;
            }

            var sortingColumn = this.sortingColumns.First();
            var comparsion = Comparer.Default.Compare(lhv[sortingColumn.Alias], rhv[sortingColumn.Alias]);
            return sortingColumn.SortDirection == ListSortDirection.Ascending ? comparsion : -comparsion;
        }
    }

    internal class SortingColumn
    {
        public ListSortDirection SortDirection;
        public string Alias;
    }
}