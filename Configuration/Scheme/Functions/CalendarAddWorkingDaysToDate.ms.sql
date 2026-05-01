CREATE FUNCTION [CalendarAddWorkingDaysToDate]
(
	@StartDateTime datetime,
	@Offset float
)
RETURNS datetime AS
BEGIN
	DECLARE @ResultDate datetime	
	SELECT TOP 1 @ResultDate = q1.StartTime
	  FROM CalendarQuants q1 WITH(NOLOCK)
	 CROSS APPLY(
	 	SELECT TOP 1 q.QuantNumber, q.Type
	 	FROM CalendarQuants q with(nolock)
		WHERE q.StartTime <= @StartDateTime 
		ORDER BY q.StartTime DESC) q2
	WHERE q1.QuantNumber = q2.QuantNumber + q2.Type + round(@Offset * 32, 0)
	     AND q1.Type = 0
	RETURN @ResultDate
END;