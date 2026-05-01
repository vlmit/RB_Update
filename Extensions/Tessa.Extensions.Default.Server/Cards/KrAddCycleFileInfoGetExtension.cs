using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform.Data;

namespace Tessa.Extensions.Default.Server.Cards
{
    public class KrAddCycleFileInfoGetExtension : CardGetExtension
    {
        #region Fields

        private readonly IKrTypesCache krTypesCache;

        #endregion

        #region Constructors

        public KrAddCycleFileInfoGetExtension(IKrTypesCache krTypesCache)
        {
            this.krTypesCache = krTypesCache;
        }

        #endregion

        #region Base Overrides

        public override async Task AfterRequest(ICardGetExtensionContext context)
        {
            Card card;
            if (!context.RequestIsSuccessful ||
                context.Request.ServiceType == CardServiceType.Default ||
                !context.ValidationResult.IsSuccessful() ||
                context.Request.RestrictionFlags.Has(CardGetRestrictionFlags.RestrictFiles) ||
                (card = context.Response.TryGetCard()) == null ||
                !(await KrComponentsHelper.GetKrComponentsAsync(card, this.krTypesCache, context.CancellationToken)).Has(KrComponents.Routes))
            {
                return;
            }

            var cardID = context.Response.Card.ID;

            var ignoreTypes = new[]
            {
                new Guid(0x75af7df8, 0xeeb9, 0x4d5d, 0x8a, 0xf9, 0x53, 0x82, 0x15, 0x9f, 0xdb, 0xb2), // 75af7df8-eeb9-4d5d-8af9-5382159fdbb2
                new Guid(0xfc6f6e71, 0xcb9c, 0x4902, 0xb9, 0xf5, 0xd6, 0xba, 0x22, 0x31, 0x74, 0xd8), // fc6f6e71-cb9c-4902-b9f5-d6ba223174d8
                DefaultTaskTypes.KrRequestCommentTypeID,
                DefaultTaskTypes.KrInfoRequestCommentTypeID
            };

            
            await using (context.DbScope.Create())
            {
                var db = context.DbScope.Db;

                var builder =
                    context.DbScope.BuilderFactory
                        .Select()
                        .C("f", "RowID")
                        .C("th", "Cycle")
                        .From("Files", "f").NoLock()
                        .InnerJoinLateral(b => b
                            .Select().Top(1)
                            .C("c", "Cycle")
                            .From(c => c
                                    .Select()
                                    .Min("th", "Created").As("CycleStart")
                                    .Max(e => e.Coalesce(q => q.C("th", "Completed").P("DateTimeNow"))).As("CycleEnd")
                                    .C("kr", "Cycle")
                                    .From("TaskHistory", "th").NoLock()
                                    .InnerJoin("KrApprovalHistory", "kr").NoLock()
                                    .On().C("th", "RowID").Equals().C("kr", "HistoryRecord")
                                    .Where()
                                    .C("f", "ID").Equals().C("th", "ID")
                                    .And()
                                    .C("th", "TypeID").NotIn(ignoreTypes)
                                    .And()
                                    .Not().E(q =>
                                        q.C("th", "TypeID").Equals().V(DefaultTaskTypes.KrInfoAdditionalApprovalTypeID)
                                            .And().C("th", "OptionID").Equals().V(DefaultCompletionOptions.AdditionalApproval))
                                    .GroupBy("kr", "Cycle")
                                , "c")
                            .Where()
                            .C("f", "Created").GreaterOrEquals().C("c", "CycleStart")
                            .And()
                            .C("f", "Created").LessOrEquals().C("c", "CycleEnd")
                            .Limit(1), "th")
                        .Where()
                        .C("f", "ID").Equals().P("ID")
                        .And()
                        .C("th", "Cycle").IsNotNull()
                        .And()
                        .C("f", "OriginalFileID").IsNotNull()
                        .OrderBy("th", "Cycle", SortOrder.Descending)
                        .By("f", "Created", SortOrder.Descending)
                        .By("f", "RowID", SortOrder.Ascending);

                await db.SetCommand(
                        builder.Build(),
                        db.Parameter("ID", cardID),
                        db.Parameter("DateTimeNow", DateTime.UtcNow))
                    .LogCommand()
                    .ExecuteNonQueryAsync(context.CancellationToken);

                var filesList = new Dictionary<string, object>();

                int maxCycleNumber = 0;
                await using (var reader = await db.ExecuteReaderAsync(context.CancellationToken))
                {
                    while (await reader.ReadAsync(context.CancellationToken))
                    {
                        Guid fileID = reader.GetGuid(0);
                        short cycle = reader.GetValue<short>(1);

                        if (cycle > maxCycleNumber)
                        {
                            maxCycleNumber = cycle;
                        }

                        filesList.Add(fileID.ToString(), (int) cycle);
                    }
                }

                context.Response.Card.Info[CycleGroupingInfoKeys.FilesByCyclesKey] = filesList;
                context.Response.Card.Info[CycleGroupingInfoKeys.MaxCycleNumberKey] = maxCycleNumber;
            }
        }

        #endregion
    }
}