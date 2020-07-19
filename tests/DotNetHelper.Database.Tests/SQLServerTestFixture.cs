using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetHelper.Database.DataSource;
using DotNetHelper.Database.Extension;
using DotNetHelper.Database.Tests.Base;
using DotNetHelper.Database.Tests.Base.Providers;
using DotNetHelper.Database.Tests.MockData;
using DotNetHelper.Database.Tests.Models;
using DotNetHelper.ObjectToSql.Enum;
using DotNetHelper.ObjectToSql.Model;
using NUnit.Framework;


namespace DotNetHelper.Database.Tests
{

	[TestFixtureSource("TestObjects")]
	public class DatabaseTextFixture : TestBase<SqlServerProvider>
	{
		private DB<DbConnection> _database;

		public DB<DbConnection> Database
		{
			get
			{
				return _database ??= new DB<DbConnection>(Provider.GetClosedConnection());
			}
		}

		public DatabaseTextFixture()
		{

		}




		[SetUp]
		public void Setup()
		{
			CreateTables();
		}


		[TearDown]
		public void Teardown()
		{
			CleanUp();
		}



		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			CreateTables();

		}

		[OneTimeTearDown]
		public void RunAfterAnyTests()
		{
			CleanUp();

		}

		private void CreateTables()
		{
			var sqls = new List<string>()
			{
				$@"IF OBJECT_ID(N'[master].[dbo].[Employee]', N'U') IS NOT NULL BEGIN DROP TABLE [master].[dbo].[Employee] END ELSE BEGIN PRINT 'Nothing To Clean Up' END
CREATE TABLE [master].[dbo].[Employee](
	[IdentityField] [int] NOT NULL IDENTITY (1,1) PRIMARY KEY,
	[FirstName] [varchar](400) NOT NULL,
	[LastName] [varchar](400) NOT NULL,
	[DOB] DateTime NOT NULL,
	[CreatedAt] DateTime NULL,
	[FavoriteColor] [varchar](400) NOT NULL
);",

				$@"IF OBJECT_ID(N'[master].[dbo].[Employee2]', N'U') IS NOT NULL BEGIN DROP TABLE [master].[dbo].[Employee2] END ELSE BEGIN PRINT 'Nothing To Clean Up' END
CREATE TABLE [master].[dbo].[Employee2](
	[IdentityField] [int] NOT NULL IDENTITY (1,1)  PRIMARY KEY,
	[EmployeeListAsJson] [varchar](800)  NULL,
	[EmployeeAsJson] [varchar](800)  NULL,
	[EmployeeListAsCsv] [varchar](800)  NULL,
	[EmployeeAsCsv] [varchar](800)  NULL,
	[EmployeeListAsXml] [varchar](800)  NULL,
	[EmployeeAsXml] [varchar](800)  NULL,
);",
				$@"IF OBJECT_ID(N'[master].[dbo].[SpecialDataTypeTable]', N'U') IS NOT NULL BEGIN DROP TABLE [master].[dbo].[SpecialDataTypeTable] END ELSE BEGIN PRINT 'Nothing To Clean Up' END
CREATE TABLE [master].[dbo].[SpecialDataTypeTable](
	[DateTimeOffset] [DATETIMEOFFSET]  NULL,
	[Bytes] varbinary(max)  NULL,
	[Id] [uniqueidentifier]  NULL
);"
			};
			var recordAffected = Database.ExecuteTransaction(sqls, true, true);

		}

		private void CleanUp()
		{
			var sqls = new List<string>()
			{
				$@"IF OBJECT_ID(N'[master].[dbo].[Employee]', N'U') IS NOT NULL BEGIN DROP TABLE[master].[dbo].[Employee] END ELSE BEGIN PRINT 'Nothing To Clean Up' END",
				$@"IF OBJECT_ID(N'[master].[dbo].[Employee]', N'U') IS NOT NULL BEGIN DROP TABLE[master].[dbo].[Employee] END ELSE BEGIN PRINT 'Nothing To Clean Up' END",
				$@"IF OBJECT_ID(N'[master].[dbo].[SpecialDataTypeTable]', N'U') IS NOT NULL BEGIN DROP TABLE[master].[dbo].[SpecialDataTypeTable] END ELSE BEGIN PRINT 'Nothing To Clean Up' END"
			};
			var recordAffected = Database.ExecuteTransaction(sqls, true, true);
		}


