/*******************************************************************
DECLARE @count INT
    ,@output INT = 0
	,@fieldName	nvarchar(255)
	,@syncDate	DATETIME
EXEC [dbo].[Chart_FuelUsed]	
	@entityguid	= '415C8959-5BFC-4203-A493-F89458AE7736'
	--@guid	= '46B0B123-D4BB-4DE2-ABB2-D15BC854B39D'
	,@invokinguser  = 'E05A4DA0-A8C5-4A4D-886D-F61EC802B5FD'              
	,@version		= 'v1'              
	,@output		= @output		OUTPUT
	,@fieldname		= @fieldName	OUTPUT	
	,@syncDate		= @syncDate		OUTPUT

SELECT @output status, @fieldName fieldName, @syncDate syncDate

001	SG-18 16-03-2020 [Nishit Khakhi]	Added Initial Version to represent Fuel Used

*******************************************************************/

CREATE PROCEDURE [dbo].[Chart_FuelUsed]
(	@entityguid		UNIQUEIDENTIFIER	= NULL	
	,@guid				UNIQUEIDENTIFIER	= NULL	
	,@invokinguser		UNIQUEIDENTIFIER	= NULL
	,@version			nvarchar(10)              
	,@output			SMALLINT			OUTPUT
	,@fieldname			nvarchar(255)		OUTPUT
	,@syncDate			DATETIME			OUTPUT
	,@culture			nvarchar(10)		= 'en-Us'	
	,@enabledebuginfo	CHAR(1)				= '0'
)
AS
BEGIN
    SET NOCOUNT ON

    IF (@enabledebuginfo = 1)
	BEGIN
        DECLARE @Param XML 
        SELECT @Param = 
        (
            SELECT 'Chart_FuelUsed' AS '@procName' 
           , CONVERT(nvarchar(MAX),@entityguid) AS '@entityguid' 
			, CONVERT(nvarchar(MAX),@guid) AS '@guid' 
            , CONVERT(nvarchar(MAX),@version) AS '@version' 
            , CONVERT(nvarchar(MAX),@invokinguser) AS '@invokinguser' 
            FOR XML PATH('Params')
	    ) 
	    INSERT INTO DebugInfo(data, dt) VALUES(Convert(nvarchar(MAX), @Param), GETUTCDATE())
    END                    
    DECLARE @startDate DATETIME, @endDate DATETIME
	IF OBJECT_ID ('tempdb..#months') IS NOT NULL BEGIN DROP TABLE #months END
		CREATE TABLE [#months] ([date] DATETIME)

		INSERT INTO [#months]
		SELECT CONVERT(DATE, DATEADD(Month, (T.i - 11), GETUTCDATE())) AS [Date]
		FROM (VALUES (11), (10), (9), (8), (7), (6), (5), (4), (3), (2), (1), (0)) AS T(i)

		SELECT @startDate = MIN(CONVERT(DATE, [Date] - DAY([Date]) + 1)), @endDate = MAX(CONVERT(DATE,EOMONTH([Date])))
		FROM [#months]

	IF OBJECT_ID ('tempdb..#result') IS NOT NULL BEGIN DROP TABLE #result END
	CREATE TABLE #result ([year] INT, [month] TINYINT, [localName] NVARCHAR(1000), [value] DECIMAL(18,2))

    BEGIN TRY            
		IF @guid IS NOT NULL
		BEGIN
			INSERT INTO #result
			SELECT DATEPART(YY,[date]),DATEPART(MM,[date]),[attribute], SUM([sum]) AS [value]
			FROM [dbo].[TelemetrySummary_Hourwise]
			WHERE [gensetGuid] = @guid ANd ([date] BETWEEN @startDate AND @endDate) AND [attribute] = 'fuelUsed'
			GROUP BY DATEPART(YY,[date]),DATEPART(MM,[date]),[attribute]
		END
		ELSE IF @entityguid IS NOT NULL
		BEGIN
			INSERT INTO #result
			SELECT DATEPART(YY,[date]),DATEPART(MM,[date]),[attribute], SUM([sum]) AS [value]
			FROM [dbo].[TelemetrySummary_Hourwise] T (NOLOCK)
			INNER JOIN [dbo].[Generator] H (NOLOCK) ON T.[gensetGuid] = H.[guid] AND H.[isDeleted] = 0
			WHERE H.[locationGuid] = @entityguid ANd ([date] BETWEEN @startDate AND @endDate) AND [attribute] = 'fuelUsed'
			GROUP BY DATEPART(YY,[date]),DATEPART(MM,[date]),[attribute]
		END

		SELECT SUBSTRING(DATENAME(MONTH, M.[date]), 1, 3) + '-' + FORMAT(M.[date],'yy') AS [month]
				, ISNULL(R.[value],0) [value]
		FROM [#months] M
		LEFT OUTER JOIN #result R ON R.[Month] = DATEPART(MM, M.[date]) AND R.[Year] = DATEPART(YY, M.[date]) 
		ORDER BY  M.[date]

		--SELECT [month],[localName],[value]
		--FROM #result
		--ORDER BY [YY],[MM]

        SET @output = 1
		SET @fieldname = 'Success'   
		SET @syncDate = (SELECT TOP 1 CONVERT(DATETIME,[value]) FROM dbo.[Configuration] (NOLOCK) WHERE [configKey] = 'telemetry-last-exectime')
              
	END TRY	
	BEGIN CATCH	
		DECLARE @errorReturnMessage nvarchar(MAX)

		SET @output = 0

		SELECT @errorReturnMessage = 
			ISNULL(@errorReturnMessage, '') +  SPACE(1)   + 
			'ErrorNumber:'  + ISNULL(CAST(ERROR_NUMBER() as nvarchar), '')  + 
			'ErrorSeverity:'  + ISNULL(CAST(ERROR_SEVERITY() as nvarchar), '') + 
			'ErrorState:'  + ISNULL(CAST(ERROR_STATE() as nvarchar), '') + 
			'ErrorLine:'  + ISNULL(CAST(ERROR_LINE () as nvarchar), '') + 
			'ErrorProcedure:'  + ISNULL(CAST(ERROR_PROCEDURE() as nvarchar), '') + 
			'ErrorMessage:'  + ISNULL(CAST(ERROR_MESSAGE() as nvarchar(max)), '')
		RAISERROR (@errorReturnMessage, 11, 1)  
 
		IF (XACT_STATE()) = -1  
		BEGIN   
			ROLLBACK TRANSACTION 
		END   
		IF (XACT_STATE()) = 1  
		BEGIN      
			ROLLBACK TRANSACTION  
		END   
		RAISERROR (@errorReturnMessage, 11, 1)   
	END CATCH
END
