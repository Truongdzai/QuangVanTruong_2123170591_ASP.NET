/*
  TruongCMS - Buổi 2
  Tạo database (chạy trên master)
  QuangVanTruong
  2123170591
  ngaytao:19/05/2026
*/

IF DB_ID(N'TruongCMS_DB') IS NOT NULL
BEGIN
    ALTER DATABASE [TruongCMS_DB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [TruongCMS_DB];
END
GO

CREATE DATABASE [TruongCMS_DB];
GO

USE [TruongCMS_DB];
GO

PRINT N'Đã tạo database TruongCMS_DB.';
GO
