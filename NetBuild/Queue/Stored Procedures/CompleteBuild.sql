-- =============================================
-- Marks specified build as completed, removing all corresponding modifications
-- and updating related timestamps.
--
-- This behavior is needed to avoid situations when we started build process for Project1,
-- then some more modifications occured, then build was completed for Project1.
-- In this case all additional modifications which happened during the build,
-- will still be processed during the next iteration.
--
-- Usage example:
-- @itemCode: 'V3.Storage'
-- @buildCode: '17.1.25.3'
-- =============================================
CREATE PROCEDURE [Queue].[CompleteBuild]
	@itemCode NVARCHAR(100),
	@buildCode NVARCHAR(100)
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @message NVARCHAR(255)

	-- resolve internal item ID
	DECLARE @itemId INT
	SELECT TOP 1 @itemId = Id FROM [Queue].Item WHERE Code = @itemCode
	IF (@itemId IS NULL)
	BEGIN
		SET @message = 'Unknown item code "' + @itemCode + '"'
		RAISERROR (@message, 16, 1)
		RETURN
	END

	-- resolve internal build ID
	DECLARE @buildId BIGINT
	SELECT TOP 1 @buildId = Id FROM [Queue].Build WHERE ItemId = @itemId AND Code = @buildCode
	IF (@buildId IS NULL)
	BEGIN
		SET @message = 'Cannot find build code "' + @buildCode + '" for item "' + @itemCode + '"'
		RAISERROR (@message, 16, 1)
		RETURN
	END

	DECLARE @completed DATETIME2(2)
	SET @completed = GETUTCDATE()

	-- update history record
	UPDATE [Queue].Build
	SET Completed = @completed
	WHERE Id = @buildId

	-- update last build timestamp
	UPDATE [Queue].Item
	SET LastBuild = @completed
	WHERE Id = @itemId

	-- remove all corresponding modifications
	DELETE FROM [Queue].Modification
	WHERE BuildId = @buildId
END