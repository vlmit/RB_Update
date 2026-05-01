CREATE FUNCTION [CalendarGetFirstQuantStart]
(
	@StartDateTime datetime,
	@Offset int
)
RETURNS datetime
AS
BEGIN
	DECLARE @FirstQuantStart datetime
	DECLARE @DateFirstQuantNumber datetime
	SELECT TOP(1) @DateFirstQuantNumber = QuantNumber
	FROM [dbo].[CalendarQuants] WITH (NOLOCK)
	WHERE StartTime >= CAST(CAST(@StartDateTime AS DATE) AS DATETIME)
      	    AND Type = 0
              ORDER BY StartTime

	SELECT TOP(1) @FirstQuantStart = StartTime
	FROM [dbo].[CalendarQuants] WITH (NOLOCK)
	WHERE QuantNumber >= @DateFirstQuantNumber + (32*@Offset )
	    AND Type = 0
	ORDER BY QuantNumber

	IF @FirstQuantStart IS NULL
	BEGIN
		RETURN @StartDateTime 
	END
	RETURN @FirstQuantStart
END;