DO $$
BEGIN
	IF NOT EXISTS (SELECT NULL FROM "KrPermissionsSystem") 
	THEN
		INSERT INTO "KrPermissionsSystem" ("Version", "Readers", "Writers")
		SELECT 0, 0, 0;
	END IF;
END; $$
LANGUAGE PLPGSQL;