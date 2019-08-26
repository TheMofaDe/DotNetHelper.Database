using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using DotNetHelper.Database.DataSource;
using DotNetHelper.Database.Extension;
using DotNetHelper.Database.Interface;
using DotNetHelper.Database.Tests.Helpers;
using DotNetHelper.Database.Tests.MockData;
using DotNetHelper.Database.Tests.Models;
using DotNetHelper.ObjectToSql.Enum;

#if SUPPORTSQLITE
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
#endif
using NUnit.Framework;


namespace DotNetHelper.Database.Tests
{

    [TestFixtureSource("TestObjects")]
    public class DatabaseTextFixture : BaseTest
    {
        private static List<IDatabaseAccess> TestObjects { get; } = GetTestObjects();

        private static bool HasConnectionIssue { get; set; }

        private static List<IDatabaseAccess> GetTestObjects()
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var list = new List<IDatabaseAccess>() { };

            list.Add(new DatabaseAccess<SqlConnection>(DataBaseType.SqlServer, TestHelper.SQLServerConnectionString));
            list.Add(new DatabaseAccess<SqlConnection>(DataBaseType.SqlServer, TestHelper.SQLServerConnectionString, TimeSpan.FromSeconds(35)));
            list.Add(new DatabaseAccess<SqlConnection>(DataBaseType.SqlServer, TestHelper.SQLServerConnectionString, TimeSpan.FromSeconds(40), TimeSpan.FromSeconds(40)));

#if SUPPORTSQLITE
            list.Add(new DatabaseAccess<SqliteConnection>(DataBaseType.Sqlite, TestHelper.SqliteConnectionString));
            list.Add(new DatabaseAccess<MySqlConnection>(DataBaseType.MySql, TestHelper.MySqlConnectionString, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(2)));
#endif


#if SUPPORTDBFACTORIES
            list.Add(new DatabaseAccessFactory(DataBaseType.SqlServer, TestHelper.SQLServerConnectionString));
#endif
            return list;
        }


        public IDatabaseAccess DatabaseAccess { get; }


        public DatabaseTextFixture(IDatabaseAccess databaseAccess) : base(databaseAccess)
        {
            DatabaseAccess = databaseAccess;
        }




        [SetUp]
        public void Setup()
        {
            var sqls = GetDBScripts(ScriptType.Initialize);
            sqls.ForEach(delegate (string s)
            {
                var result = DatabaseAccess.ExecuteNonQuery(TestHelper.GetEmbeddedResourceFile(s), CommandType.Text);
            });
        }


        [TearDown]
        public void Teardown()
        {
            CleanUp();
        }



        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {

        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
            CleanUp();
        }





        [Test]
        [Order(1)]
        public void Test_CanConnect()
        {
            var canConnect = DatabaseAccess.CanConnect();
            Assert.AreEqual(canConnect, true, "Could not connect to database");

        }


        [Test]

        public void Test_Execute_Insert_AddsNewEmployee()
        {
            var newEmployee = MockEmployee.Hashset.Take(1).Last();
            var outputtedResult = DatabaseAccess.Execute(newEmployee, ActionType.Insert);
            Assert.AreEqual(outputtedResult, 1, "Something went wrong add new employee record");


        }

        [Test]

