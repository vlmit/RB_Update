CREATE PROCEDURE [CheckProcessLock]
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
	
	    IF NOT EXISTS (SELECT TOP 1 1 FROM WorkflowEngineProcessesLocks p WITH(NOLOCK) WHERE p.RowID = @ProcessID)
	    BEGIN
	        SELECT 0
	    END
	    ELSE 
	    BEGIN	
	        SET @Counter = 0
	
	        SELECT TOP 1 0 FROM WorkflowEngineProcessesLocks p WITH(NOLOCK)
	        WHERE p.RowID = @ProcessID AND p.Locked is Null
	
	        WHILE (@@ROWCOUNT = 0)
		    BEGIN
			    SET @Counter = @Counter + 1;
			    IF @Counter > @RetryCount
			    BEGIN
				    SELECT 1
				    RETURN
			    END
	
			    WAITFOR DELAY @RetryDelayTime
		
			    SELECT TOP 1 0 FROM WorkflowEngineProcessesLocks p WITH(NOLOCK)
	            WHERE p.RowID = @ProcessID AND p.Locked is Null
		    END
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