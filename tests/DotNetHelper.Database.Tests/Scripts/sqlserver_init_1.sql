IF OBJECT_ID(N'[master].[dbo].[Employee]', N'U') IS NOT NULL BEGIN DROP TABLE [master].[dbo].[Employee] END ELSE BEGIN PRINT 'Nothing To Clean Up' END
CREATE TABLE [master].[dbo].[Employee](
	[IdentityField] [int] NOT NULL IDENTITY (1,1) PRIMARY KEY,
	[FirstName] [varchar](400) NOT NULL,
	[LastName] [varchar](400) NOT NULL,
	[DOB] DateTime NOT NULL,
	[CreatedAt] DateTime NULL,
	[FavoriteColor] [varchar](400) NOT NULL
);
IF OBJECT_ID(N'[master].[dbo].[Employee2]', N'U') IS NOT NULL BEGIN DROP TABLE [master].[dbo].[Employee2] END ELSE BEGIN PRINT 'Nothing To Clean Up' END
CREATE TABLE [master].[dbo].[Employee2](
	[IdentityField] [int] NOT NULL IDENTITY (1,1)  PRIMARY KEY,
	[EmployeeListAsJson] [varchar](800)  NULL,
	[EmployeeAsJson] [varchar](800)  NULL,
	[EmployeeListAsCsv] [varchar](800)  NULL,
	[EmployeeAsCsv] [varchar](800)  NULL,
	[EmployeeListAsXml] [varchar](800)  NULL,
	[EmployeeAsXml] [varchar](800)  NULL,
);
IF OBJECT_ID(N'[master].[dbo].[SpecialDataTypeTable]', N'U') IS NOT NULL BEGIN DROP TABLE [master].[dbo].[SpecialDataTypeTable] END ELSE BEGIN PRINT 'Nothing To Clean Up' END
CREATE TABLE [master].[dbo].[SpecialDataTypeTable](
	[DateTimeOffset] [DATETIMEOFFSET]  NULL,
	[Bytes] varbinary(max)  NULL,
	[Id] [uniqueidentifier]  NULL
);

    