        public void Test_Execute_INSERT_UPDATE_UPSERT_DELETE()
        {
            // TODO :: IMPLEMENT MYSQL TEST
            if (DatabaseAccess.DatabaseType == DataBaseType.MySql) return;
            // INSERT A EMPLOYEE WITH THEIR FAVORITE COLOR TO BE GREEN
            var newEmployee = MockEmployee.Hashset.Take(2).Last();
            newEmployee.FavoriteColor = "GREEN";
            var insertedRecordCount = DatabaseAccess.Execute(newEmployee, ActionType.Insert);
            Assert.AreEqual(insertedRecordCount, 1, "Something went wrong add new employee record");

            // RETRIEVE EMPLOYEE FROM DATABASE SO WHEN CAN HAVE THE ID
            var employees = DatabaseAccess.Get<Employee>();
            Assert.AreEqual(employees.Count, 1, "Invalid # of employees was return");

            // VERFIY DATA WAS SAVED CORRECTLY 
            var employee = employees.First();
            Assert.AreEqual(employee.FavoriteColor, "GREEN", "Employee favorite color wasn't stored correctly");

            // CHANGE EMPLOYEE FAVORITE COLOR
            employee.FavoriteColor = "RED";
            var recordsAffected = DatabaseAccess.Execute(employee, ActionType.Update);
            Assert.AreEqual(recordsAffected, 1, "Invalid # of records affected");

            // VERFIY DATA WAS SAVED CORRECTLY 
            employee = DatabaseAccess.Get<Employee>().First();
            Assert.AreEqual(employee.FavoriteColor, "RED", "Employee favorite color wasn't stored correctly");


            // PERFORM A UPSERT --> UPDATE SENARIO
            employee.FavoriteColor = "PURPLE";
            recordsAffected = DatabaseAccess.Execute(employee, ActionType.Upsert);
            Assert.AreEqual(recordsAffected, 1, "Invalid # of records affected");
            Assert.AreEqual(DatabaseAccess.Get<Employee>().First().FavoriteColor, "PURPLE", "Invalid # of records affected");


            // PERFORM A DELETE
            recordsAffected = DatabaseAccess.Execute(employee, ActionType.Delete);
            Assert.AreEqual(recordsAffected, 1, "Invalid # of records affected");
            Assert.AreEqual(DatabaseAccess.Get<Employee>().Count, 0, "Failed to delete employee");


            // PERFORM A UPSERT --> INSERT SENARIO
            recordsAffected = DatabaseAccess.Execute(employee, ActionType.Upsert);
            Assert.AreEqual(recordsAffected, 1, "Invalid # of records affected");
            employees = DatabaseAccess.Get<Employee>();
            Assert.AreEqual(employees.Count, 1, "Failed to upsert employee insert secnario");
            Assert.AreEqual(employees.First().IdentityField, 2, "Identity Value is out of sync");
        }


        [Test]

        public void Test_Anonymous_Object_Execute_INSERT_UPDATE_UPSERT_DELETE()
        {

            // TODO :: REMOVE FILTER
            if (DatabaseAccess.DatabaseType == DataBaseType.MySql) return;
            // INSERT A EMPLOYEE WITH THEIR FAVORITE COLOR TO BE GREEN
            var newEmployee = new
            {
                FirstName = "John 2",
                LastName = "Doe 2",
                DOB = new DateTime(1994, 07, 22),
                FavoriteColor = "Green",
                CreatedAt = DateTime.Now
            };
            var insertedRecordCount = DatabaseAccess.Execute(newEmployee, ActionType.Insert, "Employee");
            Assert.AreEqual(insertedRecordCount, 1, "Something went wrong add new employee record");

            // RETRIEVE EMPLOYEE FROM DATABASE SO WHEN CAN HAVE THE ID
            var employees = DatabaseAccess.Get<Employee>();
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
            var recordsAffected = DatabaseAccess.Execute(employee, ActionType.Update, "Employee", o => o.IdentityField);
            Assert.AreEqual(recordsAffected, 1, "Invalid # of records affected");

            // VERFIY DATA WAS SAVED CORRECTLY 
            Assert.AreEqual(DatabaseAccess.Get<Employee>().First().FavoriteColor, "RED", "Employee favorite color wasn't stored correctly");


            // PERFORM A UPSERT --> UPDATE SENARIO
            employees = DatabaseAccess.Get<Employee>();
            recordsAffected = DatabaseAccess.Execute(new
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
            Assert.AreEqual(DatabaseAccess.Get<Employee>().First().FavoriteColor, "PURPLE", "Invalid # of records affected");


            // PERFORM A DELETE
            employees = DatabaseAccess.Get<Employee>();
            recordsAffected = DatabaseAccess.Execute(new
            {
                FirstName = employees.First().FirstName,
                LastName = employees.First().LastName,
                DOB = employees.First().DateOfBirth,
                FavoriteColor = "PURPLE",
                IdentityField = employees.First().IdentityField
            }, ActionType.Delete, "Employee", o => o.IdentityField);
            Assert.AreEqual(recordsAffected, 1, "Invalid # of records affected");
            Assert.AreEqual(DatabaseAccess.Get<Employee>().Count, 0, "Failed to delete employee");


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
            //recordsAffected = DatabaseAccess.Execute(employee2, ActionType.Upsert, "Employee",);
            //Assert.AreEqual(recordsAffected, 1, "Invalid # of records affected");
            //employees = DatabaseAccess.Get<Employee>();
            //Assert.AreEqual(employees.Count, 1, "Failed to upsert employee insert secnario");
            //Assert.AreEqual(employees.First().IdentityField, 2, "Identity Value is out of sync");
        }


        [Test]
        public void Test_Insert_Employee_And_Output_Identity_Field()
        {
            // TODO :: REMOVE MYSQL
            if (DatabaseAccess.DatabaseType == DataBaseType.MySql) return;
            if (DatabaseAccess.DatabaseType == DataBaseType.Sqlite)
            {
                EnsureExpectedExceptionIsThrown<NotImplementedException>(delegate
                {
                    DatabaseAccess.ExecuteAndGetOutput(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert, e => e.IdentityField);
                });
                return;
            }

            var newEmployee = MockEmployee.Hashset.Take(3).Last();
            var outputtedResult = DatabaseAccess.ExecuteAndGetOutput(newEmployee, ActionType.Insert, e => e.IdentityField);
            Assert.GreaterOrEqual(outputtedResult.IdentityField, 1, "Failed to get identity field value");
        }


        [Test]

        public void Test_ExecuteScalar_Returns_First_Column_First_Row()
        {
            DatabaseAccess.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);
            var outputtedResult = DatabaseAccess.ExecuteScalar($"SELECT FirstName,LastName FROM Employee", CommandType.Text);
            Assert.AreEqual(outputtedResult, "John 3");
        }


