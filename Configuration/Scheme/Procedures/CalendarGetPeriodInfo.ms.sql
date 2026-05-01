CREATE PROCEDURE [CalendarGetPeriodInfo]
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
	DECLARE @DefaultPeriodType bit
	DECLARE @DefaultPeriodEnd datetime
	DECLARE @ExclusionPeriodType bit
	DECLARE @ExclusionPeriodEnd datetime
	
	EXEC [dbo].[CalendarGetExclusionPeriodInfo] @Date, @ExclusionPeriodType OUT, @ExclusionPeriodEnd OUT
	IF (@ExclusionPeriodType IS NOT NULL)
	BEGIN
		-- на эту дату попадает исключение
		SET @Type = @ExclusionPeriodType
		SET @EndDate = @ExclusionPeriodEnd
		RETURN
	END
	
	
	EXEC [dbo].[CalendarGetDefaultPeriodInfo] @Date, @WorkDayStartTime, @WorkDayEndTime, @LaunchStartTime, @LaunchEndTime, @DefaultPeriodType OUT, @DefaultPeriodEnd OUT
	SET @Type = @DefaultPeriodType
	IF @ExclusionPeriodEnd < @DefaultPeriodEnd
	BEGIN
		-- следующее исключение начнётся раньше, чем закончится текущий стандартный период
		SET @EndDate = @ExclusionPeriodEnd
	END
	ELSE
	BEGIN
		SET @EndDate = @DefaultPeriodEnd
	END
END