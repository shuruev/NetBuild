CREATE TABLE [Queue].[Build] (
    [Id]                  BIGINT         IDENTITY (1, 1) NOT NULL,
    [ItemId]              INT            NOT NULL,
    [ModificationCode]    NVARCHAR (100) NULL,
    [ModificationType]    NVARCHAR (100) NULL,
    [ModificationAuthor]  NVARCHAR (100) NULL,
    [ModificationItem]    NVARCHAR (200) NULL,
    [ModificationComment] NVARCHAR (200) NULL,
    [ModificationDate]    DATETIME2 (0)  NULL,
    [Created]             DATETIME2 (2)  CONSTRAINT [DF_Build_Created] DEFAULT (getutcdate()) NOT NULL,
    [ReserveCode]         NVARCHAR (100) NULL,
    [Reserved]            DATETIME2 (2)  NULL,
    CONSTRAINT [PK_Build] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Build_Item] FOREIGN KEY ([ItemId]) REFERENCES [Queue].[Item] ([Id])
);

