/*******************************************************************
DECLARE @count INT
     	,@output INT = 0
		,@fieldName	VARCHAR(255)

EXEC [dbo].[Location_List]
	 @companyGuid	= 'AB469212-2488-49AD-BC94-B3A3F45590D2'
	,@pageSize		= 10
	,@pageNumber	= 1
	,@orderby		= NULL
	,@count			= @count OUTPUT
	,@invokingUser  = 'C1596B8C-7065-4D63-BFD0-4B835B93DFF2'
	,@version		= 'v1'
	,@output		= @output	OUTPUT
	,@fieldName		= @fieldName	OUTPUT

SELECT @count count, @output status, @fieldName fieldName

001	sgh-1 04-12-2019 [Nishit Khakhi]	Added Initial Version to List Location
*******************************************************************/
CREATE PROCEDURE [dbo].[Location_List]
(   @companyGuid		UNIQUEIDENTIFIER
	,@search			VARCHAR(100)		= NULL
	,@pageSize			INT
	,@pageNumber		INT
	,@orderby			VARCHAR(100)		= NULL
	,@invokingUser		UNIQUEIDENTIFIER
	,@version			VARCHAR(10)
	,@culture			VARCHAR(10)			= 'en-Us'
	,@output			SMALLINT			OUTPUT
	,@fieldName			VARCHAR(255)		OUTPUT
	,@count				INT					OUTPUT
	,@enableDebugInfo	CHAR(1)				= '0'
)
AS
BEGIN
    SET NOCOUNT ON

    IF (@enableDebugInfo = 1)
	BEGIN
        DECLARE @Param XML
        SELECT @Param =
        (
            SELECT 'Location_List' AS '@procName'
            	, CONVERT(VARCHAR(MAX),@companyGuid) AS '@companyGuid'
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

		SET	@output = 1
		SET @count = -1

		IF OBJECT_ID('tempdb..#temp_Location') IS NOT NULL DROP TABLE #temp_Location

		CREATE TABLE #temp_Location
		(	[guid]						UNIQUEIDENTIFIER
			,[name]						NVARCHAR(500)
			,[description]				NVARCHAR(1000)
			,[address]					NVARCHAR(500)
			,[address2]					NVARCHAR(500)
			,[city]						NVARCHAR(50)
			,[zipCode]					NVARCHAR(10)
			,[stateGuid]				UNIQUEIDENTIFIER NULL
			,[countryGuid]				UNIQUEIDENTIFIER NULL
			,[image]					NVARCHAR(250)
			,[latitude]					NVARCHAR(50)
			,[longitude]				NVARCHAR(50)
			,[isActive]					BIT
			,[totalGenerators]			BIGINT
			,[rowNum]					INT
		)

		IF LEN(ISNULL(@orderby, '')) = 0
		SET @orderby = 'name asc'

		DECLARE @Sql nvarchar(MAX) = ''

		SET @Sql = '
		
		SELECT
			*
			,ROW_NUMBER() OVER (ORDER BY '+@orderby+') AS rowNum
		FROM
		(
			SELECT
			L.[guid]
			, L.[name]
			, L.[description]
			, L.[address] 
			, L.[address2] AS address2
			, L.[city]
			, L.[zipCode]
			, L.[stateGuid]
			, L.[countryGuid]
			, L.[image]
			, L.[latitude]
			, L.[longitude]
			, L.[isActive]	
			, 0 AS [totalGenerators]		
			FROM [dbo].[Location] AS L WITH (NOLOCK) 
			 WHERE L.[companyGuid]=@companyGuid AND L.[isDeleted]=0 ' 
			  + ' and L.[Guid] not in (select gensetGuid from [dbo].[Company] where [Guid]=@companyGuid) '
			+ CASE WHEN @search IS NULL THEN '' ELSE
			' AND (L.name LIKE ''%' + @search + '%''
			  OR L.address LIKE ''%' + @search + '%''
			  OR L.address2 LIKE ''%' + @search + '%''
			  OR L.zipCode LIKE ''%' + @search + '%''
			)'
			 END +
		' )  data '
		
		INSERT INTO #temp_Location
		EXEC sp_executesql 
			  @Sql
			, N'@orderby VARCHAR(100), @companyGuid UNIQUEIDENTIFIER '
			, @orderby		= @orderby			
			, @companyGuid	= @companyGuid			
			
		SET @count = @@ROWCOUNT
		
		;with CTE
		AS (
			SELECT L.[guid],COUNT(G.[guid]) [totalCount]
			FROM [dbo].[Generator] G (NOLOCK)
			INNER JOIN #temp_Location L ON G.[locationGuid] = L.[guid]
			WHERE G.[isDeleted] = 0
			GROUP BY L.[guid]
		)
		UPDATE L
		SET [totalGenerators] = ISNULL(C.[totalCount],0)
		FROM #temp_Location L
		INNER JOIN CTE C ON L.[guid] = C.[guid]

		IF(@pageSize <> -1 AND @pageNumber <> -1)
			BEGIN
				SELECT 
					L.[guid]
					, L.[name]
					, L.[description]
					, L.[address] 
					, L.[address2] AS address2
					, L.[city]
					, L.[zipCode]
					, L.[stateGuid]
					, L.[countryGuid]
					, L.[image]
					, L.[latitude]
					, L.[longitude]
					, L.[isActive]
					, L.[totalGenerators]					
				FROM #temp_Location L
				WHERE rowNum BETWEEN ((@pageNumber - 1) * @pageSize) + 1 AND (@pageSize * @pageNumber)			
			END
		ELSE
			BEGIN
				SELECT 
				L.[guid]
					, L.[name]
					, L.[description]
					, L.[address] 
					, L.[address2] AS address2
					, L.[city]
					, L.[zipCode]
					, L.[stateGuid]
					, L.[countryGuid]
					, L.[image]
					, L.[latitude]
					, L.[longitude]
					, L.[isActive]
					, L.[totalGenerators]	
				FROM #temp_Location L
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