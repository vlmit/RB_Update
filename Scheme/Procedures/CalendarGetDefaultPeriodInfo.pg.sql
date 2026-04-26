CREATE FUNCTION "CalendarGetDefaultPeriodInfo"
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
	work_day_begin timestamptz;
	work_day_end timestamptz;
	dinner_begin timestamptz;
	dinner_end timestamptz;
	period_start timestamptz;
BEGIN
	-- рабочий день
	work_day_begin = for_timestamp::date + work_day_start_time;
	work_day_end = for_timestamp::date + work_day_end_time;
	dinner_begin = for_timestamp::date + launch_start_time;
	dinner_end = for_timestamp::date + launch_end_time;

	IF "CalendarGetDayOfWeek"(for_timestamp) BETWEEN 1 AND 5 THEN
		IF for_timestamp < work_day_begin THEN
			period_type = true; -- не рабочий
			period_end = work_day_begin;
			RETURN;
		END IF;

		IF for_timestamp < dinner_begin THEN
			period_type = false; -- рабочий
			period_end = dinner_begin;
			RETURN;
		END IF;

		IF for_timestamp < dinner_end THEN
			period_type = true; -- не рабочий
			period_end = dinner_end;
			RETURN;
		END IF;

		IF for_timestamp < work_day_end THEN
			period_type = false; -- рабочий
			period_end = work_day_end;
			RETURN;
		END IF;
	END IF;
	
	-- текущий день - выходной или рабочий, но уже закончился
	period_type = true; -- не рабочий
	-- ищем следующий рабочий и берём его начало
	for_timestamp = for_timestamp + interval '1 day';

	WHILE NOT "CalendarGetDayOfWeek"(for_timestamp) BETWEEN 1 AND 5 LOOP
		for_timestamp = for_timestamp + interval '1 day';
	END LOOP;

	IF work_day_begin < dinner_begin THEN
		period_start = work_day_begin;
	END IF;

	IF dinner_begin <= work_day_begin THEN
		period_start = dinner_begin;
	END IF;

	period_end = for_timestamp::date + make_interval(hours => date_part('hour', period_start)::int);
END; $$
LANGUAGE PLPGSQL;