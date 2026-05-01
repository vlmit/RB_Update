CREATE FUNCTION "ObtainWriterLock"
(
	id uuid,
	writer_retry_count int4,
	retry_timeout int4
)
RETURNS INT
AS $$
DECLARE
	counter int;
BEGIN
	BEGIN
		IF NOT EXISTS (SELECT NULL FROM "Instances" WHERE "ID" = id) THEN
			RETURN 3;
		END IF;

		counter := 0;
		
		-- UPDATE might take a lock, even though it returns 0 rows (FOUND = false)
		-- Emulate a rollback to a savepoint, so the lock is released
		BEGIN
			UPDATE "Instances"
			SET "WritePending" = true
			WHERE "ID" = id AND "WritePending" = false;
			
			IF NOT FOUND THEN
				RAISE SQLSTATE 'TESSA';
			END IF;
			
		EXCEPTION WHEN SQLSTATE 'TESSA' THEN
			NULL;
		END;

		WHILE NOT FOUND LOOP
			counter := counter + 1;

			IF counter > writer_retry_count THEN
				RETURN 1;
			END IF;

			PERFORM pg_sleep(retry_timeout / 1000.0);
			-- Perform sets FOUND to true because pg_sleep returns a row
			FOUND:= false;
			
			BEGIN
				UPDATE "Instances"
				SET "WritePending" = true
				WHERE "ID" = id AND "WritePending" = false;
				
				IF NOT FOUND THEN
					RAISE SQLSTATE 'TESSA';
				END IF;
			
			EXCEPTION WHEN SQLSTATE 'TESSA' THEN
				NULL;
			END;
			
		END LOOP;
	EXCEPTION
		WHEN SQLSTATE '55P03' THEN
			RETURN 4;
	END;
	
	RETURN 0;
END; $$
LANGUAGE PLPGSQL;