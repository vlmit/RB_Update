using System;
using System.Threading;
using System.Threading.Tasks;
using Tessa.BusinessCalendar;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared.Workflow.Wf;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.Workflow.Wf
{
    public class WfResolutionCheckSafeLimitStoreTaskExtension : CardStoreTaskExtension
    {
        #region Base Overrides

        public override async Task StoreTaskBeforeCommitTransaction(ICardStoreTaskExtensionContext context)
        {
            if (!context.ValidationResult.IsSuccessful())
            {
                return;
            }

            if (context.Task.Info.TryGetValue(WfHelper.CheckSafeLimitKey, out var objSafeChildResolutionTimeLimit) &&
                context.Task.Info.TryGetValue(WfHelper.ParentPlannedKey, out var objParentPlanned) &&
                context.Task.Info.TryGetValue(WfHelper.StoreDateTimeKey, out var objStoreDateTime))
            {
                var safeChildResolutionTimeLimit = (double)objSafeChildResolutionTimeLimit;
                var parentPlanned = (DateTime?)objParentPlanned;
                var storeDateTime = (DateTime)objStoreDateTime;

                TimeSpan clientUtcOffset = context.Session.ClientUtcOffset;

                DateTime utcParentPlanned = parentPlanned.Value.ToUniversalTime();
                DateTime utcParentPlannedOrCurrent = utcParentPlanned >= storeDateTime
                    ? utcParentPlanned
                    : storeDateTime;

                int? taskRoleUtcOffset;
                // Получаем смещение временной зоны роли, на которую назначено задание
                await using (context.DbScope.Create())
                {
                    var db = context.DbScope.Db;
                    var builder =
                        context.DbScope.BuilderFactory
                            .Select().Top(1).C("r", "TimeZoneUtcOffsetMinutes")
                            .From("Roles", "r").NoLock()
                            .Where().C("r", "ID").Equals().P("TaskRoleID")
                            .Limit(1);

                    taskRoleUtcOffset = await db
                        .SetCommand(
                            builder.Build(),
                            db.Parameter("TaskRoleID", context.Task.RoleID))
                        .LogCommand()
                        .ExecuteAsync<int?>(context.CancellationToken);
                }

                var offset = TimeSpan.FromMinutes(taskRoleUtcOffset ?? 0);

                DateTime? safeLimit = await this.TryGetDateTimeFromCalendarAsync(
                    context.DbScope,
                    utcParentPlannedOrCurrent,
                    offset,
                    safeChildResolutionTimeLimit,
                    context.ValidationResult,
                    clientUtcOffset,
                    cancellationToken: context.CancellationToken);

                if (!safeLimit.HasValue)
                {
                    context.ValidationResult.AddError(this, "$WfResolution_Error_ChildResolutionSafeLimitIsNull");
                }
                else if (context.Task.Planned > safeLimit.Value)
                {
                    context.ValidationResult.AddError(this,
                        "$WfResolution_Error_ChildResolutionCantBePlannedAfterParent",
                        FormattingHelper.FormatDateTimeWithoutSeconds(
                            context.Task.Planned + clientUtcOffset,
                            convertToLocal: false),
                        FormattingHelper.FormatDateTimeWithoutSeconds(
                            safeLimit.Value + clientUtcOffset,
                            convertToLocal: false),
                        FormattingHelper.FormatDateTimeWithoutSeconds(
                            utcParentPlanned + clientUtcOffset,
                            convertToLocal: false));
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Возвращает дату/время в UTC, полученные по бизнес-календарю, если к <paramref name="initialDateTime"/>
        /// прибавить количество бизнес-дней, указанных в <paramref name="duration"/>,
        /// или <c>null</c>, если календарь не рассчитан на этом диапазоне.
        /// </summary>
        /// <param name="dbScope">IDbScope</param>
        /// <param name="initialDateTime">Дата/время отсчёта. Переводится в UTC, если задана как локальное время.</param>
        /// <param name = "offset" > Смещение роли задания.</param>
        /// <param name="duration">
        /// Длительность в бизнес-днях, которую надо прибавить к дате/времени <paramref name="initialDateTime"/>.
        /// Может быть отрицательным числом или нулём, только если параметр <paramref name="positiveDurationOnly"/>
        /// равен <c>false</c>.
        /// </param>
        /// <param name="validationResult">IValidationResultBuilder</param>
        /// <param name="clientUtcOffset">Клиентское смещение времени</param>
        /// <param name="positiveDurationOnly">Признак</param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>
        /// Дата/время в UTC, полученная по бизнес-календарю в результате расчётов,
        /// или <c>null</c>, если календарь не рассчитан на этом диапазоне.
        /// </returns>
        protected async Task<DateTime?> TryGetDateTimeFromCalendarAsync(
            IDbScope dbScope,
            DateTime initialDateTime,
            TimeSpan offset,
            double duration,
            IValidationResultBuilder validationResult,
            TimeSpan clientUtcOffset,
            bool positiveDurationOnly = false,
            CancellationToken cancellationToken = default)
        {
            if (positiveDurationOnly && duration <= 0)
            {
                validationResult.AddError(this,
                    "$WfResolution_Error_TaskDurationCantBeZeroOrNegative",
                    FormattingHelper.FormatDoubleAsDecimal(duration, 1));

                return null;
            }

            if (duration.Equals(0.0))
            {
                return initialDateTime.ToUniversalTime();
            }

            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                var builder = dbScope.BuilderFactory
                    .Select().Top(1).C("q1", "StartTime")
                    .From("CalendarQuants", "q1").NoLock()
                    .InnerJoinLateral(b => b
                        .Select().Top(1).C("QuantNumber").C("Type")
                        .From("CalendarQuants").NoLock()
                        .Where()
                            .C("StartTime").LessOrEquals().P("InitialDateTime")
                        .OrderBy("StartTime", SortOrder.Descending)
                        .Limit(1),
                        "q2")
                    .Where()
                        .C("q1", "QuantNumber").Equals().C("q2", "QuantNumber").Add().P("Duration").Add()
                        .If(Dbms.SqlServer,
                            p => p.C("q2", "Type"))
                        .ElseIf(Dbms.PostgreSql,
                            p => p.C("q2", "Type").Q("::int"))
                        .ElseThrow()
                        .And()
                        .C("q1", "Type").Equals().V(false)
                    .Limit(1);

                DateTime? result = await db
                    .SetCommand(
                        builder.Build(),
                        db.Parameter("InitialDateTime", initialDateTime.ToUniversalTime() + offset),
                        db.Parameter("Duration", Math.Round(duration * TimeZonesHelper.QuantsInDay)))
                    .LogCommand()
                    .ExecuteAsync<DateTime?>(cancellationToken);

                if (!result.HasValue)
                {
                    validationResult.AddError(this,
                        "$WfResolution_Error_CantGetDateTimeFromCalendar",
                        FormattingHelper.FormatDateTimeWithoutSeconds(
                            initialDateTime + clientUtcOffset,
                            convertToLocal: false),
                        FormattingHelper.FormatDateTimeWithoutSeconds(initialDateTime, convertToLocal: false),
                        FormattingHelper.FormatDoubleAsDecimal(duration, 1));
                }

                return result - offset;
            }
        }

        #endregion
    }
}