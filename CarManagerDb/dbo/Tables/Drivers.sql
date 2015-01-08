CREATE TABLE [dbo].[Driver] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [Name]            NVARCHAR (50)    NULL,
    [FamilyName]      NVARCHAR (50)    NULL,
    [Licence]         NVARCHAR (50)    NOT NULL,
    [ExperienceYears] SMALLINT         NULL,
    [UserId] INT NULL, 
    CONSTRAINT [PK_Driver] PRIMARY KEY CLUSTERED ([Id] ASC)
);

