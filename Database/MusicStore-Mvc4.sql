-- --------------------------------------------------- 1. build database --
Create database MusicStore;
-- --------------------------------------------------- 2. build tables --
Use MusicStore;

CREATE TABLE Accounts (
    AccountId     INT               AUTO_INCREMENT NOT NULL,
    Username      VARCHAR (50)      NOT NULL,
    Password      VARCHAR (50)      NOT NULL,
    Name          VARCHAR (50)      NOT NULL,
    Address       VARCHAR (300)     NOT NULL,
    ImageData     VARBINARY (10000) NULL,
    ImageMimeType VARCHAR (50)      NULL,
    PRIMARY KEY (AccountId ASC)
);

CREATE TABLE Orders (
    OrderId       INT             AUTO_INCREMENT NOT NULL,
    Username      VARCHAR (50)    NOT NULL,
    Recipient     VARCHAR (50)    NOT NULL,
    Address       VARCHAR (200) NOT NULL,
    PaymentOption VARCHAR (50)    NOT NULL,
    Line          TEXT			  NOT NULL,
    TotalPrice    DECIMAL (18)    NOT NULL,
    Datetime      DATETIME        NOT NULL,
    PRIMARY KEY (OrderId ASC)
);

CREATE TABLE Products (
    ProductID     INT               AUTO_INCREMENT NOT NULL,
    Name          NVARCHAR (200)  NOT NULL,
    Description   NVARCHAR (1000)  NOT NULL,
    Price         DECIMAL  (18, 2)  NOT NULL,
    Category      NVARCHAR (1000)  NOT NULL,
    ImageData     VARBINARY (10000) NULL,
    ImageMimeType VARCHAR  (50)     NULL,
    PRIMARY KEY (ProductID ASC)
);

-- 添加一个用户
insert into accounts(Username,Password,Name,Address) 
	values('root', 'asd', '管理员', '华星时代广场');

 
-- --------------------------------------------------- 3. Edit tables --


































