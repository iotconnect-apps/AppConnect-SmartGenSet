CREATE TABLE [dbo].[GeneratorType] (
    [guid]        UNIQUEIDENTIFIER NOT NULL,
    [name]        NVARCHAR (200)   NOT NULL,
    [isActive]    BIT              NOT NULL,
    [isDeleted]   BIT              NOT NULL,
    [createdDate] DATETIME         NULL,
    [createdBy]   UNIQUEIDENTIFIER NULL,
    [updatedDate] DATETIME         NULL,
    [updatedBy]   UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_Generator_Guid] PRIMARY KEY CLUSTERED ([guid] ASC)
);

