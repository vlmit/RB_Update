CREATE FUNCTION "CalendarAddWorkingDaysToDate"
(
	date_time timestamptz,
	days_to_add float
)
RETURNS timestamptz AS $$
DECLARE
	result_date timestamptz;
BEGIN
	SELECT "q1"."StartTime"
	INTO result_date
	FROM "CalendarQuants" AS "q1"
	INNER JOIN LATERAL (
		SELECT "q"."QuantNumber", "q"."Type"
		FROM "CalendarQuants" AS "q"
		WHERE "q"."StartTime" <= date_time
		ORDER BY "q"."StartTime" DESC
		LIMIT 1) AS q2 ON true
	WHERE "q1"."QuantNumber" = "q2"."QuantNumber" + "q2"."Type"::int + round(days_to_add * 32)
	    AND "q1"."Type" = false
	LIMIT 1;
	RETURN result_date;
END; $$
LANGUAGE PLPGSQL;