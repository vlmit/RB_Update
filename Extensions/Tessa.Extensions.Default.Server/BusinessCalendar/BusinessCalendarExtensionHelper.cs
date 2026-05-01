using System;
using Tessa.Cards;
using Tessa.Platform.Runtime;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Server.BusinessCalendar
{
    public static class BusinessCalendarExtensionHelper
    {
        #region Base Overrides

        /// <summary>
        /// Автоматически проставляем дату и время для вновь добавленных строк в таблицу исключений.
        /// Это нужно для удобства.
        /// </summary>
        /// <param name="response">Ответ на запрос на создание или получение карточки календаря.</param>
        /// <param name="session">Сессия текущего пользователя.</param>
        public static void SetupCalendarResponse(
            CardValueResponseBase response,
            ISession session)
        {
            StringDictionaryStorage<CardRow> sectionRows;
            if ((sectionRows = response.TryGetSectionRows()) == null
                || !sectionRows.TryGetValue("CalendarExclusions", out CardRow exclusionRow))
            {
                return;
            }

            // время должно быть 00:00:00, на клиенте оно отображается сразу в UTC;
            // в качестве текущей даты мы берём ту дату, которая является текущей сейчас на клиенте, отбрасываем время, и представляем как UTC
            DateTime nowDate = DateTime.SpecifyKind((DateTime.UtcNow + session.ClientUtcOffset).Date, DateTimeKind.Utc);

            exclusionRow["StartTime"] = nowDate;
            exclusionRow["EndTime"] = nowDate.AddDays(1.0).AddSeconds(-1.0); // плюс 23:59:59
        }

        #endregion
    }
}
