USE SmartJobDB;
GO

-- Standardize Application Report Configuration
UPDATE ReportConfigurations
SET 
    SelectedFields = '[{"id":"[User Name]","label":"User Name","type":"string"},{"id":"[User Email]","label":"User Email","type":"string"},{"id":"[Job Title]","label":"Job Title","type":"string"},{"id":"[Company Name]","label":"Company Name","type":"string"},{"id":"[Applied Date]","label":"Applied Date","type":"date"},{"id":"[Status]","label":"Status","type":"string"}]'
WHERE ReportName LIKE '%Application%';

-- Standardize Jobs Report Configuration
UPDATE ReportConfigurations
SET 
    SelectedFields = '[{"id":"[Job Title]","label":"Job Title","type":"string"},{"id":"[Company Name]","label":"Company Name","type":"string"},{"id":"[Job Type]","label":"Job Type","type":"string"},{"id":"[Salary Range]","label":"Salary Range","type":"string"},{"id":"[Posted Date]","label":"Posted Date","type":"date"}]'
WHERE ReportName LIKE '%Job%';
GO
