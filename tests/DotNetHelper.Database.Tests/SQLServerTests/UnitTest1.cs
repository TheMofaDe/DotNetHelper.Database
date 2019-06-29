using System;
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
using NUnit.Framework;

namespace Tests
{
    public class SqlServerTests
    {

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

        [master]
        [Order(1)]
        public void Test_Execute__AddsNewEmployee()
        {
            var newEmployee = MockEmployee.Hashset.Take(1).Last();
            var outputtedResult = DatabaseAccess.Execute(newEmployee, ActionType.Insert);
            Assert.AreEqual(outputtedResult, 1, "Something went wrong add new employee record");
        }

        [master]
        [Order(2)]
        public void Test_ExecuteNonQuery_AddsNewEmployee()
        {
            var newEmployee = MockEmployee.Hashset.Take(2).Last();
            var sql = DatabaseAccess.ObjectToSql.BuildQuery<Employee>(null, ActionType.Insert);
            var dbParameters = DatabaseAccess.ObjectToSql.BuildDbParameterList(newEmployee,(s, o) => DatabaseAccess.GetNewParameter(s,o),null,null,null);
            var outputtedResult = DatabaseAccess.ExecuteNonQuery(sql,CommandType.Text,dbParameters);
            Assert.AreEqual(outputtedResult, 1, "Something went wrong add new employee record");
        }

        [master]
        [Order(3)]
        public void Test_Insert_Employee_And_Output_Identity_Field()
        {
            var newEmployee = MockEmployee.Hashset.Take(3).Last();
            var outputtedResult = DatabaseAccess.ExecuteAndGetOutput(newEmployee, ActionType.Insert, e => e.IdentityField);
            Assert.GreaterOrEqual(outputtedResult.IdentityField,2,"Failed to get identity field value");
        }


        [master]

        public void Test_ExecuteScalar_Returns_First_Column_First_Row()
        {
            var outputtedResult = DatabaseAccess.ExecuteScalar($"SELECT FirstName,LastName FROM {nameof(Employee)}", CommandType.Text);
            Assert.AreEqual(outputtedResult, "John");
        }


        [master]
        [Order(4)]
        public void Test_GetDataTableWithSchema_Returns_All_Data_And_Has_Correct_Schema()
        {
            var dt = DatabaseAccess.GetDataTableWithSchema($"SELECT * FROM Employee");
            dt.TableName = "Employee";

            Assert.AreEqual(dt.TableName, "Employee");
            Assert.AreEqual(dt.Columns["IdentityField"].AutoIncrement, true);
            Assert.AreEqual(dt.Columns["IdentityField"].AllowDBNull, false);
            Assert.AreEqual(dt.Columns["IdentityField"].ReadOnly, true);
            Assert.AreEqual(dt.Columns["FirstName"].MaxLength, 400);
            Assert.AreEqual(dt.Rows.Count, 3);
        }

        [master]
        [Order(5)]
        public void Test_GetDataTableWithKeyInfo_Returns_All_Data_And_Has_Correct_Schema()
        {
            var dt = DatabaseAccess.GetDataTableWithKeyInfo($"SELECT * FROM Employee2");
            dt.TableName = "Employee";

            Assert.AreEqual(dt.TableName, "Employee");
            Assert.AreEqual(dt.Columns["IdentityField"].AutoIncrement, true);
            Assert.AreEqual(dt.Columns["IdentityField"].AllowDBNull, false);
            Assert.AreEqual(dt.Columns["IdentityField"].ReadOnly, true);
            Assert.AreEqual(dt.Columns["FirstName"].MaxLength, 400);
            Assert.Contains(dt.Columns["PrimaryKey"],dt.PrimaryKey);
            Assert.AreEqual(dt.Rows.Count, 0);
        }


        [master]
        [Order(6)]
        public void Test_MapDataTableToList()
        {
            var dt = DatabaseAccess.GetDataTableWithSchema($"SELECT * FROM Employee");
            var list = dt.MapToList<Employee>();
            Assert.AreEqual(dt.Rows.Count, 3);
            Assert.IsTrue(CompareEmployees(list[0], MockEmployee.Hashset.Take(1).Last()));
            Assert.IsTrue(CompareEmployees(list[1], MockEmployee.Hashset.Take(2).Last()));
            Assert.IsTrue(CompareEmployees(list[2], MockEmployee.Hashset.Take(3).Last()));
        }


        [master]
        [Order(7)]
        public void Test_MapDataReaderToList()
        {

            var list = DatabaseAccess.GetDataReader($"SELECT * FROM Employee", CommandType.Text).MapToList<Employee>(null, null, null);
            Assert.AreEqual(list.Count, 3);
            Assert.IsTrue(CompareEmployees(list[0], MockEmployee.Hashset.Take(1).Last()));
            Assert.IsTrue(CompareEmployees(list[1], MockEmployee.Hashset.Take(2).Last()));
            Assert.IsTrue(CompareEmployees(list[2], MockEmployee.Hashset.Take(3).Last()));
        }


        [master]
        [Order(8)]
        public void Test_GetData()
        {

            var list = DatabaseAccess.Get<Employee>();
            Assert.AreEqual(list.Count, 3);
            Assert.IsTrue(CompareEmployees(list[0], MockEmployee.Hashset.Take(1).Last()));
            Assert.IsTrue(CompareEmployees(list[1], MockEmployee.Hashset.Take(2).Last()));
            Assert.IsTrue(CompareEmployees(list[2], MockEmployee.Hashset.Take(3).Last()));
        }


        public bool CompareEmployees(Employee one, Employee two)
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