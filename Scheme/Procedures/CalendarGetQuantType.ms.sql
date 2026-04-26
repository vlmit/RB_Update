CREATE PROCEDURE [CalendarGetQuantType]
(
	@QuantDate datetime, -- дата, для которой необходимо определить тип (должна соответствовать врменени начала кванта)
	@WorkDayStartTime datetime, -- время начала рабочего дня.
	@WorkDayEndTime datetime, -- время конца рабочего дня.
	@LaunchStartTime datetime, -- время начала обеда.
	@LaunchEndTime datetime, -- время конца обеда.
	@Type bit OUTPUT, -- тип кванта
	@QuantEndDate datetime OUTPUT -- дата окончания кванта

)
AS 
BEGIN 
	SET NOCOUNT ON;
	EXEC [dbo].[CalendarGetPeriodInfo] @QuantDate, @WorkDayStartTime, @WorkDayEndTime, @LaunchStartTime, @LaunchEndTime, @Type OUT, @QuantEndDate OUT
	
	DECLARE @NextPeriodType bit
	DECLARE @NextPeriodEnd datetime
	EXEC [dbo].[CalendarGetPeriodInfo] @QuantEndDate, @WorkDayStartTime, @WorkDayEndTime, @LaunchStartTime, @LaunchEndTime, @NextPeriodType OUT, @NextPeriodEnd OUT
	WHILE @NextPeriodType = @Type
	BEGIN
		SET @QuantEndDate = @NextPeriodEnd
		EXEC [dbo].[CalendarGetPeriodInfo] @QuantEndDate, @WorkDayStartTime, @WorkDayEndTime, @LaunchStartTime, @LaunchEndTime, @NextPeriodType OUT, @NextPeriodEnd OUT
	END
END