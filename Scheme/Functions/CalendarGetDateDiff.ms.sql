CREATE FUNCTION [CalendarGetDateDiff]
(
	@FirstDate datetime, 
	@SecondDate datetime
)
RETURNS bigint
AS
BEGIN
	DECLARE @FirstQuant bigint
	SELECT TOP(1) @FirstQuant = QuantNumber 
	FROM [dbo].[CalendarQuants] WITH (NOLOCK)
	WHERE StartTime <= @FirstDate
	ORDER BY StartTime DESC
	
	DECLARE @SecondQuant bigint
	SELECT TOP(1) @SecondQuant = QuantNumber 
	FROM [dbo].[CalendarQuants] WITH (NOLOCK)
	WHERE StartTime <= @SecondDate
	ORDER BY StartTime DESC
	
	IF @FirstQuant IS NULL OR @SecondQuant IS NULL
	BEGIN
		RETURN 0
	END
	RETURN @SecondQuant - @FirstQuant
END