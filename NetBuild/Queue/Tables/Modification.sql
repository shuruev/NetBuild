CREATE TABLE [Queue].[Modification] (
    [Id]                  BIGINT         IDENTITY (1, 1) NOT NULL,
    [BuildItem]           NVARCHAR (100) NOT NULL,
    [ModificationCode]    NVARCHAR (100) NULL,
    [ModificationType]    NVARCHAR (100) NULL,
    [ModificationAuthor]  NVARCHAR (100) NULL,
    [ModificationItem]    NVARCHAR (200) NULL,
    [ModificationComment] NVARCHAR (200) NULL,
    [ModificationDate]    DATETIME2 (2)  NOT NULL,
    [Reserved]            DATETIME2 (2)  NULL,
    CONSTRAINT [PK_Modification] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Modification_BuildItem]
    ON [Queue].[Modification]([BuildItem] ASC);

