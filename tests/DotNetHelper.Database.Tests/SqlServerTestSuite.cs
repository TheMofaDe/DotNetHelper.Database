using System;
using System.Collections.Generic;
using DotNetHelper.Database.Tests.Services.Providers.Impl;

namespace DotNetHelper.Database.Tests
{

	public class SqlServerTestSuite : BaseTestSuite<SqlServerProvider>, IDisposable
	{


		public SqlServerTestSuite() : base(new SqlServerProvider())
		{


			// ReSharper disable once AccessToDisposedClosure
			int dropTable(string name) => Database.ExecuteNonQuery($"IF OBJECT_ID('{name}', 'U') IS NOT NULL DROP TABLE [{name}]; ");

			dropTable("Employee");
			Database.ExecuteNonQuery($@"CREATE TABLE [Employee](
	[IdentityField] [int] NOT NULL IDENTITY (1,1) PRIMARY KEY,
	[FirstName] [varchar](400) NOT NULL,
	[LastName] [varchar](400) NOT NULL,
	[DOB] DateTime NOT NULL,
	[CreatedAt] DateTime NULL,
	[FavoriteColor] [varchar](400) NOT NULL
);");
			dropTable("Employee2");
			Database.ExecuteNonQuery($@"CREATE TABLE [Employee2](
	[IdentityField] [int] NOT NULL IDENTITY (1,1)  PRIMARY KEY,
	[EmployeeListAsJson] [varchar](800)  NULL,
	[EmployeeAsJson] [varchar](800)  NULL,
	[EmployeeListAsCsv] [varchar](800)  NULL,
	[EmployeeAsCsv] [varchar](800)  NULL,
	[EmployeeListAsXml] [varchar](800)  NULL,
	[EmployeeAsXml] [varchar](800)  NULL,
);");

			dropTable("SpecialDataTypeTable");
			Database.ExecuteNonQuery($@"CREATE TABLE [SpecialDataTypeTable](
	[DateTimeOffset] [DATETIMEOFFSET]  NULL,
	[Bytes] varbinary(max)  NULL,
	[Id] [uniqueidentifier]  NULL
);");
		}







		// Flag: Has Dispose already been called?
		private bool _disposed;

		// Public implementation of Dispose pattern callable by consumers.
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Protected implementation of Dispose pattern.
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;


			if (disposing)
			{
				// Free any other managed objects here.
				Database.ExecuteTransaction(new List<string>() { "TRUNCATE TABLE Employee", "TRUNCATE TABLE Employee2", "TRUNCATE TABLE SpecialDataTypeTable" }, true, true);
			}

			// Free any unmanaged objects here.
			//
			_disposed = true;

		}


	}

}