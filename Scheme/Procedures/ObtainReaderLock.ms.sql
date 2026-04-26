CREATE PROCEDURE [ObtainReaderLock]
(
	@ID uniqueidentifier,
	@RetryCount int,
	@RetryTimeout int
)
AS
BEGIN
	DECLARE @RetryDelayTime datetime;
	SELECT @RetryDelayTime = DATEADD(millisecond, @RetryTimeout, CAST('00:00:00' AS datetime));

	DECLARE @TypeID uniqueidentifier;
	DECLARE @ErrorMessage nvarchar(4000);
	DECLARE @ErrorSeverity int;
	DECLARE @ErrorState int;

	BEGIN TRY
		SELECT TOP(1) @TypeID = [TypeID]
		FROM [Instances] WITH(NOLOCK)
		WHERE [ID] = @ID;

		IF @TypeID IS NULL
		BEGIN
			SELECT 2, NULL;
			RETURN;
		END;
	
		DECLARE @Counter int;
		SET @Counter = 0;

		UPDATE [Instances]
		SET [Readers] = [Readers] + 1
		WHERE [ID] = @ID AND [WritePending] = 0;

		WHILE (@@ROWCOUNT = 0)
		BEGIN
			SET @Counter = @Counter + 1;
			IF (@Counter > @RetryCount)
			BEGIN
				SELECT 1, NULL;
				RETURN;
			END;

			WAITFOR DELAY @RetryDelayTime;
	
			UPDATE [Instances]
			SET [Readers] = [Readers] + 1
			WHERE [ID] = @ID AND [WritePending] = 0;
		END;
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() = 1222  -- lock timeout
		BEGIN
			SELECT 3, NULL;
			RETURN;
		END
		ELSE
		BEGIN
			-- THROW нельзя в SQL Server 2008, поэтому имитируем его через RAISERROR
			SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
			RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
		END;
	END CATCH;

	SELECT 0, @TypeID;
END;