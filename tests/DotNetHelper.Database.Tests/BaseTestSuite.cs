using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using DotNetHelper.Database.DataSource;
using DotNetHelper.Database.Extension;
using DotNetHelper.Database.Tests.MockData;
using DotNetHelper.Database.Tests.Models;
using DotNetHelper.Database.Tests.Services;
using DotNetHelper.Database.Tests.Services.Providers;
using DotNetHelper.ObjectToSql.Enum;
using DotNetHelper.ObjectToSql.Model;
using DotNetHelper.Serialization.Abstractions.Interface;
using DotNetHelper.Serialization.Csv;
using DotNetHelper.Serialization.Json;
using Newtonsoft.Json;
using Xunit;
using FactAttribute = DotNetHelper.Database.Tests.SkippableFactAttribute;

namespace DotNetHelper.Database.Tests
{
	public abstract partial class BaseTestSuite<TDatabaseProvider> : IClassFixture<TDatabaseProvider> where TDatabaseProvider : class, IDatabaseProvider
		{
		protected DB<DbConnection> Database { get; private set; }

		protected BaseTestSuite(TDatabaseProvider databaseProvider)
		{
			Database = new DB<DbConnection>(databaseProvider.GetClosedConnection());

			Console.WriteLine("Using Provider: {0}", databaseProvider.GetType().FullName);
			Console.WriteLine("Using Connectionstring: {0}", databaseProvider.GetConnectionString());
			Console.WriteLine(".NET: " + Environment.Version);
			Console.WriteLine("Current Directory " + Directory.GetCurrentDirectory());
		}

		protected static CultureInfo ActiveCulture
		{
			get { return Thread.CurrentThread.CurrentCulture; }
			set { Thread.CurrentThread.CurrentCulture = value; }
		}



		public ISerializer Json { get; } = new DataSourceJson(new JsonSerializerSettings() { Formatting = Formatting.None });
		public DataSourceCsv Csv { get; } = new DataSourceCsv();
		public ISerializer Xml { get; } = new DataSourceXML();

		public EmployeeSerialize GetEmployeeSerialize(int index)
		{
			var employee = MockEmployee.Hashset.Take(index).First();
			return new EmployeeSerialize()
			{
				EmployeeAsCsv = employee
				,
				EmployeeAsJson = employee
				,
				EmployeeAsXml = employee
				,
				EmployeeListAsCsv = new List<Employee>() { employee }
				,
				EmployeeListAsJson = new List<Employee>() { employee }
				,
				EmployeeListAsXml = new List<Employee>() { employee }
			};
		}
		public bool CompareEmployees(List<Employee> one, List<Employee> two)
		{
			const int index = 0;
			foreach (var employee in one)
			{
				if (!CompareEmployees(employee, two[index]))
					return false;
			}

			return true;
		}
		public bool CompareEmployees(Employee one, Employee two)
		{
			var match =
				one.LastName == two.LastName
				&& one.CreatedAt == two.CreatedAt
				&& one.DateOfBirth == two.DateOfBirth
				&& one.FavoriteColor == two.FavoriteColor
				&& one.FirstName == two.FirstName
				&& one.IdentityField == two.IdentityField;


			return match;
		}




		[Fact]
		public void Test_CanConnect()
		{
			var canConnect = Database.CanConnect();
			Assert.True(canConnect, "Could not connect to database");
		}


		[Fact]

		public void Test_Execute_Insert_AddsNewEmployee()
		{
			var newEmployee = MockEmployee.Hashset.Take(1).Last();
			var outputtedResult = Database.Execute(newEmployee, ActionType.Insert);
			Assert.Equal(1, outputtedResult);
		}



		public byte[] ReadFully(Stream input)
		{
			var ms = new MemoryStream();
			input.CopyTo(ms);
			return ms.ToArray();
		}


		[Fact]

