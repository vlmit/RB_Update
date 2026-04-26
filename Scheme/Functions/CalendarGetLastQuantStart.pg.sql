CREATE FUNCTION "CalendarGetLastQuantStart"
(
	date_time timestamptz,
	days_to_add int
)
RETURNS timestamptz
AS $$
DECLARE
	first_quant_start timestamptz;
	last_quant_start timestamptz;
	next_day timestamptz;
BEGIN
	first_quant_start = "CalendarGetFirstQuantStart"(date_time, days_to_add);
	next_day = date_trunc('day', first_quant_start) + interval '1 day';
	
	SELECT "EndTime"
	INTO last_quant_start
	FROM "CalendarQuants"
	WHERE "EndTime" >= first_quant_start
		AND "EndTime" < next_day
		AND "Type" = false
	ORDER BY "EndTime" DESC
	LIMIT 1;

	IF last_quant_start IS NULL THEN
		RETURN date_time;
	END IF;
	RETURN last_quant_start;
END; $$
LANGUAGE PLPGSQL;