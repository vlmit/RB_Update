CREATE FUNCTION "ObtainProcessLock"
(
	processid uuid,
	retry_count int4,
	retry_timeout int4
)
RETURNS TABLE
(
	"Result" int
) AS $$
DECLARE
	counter int;
BEGIN
	BEGIN
		counter := 0;
		
		UPDATE "WorkflowEngineProcessesLocks"
		SET "Locked" = (current_timestamp at time zone 'UTC')
		WHERE "RowID" = processid AND "Locked" IS NULL;

		WHILE NOT FOUND LOOP
			counter := counter + 1;

			IF counter > retry_count THEN
				RETURN QUERY SELECT 1;
				RETURN;
			END IF;

			PERFORM pg_sleep(retry_timeout / 1000.0);
	
			UPDATE "WorkflowEngineProcessesLocks"
			SET "Locked" = (current_timestamp at time zone 'UTC')
			WHERE "RowID" = processid AND "Locked" IS NULL;
		END LOOP;
	EXCEPTION
		WHEN SQLSTATE '55P03' THEN
			RETURN QUERY SELECT 1;
			RETURN;
	END;
	
	RETURN QUERY SELECT 0;
END; $$
LANGUAGE PLPGSQL;