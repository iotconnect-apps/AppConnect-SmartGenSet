IF NOT EXISTS (SELECT TOP 1 1 FROM dbo.[configuration] WHERE [configKey] = 'db-version')
BEGIN
	INSERT [dbo].[Configuration] ([guid], [configKey], [value], [isDeleted], [createdDate], [createdBy], [updatedDate], [updatedBy]) 
	VALUES (N'cf45da4c-1b49-49f5-a5c3-8bc29c1999ea', N'db-version', N'0', 0, GETUTCDATE(), NULL, GETUTCDATE(), NULL)
END

IF NOT EXISTS (SELECT TOP 1 1 FROM dbo.[configuration] WHERE [configKey] = 'telemetry-last-exectime')
BEGIN
	INSERT [dbo].[Configuration] ([guid], [configKey], [value], [isDeleted], [createdDate], [createdBy], [updatedDate], [updatedBy]) 
	VALUES (N'465970b2-8bc3-435f-af97-8ca26f2bf383', N'telemetry-last-exectime', N'2020-01-01 12:08:02.380', 0, GETUTCDATE(), NULL, GETUTCDATE(), NULL)
END

IF NOT EXISTS(SELECT 1 FROM dbo.[configuration] WHERE [configKey] = 'db-version') 
	OR ((SELECT CONVERT(FLOAT,[value]) FROM dbo.[configuration] WHERE [configKey] = 'db-version') < 1 )
