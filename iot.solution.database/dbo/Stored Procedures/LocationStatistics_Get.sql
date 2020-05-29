/*******************************************************************
DECLARE @output INT = 0
		,@fieldName				nvarchar(255)
	    ,@syncDate	DATETIME
EXEC [dbo].[LocationStatistics_Get]
	 @guid				= '91C20C3F-FC30-41D2-9BDF-435DFAF5BAB2'	
	,@invokingUser  	= '7D31E738-5E24-4EA2-AAEF-47BB0F3CCD41'
	,@version			= 'v1'
	,@output			= @output		OUTPUT
	,@fieldName			= @fieldName	OUTPUT	
	,@syncDate		= @syncDate		OUTPUT
               
 SELECT @output status,  @fieldName AS fieldName, @syncDate syncDate   
 
001	SG-18 20-04-2020 [Nishit Khakhi]	Added Initial Version to Get Location Statistics
*******************************************************************/
create PROCEDURE [dbo].[LocationStatistics_Get]
(	 @guid				UNIQUEIDENTIFIER	
	,@invokingUser		UNIQUEIDENTIFIER	= NULL
	,@version			NVARCHAR(10)
	,@output			SMALLINT		  OUTPUT
	,@fieldName			NVARCHAR(255)	  OUTPUT
	,@syncDate			DATETIME			OUTPUT
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
            SELECT 'LocationStatistics_Get' AS '@procName'
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
		(	SELECT [attribute],D.[locationGuid],SUM([sum]) AS [value] 
			FROM [dbo].[TelemetrySummary_Hourwise] T (NOLOCK)
			INNER JOIN [dbo].[Generator] D (NOLOCK) ON T.[gensetGuid] = D.[guid] AND D.[isDeleted] = 0
			WHERE D.[locationGuid] = @guid AND T.[attribute] IN ('currentOut','fuelused') AND CONVERT(DATE,T.[date]) = CONVERT(DATE,GETUTCDATE())
			GROUP BY [attribute],D.[locationGuid]
		)
		,CTE_DeviceCount
		AS (	SELECT [locationGuid]
						, SUM(CASE WHEN [isProvisioned] = 1 THEN 1 ELSE 0 END) [totalOnGenerators] 
						, SUM(CASE WHEN [isProvisioned] = 0 THEN 1 ELSE 0 END) [totalOffGenerators] 
						, SUM(CASE WHEN [isActive] = 0 THEN 1 ELSE 0 END) [totalDisconnectedGenerators] 
				FROM [dbo].[Generator] (NOLOCK) 
				WHERE [locationGuid] = @guid AND [isDeleted] = 0
				GROUP BY [locationGuid]
		)
		,CTE_AlertCount
		AS (	SELECT [entityGuid], COUNT(1) [totalCount]  
				FROM [dbo].[IOTConnectAlert] (NOLOCK) 
				WHERE [entityGuid] = @guid 
				GROUP BY [entityGuid]
		)
		SELECT [guid]
				, ISNULL(Energy.[value],0) AS [totalEnergyGenerated]
				, ISNULL(Fuel.[value],0) AS [totalFuelUsed]
				, ISNULL(A.[totalCount],0) AS [totalAlerts]
				, ISNULL(CD.[totalDisconnectedGenerators],0) AS [totalDisconnectedGenerators]
				, ISNULL(CD.[totalDisconnectedGenerators],0) + ISNULL(CD.[totalOffGenerators],0) + ISNULL(CD.[totalOnGenerators],0) AS [totalGenerators]
				, ISNULL(CD.[totalOffGenerators],0) AS [totalOffGenerators]
				, ISNULL(CD.[totalOnGenerators],0) AS [totalOnGenerators]
		FROM [dbo].[Location] C (NOLOCK) 
		LEFT JOIN CTE_DeviceCount CD ON C.[guid] = CD.[locationGuid]
		LEFT JOIN CTE_AlertCount A ON C.[guid] = A.[entityGuid]
		LEFT JOIN CTE_attribute Energy ON C.[guid] = Energy.[locationGuid] AND Energy.[attribute] = 'currentOut'
		LEFT JOIN CTE_attribute Fuel ON C.[guid] = Fuel.[locationGuid] AND Fuel.[attribute] = 'fuelused'
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