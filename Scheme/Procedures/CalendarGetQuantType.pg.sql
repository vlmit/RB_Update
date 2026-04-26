CREATE FUNCTION "CalendarGetQuantType"
(
	for_timestamp timestamptz, -- дата, для которой необходимо определить тип (должна соответствовать врменени начала кванта)
	work_day_start_time timetz, -- время начала рабочего дня.
	work_day_end_time timetz, -- время конца рабочего дня.
	launch_start_time timetz, -- время начала обеда.
	launch_end_time timetz, -- время конца обеда.
	OUT period_type bool, -- тип кванта
	OUT period_end timestamptz -- дата окончания кванта
)
RETURNS record AS $$
DECLARE
	next_period_type bool;
	next_period_end timestamptz;
BEGIN 
	SELECT *
	INTO period_type, period_end
	FROM "CalendarGetPeriodInfo"(for_timestamp, work_day_start_time, work_day_end_time, launch_start_time, launch_end_time);

	SELECT *
	INTO next_period_type, next_period_end
	FROM "CalendarGetPeriodInfo"(period_end, work_day_start_time, work_day_end_time, launch_start_time, launch_end_time);
	
	WHILE next_period_type = period_type LOOP
		period_end = next_period_end;

		SELECT *
		INTO next_period_type, next_period_end
		FROM "CalendarGetPeriodInfo"(period_end, work_day_start_time, work_day_end_time, launch_start_time, launch_end_time);
	END LOOP;
END; $$
LANGUAGE PLPGSQL;