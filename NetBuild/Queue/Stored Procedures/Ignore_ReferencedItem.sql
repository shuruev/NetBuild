-- =============================================
-- Postpones builds for those projects, which already have some of their referenced projects
-- in the build queue (via corresponding "ReferenceItem" triggers).
-- 
-- Usage example:
-- @itemCode: 'V3.Storage'
-- =============================================
CREATE PROCEDURE [Queue].[Ignore_ReferencedItem]
	@itemCode NVARCHAR(100)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @id INT

	;WITH T (Id) AS
	(
		-- all items which reference any of the currently queued items
		SELECT
			QIT.ItemId
		FROM [Queue].Modification QM
			INNER JOIN [Queue].Item QI
			ON QI.Id = QM.ItemId
			INNER JOIN [Queue].[Trigger] QT
			ON JSON_VALUE(QT.[Value], '$.item') = QI.Code
			INNER JOIN [Queue].ItemTrigger QIT
			ON QIT.TriggerId = QT.Id
		WHERE
			QT.[Type] = 'ReferenceItem'

		UNION ALL

		-- and any other items which recursively reference these items
		SELECT
			QIT.ItemId
		FROM T
			INNER JOIN [Queue].Item QI
			ON QI.Id = T.Id
			INNER JOIN [Queue].[Trigger] QT
			ON JSON_VALUE(QT.[Value], '$.item') = QI.Code
			INNER JOIN [Queue].ItemTrigger QIT
			ON QIT.TriggerId = QT.Id
		WHERE
			QT.[Type] = 'ReferenceItem'
	)

	-- postpone build if the specified item can be found in the above set
	SELECT TOP 1
		@id = T.Id
	FROM T
		INNER JOIN [Queue].Item QI
		ON QI.Id = T.Id
	WHERE
		QI.Code = @itemCode

	IF @id IS NOT NULL RETURN 1

	RETURN 0
END