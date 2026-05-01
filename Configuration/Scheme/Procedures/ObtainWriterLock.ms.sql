CREATE PROCEDURE [ObtainWriterLock]
(
	@ID uniqueidentifier,
	@Version int,
	@WriterRetryCount int,
	@ReaderRetryCount int,
	@RetryTimeout int
)
AS
BEGIN
	DECLARE @RetryDelayTime datetime;
	SELECT @RetryDelayTime = DATEADD(millisecond, @RetryTimeout, CAST('00:00:00' AS datetime));
	
	DECLARE @Counter int;
	DECLARE @ErrorMessage nvarchar(4000);
	DECLARE @ErrorSeverity int;
	DECLARE @ErrorState int;

	BEGIN TRY
		IF NOT EXISTS (SELECT NULL FROM [Instances] WITH(NOLOCK) WHERE [ID] = @ID)
		BEGIN
			SELECT 3 AS [Result], NULL AS [Version], NULL AS [ModifiedByID], NULL AS [ModifiedByName], NULL AS [Modified];
			RETURN;
		END;

		SET @Counter = 0;

		UPDATE [Instances]
		SET [WritePending] = 1
		WHERE [ID] = @ID AND [WritePending] = 0;

		WHILE (@@ROWCOUNT = 0)
		BEGIN
			SET @Counter = @Counter + 1;
			IF @Counter > @WriterRetryCount
			BEGIN
				SELECT 1 AS [Result], NULL AS [Version], NULL AS [ModifiedByID], NULL AS [ModifiedByName], NULL AS [Modified];
				RETURN;
			END;

			WAITFOR DELAY @RetryDelayTime;
	
			UPDATE [Instances]
			SET [WritePending] = 1
			WHERE [ID] = @ID AND [WritePending] = 0;
		END;
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() = 1222  -- lock timeout
		BEGIN
			SELECT 4 AS [Result], NULL AS [Version], NULL AS [ModifiedByID], NULL AS [ModifiedByName], NULL AS [Modified];
			RETURN;
		END
		ELSE
		BEGIN
			-- THROW нельзя в SQL Server 2008, поэтому имитируем его через RAISERROR
			SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
			RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
		END;
	END CATCH;

	BEGIN TRY
		SET @Counter = 0;

		WHILE (SELECT TOP(1) [Readers] FROM [Instances] WITH(NOLOCK) WHERE [ID] = @ID) > 0
		BEGIN
			WAITFOR DELAY @RetryDelayTime;
	
			SET @Counter = @Counter + 1;
			IF @Counter > @ReaderRetryCount
			BEGIN
				UPDATE [Instances]
				SET [WritePending] = 0
				WHERE [ID] = @ID;
				
				SELECT 2 AS [Result], NULL AS [Version], NULL AS [ModifiedByID], NULL AS [ModifiedByName], NULL AS [Modified];
				RETURN;
			END;
		END;
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() = 1222  -- lock timeout
		BEGIN
			UPDATE [Instances]
			SET [WritePending] = 0
			WHERE [ID] = @ID;
			
			SELECT 4, NULL, NULL, NULL, NULL;
			RETURN;
		END
		ELSE
		BEGIN
			-- THROW нельзя в SQL Server 2008, поэтому имитируем его через RAISERROR
			SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
			RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
		END;
	END CATCH;

	IF (SELECT TOP(1) [Version] FROM [Instances] WITH(NOLOCK) WHERE [ID] = @ID) <> @Version
	BEGIN
		SELECT 5, [Version], [ModifiedByID], [ModifiedByName], [Modified]
		FROM [Instances] WITH(NOLOCK)
		WHERE [ID] = @ID;
	
		UPDATE [Instances]
		SET [WritePending] = 0
		WHERE [ID] = @ID;

		RETURN;
	END;
	
	SELECT 0 AS [Result], NULL AS [Version], NULL AS [ModifiedByID], NULL AS [ModifiedByName], NULL AS [Modified];
END;