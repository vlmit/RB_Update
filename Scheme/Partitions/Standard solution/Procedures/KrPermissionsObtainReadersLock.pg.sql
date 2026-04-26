CREATE FUNCTION "KrPermissionsObtainReadersLock"
(
	retry_count int,
	retry_timeout int
)
RETURNS INT
AS $$
DECLARE
	counter int;
BEGIN
	BEGIN
		counter := 0;

		-- UPDATE might take a lock, even though it returns 0 rows (FOUND = false)
		-- Emulate a rollback to a savepoint, so the lock is released
		BEGIN
			UPDATE "KrPermissionsSystem"
			SET "Readers" = "Readers" + 1
			WHERE "Writers" = 0;
			
			IF NOT FOUND THEN
				RAISE SQLSTATE 'TESSA';
			END IF;

		EXCEPTION WHEN SQLSTATE 'TESSA' THEN
			NULL;
		END;

		WHILE NOT FOUND LOOP
			counter := counter + 1;

			IF counter > retry_count THEN
				RETURN 1;
			END IF;

			PERFORM pg_sleep(retry_timeout / 1000.0);

			-- Perform sets FOUND to true because pg_sleep returns a row
			FOUND:= false;

			BEGIN
				UPDATE "KrPermissionsSystem"
				SET "Readers" = "Readers" + 1
				WHERE "Writers" = 0;

				IF NOT FOUND THEN
					RAISE SQLSTATE 'TESSA';
				END IF;

			EXCEPTION WHEN SQLSTATE 'TESSA' THEN
				NULL;
			END;
		END LOOP;
	EXCEPTION
		WHEN SQLSTATE '55P03' THEN
			RETURN 1;
	END;

	RETURN 0;
END; $$
LANGUAGE PLPGSQL;