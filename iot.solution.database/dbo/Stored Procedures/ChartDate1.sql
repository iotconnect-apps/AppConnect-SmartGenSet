

CREATE PROCEDURE [dbo].[ChartDate1]
(	
	@companyguid UNIQUEIDENTIFIER = NULL
	, @greenhouseguid UNIQUEIDENTIFIER = NULL
	, @hardwarekitguid UNIQUEIDENTIFIER = NULL
)
AS
BEGIN
    SET NOCOUNT ON

              SELECT '5' AS [PHLevel], '10' AS [N] , '5' AS [P] ,  '6' AS [K] 
	UNION ALL SELECT '6' AS [PHLevel], '11' AS [N] , '6' AS [P] ,  '7' AS [K] 
	UNION ALL SELECT '7' AS [PHLevel], '8' AS  [N] , '4' AS  [P] , '5' AS  [K] 
	UNION ALL SELECT '8' AS [PHLevel], '12' AS [N] , '8' AS [P] ,  '6' AS [K] 
	UNION ALL SELECT '9' AS [PHLevel], '10' AS [N] , '6' AS [P] ,  '4' AS [K] 

END