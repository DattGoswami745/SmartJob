USE SmartJobDB;
GO

-- Increase BaseTable length to support large subqueries
ALTER TABLE ReportConfigurations ALTER COLUMN BaseTable NVARCHAR(MAX) NOT NULL;
GO
