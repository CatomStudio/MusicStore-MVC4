-- --------------------------------------------------- 1. build database --
Create database MusicStore;
drop table `account`;
drop table `order`;
drop table `product`;
-- --------------------------------------------------- 2. build tables --
Use MusicStore;

CREATE TABLE `Account` (
    AccountId     INT               AUTO_INCREMENT NOT NULL,
    Username      VARCHAR (50)      NOT NULL,
    Password      VARCHAR (50)      NOT NULL,
    Name          VARCHAR (50)      NOT NULL,
    Address       VARCHAR (300)     NOT NULL,
    ImageData     VARBINARY (10000) NULL,
    ImageMimeType VARCHAR (50)      NULL,
    PRIMARY KEY (AccountId ASC)
);

CREATE TABLE `Order` (
    OrderId       INT             AUTO_INCREMENT NOT NULL,
    Username      VARCHAR (50)    NOT NULL,
    Recipient     VARCHAR (50)    NOT NULL,
    Address       VARCHAR (200)   NOT NULL,
    PaymentOption VARCHAR (50)    NOT NULL,
    `Lines`       TEXT		      NOT NULL,
    TotalPrice    DECIMAL (18,2)    NOT NULL,
    Datetime      DATETIME        NOT NULL,
    PRIMARY KEY (OrderId ASC)
);

CREATE TABLE `Product` (
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
insert into account(Username,Password,Name,Address) 
	values('root', 'asd', '管理员', '华星时代广场');
    
-- 添加一个产品
insert into product(Name,Description,price,category)
	values('p4','this is a good ep.',9.9,'hhhhhhhhhhhh');
    
-- 添加一个订单
insert into `order`(username,recipient,address,PaymentOption,`Lines`,TotalPrice,Datetime)
	values('catom.sky','r4','浙江杭州','现金','Line',9.9,'2015-04-09 00:00:00')

 
-- --------------------------------------------------- 3. Edit tables --


































