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
-- @reserveCode: '17.1.25.3'
-- =============================================
CREATE PROCEDURE [Queue].[CompleteBuild]
	@itemCode NVARCHAR(100),
	@reserveCode NVARCHAR(100)
AS
BEGIN
	SET NOCOUNT ON

	-- resolve internal item ID
	DECLARE @itemId INT
	SELECT @itemId = Id FROM [Queue].Item WHERE Code = @itemCode
	IF (@itemId IS NULL)
	BEGIN
		DECLARE @message NVARCHAR(255)
		SET @message = 'Unknown item code "' + @itemCode + '"'
		RAISERROR (@message, 16, 1)
		RETURN
	END

	-- update last build timestamp
	UPDATE [Queue].Item
	SET LastBuild = GETUTCDATE()
	WHERE Id = @itemId

	-- remove all corresponding modifications
	DELETE FROM [Queue].Build
	WHERE
		ItemId = @itemId
		AND ReserveCode = @reserveCode
END