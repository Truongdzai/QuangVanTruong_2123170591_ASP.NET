/*
  TruongCMS - Truy vấn kiểm tra sau khi tạo DB
  QuangVanTruong
  2123170591
  ngaytao:19/05/2026
*/

USE [TruongCMS_DB];
GO

-- Danh mục tin
SELECT * FROM dbo.Categories;
GO

-- Bài viết kèm tên danh mục
SELECT p.Id, p.Title, c.Name AS CategoryName, p.CreatedDate
FROM dbo.Posts p
INNER JOIN dbo.Categories c ON p.CategoryId = c.Id
ORDER BY p.CreatedDate DESC;
GO

-- Thành viên (không hiển thị mật khẩu trên UI thật)
SELECT Id, Username, FullName, Role FROM dbo.Users;
GO
