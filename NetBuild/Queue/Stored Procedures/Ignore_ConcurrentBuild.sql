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

	SELECT @count = COUNT(*)
	FROM
	(
		-- select all running builds (which are not completed yet)
		-- if they were started less than 10 minutes ago
		SELECT ItemId
		FROM [Queue].Build
		WHERE
			Completed IS NULL
			AND DATEDIFF(MINUTE, [Started], GETUTCDATE()) < 10

		UNION

		-- select all project items where there are reserved
		-- builds in the modification table (less than 10 minutes ago)
		SELECT ItemId
		FROM [Queue].Modification
		WHERE
			Reserved IS NOT NULL
			AND DATEDIFF(MINUTE, [Reserved], GETUTCDATE()) < 10
	) A

	-- allow no more than 5 builds to run in parallel
	IF @count >= 5 RETURN 1
	ELSE RETURN 0
END