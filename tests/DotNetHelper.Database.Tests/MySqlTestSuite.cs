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
			Database.ExecuteNonQuery($@"  
CREATE TABLE IF NOT EXISTS `sys`.`Employee`(
	`IdentityField` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
	`FirstName` varchar(400) NOT NULL,
	`LastName` varchar(400) NOT NULL,
	`DOB` DateTime NOT NULL,
	`CreatedAt` DateTime NULL,
	`FavoriteColor` varchar(400) NOT NULL
);");
			dropTable("Employee2");
			Database.ExecuteNonQuery($@"CREATE TABLE IF NOT EXISTS `sys`.`Employee2`(
	`IdentityField` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
	`EmployeeListAsJson` varchar(800)  NULL,
	`EmployeeAsJson` varchar(800)  NULL,
	`EmployeeListAsCsv` varchar(800)  NULL,
	`EmployeeAsCsv` varchar(800)  NULL,
	`EmployeeListAsXml` varchar(800)  NULL,
	`EmployeeAsXml` varchar(800)  NULL
);");

			dropTable("SpecialDataTypeTable");
			Database.ExecuteNonQuery($@"CREATE TABLE IF NOT EXISTS `sys`.`SpecialDataTypeTable`(
	`DateTimeOffset`  varchar(800) NULL,
	`Bytes` LONGBLOB NULL,
	`Id` varchar(800)  NULL
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