/*
  TruongCMS - Tạo 8 bảng (theo EF Core Migration InitialCreate)
  Chạy sau 00_CreateDatabase.sql
*/

USE [TruongCMS_DB];
GO

-- Xóa bảng cũ (nếu chạy lại script)
IF OBJECT_ID(N'dbo.OrderDetails', N'U') IS NOT NULL DROP TABLE dbo.OrderDetails;
IF OBJECT_ID(N'dbo.Posts', N'U') IS NOT NULL DROP TABLE dbo.Posts;
IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID(N'dbo.Categories', N'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID(N'dbo.Customers', N'U') IS NOT NULL DROP TABLE dbo.Customers;
IF OBJECT_ID(N'dbo.CategoriesProducts', N'U') IS NOT NULL DROP TABLE dbo.CategoriesProducts;
GO

-- 1. Danh mục tin (CMS)
CREATE TABLE dbo.Categories (
    Id          INT IDENTITY(1,1) NOT NULL,
    Name        NVARCHAR(MAX)     NOT NULL,
    Description NVARCHAR(MAX)     NULL,
    CONSTRAINT PK_Categories PRIMARY KEY (Id)
);
GO

-- 2. Danh mục sản phẩm (E-commerce)
CREATE TABLE dbo.CategoriesProducts (
    Id          INT IDENTITY(1,1) NOT NULL,
    Name        NVARCHAR(100)     NOT NULL,
    Description NVARCHAR(MAX)     NULL,
    CONSTRAINT PK_CategoriesProducts PRIMARY KEY (Id)
);
GO

-- 3. Khách hàng
CREATE TABLE dbo.Customers (
    Id       INT IDENTITY(1,1) NOT NULL,
    FullName NVARCHAR(MAX) NOT NULL,
    Email    NVARCHAR(MAX) NOT NULL,
    Phone    NVARCHAR(MAX) NULL,
    Address  NVARCHAR(MAX) NULL,
    Password NVARCHAR(MAX) NOT NULL,
    CONSTRAINT PK_Customers PRIMARY KEY (Id)
);
GO

-- 4. Người dùng quản trị
CREATE TABLE dbo.Users (
    Id           INT IDENTITY(1,1) NOT NULL,
    Username     NVARCHAR(MAX) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FullName     NVARCHAR(MAX) NOT NULL,
    Role         NVARCHAR(MAX) NOT NULL,
    CONSTRAINT PK_Users PRIMARY KEY (Id)
);
GO

-- 5. Bài viết
CREATE TABLE dbo.Posts (
    Id          INT IDENTITY(1,1) NOT NULL,
    Title       NVARCHAR(MAX) NOT NULL,
    Content     NVARCHAR(MAX) NOT NULL,
    ImageUrl    NVARCHAR(MAX) NULL,
    CreatedDate DATETIME2     NOT NULL,
    CategoryId  INT           NOT NULL,
    CONSTRAINT PK_Posts PRIMARY KEY (Id),
    CONSTRAINT FK_Posts_Categories_CategoryId
        FOREIGN KEY (CategoryId) REFERENCES dbo.Categories (Id)
);
GO

CREATE INDEX IX_Posts_CategoryId ON dbo.Posts (CategoryId);
GO

-- 6. Sản phẩm
CREATE TABLE dbo.Products (
    Id                INT IDENTITY(1,1) NOT NULL,
    Name              NVARCHAR(MAX) NOT NULL,
    Description       NVARCHAR(MAX) NULL,
    Price             DECIMAL(18,2) NOT NULL,
    StockQuantity     INT           NOT NULL,
    ImageUrl          NVARCHAR(MAX) NULL,
    CategoryProductId INT           NOT NULL,
    CONSTRAINT PK_Products PRIMARY KEY (Id),
    CONSTRAINT FK_Products_CategoriesProducts_CategoryProductId
        FOREIGN KEY (CategoryProductId) REFERENCES dbo.CategoriesProducts (Id)
);
GO

CREATE INDEX IX_Products_CategoryProductId ON dbo.Products (CategoryProductId);
GO

-- 7. Đơn hàng
CREATE TABLE dbo.Orders (
    Id         INT IDENTITY(1,1) NOT NULL,
    OrderDate  DATETIME2     NOT NULL,
    CustomerId INT           NOT NULL,
    Status     INT           NOT NULL,  -- 0: Chờ duyệt, 1: Đang giao, 2: Hoàn thành
    Notes      NVARCHAR(MAX) NULL,
    CONSTRAINT PK_Orders PRIMARY KEY (Id),
    CONSTRAINT FK_Orders_Customers_CustomerId
        FOREIGN KEY (CustomerId) REFERENCES dbo.Customers (Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_Orders_CustomerId ON dbo.Orders (CustomerId);
GO

-- 8. Chi tiết đơn hàng
CREATE TABLE dbo.OrderDetails (
    Id        INT IDENTITY(1,1) NOT NULL,
    OrderId   INT           NOT NULL,
    ProductId INT           NOT NULL,
    Quantity  INT           NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_OrderDetails PRIMARY KEY (Id),
    CONSTRAINT FK_OrderDetails_Orders_OrderId
        FOREIGN KEY (OrderId) REFERENCES dbo.Orders (Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetails_Products_ProductId
        FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_OrderDetails_OrderId ON dbo.OrderDetails (OrderId);
CREATE INDEX IX_OrderDetails_ProductId ON dbo.OrderDetails (ProductId);
GO

PRINT N'Đã tạo xong 8 bảng.';
GO
