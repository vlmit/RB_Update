CREATE PROCEDURE [ObtainProcessLock]
(
	@ProcessID uniqueidentifier,
	@RetryCount int,
	@RetryTimeout int
)
AS
BEGIN
	DECLARE @RetryDelayTime datetime;
	SELECT @RetryDelayTime = DATEADD(millisecond, @RetryTimeout, CAST('00:00:00' AS datetime));
	
	DECLARE @Counter int;

	BEGIN TRY
     	    SET LOCK_TIMEOUT 5000
	    SET @Counter = 0
	
	    UPDATE WorkflowEngineProcessesLocks
	    SET Locked = GETUTCDATE()
	    WHERE RowID = @ProcessID AND Locked is Null
	
	    WHILE (@@ROWCOUNT = 0)
			BEGIN
				SET @Counter = @Counter  + 1;
				IF @Counter > @RetryCount
				BEGIN
					SELECT 1
					RETURN
				END
	
				WAITFOR DELAY @RetryDelayTime
		
				UPDATE WorkflowEngineProcessesLocks
		             SET Locked = GETUTCDATE()
		             WHERE RowID = @ProcessID AND Locked is Null
			END
	
	    SET LOCK_TIMEOUT -1
	END TRY
	BEGIN CATCH
	    SET LOCK_TIMEOUT -1
	    IF ERROR_NUMBER() = 1222  -- lock timeout
	    BEGIN
		    SELECT 1
	    END
	END CATCH
END;