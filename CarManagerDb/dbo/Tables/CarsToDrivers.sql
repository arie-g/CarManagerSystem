CREATE TABLE [dbo].[CarToDriver] (
    [Id]            UNIQUEIDENTIFIER NOT NULL,
    [CarId]            UNIQUEIDENTIFIER NOT NULL,
    [DriverId]         UNIQUEIDENTIFIER NOT NULL,
    [KilometersDriven] NCHAR (10)       NULL,
    CONSTRAINT [FK_CarToDriver_Car] FOREIGN KEY ([CarId]) REFERENCES [dbo].[Car] ([Id]),
    CONSTRAINT [FK_CarToDriver_Driver] FOREIGN KEY ([DriverId]) REFERENCES [dbo].[Driver] ([Id]), 
    CONSTRAINT [PK_CarToDriver] PRIMARY KEY ([Id])
);


GO

CREATE INDEX [IX_CarToDriver_Car] ON [dbo].[CarToDriver] ([CarId])

GO

CREATE INDEX [IX_CarToDriver_Driver] ON [dbo].[CarToDriver] ([DriverId])
