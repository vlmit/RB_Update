CREATE PROCEDURE [CalendarGetDefaultPeriodInfo]
(
	@Date datetime,
	@WorkDayStartTime datetime, -- время начала рабочего дня.
	@WorkDayEndTime datetime, -- время конца рабочего дня.
	@LaunchStartTime datetime, -- время начала обеда.
	@LaunchEndTime datetime, -- время конца обеда.
	@Type bit OUTPUT,
	@EndDate datetime OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON;

	-- рабочий день
	DECLARE @WorkDayBegin datetime
	SET @WorkDayBegin = DATEADD(minute, DATEPART(minute, @WorkDayStartTime),DATEADD(HOUR, DATEPART(hour, @WorkDayStartTime), DATEADD(dd, 0, DATEDIFF(dd, 0, @Date))))
	DECLARE @WorkDayEnd datetime
	SET @WorkDayEnd = DATEADD(minute, DATEPART(minute, @WorkDayEndTime),DATEADD(HOUR, DATEPART(hour, @WorkDayEndTime), DATEADD(dd, 0, DATEDIFF(dd, 0, @Date))))
	
	if (@WorkDayStartTime > @WorkDayEndTime and 
		cast(@Date as time) >= cast('00:00:00' as time) and 
		cast(@Date as time) <= cast(@WorkDayEndTime as time))
	BEGIN
		SET @WorkDayBegin = DATEADD(day, -1, @WorkDayBegin)
	END

	if (@WorkDayStartTime > @WorkDayEndTime and 
		cast(@Date as time) <= cast('23:59:59' as time) and 
		cast(@Date as time) >= cast(@WorkDayStartTime as time))
	BEGIN
		SET @WorkDayEnd = DATEADD(day, 1, @WorkDayEnd)
	END
		
	DECLARE @WorkDayDinnerBegin datetime
	SET @WorkDayDinnerBegin = DATEADD(minute, DATEPART(minute, @LaunchStartTime),DATEADD(HOUR, DATEPART(hour, @LaunchStartTime), DATEADD(dd, 0, DATEDIFF(dd, 0, @Date))))
	
	DECLARE @WorkDayDinnerEnd datetime
	SET @WorkDayDinnerEnd = DATEADD(minute, DATEPART(minute, @LaunchEndTime),DATEADD(HOUR, DATEPART(hour, @LaunchEndTime), DATEADD(dd, 0, DATEDIFF(dd, 0, @Date))))

	if (@LaunchStartTime > @LaunchEndTime and 
		cast(@Date as time) >= cast('00:00:00' as time) and 
		cast(@Date as time) <= cast(@WorkDayEndTime as time))
	BEGIN
		SET @WorkDayDinnerBegin = DATEADD(day, -1, @WorkDayDinnerBegin)
	END

	if (@LaunchStartTime > @LaunchEndTime and 
		cast(@Date as time) <= cast('23:59:59' as time) and 
		cast(@Date as time) >= cast(@WorkDayStartTime as time))
	BEGIN
		SET @WorkDayDinnerEnd = DATEADD(day, 1, @WorkDayDinnerEnd)
	END
	
	IF [dbo].[CalendarGetDayOfWeek](@Date) BETWEEN 1 AND 5
	BEGIN
		IF (@Date < @WorkDayBegin )
		BEGIN
			SET @Type = 1 -- не рабочий
			SET @EndDate = @WorkDayBegin
			RETURN
		END
		IF (@Date < @WorkDayDinnerBegin)
		BEGIN
			SET @Type = 0 -- рабочий
			SET @EndDate = @WorkDayDinnerBegin
			RETURN
		END
		IF (@Date < @WorkDayDinnerEnd)
		BEGIN
			SET @Type = 1 -- не рабочий
			SET @EndDate = @WorkDayDinnerEnd
			RETURN
		END
		IF (@Date < @WorkDayEnd)
		BEGIN
			SET @Type = 0 -- рабочий
			SET @EndDate = @WorkDayEnd
			RETURN
		END
	END

	-- текущий день - выходной или рабочий, но уже закончился
	SET @Type = 1 -- не рабочий
	-- ищем следующий рабочий и берём его начало
	DECLARE @CurrentDate datetime
	SET @CurrentDate = DATEADD(day, 1, @Date)
	WHILE NOT ([dbo].[CalendarGetDayOfWeek](@CurrentDate) BETWEEN 1 AND 5)
	BEGIN
		SET @CurrentDate = DATEADD(day, 1, @CurrentDate)
	END

	DECLARE @MinStartDate datetime
	IF (@WorkDayBegin < @WorkDayDinnerBegin)
	BEGIN
		SET @MinStartDate = @WorkDayBegin
	END
	IF (@WorkDayDinnerBegin <= @WorkDayBegin)
	BEGIN
		SET @MinStartDate = @WorkDayDinnerBegin
	END
	SET @EndDate = DATEADD(HOUR, DATEPART(HOUR, @MinStartDate), DATEADD(dd, 0, DATEDIFF(dd, 0, @CurrentDate)))

END