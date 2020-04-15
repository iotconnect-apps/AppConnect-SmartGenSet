/*******************************************************************
DECLARE @count INT
     ,@output INT = 0
	,@fieldName				nvarchar(255)	
EXEC [dbo].[Chart_FuelUsed]	
	@entityguid	= '415C8959-5BFC-4203-A493-F89458AE7736'
	--@guid	= '46B0B123-D4BB-4DE2-ABB2-D15BC854B39D'
	,@invokinguser  = 'E05A4DA0-A8C5-4A4D-886D-F61EC802B5FD'              
	,@version		= 'v1'              
	,@output		= @output		OUTPUT
	,@fieldname		= @fieldName	OUTPUT	

SELECT @output status, @fieldName fieldName

001	SG-18 16-03-2020 [Nishit Khakhi]	Added Initial Version to represent Fuel Used

*******************************************************************/

CREATE PROCEDURE [dbo].[Chart_FuelUsed]
(	@entityguid		UNIQUEIDENTIFIER	= NULL	
	,@guid				UNIQUEIDENTIFIER	= NULL	
	,@invokinguser		UNIQUEIDENTIFIER	= NULL
	,@version			nvarchar(10)              
	,@output			SMALLINT			OUTPUT
	,@fieldname			nvarchar(255)		OUTPUT
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
    
    BEGIN TRY            
		IF @guid IS NOT NULL
		BEGIN
			SELECT CONVERT(NVARCHAR(3),[date],100) AS [month],[gensetGuid]  AS [guid],[attribute],SUM([sum]) AS [value]
			FROM [dbo].[TelemetrySummary_Daywise]
			WHERE [gensetGuid] = @guid AND [attribute] = 'fuellevel'
			GROUP BY CONVERT(NVARCHAR(3),[date],100),[gensetGuid],[attribute]
		END
		ELSE IF @entityguid IS NOT NULL
		BEGIN
			SELECT CONVERT(NVARCHAR(3),[date],100) AS [month],H.[locationGuid]  AS [guid],[attribute],SUM([sum]) AS [value]
			FROM [dbo].[TelemetrySummary_Daywise] T (NOLOCK)
			INNER JOIN [dbo].[HardwareKit] H (NOLOCK) ON T.[gensetGuid] = H.[guid] AND H.[isDeleted] = 0
			WHERE H.[locationGuid] = @entityguid AND [attribute] = 'fuellevel'
			GROUP BY CONVERT(NVARCHAR(3),[date],100),H.[locationGuid],[attribute]
		END

        SET @output = 1
		SET @fieldname = 'Success'   
              
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
