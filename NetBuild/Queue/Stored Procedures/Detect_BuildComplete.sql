-- =============================================
-- Detects "BuildComplete" signals and triggers all dependant projects, which have
-- corresponding "ReferenceItem" trigger.
--
-- Usage example:
-- @signalType: 'BuildComplete'
-- @signalValue: '{ "item": "V3.Storage" }'
-- =============================================
CREATE PROCEDURE [Queue].[Detect_BuildComplete]
	@signalType VARCHAR(100),
	@signalValue NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON

	IF (@signalType != 'BuildComplete') RETURN

	SELECT
		QI.Id,
		NULL,
		'reference',
		NULL,
		QT.[Value],
		'Referenced project was built',
		GETUTCDATE()
	FROM [Queue].[Trigger] QT
		INNER JOIN [Queue].ItemTrigger QIT
		ON QIT.TriggerId = QT.Id
		INNER JOIN [Queue].Item QI
		ON QI.Id = QIT.ItemId
	WHERE
		QT.[Type] = 'ReferenceItem'
		AND JSON_VALUE(QT.[Value], '$.item') = JSON_VALUE(@signalValue, '$.item')
END