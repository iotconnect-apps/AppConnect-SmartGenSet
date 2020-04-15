﻿
/*******************************************************************
DECLARE 
     @output INT = 0
	,@fieldName				nvarchar(255)
EXEC [dbo].[Generator_Lookup]		
	@companyGuid		= '895019CF-1D3E-420C-828F-8971253E5784'	
	,@locationGuid		= '895019CF-1D3E-420C-828F-8971253E5784'	
	,@invokingUser  	= '7D31E738-5E24-4EA2-AAEF-47BB0F3CCD41'
	,@version			= 'v1'
	,@output			= @output		OUTPUT
	,@fieldName			= @fieldName	OUTPUT	
               
 SELECT @output status,  @fieldName AS fieldName    
 
 001	sgh-1 05-12-2019 [Nishit Khakhi]	Added Initial Version to Lookup Generator
*******************************************************************/

CREATE PROCEDURE [dbo].[Generator_Lookup]
(	 @companyGuid		UNIQUEIDENTIFIER
	,@locationGuid		UNIQUEIDENTIFIER	= NULL
	,@invokingUser		UNIQUEIDENTIFIER
	,@version			NVARCHAR(10)
	,@output			SMALLINT		  OUTPUT
	,@fieldName			NVARCHAR(255)	  OUTPUT	
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
            SELECT 'Generator_Lookup' AS '@procName'			
			, CONVERT(nvarchar(MAX),@companyGuid) AS '@companyGuid'
			, CONVERT(nvarchar(MAX),@locationGuid) AS '@locationGuid'
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

    IF NOT EXISTS (SELECT TOP 1 1 FROM [dbo].[Generator] (NOLOCK) WHERE companyGuid=@companyGuid AND [locationGuid] = ISNULL(@locationGuid,[locationGuid]) AND [isDeleted]=0)
	BEGIN
		Set @output = -3
		SET @fieldName = 'GeneratorNotFound'
		RETURN;
	END
  
    BEGIN TRY
		SELECT G.[guid]
			  ,G.[name]
			  ,GT.[name] AS [type]
		FROM [dbo].[Generator] G (NOLOCK) 
		INNER JOIN [dbo].[GeneratorType] GT (NOLOCK) ON G.[typeGuid] = GT.[guid] AND GT.[isDeleted] = 0
		WHERE G.[companyguid]=@companyGuid AND G.[locationGuid] = ISNULL(@locationGuid,G.[locationGuid]) AND G.[isDeleted] = 0 
		ORDER BY G.[name]

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