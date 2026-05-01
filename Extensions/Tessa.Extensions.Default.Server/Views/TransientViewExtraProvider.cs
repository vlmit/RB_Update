using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Platform.Data;
using Tessa.Roles;
using Tessa.Scheme;
using Tessa.Views;
using Tessa.Views.Metadata;
using Tessa.Views.Metadata.Types;
using Tessa.Views.Parser;

namespace Tessa.Extensions.Default.Server.Views
{
    public sealed class TransientViewExtraProvider : IExtraViewProvider
    {
        #region Constructors

        public TransientViewExtraProvider(ISchemeTypeConverter schemeTypeConverter, IDbScope dbScope)
        {
            this.extraView = new TransientView(schemeTypeConverter, dbScope);
        }

        #endregion

        #region Fields

        /// <summary>
        ///     The extra views.
        /// </summary>
        private readonly TransientView extraView;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Возвращает программное представление.
        /// </summary>
        /// <returns></returns>
        public ITessaView GetExtraView() => this.extraView;

        #endregion

        /// <summary>
        ///     The transient view.
        /// </summary>
        private sealed class TransientView : ITessaView, ITessaViewAccess
        {
            #region Fields

            private readonly ISchemeTypeConverter schemeTypeConverter;

            private readonly IDbScope dbScope;

            private readonly IViewMetadata metadata;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="TransientView" /> class.
            /// </summary>
            public TransientView(ISchemeTypeConverter schemeTypeConverter, IDbScope dbScope)
            {
                this.schemeTypeConverter = schemeTypeConverter ?? throw new ArgumentNullException(nameof(schemeTypeConverter));
                this.dbScope = dbScope ?? throw new ArgumentNullException(nameof(dbScope));

                this.metadata = new ViewMetadata { Alias = "TransientViewExample", Caption = "Пример представления", };
                this.metadata.Parameters.Add(
                    new ViewParameterMetadata { Alias = "Name", Caption = "Название", SchemeType = SchemeType.String });
                this.metadata.Parameters.Add(
                    new ViewParameterMetadata { Alias = "Count", Caption = "Количество", SchemeType = SchemeType.Int32 });
            }

            #endregion

            #region Public Properties

            /// <summary>
            ///     Возвращает список ролей которые необходимы для доступа к представлению
            ///     реализующему данный интерфейс
            /// </summary>
            /// <returns>
            ///     Список ролей
            /// </returns>
            public IEnumerable<Role> GetRoles() => new List<Role>
                { new Role { ID = new Guid(0x132b2a3c, 0x7bc9, 0x4a0f, 0x88, 0x05, 0x91, 0xad, 0xc5, 0xb0, 0xfe, 0x46), Name = "ИТ" } }; // 132b2a3c-7bc9-4a0f-8805-91adc5b0fe46

            #endregion

            #region Public Methods and Operators

            /// <inheritdoc />
            public ValueTask<IViewMetadata> GetMetadataAsync(CancellationToken cancellationToken = default) =>
                new ValueTask<IViewMetadata>(this.metadata);

            /// <summary>
            /// Выполняет получение данных из представления
            ///     на основании полученного <see cref="ITessaViewRequest">запроса</see>
            /// </summary>
            /// <param name="request">
            /// Запрос к представлению
            /// </param>
            /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
            /// <returns>
            /// <see cref="ITessaViewResult">Результат</see> выполнения запроса
            /// </returns>
            public async Task<ITessaViewResult> GetDataAsync(ITessaViewRequest request, CancellationToken cancellationToken = default)
            {
                Dbms dbms = await this.dbScope.GetDbmsAsync(cancellationToken);

                return new TessaViewResult
                {
                    Columns = new List<object> { "Name", "Count" },
                    Rows = GetRows(request),
                    SchemeTypes = new[] { SchemeType.String, SchemeType.Int32 },
                    DataTypes = new List<object>
                    {
                        this.schemeTypeConverter.TryGetSqlTypeName(SchemeType.String, dbms),
                        this.schemeTypeConverter.TryGetSqlTypeName(SchemeType.Int32, dbms),
                    },
                };
            }

            #endregion

            #region Methods

            /// <summary>
            /// The get count parameter value.
            /// </summary>
            /// <param name="request">
            /// The request.
            /// </param>
            /// <returns>
            /// The <see cref="int"/>.
            /// </returns>
            private static int GetCountParameterValue(ITessaViewRequest request)
            {
                var param = GetParameterFirstValue(request, "Count");
                if (param == null || param.Item2 == null || param.Item2.IsNull())
                {
                    return 0;
                }

                return (int) param.Item2.Value;
            }

            /// <summary>
            /// The get name parameter value.
            /// </summary>
            /// <param name="request">
            /// The request.
            /// </param>
            /// <returns>
            /// The <see cref="string"/>.
            /// </returns>
            private static string GetNameParameterValue(ITessaViewRequest request)
            {
                var param = GetParameterFirstValue(request, "Name");
                if (param == null || param.Item2 == null || param.Item2.IsNull())
                {
                    return string.Empty;
                }

                return param.Item2.Value?.ToString();
            }

            /// <summary>
            /// The get parameter first value.
            /// </summary>
            /// <param name="request">
            /// The request.
            /// </param>
            /// <param name="alias">
            /// The alias.
            /// </param>
            /// <returns>
            /// The <see cref="Tuple"/>.
            /// </returns>
            private static Tuple<string, CriteriaValue> GetParameterFirstValue(ITessaViewRequest request, string alias)
            {
                var param = request.Values?.FirstOrDefault(p => ParserNames.IsEquals(alias, p.Name));
                if (param == null)
                {
                    return null;
                }

                var criteria = param.CriteriaValues.FirstOrDefault();
                return criteria == null
                    ? null
                    : new Tuple<string, CriteriaValue>(criteria.CriteriaName, criteria.Values.FirstOrDefault());
            }

            /// <summary>
            /// The get rows.
            /// </summary>
            /// <param name="request">
            /// The request.
            /// </param>
            /// <returns>
            /// The <see cref="IList{T}"/>.
            /// </returns>
            private static IList<object> GetRows(ITessaViewRequest request)
            {
                var result = new List<object>();
                var count = GetCountParameterValue(request);
                var name = GetNameParameterValue(request);
                for (var i = 0; i < count; i++)
                {
                    result.Add(new List<object> { string.Format("{0}: {1}", name, i), i });
                }

                return result;
            }

            #endregion
        }
    }
}