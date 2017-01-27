-- =============================================
-- Creates specified item if it does not exist yet.
--
-- Usage example:
-- @itemCode: 'MyProject'
-- =============================================
CREATE PROCEDURE [Queue].[SubmitItem]
	@itemCode NVARCHAR(100)
AS
BEGIN
	SET NOCOUNT ON

	IF EXISTS (
		SELECT TOP 1 Id
		FROM [Queue].Item
		WHERE Code = @itemCode)
		RETURN

	INSERT INTO [Queue].Item (Code) VALUES (@itemCode)
END