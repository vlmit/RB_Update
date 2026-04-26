CREATE FUNCTION "CalendarGetExclusionPeriodInfo"
(
	for_timestamp timestamptz,
	OUT period_type bool,
	OUT period_end timestamptz
)
RETURNS record AS $$
BEGIN
	-- проверяем, попадает ли дата в исключения
	SELECT "EndTime", "Type"
	INTO period_end, period_type
	FROM "CalendarExclusions"
	WHERE "StartTime" <= for_timestamp AND "EndTime" > for_timestamp
	LIMIT 1;
	
	IF period_end IS NULL THEN
		-- дата не попадает в какое-то исключение
		-- ищем дату начала следующего исключения
		SELECT MIN("StartTime")
		INTO period_end
		FROM "CalendarExclusions"
		WHERE "StartTime" > for_timestamp;
	END IF;
END; $$
LANGUAGE PLPGSQL;