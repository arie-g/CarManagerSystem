CREATE TABLE [dbo].[UserInfo] (
    
    [UserId]         INT            NOT NULL,
    [Email]          NVARCHAR (MAX) NULL,
    [Phone]          NVARCHAR (MAX) NULL,
   PRIMARY KEY CLUSTERED ([UserId])
);

