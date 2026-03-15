USE SmartJobDB;
GO

-- Dynamic Report Module Schema


-- Table to store report configurations
CREATE TABLE ReportConfigurations (
    ReportId INT PRIMARY KEY IDENTITY(1,1),
    ReportName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    BaseTable NVARCHAR(100) NOT NULL, -- e.g., 'Applications', 'Jobs', 'Users', 'Companies'
    SelectedFields NVARCHAR(MAX) NOT NULL, -- JSON string of fields: [{"id":"FullName", "label":"Full Name", "type":"string"}, ...]
    Filters NVARCHAR(MAX), -- JSON string of default or available filters
    IsActive BIT DEFAULT 1,
    CreatedBy INT,
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME DEFAULT GETUTCDATE()
);

-- Table to log report generation activities
CREATE TABLE ReportGenerationLogs (
    LogId INT PRIMARY KEY IDENTITY(1,1),
    ReportId INT,
    UserId INT,
    GeneratedAt DATETIME DEFAULT GETUTCDATE(),
    Format NVARCHAR(20), -- 'Web', 'Excel', 'PDF'
    FilterValues NVARCHAR(MAX), -- JSON of filters applied during generation
    FOREIGN KEY (ReportId) REFERENCES ReportConfigurations(ReportId)
);
