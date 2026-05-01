CREATE PROCEDURE [CalendarPrepareQuants]
(
	@PeriodStartTime datetime, -- дата\время, начиная с которой должны быть перерасчитаны кванты
	@PeriodEndTime datetime, -- дата\время, до которой должные быть перерасчитаны кванты.
	@WorkDayStartTime datetime, -- время начала рабочего дня.
	@WorkDayEndTime datetime, -- время конца рабочего дня.
	@LaunchStartTime datetime, -- время начала обеда.
	@LaunchEndTime datetime-- время конца обеда.
)
AS 
BEGIN
	SET NOCOUNT ON;
	
	IF OBJECT_ID('tempdb.dbo.#Quants', 'U') IS NOT NULL
	BEGIN
		DROP TABLE [#Quants];
	END;
	
	CREATE TABLE [#Quants]
	(
		[QuantNumber] int NOT NULL,
		[StartTime] datetime NOT NULL,
		[EndTime] datetime NOT NULL,
		[Type] int NOT NULL
	);

	SET @WorkDayEndTime  = DATEADD(minute, DATEPART(minute, @WorkDayEndTime),DATEADD(HOUR, DATEPART(hour, @WorkDayEndTime), DATEADD(dd, 0, DATEDIFF(dd, 0, @WorkDayStartTime))));
	SET @LaunchEndTime = DATEADD(minute, DATEPART(minute, @LaunchEndTime ),DATEADD(HOUR, DATEPART(hour, @LaunchEndTime ), DATEADD(dd, 0, DATEDIFF(dd, 0, @LaunchStartTime))));

	DECLARE @CurrentStartDate datetime = @PeriodStartTime;

	-- Нулевой квант не существует - создаём
	INSERT INTO [#Quants]
		(QuantNumber, StartTime, EndTime, Type)
	VALUES
		(0, DATEADD(year, -5, @PeriodStartTime), @PeriodStartTime, 1);

	DECLARE @CurrentQuantNumber int = 0;
	
	WHILE @CurrentStartDate < @PeriodEndTime
	BEGIN		
		DECLARE @CurrentType bit
		DECLARE @CurrentEndDate datetime
		EXEC [dbo].[CalendarGetQuantType] @CurrentStartDate, @WorkDayStartTime, @WorkDayEndTime, @LaunchStartTime, @LaunchEndTime, @CurrentType OUT, @CurrentEndDate OUT;
		
		IF @CurrentType = 0
		BEGIN
			DECLARE @TempStartDate datetime = @CurrentStartDate;
			DECLARE @TempEndDate datetime = DATEADD(minute, 15, @TempStartDate);
			
			WHILE (@TempEndDate < @CurrentEndDate)
			BEGIN
				SET @CurrentQuantNumber = @CurrentQuantNumber + 1;

				INSERT INTO [#Quants]
					(QuantNumber, StartTime, EndTime, Type)
				VALUES
					(@CurrentQuantNumber, @TempStartDate, @TempEndDate, @CurrentType);

				SET @TempStartDate = @TempEndDate;
				SET @TempEndDate = DATEADD(minute, 15, @TempStartDate);
			END;
			
			SET @CurrentQuantNumber = @CurrentQuantNumber + 1;
			
			INSERT INTO [#Quants]
				(QuantNumber, StartTime, EndTime, Type)
			VALUES
				(@CurrentQuantNumber, @TempStartDate, @CurrentEndDate, @CurrentType)
		END
		ELSE
		BEGIN
			INSERT INTO [#Quants]
				(QuantNumber, StartTime, EndTime, Type)
			VALUES
				(@CurrentQuantNumber, @CurrentStartDate, @CurrentEndDate, @CurrentType);
		END
		
		SET @CurrentStartDate = @CurrentEndDate;
	END
	
	DECLARE @LastQuantEndTime datetime;
	SELECT @LastQuantEndTime = MAX(EndTime) 
	FROM [#Quants] WITH (NOLOCK);
	
	-- вставляем заключительный квант
	INSERT INTO [#Quants]
		(QuantNumber, StartTime, EndTime, Type)
	VALUES
		(@CurrentQuantNumber, @LastQuantEndTime,  DATEADD(year, 5, @PeriodEndTime), 1);
	
	TRUNCATE TABLE [dbo].[CalendarQuants];
	
	INSERT INTO [dbo].[CalendarQuants]
	SELECT [QuantNumber], [StartTime], [EndTime], [Type]
	FROM [#Quants] WITH (NOLOCK);
	
	DROP TABLE [#Quants];

	ALTER INDEX ALL ON [dbo].[CalendarQuants]
	REBUILD;
END