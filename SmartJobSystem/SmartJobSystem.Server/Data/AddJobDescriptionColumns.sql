-- SQL Migration: Add Job Description Columns to Jobs Table
-- Description: Adds support for file-based and text-based job descriptions.

USE SmartJobDB;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Jobs]') AND name = N'JobDescriptionFile')
BEGIN
    ALTER TABLE [dbo].[Jobs] ADD [JobDescriptionFile] NVARCHAR(MAX) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Jobs]') AND name = N'JobDescriptionText')
BEGIN
    ALTER TABLE [dbo].[Jobs] ADD [JobDescriptionText] NVARCHAR(MAX) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Jobs]') AND name = N'JobDescriptionUpdatedAt')
BEGIN
    ALTER TABLE [dbo].[Jobs] ADD [JobDescriptionUpdatedAt] DATETIME NULL;
END
GO
