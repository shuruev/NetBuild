-- =============================================
-- Allows only limited number of projects to be built at the same time.
-- 
-- Usage example:
-- @itemCode: 'V3.Storage'
-- =============================================
CREATE PROCEDURE [Queue].[Ignore_ConcurrentBuild]
	@itemCode NVARCHAR(100)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @count INT

	-- select all running builds (which are not completed yet)
	-- if they were started less than 10 minutes ago
	SELECT @count = COUNT(*)
	FROM [Queue].Build
	WHERE
		Completed IS NULL
		AND DATEDIFF(MINUTE, [Started], GETUTCDATE()) < 10

	-- allow no more than 5 builds to run in parallel
	IF @count >= 5 RETURN 1
	ELSE RETURN 0
END