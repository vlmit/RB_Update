CREATE FUNCTION "CalendarGetPeriodInfo"
(
	for_timestamp timestamptz,
	work_day_start_time timetz, -- время начала рабочего дня.
	work_day_end_time timetz, -- время конца рабочего дня.
	launch_start_time timetz, -- время начала обеда.
	launch_end_time timetz, -- время конца обеда.
	OUT period_type bool,
	OUT period_end timestamptz
)
RETURNS record AS $$
DECLARE
	default_period_type bool;
	default_period_end timestamptz;
	exclusion_period_type bool;
	exclusion_period_end timestamptz;
BEGIN
	SELECT *
	INTO exclusion_period_type, exclusion_period_end
	FROM "CalendarGetExclusionPeriodInfo"(for_timestamp);

	IF exclusion_period_type IS NOT NULL THEN
		-- на эту дату попадает исключение
		period_type = exclusion_period_type;
		period_end = exclusion_period_end;
		RETURN;
	END IF;

	SELECT *
	INTO default_period_type, default_period_end
	FROM "CalendarGetDefaultPeriodInfo"(for_timestamp, work_day_start_time, work_day_end_time, launch_start_time, launch_end_time);
	
	period_type = default_period_type;

	IF exclusion_period_end < default_period_end THEN
		-- следующее исключение начнётся раньше, чем закончится текущий стандартный период
		period_end = exclusion_period_end;
	ELSE
		period_end = default_period_end;
	END IF;
END; $$
LANGUAGE PLPGSQL;