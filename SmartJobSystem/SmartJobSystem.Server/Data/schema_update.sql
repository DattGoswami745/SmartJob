USE SmartJobDB;
GO

-- Create CompanyVerificationDocuments table in dbo schema
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CompanyVerificationDocuments' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.CompanyVerificationDocuments (
        nDocumentId BIGINT IDENTITY PRIMARY KEY,
        nCompanyId BIGINT NOT NULL,
        vDocumentType VARCHAR(100) NOT NULL, -- 'Incorporation', 'GST', 'PAN'
        vFileName VARCHAR(500) NOT NULL,
        vFilePath VARCHAR(1000) NOT NULL,
        IsVerified BIT DEFAULT 0,
        nVerifiedBy BIGINT,
        dVerifiedOnUTC DATETIME,
        IsRejected BIT DEFAULT 0,
        vRejectReason VARCHAR(500),
        nRecordedBy BIGINT,
        dRecordedOnUTC DATETIME DEFAULT GETUTCDATE()
    );
END
GO

-- Add IsCompanyVerified to Companies table if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Companies') AND name = 'IsCompanyVerified')
BEGIN
    ALTER TABLE dbo.Companies ADD IsCompanyVerified BIT DEFAULT 0;
END
GO
