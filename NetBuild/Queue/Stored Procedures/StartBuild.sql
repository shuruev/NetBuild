-- =============================================
-- Starts build process for specified item, marking all the current
-- modifications with specified build code.
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
CREATE PROCEDURE [Queue].[StartBuild]
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

	DECLARE @buildId BIGINT

	-- add new build record
	INSERT INTO [Queue].Build (
		ItemId,
		Code,
		[Started])
	VALUES (
		@itemId,
		@buildCode,
		GETUTCDATE())

	SET @buildId = @@IDENTITY

	-- update build ID for all the current modifications
	UPDATE [Queue].Modification
	SET BuildId = @buildId
	WHERE ItemId = @itemId
END