		[Test]
		public void Test_CanConnect()
		{
			var canConnect = Database.CanConnect();
			Assert.IsTrue(canConnect, "Could not connect to database");
		}


		[Test]

		public void Test_Execute_Insert_AddsNewEmployee()
		{
			var newEmployee = MockEmployee.Hashset.Take(1).Last();
			var outputtedResult = Database.Execute(newEmployee, ActionType.Insert);
			Assert.AreEqual(outputtedResult, 1, "Something went wrong add new employee record");
		}



		public byte[] ReadFully(Stream input)
		{
			var ms = new MemoryStream();
			input.CopyTo(ms);
			return ms.ToArray();
		}


		[Test]

		public void Test_Execute_Insert_WithSpecialDataType()
		{
			var obj = new SpecialDataTypeTable()
			{
				DateTimeOffset = DateTimeOffset.Now
				,
				Id = Guid.Parse("a19ed8e6-c455-4164-afac-d4043095a4ee")
			};
			using (var stream = typeof(DatabaseTextFixture).Assembly.GetManifestResourceStream("DotNetHelper.Database.Tests.Assets.calendar-16.png"))
			{
				obj.Bytes = ReadFully(stream);
			}


			var outputtedResult = Database.Execute(obj, ActionType.Insert);
			Assert.AreEqual(outputtedResult, 1, "Something went wrong add new employee record");
#if DOTNETCORE // OLD VERSION OF SQLITE PACKAGE IS BROKEN

			var data = Database.Get<SpecialDataTypeTable>().First();


			Assert.AreEqual(obj.Bytes, data.Bytes, $"Failed for database {Database.DatabaseType}");
			if (Database.DatabaseType != DataBaseType.MySql) // MYSQL WORKS BUT DOESN"T KEEP THE MILLISECONDS
				Assert.AreEqual(obj.DateTimeOffset, data.DateTimeOffset, $"Failed for database {Database.DatabaseType}");
			Assert.AreEqual(obj.Id, data.Id, $"Failed for database {Database.DatabaseType}");
#endif


		}

		//[Test]
		//public void Test_Execute_Insert_SpecialDataType()
		//{
		//    var obj = new TestModel();
		//    obj.DateTimeOffset = DateTimeOffset.UtcNow;
		//    obj.Bytes = Encoding.UTF8.GetBytes("DSfjio");
		//    obj.Id =  Guid.Parse("C56A4180-65AA-42EC-A945-5FD21DEC0538");
		//    var outputtedResult = Database.Execute(obj, ActionType.Insert);
		//    Assert.AreEqual(outputtedResult, 1, "Something went wrong add new employee record");

		//}

		[Test]