		public void Test_Execute_Insert_WithSpecialDataType()
		{
			var obj = new SpecialDataTypeTable()
			{
				DateTimeOffset = DateTimeOffset.Now
				, Id = Guid.Parse("a19ed8e6-c455-4164-afac-d4043095a4ee")
			};
			using (var stream = typeof(IDatabaseProvider).Assembly.GetManifestResourceStream("DotNetHelper.Database.Tests.Assets.calendar-16.png"))
			{
				obj.Bytes = ReadFully(stream);
			}


			var outputtedResult = Database.Execute(obj, ActionType.Insert);
			Assert.Equal(1, outputtedResult);
#if NETCORE // OLD VERSION OF SQLITE PACKAGE IS BROKEN

			var data = Database.Get<SpecialDataTypeTable>().First();


			Assert.Equal(obj.Bytes, data.Bytes);
			if (Database.DatabaseType != DataBaseType.MySql) // MYSQL WORKS BUT DOESN"T KEEP THE MILLISECONDS
				Assert.Equal(obj.DateTimeOffset, data.DateTimeOffset);
			Assert.Equal(obj.Id, data.Id);
#endif


		}

		[Fact]

		public void Test_Execute_INSERT_UPDATE_UPSERT_DELETE()
		{
			// TODO :: IMPLEMENT MYSQL TEST
			if (Database.DatabaseType == DataBaseType.MySql)
				return;
			// INSERT A EMPLOYEE WITH THEIR FAVORITE COLOR TO BE GREEN
			var newEmployee = MockEmployee.Hashset.Take(2).Last();
			newEmployee.FavoriteColor = "GREEN";
			var insertedRecordCount = Database.Execute(newEmployee, ActionType.Insert);
			Assert.Equal(1, insertedRecordCount);

			// RETRIEVE EMPLOYEE FROM DATABASE SO WHEN CAN HAVE THE ID
			var employees = Database.Get<Employee>();
			Assert.Single(employees);

			// VERFIY DATA WAS SAVED CORRECTLY 
			var employee = employees.First();
			Assert.Equal("GREEN", employee.FavoriteColor);

			// CHANGE EMPLOYEE FAVORITE COLOR
			employee.FavoriteColor = "RED";
			var recordsAffected = Database.Execute(employee, ActionType.Update);
			Assert.Equal(1, recordsAffected);

			// VERFIY DATA WAS SAVED CORRECTLY 
			employee = Database.Get<Employee>().First();
			Assert.Equal("RED", employee.FavoriteColor);


			// PERFORM A UPSERT --> UPDATE SENARIO
			employee.FavoriteColor = "PURPLE";
			recordsAffected = Database.Execute(employee, ActionType.Upsert);
			Assert.Equal(1, recordsAffected);
			Assert.Equal("PURPLE", Database.Get<Employee>().First().FavoriteColor);


			// PERFORM A DELETE
			recordsAffected = Database.Execute(employee, ActionType.Delete);
			Assert.Equal(1, recordsAffected);
			Assert.Empty(Database.Get<Employee>());


			// PERFORM A UPSERT --> INSERT SENARIO
			recordsAffected = Database.Execute(employee, ActionType.Upsert);
			Assert.Equal(1, recordsAffected);
			employees = Database.Get<Employee>();
			Assert.Single(employees);
			Assert.Equal(2, employees.First().IdentityField);
		}


		[Fact]

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
			Assert.Equal(1, insertedRecordCount);

			// RETRIEVE EMPLOYEE FROM DATABASE SO WHEN CAN HAVE THE ID
			var employees = Database.Get<Employee>();
			Assert.Single(employees);

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
			Assert.Equal(1, recordsAffected);

			// VERFIY DATA WAS SAVED CORRECTLY 
			Assert.Equal("RED", Database.Get<Employee>().First().FavoriteColor);


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
			Assert.Equal(1, recordsAffected);
			Assert.Equal("PURPLE", Database.Get<Employee>().First().FavoriteColor);


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
			Assert.Equal(1, recordsAffected);
			Assert.Empty(Database.Get<Employee>());


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
			//Assert.Equal(recordsAffected, 1, "Invalid # of records affected");
			//employees = Database.Get<Employee>();
			//Assert.Equal(employees.Count, 1, "Failed to upsert employee insert secnario");
			//Assert.Equal(employees.First().IdentityField, 2, "Identity Value is out of sync");
		}


