/*
  TruongCMS - Xóa toàn bộ database
  Chạy khi cần tạo lại từ đầu
  QuangVanTruong
  2123170591
  ngaytao:19/05/2026
*/

USE [master];
GO

IF DB_ID(N'TruongCMS_DB') IS NOT NULL
BEGIN
    ALTER DATABASE [TruongCMS_DB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [TruongCMS_DB];
    PRINT N'Đã xóa database TruongCMS_DB.';
END
ELSE
    PRINT N'Database TruongCMS_DB không tồn tại.';
GO
