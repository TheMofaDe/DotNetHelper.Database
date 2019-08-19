#if NET452

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using CsvHelper.Configuration;
using DotNetHelper.Database.DataSource;
using DotNetHelper.Database.Extension;
using DotNetHelper.Database.Interface;
using DotNetHelper.Database.Tests.Helpers;
using DotNetHelper.Database.Tests.MockData;
using DotNetHelper.Database.Tests.Models;
using DotNetHelper.ObjectToSql.Enum;
using NUnit.Framework;

namespace DotNetHelper.Database.Tests.SQLServerTests
{
    public class DatabaseAccessFixture11 : BaseTest
    {

        public IDatabaseAccess DatabaseAccess { get; set; } = new DatabaseAccessFactory(DataBaseType.SqlServer,TestHelper.SQLServerConnectionString);


        public DatabaseAccessFixture11()
        {
           
        }

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
            var sqls = assemblyResources.Where(str => str.EndsWith($"{DatabaseAccess.DatabaseType}.sql", StringComparison.OrdinalIgnoreCase)).ToList();
            sqls.ForEach(delegate (string s)
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
        public void Test_CanConnect()
        {
            var canConnect = DatabaseAccess.CanConnect();
            Assert.AreEqual(canConnect, true, "Could not connect to database");


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
            //var sql = DatabaseAccess.ObjectToSql.BuildQuery<Employee>(null, ActionType.Insert);
            //var dbParameters = DatabaseAccess.ObjectToSql.BuildDbParameterList(newEmployee,(s, o) => DatabaseAccess.GetNewParameter(s,o),null,null,null);
            //var outputtedResult = DatabaseAccess.ExecuteNonQuery(sql,CommandType.Text,dbParameters);
            var outputtedResult = DatabaseAccess.Execute(newEmployee, ActionType.Insert);
            Assert.AreEqual(outputtedResult, 1, "Something went wrong add new employee record");
        }

        [Test]
        [Order(3)]
        public void Test_Insert_Employee_And_Output_Identity_Field()
        {
            var newEmployee = MockEmployee.Hashset.Take(3).Last();
            var outputtedResult = DatabaseAccess.ExecuteAndGetOutput(newEmployee, ActionType.Insert, e => e.IdentityField);
            Assert.GreaterOrEqual(outputtedResult.IdentityField, 2, "Failed to get identity field value");
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
            Assert.Contains(dt.Columns["IdentityField"], dt.PrimaryKey);
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



        [Test]
        [Order(9)]
        public void Test_Transaction()
        {
            var createdAt = DateTime.Now;
            var transactionSql = new List<string>()
            {
                $"UPDATE Employee SET CreatedAt = '{createdAt}'"
                , $"UPDATE Employee SET CreatedAt = '2019-08-20 07:48:18.000'"
            };
            var recordAffected = DatabaseAccess.ExecuteTransaction(transactionSql, true, true);
            var list = DatabaseAccess.GetDataReader("SELECT CreatedAt FROM Employee", CommandType.Text, null)
                .MapToList<string>()
                .ToList();

            Assert.AreEqual(recordAffected, 6);
            Assert.IsTrue(list.TrueForAll(e => e == "8/20/2019 7:48:18 AM"), "ExecuteTransaction didn't execute the update statement succesfully");
        }





        [Test]
        [Order(1)]
        public void Test_Object_With_Serialize_Attribute_Insert_Without_Error()
        {

            var obj = GetEmployeeSerialize(1);
            var recordCount = DatabaseAccess.Execute(obj, ActionType.Insert, null, SerializeObject, Json.SerializeToString, Csv.SerializeToString);
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
        [Order(2)]
        public void Test_Object_With_Serialized_Value_Pull_From_DataBase_Deserialize()
        {
            var mockObj = GetEmployeeSerialize(1);
            var objs = DatabaseAccess.Get<EmployeeSerialize>(DeSerializeObject, (s, type) => Json.Deserialize(s, type), (s, type) => Csv.Deserialize(s, type));
            var obj = objs.First();


            Assert.IsTrue(CompareEmployees(obj.EmployeeAsCsv, mockObj.EmployeeAsCsv));
            Assert.IsTrue(CompareEmployees(obj.EmployeeAsJson, mockObj.EmployeeAsJson));
            Assert.IsTrue(CompareEmployees(obj.EmployeeAsXml, mockObj.EmployeeAsXml));
            Assert.IsTrue(CompareEmployees(obj.EmployeeListAsCsv, mockObj.EmployeeListAsCsv));
            Assert.IsTrue(CompareEmployees(obj.EmployeeListAsJson, mockObj.EmployeeListAsJson));
            Assert.IsTrue(CompareEmployees(obj.EmployeeListAsXml, mockObj.EmployeeListAsXml));


        }







    }
}
#endif