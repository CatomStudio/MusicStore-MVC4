CREATE TABLE [dbo].[Accounts] (
    [AccountId]     INT             IDENTITY (1, 1) NOT NULL,
    [Username]      VARCHAR (50)    NOT NULL,
    [Password]      VARCHAR (50)    NOT NULL,
    [Name]          VARCHAR (50)    NOT NULL,
    [Address]       VARCHAR (300)   NOT NULL,
    [ImageData]     VARBINARY (MAX) NULL,
    [ImageMimeType] VARCHAR (50)    NULL,
    PRIMARY KEY CLUSTERED ([AccountId] ASC),
    UNIQUE NONCLUSTERED ([Username] ASC)
);

