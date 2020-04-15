
CREATE PROCEDURE [dbo].[ChartDate]
(	
	@companyguid UNIQUEIDENTIFIER = NULL
	, @greenhouseguid UNIQUEIDENTIFIER = NULL
	, @hardwarekitguid UNIQUEIDENTIFIER = NULL
)
AS
BEGIN
    SET NOCOUNT ON

    SELECT 'Jan' AS [Month], 10 AS [Value] 
	UNION ALL SELECT 'Fab' AS [Month], '11' AS [Value] 
	UNION ALL SELECT 'Mar' AS [Month], '8' AS [Value] 
	UNION ALL SELECT 'Apr' AS [Month], '12' AS [Value] 
	UNION ALL SELECT 'May' AS [Month], '10' AS [Value] 
	UNION ALL SELECT 'Jun' AS [Month], '11' AS [Value] 
	UNION ALL SELECT 'Jul' AS [Month], '6' AS [Value] 
	UNION ALL SELECT 'Aug' AS [Month], '8' AS [Value] 
	UNION ALL SELECT 'Sep' AS [Month], '14' AS [Value] 
	UNION ALL SELECT 'Oct' AS [Month], '10' AS [Value] 
	UNION ALL SELECT 'Nov' AS [Month], '8' AS [Value] 
	UNION ALL SELECT 'Dec' AS [Month], '10' AS [Value] 

END