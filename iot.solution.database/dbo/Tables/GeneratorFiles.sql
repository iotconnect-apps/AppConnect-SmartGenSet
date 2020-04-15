CREATE TABLE [dbo].[GeneratorFiles] (
    [guid]          UNIQUEIDENTIFIER NOT NULL,
    [generatorGuid] UNIQUEIDENTIFIER NOT NULL,
    [filePath]      NVARCHAR (500)   NOT NULL,
    [description]   NVARCHAR (200)   NULL,
    [isDeleted]     BIT              NOT NULL,
    [createdDate]   DATETIME         NOT NULL,
    [createdBy]     UNIQUEIDENTIFIER NOT NULL,
    [updatedDate]   DATETIME         NULL,
    [updatedBy]     UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK__GeneratorFiles__GUID] PRIMARY KEY CLUSTERED ([guid] ASC)
);

