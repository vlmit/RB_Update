CREATE FUNCTION "ObtainWriterLock2"
(
	id uuid,
	version int4,
	reader_retry_count int4,
	retry_timeout int4
)
RETURNS TABLE
(
	"Result" int,
	"Version" int,
	"ModifiedByID" uuid,
	"ModifiedByName" text,
	"Modified" timestamptz
) AS $$
DECLARE
	counter int;
BEGIN
	BEGIN
		counter := 0;

		WHILE (SELECT i."Readers" FROM "Instances" AS i WHERE "ID" = id LIMIT 1) > 0 LOOP
			PERFORM pg_sleep(retry_timeout / 1000.0);
			counter := counter + 1;

			IF counter > reader_retry_count THEN
				UPDATE "Instances"
				SET "WritePending" = false
				WHERE "ID" = id;
				
				RETURN QUERY SELECT 2, NULL::int4, NULL::uuid, NULL::text, NULL::timestamptz;
				RETURN;
			END IF;
		END LOOP;
	EXCEPTION
		WHEN SQLSTATE '55P03' THEN
			UPDATE "Instances"
			SET "WritePending" = false
			WHERE "ID" = id;
			
			RETURN QUERY SELECT 4, NULL::int4, NULL::uuid, NULL::text, NULL::timestamptz;
			RETURN;
	END;

	IF (SELECT i."Version" FROM "Instances" AS i WHERE "ID" = id LIMIT 1) <> version THEN
		RETURN QUERY SELECT 5, i."Version", i."ModifiedByID", i."ModifiedByName", i."Modified"
		FROM "Instances" AS i
		WHERE "ID" = id;
	
		UPDATE "Instances"
		SET "WritePending" = false
		WHERE "ID" = id;

		RETURN;
	END IF;
	
	RETURN QUERY SELECT 0, NULL::int4, NULL::uuid, NULL::text, NULL::timestamptz;
END; $$
LANGUAGE PLPGSQL;