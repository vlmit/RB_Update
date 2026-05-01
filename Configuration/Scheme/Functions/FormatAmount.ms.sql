CREATE FUNCTION [FormatAmount]
(
	@amount decimal
)
RETURNS nvarchar(40)
WITH RETURNS NULL ON NULL INPUT
AS
BEGIN
	RETURN REPLACE(CONVERT(nvarchar, CAST(@amount AS money), 1), N',', char(160))
END;