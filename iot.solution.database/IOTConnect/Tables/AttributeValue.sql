CREATE TABLE [IOTConnect].[AttributeValue](
	[guid] [uniqueidentifier] NOT NULL,
	[companyGuid] [uniqueidentifier] NULL,
	[localName] [nvarchar](100) NULL,
	[uniqueId] [nvarchar](200) NULL,
	[tag] [nvarchar](200) NULL,
	[attributeValue] [nvarchar](1000) NULL DEFAULT (0),
	[createdDate] [datetime] NULL,
	[sdkUpdatedDate] [datetime] NULL,
	[gatewayUpdatedDate] [datetime] NULL,
	[deviceUpdatedDate] [datetime] NULL,
	[aggregateType] [int] NOT NULL,
	PRIMARY KEY ([guid])
)