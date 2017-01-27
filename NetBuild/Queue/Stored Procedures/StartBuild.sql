-- =============================================
-- Starts build process for specified item, marking all the current
-- modifications with specified reserve code.
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
CREATE PROCEDURE [Queue].[StartBuild]
	@itemCode NVARCHAR(100),
	@reserveCode NVARCHAR(100)
AS
BEGIN
	SET NOCOUNT ON

	-- get all the current modifications
	DECLARE @build TABLE (
		BuildId BIGINT NOT NULL,
		ModificationCode NVARCHAR(100),
		ModificationType NVARCHAR(100),
		ModificationAuthor NVARCHAR(100),
		ModificationItem NVARCHAR(200),
		ModificationComment NVARCHAR(200),
		ModificationDate DATETIME2(0),
		Created DATETIME2(2) NOT NULL)

	INSERT INTO @build EXEC [Queue].ShouldBuild @itemCode

	-- update reserve code
	UPDATE QB
	SET
		ReserveCode = @reserveCode,
		Reserved = GETUTCDATE()
	FROM [Queue].Build QB
		INNER JOIN @build B
		ON B.BuildId = QB.Id
END