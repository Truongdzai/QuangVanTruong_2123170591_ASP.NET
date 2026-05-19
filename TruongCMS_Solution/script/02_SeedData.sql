/*
  TruongCMS - Dữ liệu mẫu (Buổi 1 & 2)
  QuangVanTruong
  2123170591
  ngaytao:19/05/2026
*/

USE [TruongCMS_DB];
GO

SET IDENTITY_INSERT dbo.Categories ON;
INSERT INTO dbo.Categories (Id, Name, Description) VALUES
(1, N'Tin tức Công nghệ',   N'Cập nhật xu hướng AI, IoT và lập trình.'),
(2, N'Đời sống du lịch',    N'Kinh nghiệm phượt và các điểm đến hấp dẫn.'),
(3, N'Sức khỏe Thể thao',   N'Các bài tập và chế độ ăn uống lành mạnh.'),
(4, N'Giáo dục Kỹ năng',    N'Phương pháp học tập và kỹ năng mềm.'),
(5, N'Góc lập trình viên',  N'Tài liệu ASP.NET Core và SQL Server.');
SET IDENTITY_INSERT dbo.Categories OFF;
GO

SET IDENTITY_INSERT dbo.Posts ON;
INSERT INTO dbo.Posts (Id, Title, Content, ImageUrl, CreatedDate, CategoryId) VALUES
(1, N'Lộ trình học ASP.NET', N'Hướng dẫn chi tiết cho người mới bắt đầu...',
    N'https://via.placeholder.com/400x200?text=ASP.NET', '2026-04-01', 5),
(2, N'Top 5 bãi biển đẹp', N'Những địa điểm không thể bỏ qua mùa hè này...',
    N'https://via.placeholder.com/400x200?text=Beach', '2026-04-02', 2),
(3, N'Chạy bộ đúng cách', N'Lợi ích tuyệt vời của việc chạy bộ mỗi sáng...',
    N'https://via.placeholder.com/400x200?text=Running', '2026-04-03', 3),
(4, N'AI và tương lai', N'Trí tuệ nhân tạo đang thay đổi cuộc sống...',
    N'https://via.placeholder.com/400x200?text=AI', '2026-04-04', 1),
(5, N'Kỹ năng Teamwork', N'Cách phối hợp hiệu quả trong nhóm dự án...',
    N'https://via.placeholder.com/400x200?text=Team', '2026-04-05', 4);
SET IDENTITY_INSERT dbo.Posts OFF;
GO

SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (Id, Username, PasswordHash, FullName, Role) VALUES
(1, N'admin',     N'123456',   N'Quản trị viên hệ thống', N'Admin'),
(2, N'thai_gv',   N'thai1969', N'Nguyễn Cao Thái',        N'Editor'),
(3, N'sv_01',     N'student1', N'Nguyễn Văn A',           N'User'),
(4, N'sv_02',     N'student2', N'Trần Thị B',             N'User'),
(5, N'moderator', N'mod789',   N'Lê Văn C',               N'Moderator');
SET IDENTITY_INSERT dbo.Users OFF;
GO

-- Dữ liệu mẫu E-commerce (tùy chọn)
SET IDENTITY_INSERT dbo.CategoriesProducts ON;
INSERT INTO dbo.CategoriesProducts (Id, Name, Description) VALUES
(1, N'Thiết bị điện tử', N'Điện thoại, laptop, phụ kiện'),
(2, N'Thời trang',       N'Quần áo, giày dép');
SET IDENTITY_INSERT dbo.CategoriesProducts OFF;
GO

SET IDENTITY_INSERT dbo.Products ON;
INSERT INTO dbo.Products (Id, Name, Description, Price, StockQuantity, ImageUrl, CategoryProductId) VALUES
(1, N'Laptop học tập', N'Cấu hình phù hợp sinh viên IT', 15990000, 10, NULL, 1),
(2, N'Áo thun CMS',    N'Áo in logo dự án',                 199000, 50, NULL, 2);
SET IDENTITY_INSERT dbo.Products OFF;
GO

SET IDENTITY_INSERT dbo.Customers ON;
INSERT INTO dbo.Customers (Id, FullName, Email, Phone, Address, Password) VALUES
(1, N'Nguyễn Văn Khách', N'khach1@email.com', N'0901000001', N'Hà Nội', N'123456');
SET IDENTITY_INSERT dbo.Customers OFF;
GO

PRINT N'Đã nạp dữ liệu mẫu.';
GO

-- Kiểm tra nhanh
SELECT N'Categories' AS [Table], COUNT(*) AS [Rows] FROM dbo.Categories
UNION ALL SELECT N'Posts', COUNT(*) FROM dbo.Posts
UNION ALL SELECT N'Users', COUNT(*) FROM dbo.Users
UNION ALL SELECT N'CategoriesProducts', COUNT(*) FROM dbo.CategoriesProducts
UNION ALL SELECT N'Products', COUNT(*) FROM dbo.Products;
GO
