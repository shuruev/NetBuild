CREATE TABLE [Queue].[Build] (
    [Id]     BIGINT         IDENTITY (1, 1) NOT NULL,
    [Date]   DATETIME2 (2)  CONSTRAINT [DF_Build_Date] DEFAULT (getutcdate()) NOT NULL,
    [Item]   NVARCHAR (100) NOT NULL,
    [Action] VARCHAR (10)   NOT NULL,
    [Label]  NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_Log] PRIMARY KEY CLUSTERED ([Id] ASC)
);

