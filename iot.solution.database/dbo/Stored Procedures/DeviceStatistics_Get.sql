/*******************************************************************
DECLARE @output INT = 0
		,@fieldName	nvarchar(255)
		,@syncDate	DATETIME
EXEC [dbo].[DeviceStatistics_Get]
	 @guid				= '553C958A-7BAA-452A-93CA-59670AFFBAD4'	
	,@invokingUser  	= '7D31E738-5E24-4EA2-AAEF-47BB0F3CCD41'
	,@version			= 'v1'
	,@output			= @output		OUTPUT
	,@fieldName			= @fieldName	OUTPUT	
	,@syncDate			= @syncDate		OUTPUT
               
 SELECT @output status,  @fieldName AS fieldName, @syncDate syncDate    
 
001	SG-18 20-04-2020 [Nishit Khakhi]	Added Initial Version to Get Location Statistics
*******************************************************************/
create PROCEDURE [dbo].[DeviceStatistics_Get]
(	 @guid				UNIQUEIDENTIFIER	
	,@invokingUser		UNIQUEIDENTIFIER	= NULL
	,@version			NVARCHAR(10)
	,@output			SMALLINT		  OUTPUT
	,@fieldName			NVARCHAR(255)	  OUTPUT
	,@syncDate			DATETIME		  OUTPUT
	,@culture			NVARCHAR(10)	  = 'en-Us'
	,@enableDebugInfo	CHAR(1)			  = '0'
)
AS
BEGIN
    SET NOCOUNT ON
	IF (@enableDebugInfo = 1)
	BEGIN
        DECLARE @Param XML
        SELECT @Param =
        (
            SELECT 'DeviceStatistics_Get' AS '@procName'
			, CONVERT(nvarchar(MAX),@guid) AS '@guid'			
	        , CONVERT(nvarchar(MAX),@invokingUser) AS '@invokingUser'
			, CONVERT(nvarchar(MAX),@version) AS '@version'
			, CONVERT(nvarchar(MAX),@output) AS '@output'
            , CONVERT(nvarchar(MAX),@fieldName) AS '@fieldName'
            FOR XML PATH('Params')
	    )
	    INSERT INTO DebugInfo(data, dt) VALUES(Convert(nvarchar(MAX), @Param), GETUTCDATE())
    END
    Set @output = 1
    SET @fieldName = 'Success'

    BEGIN TRY
		SET @syncDate = (SELECT TOP 1 CONVERT(DATETIME,[value]) FROM dbo.[Configuration] (NOLOCK) WHERE [configKey] = 'telemetry-last-exectime')
		;WITH CTE_attribute
		AS 
		(	SELECT [attribute],D.[guid],SUM([sum]) AS [value] 
			FROM [dbo].[TelemetrySummary_Hourwise] T (NOLOCK)
			INNER JOIN [dbo].[Generator] D (NOLOCK) ON T.[gensetGuid] = D.[guid] AND D.[isDeleted] = 0
			WHERE D.[guid] = @guid AND T.[attribute] IN ('currentOut','fuelused')
			GROUP BY [attribute],D.[guid]
		)
		,CTE_AvgAttribute
		AS 
		(	SELECT [attribute],D.[guid],AVG([avg]) AS [value] 
			FROM [dbo].[TelemetrySummary_Hourwise] T (NOLOCK)
			INNER JOIN [dbo].[Generator] D (NOLOCK) ON T.[gensetGuid] = D.[guid] AND D.[isDeleted] = 0
			WHERE D.[guid] = @guid AND T.[attribute] IN ('engRPM','volt')
			GROUP BY [attribute],D.[guid]
		)
		,CTE_LatestAttribute
		AS 
		(	SELECT [attribute],D.[guid],ROW_NUMBER() OVER(PARTITION BY T.[gensetGuid], [attribute] ORDER BY T.[date]) AS [no], [latest] AS [value] 
			FROM [dbo].[TelemetrySummary_Hourwise] T (NOLOCK)
			INNER JOIN [dbo].[Generator] D (NOLOCK) ON T.[gensetGuid] = D.[guid] AND D.[isDeleted] = 0
			WHERE D.[guid] = @guid AND T.[attribute] IN ('battlevel','fuelLevel')
		)
		SELECT C.[guid]
				, ISNULL(Energy.[value],0) AS [totalCurrent]
				, ISNULL(Fuel.[value],0) AS [totalFuelUsed]
				, ISNULL(volt.[value],0) AS [avgVolt]
				, ISNULL(engRPM.[value],0) AS [avgRPM]
				, ISNULL(battlevel.[value],0) AS [latestBattlevel]
				, ISNULL(fuelLevel.[value],0) AS [latestFuelLevel]
		FROM [dbo].[Generator] C (NOLOCK) 
		LEFT JOIN CTE_attribute Energy ON C.[guid] = Energy.[guid] AND Energy.[attribute] = 'currentOut'
		LEFT JOIN CTE_attribute Fuel ON C.[guid] = Fuel.[guid] AND Fuel.[attribute] = 'fuelused'
		LEFT JOIN CTE_AvgAttribute engRPM ON C.[guid] = engRPM.[guid] AND engRPM.[attribute] = 'engRPM'
		LEFT JOIN CTE_AvgAttribute volt ON C.[guid] = volt.[guid] AND volt.[attribute] = 'volt'
		LEFT JOIN CTE_LatestAttribute battlevel ON C.[guid] = battlevel.[guid] AND battlevel.[attribute] = 'battlevel' AND battlevel.[no] = 1
		LEFT JOIN CTE_LatestAttribute fuelLevel ON C.[guid] = fuelLevel.[guid] AND fuelLevel.[attribute] = 'fuelLevel' AND fuelLevel.[no] = 1
		WHERE C.[guid]=@guid AND C.[isDeleted]=0
		
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