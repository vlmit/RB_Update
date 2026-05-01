CREATE PROCEDURE [CalendarGetExclusionPeriodInfo]
(
	@Date datetime,
	@Type bit OUTPUT,
	@EndDate datetime OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON;
	-- проверяем, попадает ли дата в исключения
	SELECT @EndDate = EndTime, @Type = [Type]
	FROM [dbo].[CalendarExclusions] WITH (NOLOCK)
	WHERE StartTime <= @Date AND EndTime > @Date
	
	IF @EndDate IS NULL
	BEGIN
		-- дата не попадает в какое-то исключение
		-- ищем дату начала следующего исключения
		SELECT @EndDate = MIN(StartTime)
		FROM [dbo].[CalendarExclusions] WITH (NOLOCK)
		WHERE StartTime > @Date
	END
END