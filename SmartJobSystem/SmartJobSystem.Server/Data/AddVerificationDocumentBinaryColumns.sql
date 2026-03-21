USE SmartJobDB;
GO

-- Add binary storage and content type columns to CompanyVerificationDocuments
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CompanyVerificationDocuments') AND name = 'vDocumentFile')
BEGIN
    ALTER TABLE dbo.CompanyVerificationDocuments ADD vDocumentFile VARBINARY(MAX);
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CompanyVerificationDocuments') AND name = 'vDocumentContentType')
BEGIN
    ALTER TABLE dbo.CompanyVerificationDocuments ADD vDocumentContentType NVARCHAR(100);
END
GO
