CREATE TABLE [Queue].[Modification] (
    [Id]                  BIGINT         IDENTITY (1, 1) NOT NULL,
    [ItemId]              INT            NOT NULL,
    [ModificationCode]    NVARCHAR (100) NULL,
    [ModificationType]    NVARCHAR (100) NULL,
    [ModificationAuthor]  NVARCHAR (100) NULL,
    [ModificationItem]    NVARCHAR (200) NULL,
    [ModificationComment] NVARCHAR (200) NULL,
    [ModificationDate]    DATETIME2 (0)  NULL,
    [Created]             DATETIME2 (2)  CONSTRAINT [DF_Build_Created] DEFAULT (getutcdate()) NOT NULL,
    [Reserved]            DATETIME2 (2)  NULL,
    [BuildId]             BIGINT         NULL,
    CONSTRAINT [PK_Modification] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Modification_Build] FOREIGN KEY ([BuildId]) REFERENCES [Queue].[Build] ([Id]),
    CONSTRAINT [FK_Modification_Item] FOREIGN KEY ([ItemId]) REFERENCES [Queue].[Item] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_Modification_ItemId]
    ON [Queue].[Modification]([ItemId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Modification_BuildId]
    ON [Queue].[Modification]([BuildId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Modification_Reserved]
    ON [Queue].[Modification]([Reserved] ASC);

