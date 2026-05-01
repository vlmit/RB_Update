CREATE FUNCTION "CalendarPrepareQuants"
(
	period_start_time timestamptz, -- дата\время, начиная с которой должны быть перерасчитаны кванты
	period_end_time timestamptz, -- дата\время, до которой должные быть перерасчитаны кванты.
	work_day_start_time timetz, -- время начала рабочего дня.
	work_day_end_time timetz, -- время конца рабочего дня.
	launch_start_time timetz, -- время начала обеда.
	launch_end_time timetz -- время конца обеда.
)
RETURNS void AS $$
DECLARE
	current_period_start timestamptz;
	current_quant_number int4;
	current_period_type bool;
	current_period_end timestamptz;
	temp_period_start timestamptz;
	temp_period_end timestamptz;
	last_period_end timestamptz;
BEGIN
	TRUNCATE "CalendarQuants";

	-- Нулевой квант не существует - создаём
	INSERT INTO "CalendarQuants"
		("QuantNumber", "StartTime", "EndTime", "Type")
	VALUES
		(0, period_start_time - interval '5 years', period_start_time, true);

	current_period_start = period_start_time;
	current_quant_number = 0;
	
	WHILE current_period_start < period_end_time LOOP
		SELECT *
		INTO current_period_type, current_period_end
		FROM "CalendarGetQuantType"(current_period_start, work_day_start_time, work_day_end_time, launch_start_time, launch_end_time);

		IF current_period_type = false THEN
			temp_period_start = current_period_start;
			temp_period_end = temp_period_start + interval '15 minutes';

			WHILE temp_period_end <= current_period_end LOOP
				current_quant_number = current_quant_number + 1;

				INSERT INTO "CalendarQuants"
					("QuantNumber", "StartTime", "EndTime", "Type")
				VALUES
					(current_quant_number, temp_period_start, temp_period_end, current_period_type);

				temp_period_start = temp_period_end;
				temp_period_end = temp_period_start + interval '15 minutes';
			END LOOP;
		ELSE
			INSERT INTO "CalendarQuants"
				("QuantNumber", "StartTime", "EndTime", "Type")
			VALUES
				(current_quant_number, current_period_start, current_period_end, current_period_type);
		END IF;
		
		current_period_start = current_period_end;
	END LOOP;
	
	SELECT MAX("EndTime")
	INTO last_period_end
	FROM "CalendarQuants";
	
	-- вставляем заключительный квант
	INSERT INTO "CalendarQuants"
		("QuantNumber", "StartTime", "EndTime", "Type")
	VALUES
		(current_quant_number, last_period_end, last_period_end + interval '5 years', true);
END; $$
LANGUAGE PLPGSQL;