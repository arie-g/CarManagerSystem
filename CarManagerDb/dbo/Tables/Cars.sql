CREATE TABLE [dbo].[Car] (
    [Id]     UNIQUEIDENTIFIER NOT NULL,
    [Model]  NVARCHAR (50)    NULL,
    [Number] BIGINT           NOT NULL,
    CONSTRAINT [PK_Car] PRIMARY KEY CLUSTERED ([Id] ASC)
);

