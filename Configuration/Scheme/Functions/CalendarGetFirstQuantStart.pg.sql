CREATE FUNCTION "CalendarGetFirstQuantStart"
(
	date_time timestamptz,
	days_to_add int
)
RETURNS timestamptz
AS $$
DECLARE
	first_quant_number bigint;
	first_quant_start timestamptz;
BEGIN
	SELECT "QuantNumber"
	INTO first_quant_number
	FROM "CalendarQuants"
	WHERE "StartTime" >= date_trunc('day', date_time)
		AND "Type" = false
	ORDER BY "StartTime"
	LIMIT 1;

	SELECT "StartTime"
	INTO first_quant_start
	FROM "CalendarQuants"
	WHERE "QuantNumber" >= first_quant_number + (32 * days_to_add)
	    AND "Type" = false
	ORDER BY "QuantNumber"
	LIMIT 1;

	IF first_quant_start IS NULL THEN
		RETURN date_time;
	END IF;
	RETURN first_quant_start;
END; $$
LANGUAGE PLPGSQL;