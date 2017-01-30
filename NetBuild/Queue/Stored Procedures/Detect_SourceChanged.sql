-- =============================================
-- Detects "SourceChanged" signals and triggers projects which have "SourcePath" triggers
-- for corresponding path.
-- 
-- Usage example:
-- @signalType: 'SourceChanged'
-- @signalValue: '{
--     "id": "11584",
--     "path": "$/Main/ContentCast/V3/V3.Storage/Client/V3Client.cs",
--     "author": "Shuruev, Oleg",
--     "type": "edit",
--     "comment": "Implemented initial version",
--     "date": "2016-12-23 00:22:58"
-- }'
-- =============================================
CREATE PROCEDURE [Queue].[Detect_SourceChanged]
	@signalType VARCHAR(100),
	@signalValue NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON

	IF (@signalType != 'SourceChanged') RETURN

	DECLARE @comment NVARCHAR(200)
	IF (LEN(JSON_VALUE(@signalValue, '$.id')) > 0)
	BEGIN
		IF (LEN(JSON_VALUE(@signalValue, '$.comment')) > 0)
		BEGIN
			SET @comment = '#' + JSON_VALUE(@signalValue, '$.id') + ': ' + JSON_VALUE(@signalValue, '$.comment')
		END
		ELSE
		BEGIN
			SET @comment = '#' + JSON_VALUE(@signalValue, '$.id')
		END
	END
	ELSE
	BEGIN
		SET @comment = JSON_VALUE(@signalValue, '$.comment')
	END

	SELECT
		QI.Id,
		JSON_VALUE(@signalValue, '$.id'),
		JSON_VALUE(@signalValue, '$.type'),
		JSON_VALUE(@signalValue, '$.author'),
		JSON_VALUE(@signalValue, '$.path'),
		@comment,
		JSON_VALUE(@signalValue, '$.date')
	FROM [Queue].[Trigger] QT
		INNER JOIN [Queue].ItemTrigger QIT
		ON QIT.TriggerId = QT.Id
		INNER JOIN [Queue].Item QI
		ON QI.Id = QIT.ItemId
	WHERE
		QT.[Type] = 'SourcePath'
		AND JSON_VALUE(@signalValue, '$.path') LIKE JSON_VALUE(QT.[Value], '$.path') + '%'
END