CREATE TABLE [Queue].[ItemTrigger] (
    [ItemId]    INT           NOT NULL,
    [TriggerId] INT           NOT NULL,
    [Created]   DATETIME2 (2) CONSTRAINT [DF_ItemTrigger_Created] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_ItemTrigger] PRIMARY KEY CLUSTERED ([ItemId] ASC, [TriggerId] ASC),
    CONSTRAINT [FK_ItemTrigger_Item] FOREIGN KEY ([ItemId]) REFERENCES [Queue].[Item] ([Id]),
    CONSTRAINT [FK_ItemTrigger_Trigger] FOREIGN KEY ([TriggerId]) REFERENCES [Queue].[Trigger] ([Id])
);

