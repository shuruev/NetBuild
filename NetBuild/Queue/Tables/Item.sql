CREATE TABLE [Queue].[Item] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [Code]           NVARCHAR (100) NOT NULL,
    [Created]        DATETIME2 (2)  CONSTRAINT [DF_Project_Created] DEFAULT (getutcdate()) NOT NULL,
    [TriggerUpdated] DATETIME2 (2)  NULL,
    [LastBuild]      DATETIME2 (2)  NULL,
    CONSTRAINT [PK_Item] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_Item_Code]
    ON [Queue].[Item]([Code] ASC);

