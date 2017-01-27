-- =============================================
-- Checks whether specified item should be built, and returns all corresponding
-- modifications related to this build.
--
-- Enumerates through all [Queue].[Ignore_*] procedures in the database.
-- Every ignoring procedure is supposed to take a specified item and return 0 or 1
-- depending on whether this item should not be currently built by some reason.
--
-- For example, if Project1 and Project2 are already in the build queue, and Project2
-- references Project1, we would not rather start building Project2 until Project1 is built completely.
--
-- Usage example:
-- @itemCode: 'V3.Storage'
-- =============================================
CREATE PROCEDURE [Queue].[ShouldBuild]
	@itemCode NVARCHAR(100)
AS
BEGIN
	SET NOCOUNT ON

	-- check if there are any modifications in the build queue
	DECLARE @build TABLE (
		BuildId BIGINT NOT NULL,
		ModificationCode NVARCHAR(100),
		ModificationType NVARCHAR(100),
		ModificationAuthor NVARCHAR(100),
		ModificationItem NVARCHAR(200),
		ModificationComment NVARCHAR(200),
		ModificationDate DATETIME2(0),
		Created DATETIME2(2) NOT NULL)

	INSERT INTO @build
	SELECT
		QB.Id,
		QB.ModificationCode,
		QB.ModificationType,
		QB.ModificationAuthor,
		QB.ModificationItem,
		QB.ModificationComment,
		QB.ModificationDate,
		QB.Created
	FROM [Queue].Build QB
		INNER JOIN [Queue].Item QI
		ON QI.Id = QB.ItemId
	WHERE
		QI.Code = @itemCode

	-- if no modifications found, return nothing
	IF NOT EXISTS (SELECT * FROM @build)
	BEGIN
		RETURN
	END

	-- if any modifications found, let's check whether build should be ignored for this project
	PRINT 'Item code: ' + @itemCode

	DECLARE @name VARCHAR(100)

	DECLARE ProcedureCursor CURSOR FOR
	SELECT OBJECT_SCHEMA_NAME([object_id]) + '.' + [name] FROM sys.procedures
	WHERE
		OBJECT_SCHEMA_NAME([object_id]) = 'Queue'
		AND [name] LIKE 'Ignore[_]%'

	DECLARE @result INT
	DECLARE @ignore INT

	OPEN ProcedureCursor
	FETCH NEXT FROM ProcedureCursor INTO @name

	SET @result = 0

	WHILE @@FETCH_STATUS = 0
	BEGIN
		PRINT 'Executing ' + @name + '...'
		EXEC @ignore = @name @itemCode
		PRINT 'Returned ' + CAST(@ignore AS VARCHAR(10))

		SET @result = @result + @ignore

		FETCH NEXT FROM ProcedureCursor INTO @name
	END

	CLOSE ProcedureCursor
	DEALLOCATE ProcedureCursor

	-- if build should be ignored, return nothing
	IF (@result > 0)
	BEGIN
		PRINT 'Build should be ignored.'
		RETURN
	END

	-- otherwise return all corresponding modifications
	PRINT 'Build should not be ignored.'
	SELECT
		BuildId,
		ModificationCode,
		ModificationType,
		ModificationAuthor,
		ModificationItem,
		ModificationComment,
		ModificationDate,
		Created
	FROM @build
END