﻿/*******************************************************************
DECLARE @count INT
     	,@output INT = 0
		,@fieldName	VARCHAR(255)

EXEC [dbo].[GeneratorFiles_List]
	 @generatorGuid	= '895019CF-1D3E-420C-828F-8971253E5784'
	,@pageSize		= 10
	,@pageNumber	= 1
	,@orderby		= NULL
	,@count			= @count OUTPUT
	,@invokingUser  = 'C1596B8C-7065-4D63-BFD0-4B835B93DFF2'
	,@version		= 'v1'
	,@output		= @output	OUTPUT
	,@fieldName		= @fieldName	OUTPUT

SELECT @count count, @output status, @fieldName fieldName

001	SG-18 18-02-2020 [Nishit Khakhi]	Added Initial Version to get List of Generator Files
*******************************************************************/
CREATE PROCEDURE [dbo].[GeneratorFiles_List]
(	@generatorGuid		UNIQUEIDENTIFIER
	,@search			VARCHAR(100)		= NULL
	,@pageSize			INT
	,@pageNumber		INT
	,@orderby			VARCHAR(100)		= NULL
	,@invokingUser		UNIQUEIDENTIFIER
	,@version			VARCHAR(10)
	,@culture			VARCHAR(10)			= 'en-Us'
	,@output			SMALLINT			OUTPUT
	,@fieldName			VARCHAR(255)		OUTPUT
	,@count				INT OUTPUT
	,@enableDebugInfo		CHAR(1)			= '0'
)
AS
BEGIN
    SET NOCOUNT ON

    IF (@enableDebugInfo = 1)
	BEGIN
        DECLARE @Param XML
        SELECT @Param =
        (
            SELECT 'GeneratorFiles_List' AS '@procName'
            	, CONVERT(VARCHAR(MAX),@generatorGuid) AS '@generatorGuid'
            	, CONVERT(VARCHAR(MAX),@search) AS '@search'
				, CONVERT(VARCHAR(MAX),@pageSize) AS '@pageSize'
				, CONVERT(VARCHAR(MAX),@pageNumber) AS '@pageNumber'
				, CONVERT(VARCHAR(MAX),@orderby) AS '@orderby'
				, CONVERT(VARCHAR(MAX),@version) AS '@version'
            	, CONVERT(VARCHAR(MAX),@invokingUser) AS '@invokingUser'
            FOR XML PATH('Params')
	    )
	    INSERT INTO DebugInfo(data, dt) VALUES(Convert(VARCHAR(MAX), @Param), GETDATE())
    END
    
    BEGIN TRY

		SELECT @output = 1
			  ,@count = -1

		IF OBJECT_ID('tempdb..#temp_GeneratorFiles') IS NOT NULL DROP TABLE #temp_GeneratorFiles

		CREATE TABLE #temp_GeneratorFiles
		(	[guid]				UNIQUEIDENTIFIER
			,[generatorGuid]	UNIQUEIDENTIFIER
			,[generatorName]	NVARCHAR(500)
			,[filePath]			NVARCHAR(500)
			,[description]		NVARCHAR(200)
			,[createdDate]		DATETIME
			,[createdBy]		UNIQUEIDENTIFIER
			,[count]			BIGINT
			,[rowNum]			INT
		)

		IF LEN(ISNULL(@orderby, '')) = 0
		SET @orderby = 'createdDate asc'

		DECLARE @Sql nvarchar(MAX) = ''

		SET @Sql = '
		SELECT
			*
			,ROW_NUMBER() OVER (ORDER BY '+@orderby+') AS rowNum
		FROM
		( SELECT
			G.[guid]
			, GF.[generatorGuid]
			, G.[name]
			, GF.[filePath]
			, GF.[description]
			, GF.[createdDate]
			, GF.[createdBy]
			, 0 AS [count]						
			FROM [dbo].[Generator] G WITH (NOLOCK) 
			INNER JOIN [dbo].[GeneratorFiles] GF WITH (NOLOCK) ON GF.[generatorGuid] = G.[guid]
			WHERE G.[guid] = @generatorGuid AND G.[isDeleted]=0 AND GF.[isDeleted] = 0 '
			+ CASE WHEN @search IS NULL THEN '' ELSE
			' AND (G.[name] LIKE ''%' + @search + '%''
			  OR G.[uniqueId] LIKE ''%' + @search + '%'' 
			  OR GF.[description] LIKE ''%' + @search + '%'' 
			)'
			 END +
		' )  data '
		
		INSERT INTO #temp_GeneratorFiles
		EXEC sp_executesql 
			  @Sql
			, N'@orderby VARCHAR(100), @generatorGuid UNIQUEIDENTIFIER '
			, @orderby		= @orderby			
			, @generatorGuid = @generatorGuid
			
		SET @count = @@ROWCOUNT

		IF(@pageSize <> -1 AND @pageNumber <> -1)
			BEGIN
				SELECT 
					GF.[guid]
					, GF.[generatorGuid]
					, GF.[generatorName]
					, GF.[filePath]
					, GF.[description]
					, GF.[createdDate]
					, GF.[createdBy]
					, GF.[count]					
				FROM #temp_GeneratorFiles GF
				WHERE rowNum BETWEEN ((@pageNumber - 1) * @pageSize) + 1 AND (@pageSize * @pageNumber)			
			END
		ELSE
			BEGIN
					SELECT 
					GF.[guid]
					, GF.[generatorGuid]
					, GF.[generatorName]
					, GF.[filePath]
					, GF.[description]
					, GF.[createdDate]
					, GF.[createdBy]
					, GF.[count]					
				FROM #temp_GeneratorFiles GF
			END
	   
        SET @output = 1
		SET @fieldName = 'Success'
	END TRY	
	BEGIN CATCH	
		DECLARE @errorReturnMessage VARCHAR(MAX)

		SET @output = 0

		SELECT @errorReturnMessage = 
			ISNULL(@errorReturnMessage, '') +  SPACE(1)   + 
			'ErrorNumber:'  + ISNULL(CAST(ERROR_NUMBER() as VARCHAR), '')  + 
			'ErrorSeverity:'  + ISNULL(CAST(ERROR_SEVERITY() as VARCHAR), '') + 
			'ErrorState:'  + ISNULL(CAST(ERROR_STATE() as VARCHAR), '') + 
			'ErrorLine:'  + ISNULL(CAST(ERROR_LINE () as VARCHAR), '') + 
			'ErrorProcedure:'  + ISNULL(CAST(ERROR_PROCEDURE() as VARCHAR), '') + 
			'ErrorMessage:'  + ISNULL(CAST(ERROR_MESSAGE() as VARCHAR(max)), '')
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