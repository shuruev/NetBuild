CREATE TABLE [Queue].[Trigger] (
    [Id]      INT            IDENTITY (1, 1) NOT NULL,
    [Type]    VARCHAR (100)  NOT NULL,
    [Value]   NVARCHAR (MAX) NOT NULL,
    [Crc]     AS             (CONVERT([uniqueidentifier],hashbytes('MD5',hashbytes('MD5',[Type])+hashbytes('MD5',[Value])))) PERSISTED,
    [Created] DATETIME2 (2)  CONSTRAINT [DF_Trigger_Created] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_Trigger] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_Trigger_Crc]
    ON [Queue].[Trigger]([Crc] ASC);