		public void Test_Execute_INSERT_UPDATE_UPSERT_DELETE()
		{
			// TODO :: IMPLEMENT MYSQL TEST
			if (Database.DatabaseType == DataBaseType.MySql)
				return;
			// INSERT A EMPLOYEE WITH THEIR FAVORITE COLOR TO BE GREEN
			var newEmployee = MockEmployee.Hashset.Take(2).Last();
			newEmployee.FavoriteColor = "GREEN";
			var insertedRecordCount = Database.Execute(newEmployee, ActionType.Insert);
			Assert.AreEqual(insertedRecordCount, 1, "Something went wrong add new employee record");

			// RETRIEVE EMPLOYEE FROM DATABASE SO WHEN CAN HAVE THE ID
			var employees = Database.Get<Employee>();
			Assert.AreEqual(employees.Count, 1, "Invalid # of employees was return");

			// VERFIY DATA WAS SAVED CORRECTLY 
			var employee = employees.First();
			Assert.AreEqual(employee.FavoriteColor, "GREEN", "Employee favorite color wasn't stored correctly");

			// CHANGE EMPLOYEE FAVORITE COLOR
			employee.FavoriteColor = "RED";
			var recordsAffected = Database.Execute(employee, ActionType.Update);
			Assert.AreEqual(recordsAffected, 1, "Invalid # of records affected");

			// VERFIY DATA WAS SAVED CORRECTLY 
			employee = Database.Get<Employee>().First();
			Assert.AreEqual(employee.FavoriteColor, "RED", "Employee favorite color wasn't stored correctly");


			// PERFORM A UPSERT --> UPDATE SENARIO
			employee.FavoriteColor = "PURPLE";
			recordsAffected = Database.Execute(employee, ActionType.Upsert);
			Assert.AreEqual(recordsAffected, 1, "Invalid # of records affected");
			Assert.AreEqual(Database.Get<Employee>().First().FavoriteColor, "PURPLE", "Invalid # of records affected");


			// PERFORM A DELETE
			recordsAffected = Database.Execute(employee, ActionType.Delete);
			Assert.AreEqual(recordsAffected, 1, "Invalid # of records affected");
			Assert.AreEqual(Database.Get<Employee>().Count, 0, "Failed to delete employee");


			// PERFORM A UPSERT --> INSERT SENARIO
			recordsAffected = Database.Execute(employee, ActionType.Upsert);
			Assert.AreEqual(recordsAffected, 1, "Invalid # of records affected");
			employees = Database.Get<Employee>();
			Assert.AreEqual(employees.Count, 1, "Failed to upsert employee insert secnario");
			Assert.AreEqual(employees.First().IdentityField, 2, "Identity Value is out of sync");
		}


		[Test]

		public void Test_Anonymous_Object_Execute_INSERT_UPDATE_UPSERT_DELETE()
		{

			// TODO :: REMOVE FILTER
			if (Database.DatabaseType == DataBaseType.MySql)
				return;
			// INSERT A EMPLOYEE WITH THEIR FAVORITE COLOR TO BE GREEN
			var newEmployee = new
			{
				FirstName = "John 2",
				LastName = "Doe 2",
				DOB = new DateTime(1994, 07, 22),
				FavoriteColor = "Green",
				CreatedAt = DateTime.Now
			};
			var insertedRecordCount = Database.Execute(newEmployee, ActionType.Insert, "Employee");
			Assert.AreEqual(insertedRecordCount, 1, "Something went wrong add new employee record");

			// RETRIEVE EMPLOYEE FROM DATABASE SO WHEN CAN HAVE THE ID
			var employees = Database.Get<Employee>();
			Assert.AreEqual(employees.Count, 1, "Invalid # of employees was return");

			// CHANGE EMPLOYEE FAVORITE COLOR
			var employee = new
			{
				FirstName = employees.First().FirstName,
				LastName = employees.First().LastName,
				DOB = employees.First().DateOfBirth,
				FavoriteColor = "RED",
				IdentityField = employees.First().IdentityField
			};
			var recordsAffected = Database.Execute(employee, ActionType.Update, "Employee", o => o.IdentityField);
			Assert.AreEqual(recordsAffected, 1, "Invalid # of records affected");

			// VERFIY DATA WAS SAVED CORRECTLY 
			Assert.AreEqual(Database.Get<Employee>().First().FavoriteColor, "RED", "Employee favorite color wasn't stored correctly");


			// PERFORM A UPSERT --> UPDATE SENARIO
			employees = Database.Get<Employee>();
			recordsAffected = Database.Execute(new
			{
				FirstName = employees.First().FirstName,
				LastName = employees.First().LastName,
				DOB = employees.First().DateOfBirth,
				FavoriteColor = "PURPLE",
				IdentityField = employees.First().IdentityField
				,
				CreatedAt = DateTime.Now
			}, ActionType.Upsert, "Employee", o => o.IdentityField);
			Assert.AreEqual(recordsAffected, 1, "Invalid # of records affected");
			Assert.AreEqual(Database.Get<Employee>().First().FavoriteColor, "PURPLE", "Invalid # of records affected");


			// PERFORM A DELETE
			employees = Database.Get<Employee>();
			recordsAffected = Database.Execute(new
			{
				FirstName = employees.First().FirstName,
				LastName = employees.First().LastName,
				DOB = employees.First().DateOfBirth,
				FavoriteColor = "PURPLE",
				IdentityField = employees.First().IdentityField
			}, ActionType.Delete, "Employee", o => o.IdentityField);
			Assert.AreEqual(recordsAffected, 1, "Invalid # of records affected");
			Assert.AreEqual(Database.Get<Employee>().Count, 0, "Failed to delete employee");


			// PERFORM A UPSERT --> INSERT SENARIO
			var employee2 = new // have to not 
			{
				FirstName = employees.First().FirstName,
				LastName = employees.First().LastName,
				DOB = employees.First().DateOfBirth,
				FavoriteColor = "RED",
				CreatedAt = DateTime.Now

			};


			// TODO :: SEPARATE TO OWN UNIT TEST BUT YOU CAN'T DO UPSERT WITH AN ANYMOUNS OBJECT INTO A TABLE THAT HAVE A IDENTITY FIELD BECAUSE INSERT
			// REQUIRED IDENTITY KEY TO NOT BE PART OF THE OBJECT AND UPDATE NEED THE KEY SO IT KNOW HOW TO BUILD THE WHERE CLAUSE
			//recordsAffected = Database.Execute(employee2, ActionType.Upsert, "Employee",);
			//Assert.AreEqual(recordsAffected, 1, "Invalid # of records affected");
			//employees = Database.Get<Employee>();
			//Assert.AreEqual(employees.Count, 1, "Failed to upsert employee insert secnario");
			//Assert.AreEqual(employees.First().IdentityField, 2, "Identity Value is out of sync");
		}