BEGIN

	INSERT INTO [dbo].[KitType] ([guid], [name], [code], [tag], [isActive], [isDeleted], [createdDate], [createdBy], [updatedDate], [updatedBy]) VALUES (N'8437013f-4c69-4a1d-b190-4389968f04e3', N'Default', NULL, NULL, 1, 0, CAST(N'2020-02-12T13:20:44.217' AS DateTime), N'68aa338c-ebd7-4686-b350-de844c71db1f', NULL, NULL)

	INSERT [dbo].[KitTypeAttribute] ([guid], [parentTemplateAttributeGuid], [templateGuid], [localName], [code], [tag], [description]) VALUES (N'18c42e60-9373-4bae-860d-1608adc590b4', NULL, N'8437013f-4c69-4a1d-b190-4389968f04e3', N'batt_level', N'battlevel', NULL, NULL)
	INSERT [dbo].[KitTypeAttribute] ([guid], [parentTemplateAttributeGuid], [templateGuid], [localName], [code], [tag], [description]) VALUES (N'823601c5-fdb4-436d-a39b-16c4f42a680b', NULL, N'8437013f-4c69-4a1d-b190-4389968f04e3', N'currentout', N'currentOut', NULL, NULL)
	INSERT [dbo].[KitTypeAttribute] ([guid], [parentTemplateAttributeGuid], [templateGuid], [localName], [code], [tag], [description]) VALUES (N'4d92323c-395a-4983-aac4-37f03f30c397', NULL, N'8437013f-4c69-4a1d-b190-4389968f04e3', N'location.latitude', N'lat', NULL, NULL)
	INSERT [dbo].[KitTypeAttribute] ([guid], [parentTemplateAttributeGuid], [templateGuid], [localName], [code], [tag], [description]) VALUES (N'c1e81a43-08ef-4680-8c11-3ae3c635b88e', NULL, N'8437013f-4c69-4a1d-b190-4389968f04e3', N'location.longitude', N'long', NULL, NULL)
	INSERT [dbo].[KitTypeAttribute] ([guid], [parentTemplateAttributeGuid], [templateGuid], [localName], [code], [tag], [description]) VALUES (N'31f4635d-975e-4121-b054-5f2b197203e9', NULL, N'8437013f-4c69-4a1d-b190-4389968f04e3', N'batt_voltage', N'volt', NULL, NULL)
	INSERT [dbo].[KitTypeAttribute] ([guid], [parentTemplateAttributeGuid], [templateGuid], [localName], [code], [tag], [description]) VALUES (N'6574e873-8668-42f2-8601-6c5628b757ff', NULL, N'8437013f-4c69-4a1d-b190-4389968f04e3', N'eng_temp', N'engTemp', NULL, NULL)
	INSERT [dbo].[KitTypeAttribute] ([guid], [parentTemplateAttributeGuid], [templateGuid], [localName], [code], [tag], [description]) VALUES (N'0c1fe956-8617-4a49-8c6a-bffdf75f494a', NULL, N'8437013f-4c69-4a1d-b190-4389968f04e3', N'engine_rpm', N'engRPM', NULL, NULL)
	INSERT [dbo].[KitTypeAttribute] ([guid], [parentTemplateAttributeGuid], [templateGuid], [localName], [code], [tag], [description]) VALUES (N'37158df2-dc2f-443d-b1dc-f3b2dce8fc6b', NULL, N'8437013f-4c69-4a1d-b190-4389968f04e3', N'fuel_used', N'fuelused', NULL, NULL)
	INSERT [dbo].[KitTypeAttribute] ([guid], [parentTemplateAttributeGuid], [templateGuid], [localName], [code], [tag], [description]) VALUES (N'cc17d942-c5fa-419a-8580-f6244f72fab2', NULL, N'8437013f-4c69-4a1d-b190-4389968f04e3', N'coolant_level', N'coolLevel', NULL, NULL)
	INSERT [dbo].[KitTypeAttribute] ([guid], [parentTemplateAttributeGuid], [templateGuid], [localName], [code], [tag], [description]) VALUES (N'0c720455-2f66-4c2f-a2f8-fdf7d65709d2', NULL, N'8437013f-4c69-4a1d-b190-4389968f04e3', N'fuel_level', N'fuelLevel', NULL, NULL)
	
	INSERT INTO [dbo].[KitTypeCommand] ([guid], [templateGuid], [name], [command], [requiredParam], [requiredAck], [isOTACommand], [tag]) VALUES (N'8feda114-0b69-4b6d-9d85-61b7bf7e6fb3', N'8437013f-4c69-4a1d-b190-4389968f04e3', N'Light_ON_OFF', N'light', 1, 0, 0, N'light')
	INSERT INTO [dbo].[KitTypeCommand] ([guid], [templateGuid], [name], [command], [requiredParam], [requiredAck], [isOTACommand], [tag]) VALUES (N'5403740a-49c7-4003-a262-6ca824115477', N'8437013f-4c69-4a1d-b190-4389968f04e3', N'Power_ON_OFF', N'power', 1, 0, 0, N'power')
	INSERT INTO [dbo].[KitTypeCommand] ([guid], [templateGuid], [name], [command], [requiredParam], [requiredAck], [isOTACommand], [tag]) VALUES (N'dcc5a8f8-e6de-4456-abc7-712891f38ec7', N'8437013f-4c69-4a1d-b190-4389968f04e3', N'Pump_ON_OFF', N'pump', 1, 0, 0, N'pump')

	INSERT INTO [dbo].[AdminUser] ([guid],[email],[companyGuid],[firstName],[lastName],[password],[isActive],[isDeleted],[createdDate]) VALUES (NEWID(),'admin@genset.com','AB469212-2488-49AD-BC94-B3A3F45590D2','Genset','admin','Softweb#123',1,0,GETUTCDATE())

	INSERT INTO GeneratorType ([guid],[name],[isActive],[isDeleted],[createdDate]) VALUES (NEWID(),'Inverter',1,0,GETUTCDATE())
	INSERT INTO GeneratorType ([guid],[name],[isActive],[isDeleted],[createdDate]) VALUES (NEWID(),'Standby',1,0,GETUTCDATE())
	INSERT INTO GeneratorType ([guid],[name],[isActive],[isDeleted],[createdDate]) VALUES (NEWID(),'Portable',1,0,GETUTCDATE())

	UPDATE [dbo].[Configuration]
	SET [value]  = '1'
	WHERE [configKey] = 'db-version'

END
GO