        [Test]
        public void Test_GetDataTableWithSchema_Returns_All_Data_And_Has_Correct_Schema()
        {
#if NET452
            if (DatabaseAccess.DatabaseType == DataBaseType.Sqlite)
                return;
#endif
            var dt = DatabaseAccess.GetDataTableWithSchema($"SELECT * FROM Employee");
            dt.TableName = "Employee";

            Assert.AreEqual(dt.TableName, "Employee");
            Assert.AreEqual(dt.Columns["IdentityField"].AutoIncrement, true);
            Assert.AreEqual(dt.Columns["IdentityField"].AllowDBNull, false);
#if NET452

#else
            if (DatabaseAccess.DatabaseType != DataBaseType.MySql)
                Assert.AreEqual(dt.Columns["IdentityField"].ReadOnly, DatabaseAccess.DatabaseType != DataBaseType.Sqlite);
            Assert.AreEqual(dt.Columns["FirstName"].MaxLength, DatabaseAccess.DatabaseType == DataBaseType.Sqlite ? -1 : 400);
#endif
            Assert.AreEqual(dt.Rows.Count, 0);
        }

        [Test]
        public void Test_GetDataTableWithKeyInfo_Returns_All_Data_And_Has_Correct_Schema()
        {
#if NET452
            if(DatabaseAccess.DatabaseType == DataBaseType.Sqlite)
            return;
#endif
            var dt = DatabaseAccess.GetDataTableWithKeyInfo($"SELECT * FROM Employee");
            dt.TableName = "Employee";

            Assert.AreEqual(dt.TableName, "Employee");
            Assert.AreEqual(dt.Columns["IdentityField"].AutoIncrement, true);
            Assert.AreEqual(dt.Columns["IdentityField"].AllowDBNull, false);

            // SQLITE LITE DON'T TREAT IDENTITY PRIMARY KEYS AS READ ONLY
            if (DatabaseAccess.DatabaseType == DataBaseType.MySql || DatabaseAccess.DatabaseType == DataBaseType.Sqlite)
            {
                Assert.AreEqual(dt.Columns["IdentityField"].ReadOnly, false);
            }
            else
            {
                Assert.AreEqual(dt.Columns["IdentityField"].ReadOnly, true);
            }

            // SQLITE LITE DOESN'T CARRY OVER ACTUAL MAX LENGHT 
            Assert.AreEqual(dt.Columns["FirstName"].MaxLength, DatabaseAccess.DatabaseType == DataBaseType.Sqlite ? -1 : 400);

            Assert.Contains(dt.Columns["IdentityField"], dt.PrimaryKey);
            Assert.AreEqual(dt.Rows.Count, 0);
        }



