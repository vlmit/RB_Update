using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tessa.BusinessCalendar;
using Tessa.Cards;
using Tessa.Cards.ComponentModel;
using Tessa.Cards.Extensions;
using Tessa.Localization;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.BusinessCalendar
{
    public sealed class CalendarStoreExtension :
        CardStoreExtension
    {
        #region Constructors

        public CalendarStoreExtension(
            IBusinessCalendarService businessCalendarService,
            ICardGetStrategy getStrategy,
            ICardTransactionStrategy transactionStrategy,
            IDbScope dbScope)
        {
            this.businessCalendarService = businessCalendarService;
            this.getStrategy = getStrategy;
            this.transactionStrategy = transactionStrategy;
            this.dbScope = dbScope;
        }

        #endregion

        #region Fields

        private readonly IBusinessCalendarService businessCalendarService;

        private readonly ICardGetStrategy getStrategy;

        private readonly ICardTransactionStrategy transactionStrategy;

        private readonly IDbScope dbScope;

        #endregion

        #region Base Overrides

        public override Task BeforeRequest(ICardStoreExtensionContext context)
        {
            Card card;
            StringDictionaryStorage<CardSection> sections;
            if (!context.ValidationResult.IsSuccessful()
                || (card = context.Request.TryGetCard()) == null
                || (sections = card.TryGetSections()) == null
                || !sections.TryGetValue("CalendarSettings", out CardSection settings))
            {
                return Task.CompletedTask;
            }

            Dictionary<string, object> settingsFields = settings.RawFields;

            // в полях задаётся дата и время, но поля редактируются как даты
            // поэтому для даты начала всегда указываем время 00:00:00, а для даты окончания - 23:59:59

            if (settingsFields.ContainsKey("CalendarStart"))
            {
                settingsFields["CalendarStart"] = settingsFields.Get<DateTime>("CalendarStart")
                    .Date;
            }

            if (settingsFields.ContainsKey("CalendarEnd"))
            {
                settingsFields["CalendarEnd"] = settingsFields.Get<DateTime>("CalendarEnd")
                    .Date
                    .AddDays(1.0)
                    .AddSeconds(-1.0);
            }

            //Если какое-то из значений выходит за пределы SQLDateTime - нужно это исправить
            DateTime min = new DateTime(1753, 1, 1);
            if (settingsFields.ContainsKey("WorkDayStart"))
            {
                var time = settingsFields.Get<DateTime>("WorkDayStart");
                if (time.ToUniversalTime() < min)
                {
                    settingsFields["WorkDayStart"] = min.Date.Add(time.TimeOfDay);
                }
            }
            if (settingsFields.ContainsKey("WorkDayEnd"))
            {
                var time = settingsFields.Get<DateTime>("WorkDayEnd");
                if (time.ToUniversalTime() < min)
                {
                    settingsFields["WorkDayEnd"] = min.Date.Add(time.TimeOfDay);
                }
            }
            if (settingsFields.ContainsKey("LunchStart"))
            {
                var time = settingsFields.Get<DateTime>("LunchStart");
                if (time.ToUniversalTime() < min)
                {
                    settingsFields["LunchStart"] = min.Date.Add(time.TimeOfDay);
                }
            }
            if (settingsFields.ContainsKey("LunchEnd"))
            {
                var time = settingsFields.Get<DateTime>("LunchEnd");
                if (time.ToUniversalTime() < min)
                {
                    settingsFields["LunchEnd"] = min.Date.Add(time.TimeOfDay);
                }
            }

            return Task.CompletedTask;
        }


        public override async Task AfterRequest(ICardStoreExtensionContext context)
        {
            Guid? operationGuid;

            Dictionary<string, object> info;
            if (!context.RequestIsSuccessful
                || (info = context.Request.TryGetInfo()) == null
                || !(operationGuid = info.TryGet<Guid?>(BusinessCalendarHelper.RebuildOperationGuidKey)).HasValue)
            {
                return;
            }

            Card card = null;
            bool succeeded = await this.transactionStrategy
                .ExecuteInReaderLockAsync(
                    context.Request.Card.ID,
                    context.ValidationResult,
                    async p =>
                    {
                        CardGetContext getContext = await this.getStrategy
                            .TryLoadCardInstanceAsync(
                                context.Request.Card.ID,
                                p.DbScope.Db,
                                context.CardMetadata,
                                p.ValidationResult,
                                cancellationToken: p.CancellationToken);

                        await this.getStrategy.LoadSectionsAsync(getContext, p.CancellationToken);

                        if (getContext != null)
                        {
                            card = getContext.Card;
                        }
                    },
                    context.CancellationToken);

            if (!succeeded || card == null)
            {
                return;
            }

            Dictionary<string, object> sectionFields = card.Sections["CalendarSettings"].RawFields;
            DateTime dateTimeStart = sectionFields.Get<DateTime>("CalendarStart").ToUniversalTime();
            DateTime dateTimeEnd = sectionFields.Get<DateTime>("CalendarEnd").ToUniversalTime();
            DateTime workTimeStart = sectionFields.Get<DateTime>("WorkDayStart").ToUniversalTime();
            DateTime workTimeEnd = sectionFields.Get<DateTime>("WorkDayEnd").ToUniversalTime();
            DateTime lunchTimeStart = sectionFields.Get<DateTime>("LunchStart").ToUniversalTime();
            DateTime lunchTimeEnd = sectionFields.Get<DateTime>("LunchEnd").ToUniversalTime();

            await this.businessCalendarService.RebuildCalendarAsync(
                operationGuid.Value,
                dateTimeStart,
                dateTimeEnd,
                workTimeStart,
                workTimeEnd,
                lunchTimeStart,
                lunchTimeEnd,
                context.CancellationToken);
        }

        public override async Task BeforeCommitTransaction(ICardStoreExtensionContext context)
        {
            if (context.Request.Card.Sections.TryGetValue("CalendarExclusions", out CardSection exclusionSection))
            {
                if (exclusionSection.Rows.Count > 0)
                {
                    var rowIDList = exclusionSection.Rows.Select(x => x.RowID).ToArray();
                    await using (this.dbScope.Create())
                    {
                        DbManager db = this.dbScope.Db;
                        db
                            .SetCommand(this.dbScope.BuilderFactory
                                .Select().C(null, "StartTime", "EndTime")
                                .From("CalendarExclusions").NoLock()
                                .Where().C("RowID").In(rowIDList)
                                .Build())
                            .LogCommand();

                        await using var reader = await db.ExecuteReaderAsync(context.CancellationToken);
                        while (await reader.ReadAsync(context.CancellationToken))
                        {
                            var startTime = reader.GetDateTimeUtc(0);
                            var endTime = reader.GetDateTimeUtc(1);
                            if (startTime > endTime)
                            {
                                context.ValidationResult.AddError(this,
                                    LocalizationManager.LocalizeAndEscapeFormat("$BusinessCalendar_Exclusions_EndDateMustBeGEThenStartDate"),
                                    Environment.NewLine, startTime, endTime);

                                return;
                            }
                        }
                    }
                }
            }

            if (context.Request.Card.Sections.TryGetValue("CalendarSettings", out _))
            {
                await using (this.dbScope.Create())
                {
                    DbManager db = this.dbScope.Db;

                    db
                        .SetCommand(this.dbScope.BuilderFactory
                            .Select().C(null, "CalendarStart", "CalendarEnd", "WorkDayStart", "WorkDayEnd", "LunchStart", "LunchEnd")
                            .From("CalendarSettings").NoLock()
                            .Where().C("ID").Equals().P("ID")
                            .Build(),
                            db.Parameter("ID", context.Request.Card.ID))
                        .LogCommand();

                    await using var reader = await db.ExecuteReaderAsync(context.CancellationToken);
                    if (await reader.ReadAsync(context.CancellationToken))
                    {
                        var calendarStart = reader.GetDateTimeUtc(0);
                        var calendarEnd = reader.GetDateTimeUtc(1);

                        if (calendarStart >= calendarEnd)
                        {
                            context.ValidationResult.AddError(this,
                                LocalizationManager.LocalizeAndEscapeFormat("$BusinessCalendar_EndDateMustBeGEThenStartDate"),
                                Environment.NewLine, calendarStart, calendarEnd);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
