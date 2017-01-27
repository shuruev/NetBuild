-- =============================================
-- For a specified item, updates a complete set of its triggers with a given type.
--
-- Usage example:
-- @itemCode: 'MyProject'
-- @triggerType: 'MyTrigger'
-- @@triggerValues: 'Value1', 'Value2', 'Value3'
-- =============================================
CREATE PROCEDURE [Queue].[SetTriggers]
	@itemCode NVARCHAR(100),
	@triggerType VARCHAR(100),
	@triggerValues [Queue].TriggerValue READONLY
AS
BEGIN
	SET NOCOUNT ON

	-- resolve all existing triggers and create those which do not exist yet
	DECLARE @triggers TABLE (
		Id INT NULL,
		[Type] VARCHAR(100) NOT NULL,
		[Value] NVARCHAR(MAX) NOT NULL,
		Crc UNIQUEIDENTIFIER NOT NULL)

	-- calculate CRC for the given triggers
	INSERT INTO @triggers ([Type], [Value], Crc)
	SELECT
		@triggerType,
		[Value],
		HASHBYTES('MD5', HASHBYTES('MD5', @triggerType) + HASHBYTES('MD5', [Value]))
	FROM @triggerValues

	-- create triggers which do not exist yet
	INSERT INTO [Queue].[Trigger] ([Type], [Value])
	SELECT
		T.[Type],
		T.[Value]
	FROM @triggers T
		LEFT JOIN [Queue].[Trigger] QT
		ON QT.Crc = T.Crc
	WHERE
		QT.Id IS NULL

	-- resolve IDs for all given triggers
	UPDATE T
	SET Id = QT.Id
	FROM @triggers T
		INNER JOIN [Queue].[Trigger] QT
		ON QT.Crc = T.Crc

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

	-- add missing triggers to specified item
	INSERT INTO [Queue].ItemTrigger (ItemId, TriggerId)
	SELECT
		@itemId,
		T.Id
	FROM @triggers T
		LEFT JOIN [Queue].ItemTrigger QIT
		ON QIT.ItemId = @itemId
			AND QIT.TriggerId = T.Id
	WHERE
		QIT.ItemId IS NULL

	IF (@@ROWCOUNT > 0)
	BEGIN
		UPDATE [Queue].Item
		SET TriggerUpdated = GETUTCDATE()
		WHERE Id = @itemId
	END

	-- remove unnecessary triggers from specified item
	DELETE QIT
	FROM [Queue].ItemTrigger QIT
		LEFT JOIN @triggers T
		ON QIT.TriggerId = T.Id
	WHERE
		QIT.ItemId = @itemId
		AND T.Id IS NULL

	IF (@@ROWCOUNT > 0)
	BEGIN
		UPDATE [Queue].Item
		SET TriggerUpdated = GETUTCDATE()
		WHERE Id = @itemId
	END
END