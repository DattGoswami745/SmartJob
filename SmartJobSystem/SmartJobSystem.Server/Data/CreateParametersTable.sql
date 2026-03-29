CREATE TABLE [dbo].[Parameters] (
    [ParamKey]    NVARCHAR (100) NOT NULL,
    [ParamValue]  NVARCHAR (MAX) NOT NULL,
    [Description] NVARCHAR (255) NULL,
    CONSTRAINT [PK_Parameters] PRIMARY KEY CLUSTERED ([ParamKey] ASC)
);
GO

-- Seed data from appsettings.json
INSERT INTO [dbo].[Parameters] ([ParamKey], [ParamValue], [Description]) VALUES 
('Gemini:ChatApiKey', 'f3Un2NY62H7cBzY0JPtGgfZ9B3OBIYj7DyfuF6NpUOSluQzAo5Uo43sBXx/cyukkYBUramSxn9yh18EECw79mQ==', 'API key for Gemini Chat'),
('Gemini:ResumeApiKey', 'JuKtpIbyqUOPnW03mGWuBWwKDcdAEfxR1w5ZkU8mpJDk9+l7/c2gZzQxPuhHbD857mCn97euchdioMvgbaNtcQ==', 'API key for Gemini Resume Analysis'),
('SmtpSettings:Host', 'goUIFhp3ezziGdRLquUYmGuDOIEZLLFvuPF3nsWid7E=', 'SMTP Server Host'),
('SmtpSettings:Port', '/fFvtpvS0AjLBi7SQSi9ctArtGQG4BmVOGSa0iEtgLI=', 'SMTP Server Port'),
('SmtpSettings:Username', 'ET+YE5mmhgt9zPH69GucNHGRxfeTIBQOOhHiPnBMzn0SIbEE/xsv1CrqyxqQFnnL', 'SMTP Username'),
('SmtpSettings:Password', '0YpC69jI12/6cUeXnAKznFmOYpmMRuyOlNUe4hnIbPJA3cJiuc1gKyVYO29EyJbV', 'SMTP Password'),
('SmtpSettings:FromEmail', 'PdQ14AhbdTQeDFjl6oFVPH6lvZSI6esiMANBR8fKzuJTf3Y31XpN9AEER1YBbKXP', 'Email address to send from'),
('SecuritySettings:EncryptionKey', '1smart7job7system#5', 'Master encryption key for secure parameters');
GO
