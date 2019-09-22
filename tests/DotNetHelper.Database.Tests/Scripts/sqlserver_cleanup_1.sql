IF OBJECT_ID(N'[master].[dbo].[Employee]', N'U') IS NOT NULL BEGIN DROP TABLE [master].[dbo].[Employee] END ELSE BEGIN PRINT 'Nothing To Clean Up' END
IF OBJECT_ID(N'[master].[dbo].[Employee2]', N'U') IS NOT NULL BEGIN DROP TABLE [master].[dbo].[Employee2] END ELSE BEGIN PRINT 'Nothing To Clean Up' END
IF OBJECT_ID(N'[master].[dbo].[SpecialDataTypeTable]', N'U') IS NOT NULL BEGIN DROP TABLE [master].[dbo].[SpecialDataTypeTable] END ELSE BEGIN PRINT 'Nothing To Clean Up' END