		[Fact]
		public virtual void Test_Insert_Employee_And_Output_Identity_Field()
		{
			// TODO :: REMOVE MYSQL
			if (Database.DatabaseType == DataBaseType.MySql)
				return;
	

			var newEmployee = MockEmployee.Hashset.Take(3).Last();
			var outputtedResult = Database.ExecuteAndGetOutput(newEmployee, ActionType.Insert, e => e.IdentityField);
			Assert.True(outputtedResult.IdentityField >= 1);
		}


		[Fact]

		public void Test_ExecuteScalar_Returns_First_Column_First_Row()
		{
			Database.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);
			var outputtedResult = Database.ExecuteScalar($"SELECT FirstName,LastName FROM Employee", CommandType.Text);
			Assert.Equal("John 3", outputtedResult);
		}


		[Fact]
		public void Test_GetDataTableWithSchema_Returns_All_Data_And_Has_Correct_Schema()
		{
#if NETFRAMEWORK
			if (Database.DatabaseType == DataBaseType.Sqlite)
				return;
#endif
			var dt = Database.GetDataTableWithSchema($"SELECT * FROM Employee");
			dt.TableName = "Employee";

			Assert.Equal("Employee", dt.TableName);
			Assert.True(dt.Columns["IdentityField"].AutoIncrement);
			Assert.False(dt.Columns["IdentityField"].AllowDBNull);
#if NETFRAMEWORK

			if (Database.DatabaseType != DataBaseType.MySql)
				Assert.Equal(dt.Columns["IdentityField"].ReadOnly, Database.DatabaseType != DataBaseType.Sqlite);
#else
#endif
			Assert.Equal(dt.Columns["FirstName"].MaxLength, Database.DatabaseType == DataBaseType.Sqlite ? -1 : 400);

			Assert.Empty(dt.Rows);
		}

		[Fact]
		public void Test_GetDataTableWithKeyInfo_Returns_All_Data_And_Has_Correct_Schema()
		{
#if NETFRAMEWORK
			if (Database.DatabaseType == DataBaseType.Sqlite)
				return;
#endif
			var dt = Database.GetDataTableWithKeyInfo($"SELECT * FROM Employee");
			dt.TableName = "Employee";

			Assert.Equal("Employee", dt.TableName);
			Assert.True(dt.Columns["IdentityField"].AutoIncrement);
			Assert.False(dt.Columns["IdentityField"].AllowDBNull);

			// SQLITE LITE DON'T TREAT IDENTITY PRIMARY KEYS AS READ ONLY
			if (Database.DatabaseType == DataBaseType.MySql || Database.DatabaseType == DataBaseType.Sqlite)
			{
				Assert.False(dt.Columns["IdentityField"].ReadOnly);
			}
			else
			{
#if NETFRAMEWORK
				Assert.True(dt.Columns["IdentityField"].ReadOnly);
#else
#endif
			}

			// SQLITE LITE DOESN'T CARRY OVER ACTUAL MAX LENGHT 
			Assert.Equal(dt.Columns["FirstName"].MaxLength, Database.DatabaseType == DataBaseType.Sqlite ? -1 : 400);

			Assert.Contains(dt.Columns["IdentityField"], dt.PrimaryKey);
			Assert.Equal(0, dt.Rows.Count);
		}



		[Fact]

		public void Test_MapDataTableToList()
		{
#if NETFRAMEWORK
			if (Database.DatabaseType == DataBaseType.Sqlite)
				return;
#endif
			Database.Execute(MockEmployee.Hashset.Take(1).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(2).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);

			var dt = Database.GetDataTableWithSchema($"SELECT * FROM Employee");
			var list = dt.MapToList<Employee>(); // .OrderBy(e => e.IdentityField ).ToList();
			Assert.Equal(3, dt.Rows.Count);
			Assert.True(CompareEmployees(list[0], MockEmployee.Hashset.Take(1).Last()));
			Assert.True(CompareEmployees(list[1], MockEmployee.Hashset.Take(2).Last()));
			Assert.True(CompareEmployees(list[2], MockEmployee.Hashset.Take(3).Last()));
		}


		[Fact]
		public void Test_MapDataReaderToList()
		{


			Database.Execute(MockEmployee.Hashset.Take(1).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(2).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);

			var list = Database.GetDataReader($"SELECT * FROM Employee", CommandType.Text).MapToList<Employee>(null, null, null);
			Assert.Equal(3, list.Count);
			Assert.True(CompareEmployees(list[0], MockEmployee.Hashset.Take(1).Last()));
			Assert.True(CompareEmployees(list[1], MockEmployee.Hashset.Take(2).Last()));
			Assert.True(CompareEmployees(list[2], MockEmployee.Hashset.Take(3).Last()));
		}


		[Fact]
		public void Test_GetData()
		{

			Database.Execute(MockEmployee.Hashset.Take(1).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(2).Last(), ActionType.Insert);
			Database.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);

			var dataReader = Database.GetDataReader("SELECT * FROM Employee");
			var list = Database.Get<Employee>();
			Assert.Equal(3, list.Count);
			Assert.True(CompareEmployees(list[0], MockEmployee.Hashset.Take(1).Last()));
			Assert.True(CompareEmployees(list[1], MockEmployee.Hashset.Take(2).Last()));
			Assert.True(CompareEmployees(list[2], MockEmployee.Hashset.Take(3).Last()));
		}



		[Fact]

		public void Test_Execute_List_Of_Objects()
		{

#if NETFRAMEWORK
			// TODO :: FIND OUT WHY DBFACTORY ISN'T CREATE PARAMETERS
#else
			MockEmployee.Hashset.First().CreatedAt = DateTime.Parse("2019-01-01");
			var data = MockEmployee.Hashset.ToList();
			var recordAffected = Database.Execute(data, ActionType.Insert);
			var employees = Database.Get<Employee>();

			Assert.Equal(3,recordAffected);
			Assert.True(employees[0].CreatedAt == DateTime.Parse("2019-01-01"), "ExecuteTransaction didn't execute the update statement succesfully");
			Assert.True(employees[1].CreatedAt == null, "ExecuteTransaction didn't execute the update statement succesfully");
			Assert.True(employees[2].CreatedAt == null, "ExecuteTransaction didn't execute the update statement succesfully");
#endif
		}



		[Fact]

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

			Assert.Equal(3, recordAffected);
			Assert.True(DateTime.Parse(list[0]) == DateTime.Parse("2019-01-01"), "ExecuteTransaction didn't execute the update statement succesfully");
			Assert.True(DateTime.Parse(list[1]) == DateTime.Parse("2019-01-02"), "ExecuteTransaction didn't execute the update statement succesfully");
			Assert.True(DateTime.Parse(list[2]) == DateTime.Parse("2019-01-03"), "ExecuteTransaction didn't execute the update statement succesfully");
		}


		// [Fact]

		//public void Test_Failure_Transaction()
		//{

		//	Database.Execute(MockEmployee.Hashset.Take(1).Last(), ActionType.Insert);
		//	Database.Execute(MockEmployee.Hashset.Take(2).Last(), ActionType.Insert);
		//	Database.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);


		//	var transactionSql = new List<string>()
		//	{
		//		   $"UPDATE Employee SET CreatedAt = '2019-01-01' WHERE IdentityField = 1"
		//		,  $"UPDATE Employee SET CreatedAt = '2019-01-02' WHERE IdentityField = 2"
		//		,  $"UPDATE Employee SET CreatedAt = '2019-01-03' WHERE IdentityField = 3sdf"
		//	};

		//	var recordAffected = 0;
		//	Assert.Throws(Is.InstanceOf<Exception>(), delegate
		//	{
		//		recordAffected = Database.ExecuteTransaction(transactionSql, true, true);
		//	});

		//	Assert.That(recordAffected == 0);


		//	var list = Database.GetDataReader("SELECT CreatedAt FROM Employee")
		//		.MapToList<string>()
		//		.ToList();


		//	Assert.That(list[2] == null, $"CreatedAt column should be NULL but is {list[2]}");
		//	Assert.That(list[1] == null, $"CreatedAt column should be NULL but is {list[1]}");
		//	Assert.That(list[0] == null, $"CreatedAt column should be NULL but is {list[0]}");
		//}




		[Fact]

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
			Assert.True(recordAffected == 2);

		}


		[Fact]

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
			Assert.True(recordAffected == 2);

			var list = Database.GetDataReader("SELECT CreatedAt FROM Employee", CommandType.Text, null)
				.MapToList<string>()
				.ToList();


			Assert.True(DateTime.Parse(list[0]) == DateTime.Parse("2019-01-01"), "ExecuteTransaction didn't execute the update statement succesfully");
			Assert.True(DateTime.Parse(list[1]) == DateTime.Parse("2019-01-02"), "ExecuteTransaction didn't execute the update statement succesfully");
			Assert.True(list[2] == null, "ExecuteTransaction didn't execute the update statement succesfully");
		}



		[Fact]

		public void Test_Object_With_Serialize_Attribute_Insert_Without_Error()
		{

			var obj = GetEmployeeSerialize(1);
			var recordCount = Database.Execute(obj, ActionType.Insert, null, Xml.SerializeToString, Json.SerializeToString, Csv.SerializeToString);
		}

		[Fact]
		public void Test_Get_Throws_NullArgument_When_No_Deserializer_Func_Is_Not_Provided()
		{
			var obj = GetEmployeeSerialize(1);
			Assert.Throws<ArgumentNullException>(() => Database.Execute(obj, ActionType.Insert));
		}

		[Fact]

		public void Test_Object_With_Serialized_Value_Pull_From_DataBase_Deserialize()
		{

			var mockObj = GetEmployeeSerialize(1);

			Database.Execute(mockObj, ActionType.Insert, null, Xml.SerializeToString, Json.SerializeToString, Csv.SerializeToString);

			var objs = Database.Get<EmployeeSerialize>((s, type) => Xml.Deserialize(s, type), (s, type) => Json.Deserialize(s, type), (s, type) => Csv.Deserialize(s, type));
			var obj = objs.First();




			Assert.True(CompareEmployees(obj.EmployeeAsCsv, mockObj.EmployeeAsCsv));
			Assert.True(CompareEmployees(obj.EmployeeAsJson, mockObj.EmployeeAsJson));
			Assert.True(CompareEmployees(obj.EmployeeAsXml, mockObj.EmployeeAsXml));
			Assert.True(CompareEmployees(obj.EmployeeListAsCsv, mockObj.EmployeeListAsCsv));
			Assert.True(CompareEmployees(obj.EmployeeListAsJson, mockObj.EmployeeListAsJson));
			Assert.True(CompareEmployees(obj.EmployeeListAsXml, mockObj.EmployeeListAsXml));


		}


		[Fact]
		public void Test_GetDataTable()
		{
#if NETFRAMEWORK

#else
			var mockObj = GetEmployeeSerialize(1);
			Database.Execute(mockObj, ActionType.Insert, null, Xml.SerializeToString, Json.SerializeToString, Csv.SerializeToString, s => s.IdentityField);
			var dt = Database.GetDataTable($"SELECT * FROM {new SqlTable(Database.DatabaseType, mockObj.GetType()).TableName}", CommandType.Text);
			Assert.NotNull(dt);
			Assert.Equal(1,dt.Rows.Count);
#endif
		}



		[Fact]
		public void Test_Bulk_Insert()
		{
			if (Database.DatabaseType != DataBaseType.SqlServer)
				return;
			var recordCount = Database.SqlServerBulkCopy(MockEmployee.Hashset.ToList(), SqlBulkCopyOptions.Default);
			Assert.Equal(recordCount, MockEmployee.Hashset.Count);
		}














	}
}