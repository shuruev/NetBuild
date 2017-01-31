﻿-- =============================================
-- Processes any external signal which can trigger new builds, and returns a list
-- of all potentially affected items.
--
-- Enumerates through all [Queue].[Detect_*] procedures in the database.
-- Every detection procedure is supposed to take a specified signal and return
-- corresponding items which should be triggered for build, including modification details:
-- 1). internal item ID
-- 2). modification ID
-- 3). modification type
-- 4). modification author
-- 5). modification item
-- 6). modification comment
-- 7). modification date
--
-- Usage example:
-- @signalType: 'BuildComplete'
-- @signalValue: '{ "item": "V3.Storage" }'
-- =============================================
CREATE PROCEDURE [Queue].[ProcessSignal]
	@signalType VARCHAR(100),
	@signalValue NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON

	PRINT 'Signal type: ' + @signalType
	PRINT 'Signal value: ' + @signalValue

	DECLARE @name VARCHAR(100)

	DECLARE ProcedureCursor CURSOR FOR
	SELECT OBJECT_SCHEMA_NAME([object_id]) + '.' + [name] FROM sys.procedures
	WHERE
		OBJECT_SCHEMA_NAME([object_id]) = 'Queue'
		AND [name] LIKE 'Detect[_]%'

	DECLARE @modification TABLE (
		ItemId INT NOT NULL,
		ModificationCode NVARCHAR(100),
		ModificationType NVARCHAR(100),
		ModificationAuthor NVARCHAR(100),
		ModificationItem NVARCHAR(200),
		ModificationComment NVARCHAR(200),
		ModificationDate DATETIME2(0))

	OPEN ProcedureCursor
	FETCH NEXT FROM ProcedureCursor INTO @name

	WHILE @@FETCH_STATUS = 0
	BEGIN
		PRINT 'Executing ' + @name + '...'
		INSERT INTO @modification EXEC @name @signalType, @signalValue
		PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' item(s) found.'

		FETCH NEXT FROM ProcedureCursor INTO @name
	END

	CLOSE ProcedureCursor
	DEALLOCATE ProcedureCursor

	INSERT INTO [Queue].Modification (
		ItemId,
		ModificationCode,
		ModificationType,
		ModificationAuthor,
		ModificationItem,
		ModificationComment,
		ModificationDate)
	SELECT
		ItemId,
		ModificationCode,
		ModificationType,
		ModificationAuthor,
		ModificationItem,
		ModificationComment,
		ModificationDate
	FROM @modification

	SELECT DISTINCT
		QI.Code
	FROM [Queue].Modification QM
		INNER JOIN [Queue].Item QI
		ON QI.Id = QM.ItemId
END