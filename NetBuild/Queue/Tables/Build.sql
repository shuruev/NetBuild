CREATE TABLE [Queue].[Build] (
    [Id]        BIGINT         IDENTITY (1, 1) NOT NULL,
    [ItemId]    INT            NOT NULL,
    [Code]      NVARCHAR (100) NOT NULL,
    [Started]   DATETIME2 (2)  NOT NULL,
    [Completed] DATETIME2 (2)  NULL,
    CONSTRAINT [PK_Build] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Build_Item] FOREIGN KEY ([ItemId]) REFERENCES [Queue].[Item] ([Id])
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_Build_ItemAndCode]
    ON [Queue].[Build]([ItemId] ASC, [Code] ASC);

