DO $$
BEGIN
	IF NOT EXISTS ( SELECT NULL FROM pg_ts_config WHERE cfgname = 'tessa') 
	THEN
		CREATE TEXT SEARCH CONFIGURATION public.tessa ( COPY = pg_catalog.russian );
	END IF;
END; $$
LANGUAGE PLPGSQL;