		[Test]
		public void Test_Insert_Employee_And_Output_Identity_Field()
		{
			// TODO :: REMOVE MYSQL
			if (Database.DatabaseType == DataBaseType.MySql)
				return;
			if (Database.DatabaseType == DataBaseType.Sqlite)
			{
				EnsureExpectedExceptionIsThrown<NotImplementedException>(delegate
				{
					Database.ExecuteAndGetOutput(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert, e => e.IdentityField);
				});
				return;
			}

			var newEmployee = MockEmployee.Hashset.Take(3).Last();
			var outputtedResult = Database.ExecuteAndGetOutput(newEmployee, ActionType.Insert, e => e.IdentityField);
			Assert.GreaterOrEqual(outputtedResult.IdentityField, 1, "Failed to get identity field value");
		}


		[Test]

		public void Test_ExecuteScalar_Returns_First_Column_First_Row()
		{
			Database.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);
			var outputtedResult = Database.ExecuteScalar($"SELECT FirstName,LastName FROM Employee", CommandType.Text);
			Assert.AreEqual(outputtedResult, "John 3");
		}


		[Test]
		public void Test_GetDataTableWithSchema_Returns_All_Data_And_Has_Correct_Schema()
		{
#if NET452
            if (Database.DatabaseType == DataBaseType.Sqlite)
                return;
#endif
			var dt = Database.GetDataTableWithSchema($"SELECT * FROM Employee");
			dt.TableName = "Employee";

			Assert.AreEqual(dt.TableName, "Employee");
			Assert.AreEqual(dt.Columns["IdentityField"].AutoIncrement, true);
			Assert.AreEqual(dt.Columns["IdentityField"].AllowDBNull, false);
#if NET452

			if (Database.DatabaseType != DataBaseType.MySql)
				Assert.AreEqual(dt.Columns["IdentityField"].ReadOnly, Database.DatabaseType != DataBaseType.Sqlite);
#else
#endif
			Assert.AreEqual(dt.Columns["FirstName"].MaxLength, Database.DatabaseType == DataBaseType.Sqlite ? -1 : 400);

			Assert.AreEqual(dt.Rows.Count, 0);
		}

		[Test]
		public void Test_GetDataTableWithKeyInfo_Returns_All_Data_And_Has_Correct_Schema()
		{
#if NET452
            if(Database.DatabaseType == DataBaseType.Sqlite)
            return;
#endif
			var dt = Database.GetDataTableWithKeyInfo($"SELECT * FROM Employee");
			dt.TableName = "Employee";

			Assert.AreEqual(dt.TableName, "Employee");
			Assert.AreEqual(dt.Columns["IdentityField"].AutoIncrement, true);
			Assert.AreEqual(dt.Columns["IdentityField"].AllowDBNull, false);

			// SQLITE LITE DON'T TREAT IDENTITY PRIMARY KEYS AS READ ONLY
			if (Database.DatabaseType == DataBaseType.MySql || Database.DatabaseType == DataBaseType.Sqlite)
			{
				Assert.AreEqual(dt.Columns["IdentityField"].ReadOnly, false);
			}
			else
			{
#if NET452
				Assert.AreEqual(dt.Columns["IdentityField"].ReadOnly, true);
#else
#endif
			}

			// SQLITE LITE DOESN'T CARRY OVER ACTUAL MAX LENGHT 
			Assert.AreEqual(dt.Columns["FirstName"].MaxLength, Database.DatabaseType == DataBaseType.Sqlite ? -1 : 400);

			Assert.Contains(dt.Columns["IdentityField"], dt.PrimaryKey);
			Assert.AreEqual(dt.Rows.Count, 0);
		}



		[Test]

		public void Test_MapDataTableToList()
		{
#if NET452
            if (Database.DatabaseType == DataBaseType.Sqlite)
                return;
#endif
			Database.Execute(MockEmployee.Hashset.Take(1).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(2).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);

			var dt = Database.GetDataTableWithSchema($"SELECT * FROM Employee");
			var list = dt.MapToList<Employee>(); // .OrderBy(e => e.IdentityField ).ToList();
			Assert.AreEqual(dt.Rows.Count, 3);
			Assert.IsTrue(CompareEmployees(list[0], MockEmployee.Hashset.Take(1).Last()));
			Assert.IsTrue(CompareEmployees(list[1], MockEmployee.Hashset.Take(2).Last()));
			Assert.IsTrue(CompareEmployees(list[2], MockEmployee.Hashset.Take(3).Last()));
		}


		[Test]
		public void Test_MapDataReaderToList()
		{


			Database.Execute(MockEmployee.Hashset.Take(1).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(2).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);

			var list = Database.GetDataReader($"SELECT * FROM Employee", CommandType.Text).MapToList<Employee>(null, null, null);
			Assert.AreEqual(list.Count, 3);
			Assert.IsTrue(CompareEmployees(list[0], MockEmployee.Hashset.Take(1).Last()));
			Assert.IsTrue(CompareEmployees(list[1], MockEmployee.Hashset.Take(2).Last()));
			Assert.IsTrue(CompareEmployees(list[2], MockEmployee.Hashset.Take(3).Last()));
		}


		[Test]
		public void Test_GetData()
		{

			Database.Execute(MockEmployee.Hashset.Take(1).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(2).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);

			var list = Database.Get<Employee>();
			Assert.AreEqual(list.Count, 3);
			Assert.IsTrue(CompareEmployees(list[0], MockEmployee.Hashset.Take(1).Last()));
			Assert.IsTrue(CompareEmployees(list[1], MockEmployee.Hashset.Take(2).Last()));
			Assert.IsTrue(CompareEmployees(list[2], MockEmployee.Hashset.Take(3).Last()));
		}



		[Test]

		public void Test_Execute_List_Of_Objects()
		{

#if SUPPORTDBFACTORIES
            // TODO :: FIND OUT WHY DBFACTORY ISN'T CREATE PARAMETERS
#else
			MockEmployee.Hashset.First().CreatedAt = DateTime.Parse("2019-01-01");
			var data = MockEmployee.Hashset.ToList();
			var recordAffected = Database.Execute(data, ActionType.Insert);
			var employees = Database.Get<Employee>();

			Assert.AreEqual(recordAffected, 3);
			Assert.IsTrue(employees[0].CreatedAt == DateTime.Parse("2019-01-01"), "ExecuteTransaction didn't execute the update statement succesfully");
			Assert.IsTrue(employees[1].CreatedAt == null, "ExecuteTransaction didn't execute the update statement succesfully");
			Assert.IsTrue(employees[2].CreatedAt == null, "ExecuteTransaction didn't execute the update statement succesfully");
#endif
		}



		[Test]

		public void Test_Successful_Transaction()
		{

			Database.Execute(MockEmployee.Hashset.Take(1).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(2).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);

			var createdAt = DateTime.Now;
			var transactionSql = new List<string>()
			{
				   $"UPDATE Employee SET CreatedAt = '2019-01-01' WHERE IdentityField = 1"
				,  $"UPDATE Employee SET CreatedAt = '2019-01-02' WHERE IdentityField = 2"
				,  $"UPDATE Employee SET CreatedAt = '2019-01-03' WHERE IdentityField = 3"
			};
			var recordAffected = Database.ExecuteTransaction(transactionSql, true, true);
			var list = Database.GetDataReader("SELECT CreatedAt FROM Employee", CommandType.Text, null)
				.MapToList<string>()
				.ToList();

			Assert.AreEqual(recordAffected, 3);
			Assert.IsTrue(DateTime.Parse(list[0]) == DateTime.Parse("2019-01-01"), "ExecuteTransaction didn't execute the update statement succesfully");
			Assert.IsTrue(DateTime.Parse(list[1]) == DateTime.Parse("2019-01-02"), "ExecuteTransaction didn't execute the update statement succesfully");
			Assert.IsTrue(DateTime.Parse(list[2]) == DateTime.Parse("2019-01-03"), "ExecuteTransaction didn't execute the update statement succesfully");
		}


		[Test]

		public void Test_Failure_Transaction()
		{

			Database.Execute(MockEmployee.Hashset.Take(1).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(2).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);

		
			var transactionSql = new List<string>()
			{
				  $"UPDATE Employee SET CreatedAt = '2019-01-01' WHERE IdentityField = 1"
				,  $"UPDATE Employee SET CreatedAt = '2019-01-02' WHERE IdentityField = 2"
				,  $"UPDATE Employee SET CreatedAt = '2019-01-03' WHERE IdentityField = 3sdf"
			};

			var recordAffected = 0;
			Assert.Throws(Is.InstanceOf<Exception>(), delegate
			{
				recordAffected = Database.ExecuteTransaction(transactionSql, true, true);
			});

			Assert.That(recordAffected == 0);


			var list = Database.GetDataReader("SELECT CreatedAt FROM Employee", CommandType.Text, null)
				.MapToList<string>()
				.ToList();

			
			Assert.That(list[0] == null, $"ExecuteTransaction didn't execute the update statement succesfully {list[0]}");
			Assert.That(list[1] == null, $"ExecuteTransaction didn't execute the update statement succesfully {list[1]}");
			Assert.That(list[2] == null, $"ExecuteTransaction didn't execute the update statement succesfully {list[2]}");
		}




		[Test]

		public void Test_Transaction_Return_Accurate_RecordAffected()
		{

			Database.Execute(MockEmployee.Hashset.Take(1).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(2).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);

			var transactionSql = new List<string>()
			{
				   $"UPDATE Employee SET CreatedAt = '2019-01-01' WHERE IdentityField = 1"
				,  $"UPDATE Employee SET CreatedAt = '2019-01-02' WHERE IdentityField = 2"
				,  $"UPDATE Employee SET CreatedAt = '2019-01-03' WHERE IdentityField = 3sdf"
			};


			var recordAffected = Database.ExecuteTransaction(transactionSql, false, false);
			Assert.That(recordAffected == 2);

		}


		[Test]

		public void Test_Transaction_Doesnt_Rollback()
		{
			Database.Execute(MockEmployee.Hashset.Take(1).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(2).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);

			var createdAt = DateTime.Now;
			var transactionSql = new List<string>()
			{
				$"UPDATE Employee SET CreatedAt = '2019-01-01' WHERE IdentityField = 1"
				,  $"UPDATE Employee SET CreatedAt = '2019-01-02' WHERE IdentityField = 2"
				,  $"UPDATE Employee SET CreatedAt = '2019-01-03' WHERE IdentityField = 3sdf"
			};


			var recordAffected = Database.ExecuteTransaction(transactionSql, false, false);
			Assert.That(recordAffected == 2);

			var list = Database.GetDataReader("SELECT CreatedAt FROM Employee", CommandType.Text, null)
				.MapToList<string>()
				.ToList();


			Assert.That(DateTime.Parse(list[0]) == DateTime.Parse("2019-01-01"), "ExecuteTransaction didn't execute the update statement succesfully");
			Assert.That(DateTime.Parse(list[1]) == DateTime.Parse("2019-01-02"), "ExecuteTransaction didn't execute the update statement succesfully");
			Assert.That(list[2] == null, "ExecuteTransaction didn't execute the update statement succesfully");
		}



		[Test]

		public void Test_Object_With_Serialize_Attribute_Insert_Without_Error()
		{

			var obj = GetEmployeeSerialize(1);
			var recordCount = Database.Execute(obj, ActionType.Insert, null, Xml.SerializeToString, Json.SerializeToString, Csv.SerializeToString);
		}

		[Test]
		public void Test_Get_Throws_NullArgument_When_No_Deserializer_Func_Is_Not_Provided()
		{
			var obj = GetEmployeeSerialize(1);
			Assert.That(() => Database.Execute(obj, ActionType.Insert),
				Throws.Exception
					.TypeOf<ArgumentNullException>());
		}

		[Test]

		public void Test_Object_With_Serialized_Value_Pull_From_DataBase_Deserialize()
		{

			var mockObj = GetEmployeeSerialize(1);

			Database.Execute(mockObj, ActionType.Insert, null, Xml.SerializeToString, Json.SerializeToString, Csv.SerializeToString);

			var objs = Database.Get<EmployeeSerialize>((s, type) => Xml.Deserialize(s, type), (s, type) => Json.Deserialize(s, type), (s, type) => Csv.Deserialize(s, type));
			var obj = objs.First();




			Assert.IsTrue(CompareEmployees(obj.EmployeeAsCsv, mockObj.EmployeeAsCsv));
			Assert.IsTrue(CompareEmployees(obj.EmployeeAsJson, mockObj.EmployeeAsJson));
			Assert.IsTrue(CompareEmployees(obj.EmployeeAsXml, mockObj.EmployeeAsXml));
			Assert.IsTrue(CompareEmployees(obj.EmployeeListAsCsv, mockObj.EmployeeListAsCsv));
			Assert.IsTrue(CompareEmployees(obj.EmployeeListAsJson, mockObj.EmployeeListAsJson));
			Assert.IsTrue(CompareEmployees(obj.EmployeeListAsXml, mockObj.EmployeeListAsXml));


		}


		[Test]
		public void Test_GetDataTable()
		{
#if NET452

#else
			var mockObj = GetEmployeeSerialize(1);
			Database.Execute(mockObj, ActionType.Insert, null, Xml.SerializeToString, Json.SerializeToString, Csv.SerializeToString, s => s.IdentityField);
			var dt = Database.GetDataTable($"SELECT * FROM {new SqlTable(Database.DatabaseType, mockObj.GetType()).TableName}", CommandType.Text);
			Assert.IsNotNull(dt);
			Assert.AreEqual(dt.Rows.Count, 1);
#endif
		}



		[Test]
		public void Test_Bulk_Insert()
		{
			if (Database.DatabaseType != DataBaseType.SqlServer)
				return;
			var recordCount = Database.SqlServerBulkCopy(MockEmployee.Hashset.ToList(), SqlBulkCopyOptions.Default);
			Assert.AreEqual(recordCount, MockEmployee.Hashset.Count);
		}

		[Test]
		public void Test_Bulk_Insert_Async()
		{
			if (Database.DatabaseType != DataBaseType.SqlServer)
				return;
			long recordCount = 0;
			Task.Run(async delegate
			{
				recordCount = await Database.SqlServerBulkCopyAsync(MockEmployee.Hashset.ToList(), SqlBulkCopyOptions.Default);
			}).Wait(CancellationToken.None);

			Assert.AreEqual(recordCount, MockEmployee.Hashset.Count);
		}


	

	}
}