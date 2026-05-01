CREATE FUNCTION [CalendarGetLastQuantStart]
(
	@BaseDateTime datetime,
	@Offset int
)
RETURNS datetime
AS
BEGIN
	DECLARE @FirstQuantStart datetime
	SELECT @FirstQuantStart = [dbo].[CalendarGetFirstQuantStart](@BaseDateTime , @Offset)
	DECLARE @NextDay datetime
	SELECT @NextDay = DATEADD(Day, 1,CAST(CAST(@FirstQuantStart AS DATE) AS DATETIME))

    	DECLARE @LastQuantStart datetime
	SELECT TOP(1) @LastQuantStart = EndTime
	FROM [dbo].[CalendarQuants] WITH (NOLOCK)
	WHERE EndTime >= @FirstQuantStart
	    AND EndTime < @NextDay
      	    AND Type = 0
   	 ORDER BY EndTime DESC

	IF @LastQuantStart IS NULL
	BEGIN
		RETURN @BaseDateTime 
	END
	RETURN @LastQuantStart 
END;