        [Test]

        public void Test_MapDataTableToList()
        {
#if NET452
            if (DatabaseAccess.DatabaseType == DataBaseType.Sqlite)
                return;
#endif
            DatabaseAccess.Execute(MockEmployee.Hashset.Take(1).Last(), ActionType.Insert);
            DatabaseAccess.Execute(MockEmployee.Hashset.Take(2).Last(), ActionType.Insert);
            DatabaseAccess.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);

            var dt = DatabaseAccess.GetDataTableWithSchema($"SELECT * FROM Employee");
            var list = dt.MapToList<Employee>(); // .OrderBy(e => e.IdentityField ).ToList();
            Assert.AreEqual(dt.Rows.Count, 3);
            Assert.IsTrue(CompareEmployees(list[0], MockEmployee.Hashset.Take(1).Last()));
            Assert.IsTrue(CompareEmployees(list[1], MockEmployee.Hashset.Take(2).Last()));
            Assert.IsTrue(CompareEmployees(list[2], MockEmployee.Hashset.Take(3).Last()));
        }


        [Test]
        public void Test_MapDataReaderToList()
        {


            DatabaseAccess.Execute(MockEmployee.Hashset.Take(1).Last(), ActionType.Insert);
            DatabaseAccess.Execute(MockEmployee.Hashset.Take(2).Last(), ActionType.Insert);
            DatabaseAccess.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);

            var list = DatabaseAccess.GetDataReader($"SELECT * FROM Employee", CommandType.Text).MapToList<Employee>(null, null, null);
            Assert.AreEqual(list.Count, 3);
            Assert.IsTrue(CompareEmployees(list[0], MockEmployee.Hashset.Take(1).Last()));
            Assert.IsTrue(CompareEmployees(list[1], MockEmployee.Hashset.Take(2).Last()));
            Assert.IsTrue(CompareEmployees(list[2], MockEmployee.Hashset.Take(3).Last()));
        }


        [Test]
        public void Test_GetData()
        {

            DatabaseAccess.Execute(MockEmployee.Hashset.Take(1).Last(), ActionType.Insert);
            DatabaseAccess.Execute(MockEmployee.Hashset.Take(2).Last(), ActionType.Insert);
            DatabaseAccess.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);

            var list = DatabaseAccess.Get<Employee>();
            Assert.AreEqual(list.Count, 3);
            Assert.IsTrue(CompareEmployees(list[0], MockEmployee.Hashset.Take(1).Last()));
            Assert.IsTrue(CompareEmployees(list[1], MockEmployee.Hashset.Take(2).Last()));
            Assert.IsTrue(CompareEmployees(list[2], MockEmployee.Hashset.Take(3).Last()));
        }





        [Test]

        public void Test_Successful_Transaction()
        {

            DatabaseAccess.Execute(MockEmployee.Hashset.Take(1).Last(), ActionType.Insert);
            DatabaseAccess.Execute(MockEmployee.Hashset.Take(2).Last(), ActionType.Insert);
            DatabaseAccess.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);

            var createdAt = DateTime.Now;
            var transactionSql = new List<string>()
            {
                   $"UPDATE Employee SET CreatedAt = '2019-01-01' WHERE IdentityField = 1"
                ,  $"UPDATE Employee SET CreatedAt = '2019-01-02' WHERE IdentityField = 2"
                ,  $"UPDATE Employee SET CreatedAt = '2019-01-03' WHERE IdentityField = 3"
            };
            var recordAffected = DatabaseAccess.ExecuteTransaction(transactionSql, true, true);
            var list = DatabaseAccess.GetDataReader("SELECT CreatedAt FROM Employee", CommandType.Text, null)
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

            DatabaseAccess.Execute(MockEmployee.Hashset.Take(1).Last(), ActionType.Insert);
            DatabaseAccess.Execute(MockEmployee.Hashset.Take(2).Last(), ActionType.Insert);
            DatabaseAccess.Execute(MockEmployee.Hashset.Take(3).Last(), ActionType.Insert);

            var createdAt = DateTime.Now;
            var transactionSql = new List<string>()
            {
                $"UPDATE Employee SET CreatedAt = '2019-01-01' WHERE IdentityField = 1"
                ,  $"UPDATE Employee SET CreatedAt = '2019-01-02' WHERE IdentityField = 2"
                ,  $"UPDATE Employee SET CreatedAt = '2019-01-03' WHERE IdentityField = 3"
            };
            var recordAffected = DatabaseAccess.ExecuteTransaction(transactionSql, true, true);
            var list = DatabaseAccess.GetDataReader("SELECT CreatedAt FROM Employee", CommandType.Text, null)
                .MapToList<string>()
                .ToList();

            Assert.AreEqual(recordAffected, 3);
            Assert.IsTrue(DateTime.Parse(list[0]) == DateTime.Parse("2019-01-01"), "ExecuteTransaction didn't execute the update statement succesfully");
            Assert.IsTrue(DateTime.Parse(list[1]) == DateTime.Parse("2019-01-02"), "ExecuteTransaction didn't execute the update statement succesfully");
            Assert.IsTrue(DateTime.Parse(list[2]) == DateTime.Parse("2019-01-03"), "ExecuteTransaction didn't execute the update statement succesfully");
        }






        [Test]

        public void Test_Object_With_Serialize_Attribute_Insert_Without_Error()
        {

            var obj = GetEmployeeSerialize(1);
            var recordCount = DatabaseAccess.Execute(obj, ActionType.Insert, null, Xml.SerializeToString, Json.SerializeToString, Csv.SerializeToString);
        }

        [Test]
        public void Test_Get_Throws_NullArgument_When_No_Deserializer_Func_Is_Not_Provided()
        {
            var obj = GetEmployeeSerialize(1);
            Assert.That(() => DatabaseAccess.Execute(obj, ActionType.Insert),
                Throws.Exception
                    .TypeOf<ArgumentNullException>());
        }

        [Test]

        public void Test_Object_With_Serialized_Value_Pull_From_DataBase_Deserialize()
        {

            var mockObj = GetEmployeeSerialize(1);

            DatabaseAccess.Execute(mockObj, ActionType.Insert, null, Xml.SerializeToString, Json.SerializeToString, Csv.SerializeToString);

            var objs = DatabaseAccess.Get<EmployeeSerialize>((s, type) => Xml.Deserialize(s, type), (s, type) => Json.Deserialize(s, type), (s, type) => Csv.Deserialize(s, type));
            var obj = objs.First();




            Assert.IsTrue(CompareEmployees(obj.EmployeeAsCsv, mockObj.EmployeeAsCsv));
            Assert.IsTrue(CompareEmployees(obj.EmployeeAsJson, mockObj.EmployeeAsJson));
            Assert.IsTrue(CompareEmployees(obj.EmployeeAsXml, mockObj.EmployeeAsXml));
            Assert.IsTrue(CompareEmployees(obj.EmployeeListAsCsv, mockObj.EmployeeListAsCsv));
            Assert.IsTrue(CompareEmployees(obj.EmployeeListAsJson, mockObj.EmployeeListAsJson));
            Assert.IsTrue(CompareEmployees(obj.EmployeeListAsXml, mockObj.EmployeeListAsXml));


        }



        [Test]

        public void Test_Bulk_Insert()
        {
            if (DatabaseAccess.DatabaseType != DataBaseType.SqlServer) return;
            DatabaseAccess.SqlServerBulkInsert(MockEmployee.Hashset.ToList(),SqlBulkCopyOptions.Default);
        }



    }
}