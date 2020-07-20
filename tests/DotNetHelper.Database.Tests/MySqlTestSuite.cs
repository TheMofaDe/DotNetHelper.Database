using System;
using System.Collections.Generic;
using System.Linq;
using DotNetHelper.Database.Tests.MockData;
using DotNetHelper.Database.Tests.Services.Providers.Impl;
using DotNetHelper.ObjectToSql.Enum;
using Xunit;

namespace DotNetHelper.Database.Tests
{

	public class MySqlTestSuite : BaseTestSuite<MySqlProvider>, IDisposable
	{


		public MySqlTestSuite() : base(new MySqlProvider())
		{


			// ReSharper disable once AccessToDisposedClosure
			int dropTable(string name) => Database.ExecuteNonQuery($"DROP TABLE IF EXISTS {name}");

			dropTable("Employee");
			Database.ExecuteNonQuery($@"CREATE TABLE IF NOT EXISTS [Employee] (
	[IdentityField] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[FirstName] TEXT(400) NOT NULL,
	[LastName] TEXT(400) NOT NULL,
	[DOB] DateTime NOT NULL,
	[CreatedAt] DateTime NULL,
	[FavoriteColor] TEXT(400) NOT NULL
);");
			dropTable("Employee2");
			Database.ExecuteNonQuery($@"CREATE TABLE IF NOT EXISTS [Employee2] (
	[IdentityField] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[EmployeeListAsJson] VARCHAR(400)  NULL,
	[EmployeeAsJson] VARCHAR(400)  NULL,
	[EmployeeListAsCsv] VARCHAR(400)  NULL,
	[EmployeeAsCsv] VARCHAR(400)  NULL,
	[EmployeeListAsXml] VARCHAR(400)  NULL,
	[EmployeeAsXml] VARCHAR(400)  NULL
);");

			dropTable("SpecialDataTypeTable");
			Database.ExecuteNonQuery($@"CREATE TABLE IF NOT EXISTS [SpecialDataTypeTable](
	[DateTimeOffset] VARCHAR(400) NULL,
	[Bytes] BLOB NULL,
	[Id] VARCHAR(400) NULL
);");
		}


		public override void Test_Insert_Employee_And_Output_Identity_Field()
		{

			
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
				Database.ExecuteTransaction(new List<string>() { "DELETE FROM Employee", "DELETE FROM Employee2", "DELETE FROM  SpecialDataTypeTable" }, true, true);
			}

			// Free any unmanaged objects here.
			//
			_disposed = true;

		}


	}

}
