using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using DotNetHelper.Database.DataSource;
using DotNetHelper.Database.Extension;
using DotNetHelper.Database.Tests;
using DotNetHelper.Database.Tests.MockData;
using DotNetHelper.Database.Tests.Models;
using DotNetHelper.ObjectToSql.Enum;
using DotNetHelper.ObjectToSql.Model;
using NUnit.Framework;

namespace Tests
{
    [Parallelizable(ParallelScope.None)]
    public class MapperFixture
    {

        public SQLTable Table { get; } = new SQLTable(DataBaseType.SqlServer, typeof(EmployeeWithKey));
        public  HashSet<EmployeeWithKey> Hashset = new HashSet<EmployeeWithKey>(new List<EmployeeWithKey>()
        {
            new EmployeeWithKey()
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1994, 07, 22),
                FavoriteColor = "Blue"
                ,IdentityField = 1
                ,PrimaryKey = "Key1"
            },
            new EmployeeWithKey()
            {
                FirstName = "John 2",
                LastName = "Doe 2",
                DateOfBirth = new DateTime(1994, 07, 22),
                FavoriteColor = "Green"
                ,IdentityField = 2
                ,PrimaryKey = "Key2"
            },
            new EmployeeWithKey()
            {
                FirstName = "John 3",
                LastName = "Doe 3",
                DateOfBirth = new DateTime(1994, 07, 22),
                FavoriteColor = "Yellow"
                ,IdentityField = 3
                ,PrimaryKey = "Key3"
            }
        });

        public DatabaseAccess<SqlConnection,SqlParameter> DatabaseAccess { get; set; } = new DatabaseAccess<SqlConnection, SqlParameter>(DataBaseType.SqlServer,TestHelper.SQLServerConnectionString);


        [SetUp]
        public void Setup()
        {

        }


        [TearDown]
        public void Teardown()
        {

        }

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
        
            var deleteTableIfExist = $"IF OBJECT_ID(N'[master].[dbo].[Employee]', N'U') IS NOT NULL BEGIN DROP TABLE [master].[dbo].[Employee] END ELSE BEGIN PRINT 'Nothing To Clean Up' END";
            DatabaseAccess.ExecuteNonQuery(deleteTableIfExist, CommandType.Text);
            var deleteTableIfExist2 = $"IF OBJECT_ID(N'[master].[dbo].[Employee2]', N'U') IS NOT NULL BEGIN DROP TABLE [master].[dbo].[Employee2] END ELSE BEGIN PRINT 'Nothing To Clean Up' END";
            DatabaseAccess.ExecuteNonQuery(deleteTableIfExist2, CommandType.Text);
            var employeeTableSql = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith("EmployeeTable.sql"));
            var sql = TestHelper.GetEmbeddedResourceFile(employeeTableSql);
            var createTableResult = DatabaseAccess.ExecuteNonQuery(TestHelper.GetEmbeddedResourceFile(employeeTableSql), CommandType.Text);
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
           
        }

        [Test]
        [Order(1)]
        public void Test_Execute__AddsNewEmployee()
        {
            var newEmployee = Hashset.Take(1).Last();
            var outputtedResult = DatabaseAccess.Execute(newEmployee, ActionType.Insert);
            Assert.AreEqual(outputtedResult, 1, "Something went wrong add new employee record");
        }

        [Test]
        [Order(2)]
        public void Test_ExecuteNonQuery_AddsNewEmployee()
        {
            var newEmployee = Hashset.Take(2).Last();
            var sql = DatabaseAccess.ObjectToSql.BuildQuery<EmployeeWithKey>(null, ActionType.Insert);
            var dbParameters = DatabaseAccess.ObjectToSql.BuildDbParameterList(newEmployee,(s, o) => DatabaseAccess.GetNewParameter(s,o),null,null,null);
            var outputtedResult = DatabaseAccess.ExecuteNonQuery(sql,CommandType.Text,dbParameters);
            Assert.AreEqual(outputtedResult, 1, "Something went wrong add new employee record");
        }

        [Test]
        [Order(3)]
        public void Test_Insert_Employee_And_Output_Identity_Field()
        {
            var newEmployee = Hashset.Take(3).Last();
            var outputtedResult = DatabaseAccess.ExecuteAndGetOutput(newEmployee, ActionType.Insert, e => e.IdentityField);
            Assert.GreaterOrEqual(outputtedResult.IdentityField,2,"Failed to get identity field value");
        }


        [Test]

        public void Test_ExecuteScalar_Returns_First_Column_First_Row()
        {
            var outputtedResult = DatabaseAccess.ExecuteScalar($"SELECT FirstName,LastName FROM {Table.TableName}", CommandType.Text);
            Assert.AreEqual(outputtedResult, "John");
        }


        [Test]
        [Order(4)]
        public void Test_GetDataTableWithSchema_Returns_All_Data_And_Has_Correct_Schema()
        {
            var dt = DatabaseAccess.GetDataTableWithSchema($"SELECT * FROM {Table.TableName}");
            dt.TableName = "Employee";

            Assert.AreEqual(dt.TableName, "Employee");
            Assert.AreEqual(dt.Columns["IdentityField"].AutoIncrement, true);
            Assert.AreEqual(dt.Columns["IdentityField"].AllowDBNull, false);
            Assert.AreEqual(dt.Columns["IdentityField"].ReadOnly, true);
            Assert.AreEqual(dt.Columns["FirstName"].MaxLength, 400);
            Assert.AreEqual(dt.Rows.Count, 3);
        }

        [Test]
        [Order(5)]
        public void Test_GetDataTableWithKeyInfo_Returns_All_Data_And_Has_Correct_Schema()
        {
            var dt = DatabaseAccess.GetDataTableWithKeyInfo($"SELECT * FROM {Table.TableName}");
           //  dt.TableName = Table.TableName;

            Assert.AreEqual(dt.TableName, "Employee2"); // DATA COMING FROM A TABLE SHOULD HAVE TABLE NAME AUTO-SET
            Assert.AreEqual(dt.Columns["IdentityField"].AutoIncrement, true);
            Assert.AreEqual(dt.Columns["IdentityField"].AllowDBNull, false);
            Assert.AreEqual(dt.Columns["IdentityField"].ReadOnly, true);
            Assert.AreEqual(dt.Columns["FirstName"].MaxLength, 400);
            Assert.Contains(dt.Columns["PrimaryKey"],dt.PrimaryKey);
            Assert.AreEqual(dt.Rows.Count, 3);
        }


        [Test]
        [Order(6)]
        public void Test_MapDataTableToList()
        {
            var dt = DatabaseAccess.GetDataTableWithSchema($"SELECT * FROM {Table.TableName}");
            var list = dt.MapToList<EmployeeWithKey>();
            Assert.AreEqual(dt.Rows.Count, 3);
            Assert.IsTrue(CompareEmployees(list[0], Hashset.Take(1).Last()));
            Assert.IsTrue(CompareEmployees(list[1], Hashset.Take(2).Last()));
            Assert.IsTrue(CompareEmployees(list[2], Hashset.Take(3).Last()));
        }


        [Test]
        [Order(7)]
        public void Test_MapDataReaderToList()
        {

            var list = DatabaseAccess.GetDataReader($"SELECT * FROM {Table.TableName}", CommandType.Text).MapToList<EmployeeWithKey>(null, null, null);
            Assert.AreEqual(list.Count, 3);
            Assert.IsTrue(CompareEmployees(list[0], Hashset.Take(1).Last()));
            Assert.IsTrue(CompareEmployees(list[1], Hashset.Take(2).Last()));
            Assert.IsTrue(CompareEmployees(list[2], Hashset.Take(3).Last()));
        }


        [Test]
        [Order(8)]
        public void Test_GetData()
        {

            var list = DatabaseAccess.Get<EmployeeWithKey>();
            Assert.AreEqual(list.Count, 3);
            Assert.IsTrue(CompareEmployees(list[0], Hashset.Take(1).Last()));
            Assert.IsTrue(CompareEmployees(list[1], Hashset.Take(2).Last()));
            Assert.IsTrue(CompareEmployees(list[2], Hashset.Take(3).Last()));
        }



        [Test]
        [Order(8)]
        public void Test_MapToDataTable()
        {

            var list = DatabaseAccess.Get<EmployeeWithKey>();
            var dt = list.MapToDataTable();
            Assert.AreEqual(dt.Rows.Count, 3);
            Assert.IsTrue(CompareEmployees(dt.Rows[0].MapTo<EmployeeWithKey>(), Hashset.Take(1).Last()));
            Assert.IsTrue(CompareEmployees(dt.Rows[1].MapTo<EmployeeWithKey>(), Hashset.Take(2).Last()));
            Assert.IsTrue(CompareEmployees(dt.Rows[2].MapTo<EmployeeWithKey>(), Hashset.Take(3).Last()));
        }

        public bool CompareEmployees(EmployeeWithKey one, EmployeeWithKey two)
        {
            return
                one.LastName == two.LastName
                && one.CreatedAt == two.CreatedAt
                && one.DateOfBirth == two.DateOfBirth
                && one.FavoriteColor == two.FavoriteColor
                && one.FirstName == two.FirstName
                && one.IdentityField == two.IdentityField;
        }





    }
}