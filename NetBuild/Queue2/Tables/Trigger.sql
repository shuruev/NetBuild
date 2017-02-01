CREATE TABLE [Queue2].[Trigger] (
    [Id]           BIGINT         IDENTITY (1, 1) NOT NULL,
    [BuildItem]    NVARCHAR (100) NOT NULL,
    [TriggerType]  VARCHAR (100)  NOT NULL,
    [TriggerValue] NVARCHAR (MAX) NOT NULL,
    [TriggerDate]  DATETIME2 (2)  NOT NULL,
    CONSTRAINT [PK_Trigger] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Trigger_BuildItem]
    ON [Queue2].[Trigger]([BuildItem] ASC);

