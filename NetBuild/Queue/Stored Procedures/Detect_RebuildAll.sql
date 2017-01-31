-- =============================================
-- Detects "RebuildAll" signal which forces all existing projects to rebuild.
-- 
-- Usage example:
-- @signalType: 'RebuildAll'
-- @signalValue: NULL
-- =============================================
CREATE PROCEDURE [Queue].[Detect_RebuildAll]
	@signalType VARCHAR(100),
	@signalValue NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON

	IF (@signalType != 'RebuildAll') RETURN

	SELECT
		Id,
		NULL,
		'force',
		NULL,
		Code,
		'Project was internally forced to rebuild',
		GETUTCDATE()
	FROM [Queue].Item
END