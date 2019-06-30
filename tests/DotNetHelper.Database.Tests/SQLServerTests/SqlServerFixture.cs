using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using DotNetHelper.Database.DataSource;
using DotNetHelper.Database.Extension;
using DotNetHelper.Database.Tests;
using DotNetHelper.Database.Tests.MockData;
using DotNetHelper.Database.Tests.Models;
using DotNetHelper.ObjectToSql.Enum;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tests
{
    public class SqlServerFixture
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
            var assemblyResources = Assembly.GetExecutingAssembly().GetManifestResourceNames(); //example DotNetHelper.Database.Tests.Scripts.sqlserver.sql
            var sqls = assemblyResources.Where(str => str.EndsWith($"{DatabaseAccess.SqlSyntaxHelper.DataBaseType}.sql",StringComparison.OrdinalIgnoreCase)).ToList();
            sqls.ForEach(delegate(string s)
            {
               var result = DatabaseAccess.ExecuteNonQuery(TestHelper.GetEmbeddedResourceFile(s), CommandType.Text);
            });
            
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
           
        }





        [Test]
        [Order(1)]
        public void Test_Execute_AddsNewEmployee()
        {
            var newEmployee = MockEmployee.Hashset.Take(1).Last();
            var outputtedResult = DatabaseAccess.Execute(newEmployee, ActionType.Insert);
            Assert.AreEqual(outputtedResult, 1, "Something went wrong add new employee record");

            
        }

        [Test]
        [Order(2)]
        public void Test_ExecuteNonQuery_AddsNewEmployee()
        {
            var newEmployee = MockEmployee.Hashset.Take(2).Last();
            var sql = DatabaseAccess.ObjectToSql.BuildQuery<Employee>(null, ActionType.Insert);
            var dbParameters = DatabaseAccess.ObjectToSql.BuildDbParameterList(newEmployee,(s, o) => DatabaseAccess.GetNewParameter(s,o),null,null,null);
            var outputtedResult = DatabaseAccess.ExecuteNonQuery(sql,CommandType.Text,dbParameters);
            Assert.AreEqual(outputtedResult, 1, "Something went wrong add new employee record");
        }

        [Test]
        [Order(3)]
        public void Test_Insert_Employee_And_Output_Identity_Field()
        {
            var newEmployee = MockEmployee.Hashset.Take(3).Last();
            var outputtedResult = DatabaseAccess.ExecuteAndGetOutput(newEmployee, ActionType.Insert, e => e.IdentityField);
            Assert.GreaterOrEqual(outputtedResult.IdentityField,2,"Failed to get identity field value");
        }


        [Test]

        public void Test_ExecuteScalar_Returns_First_Column_First_Row()
        {
            var outputtedResult = DatabaseAccess.ExecuteScalar($"SELECT FirstName,LastName FROM {nameof(Employee)}", CommandType.Text);
            Assert.AreEqual(outputtedResult, "John");
        }


        [Test]
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

        [Test]
        [Order(5)]
        public void Test_GetDataTableWithKeyInfo_Returns_All_Data_And_Has_Correct_Schema()
        {
            var dt = DatabaseAccess.GetDataTableWithKeyInfo($"SELECT * FROM Employee");
            dt.TableName = "Employee";

            Assert.AreEqual(dt.TableName, "Employee");
            Assert.AreEqual(dt.Columns["IdentityField"].AutoIncrement, true);
            Assert.AreEqual(dt.Columns["IdentityField"].AllowDBNull, false);
            Assert.AreEqual(dt.Columns["IdentityField"].ReadOnly, true);
            Assert.AreEqual(dt.Columns["FirstName"].MaxLength, 400);
            Assert.Contains(dt.Columns["IdentityField"],dt.PrimaryKey);
            Assert.AreEqual(dt.Rows.Count, 3);
        }


        [Test]
        [Order(6)]
        public void Test_MapDataTableToList()
        {
            var dt = DatabaseAccess.GetDataTableWithSchema($"SELECT * FROM Employee");
            var list = dt.MapToList<Employee>(); // .OrderBy(e => e.IdentityField ).ToList();
            Assert.AreEqual(dt.Rows.Count, 3);
            Assert.IsTrue(CompareEmployees(list[0], MockEmployee.Hashset.Take(1).Last()));
            Assert.IsTrue(CompareEmployees(list[1], MockEmployee.Hashset.Take(2).Last()));
            Assert.IsTrue(CompareEmployees(list[2], MockEmployee.Hashset.Take(3).Last()));
        }


        [Test]
        [Order(7)]
        public void Test_MapDataReaderToList()
        {

            var list = DatabaseAccess.GetDataReader($"SELECT * FROM Employee", CommandType.Text).MapToList<Employee>(null, null, null);
            Assert.AreEqual(list.Count, 3);
            Assert.IsTrue(CompareEmployees(list[0], MockEmployee.Hashset.Take(1).Last()));
            Assert.IsTrue(CompareEmployees(list[1], MockEmployee.Hashset.Take(2).Last()));
            Assert.IsTrue(CompareEmployees(list[2], MockEmployee.Hashset.Take(3).Last()));
        }


        [Test]
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
            var match = 
                one.LastName == two.LastName
                && one.CreatedAt == two.CreatedAt
                && one.DateOfBirth == two.DateOfBirth
                && one.FavoriteColor == two.FavoriteColor
                && one.FirstName == two.FirstName
                && one.IdentityField == two.IdentityField;
            //if (!match)
            //{
            //     var json1 = JsonConvert.SerializeObject(one, Formatting.Indented);
            //     var json2 = JsonConvert.SerializeObject(two, Formatting.Indented);
            //     Directory.CreateDirectory($@"C:\Temp\TestResult\");
            //     File.WriteAllText($@"C:\Temp\TestResult\{DateTime.Now:HH-mm-ss}",$"{json1}{Environment.NewLine}{json2}");
            //}

            return match;
        }




    }
}