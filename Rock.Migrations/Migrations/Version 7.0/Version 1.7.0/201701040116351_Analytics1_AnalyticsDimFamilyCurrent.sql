IF OBJECT_ID(N'[dbo].[AnalyticsDimFamilyCurrent]', 'V') IS NOT NULL
    DROP VIEW [dbo].[AnalyticsDimFamilyCurrent]
GO

CREATE VIEW [dbo].[AnalyticsDimFamilyCurrent]
AS
SELECT * FROM AnalyticsDimFamilyHistorical where CurrentRowIndicator = 1