CREATE TABLE [dbo].[Orders] (
    [OrderId]       INT           IDENTITY (1, 1) NOT NULL,
    [Username]      VARCHAR (50)  NOT NULL,
    [Recipient]     VARCHAR (50)  NOT NULL,
    [Address]       VARCHAR (MAX) NOT NULL,
    [PaymentOption] VARCHAR (50)  NOT NULL,
    [Lines]         VARCHAR (MAX) NOT NULL,
    [TotalPrice]    DECIMAL (18)  NOT NULL,
    [Datetime]      DATETIME      NOT NULL,
    PRIMARY KEY CLUSTERED ([OrderId] ASC)
);

