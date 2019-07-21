#if SUPPORTSQLITE
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using CsvHelper.Configuration;
using DotNetHelper.Database.DataSource;
using DotNetHelper.Database.Tests.Helpers;
using DotNetHelper.Database.Tests.MockData;
using DotNetHelper.Database.Tests.Models;
using DotNetHelper.ObjectToSql.Enum;
using DotNetHelper.ObjectToSql.Model;
using DotNetHelper.Serialization.Abstractions.Interface;
using DotNetHelper.Serialization.Csv;
using DotNetHelper.Serialization.Json;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using NUnit.Framework;

using System;

namespace DotNetHelper.Database.Tests.SQLiteTest
{
    [Parallelizable(ParallelScope.None)]

    public class SqliteAttributeFixture
    {

        public SQLTable Table { get; } = new SQLTable(DataBaseType.Sqlite, typeof(EmployeeSerialize));
       
        public ISerializer Json { get; } = new DataSourceJson(new JsonSerializerSettings(){Formatting = Formatting.None});
        public ISerializer Csv { get; } = new DataSourceCsv(new Configuration());
        public DatabaseAccess<SqliteConnection, SqliteParameter> DatabaseAccess { get; set; } = new DatabaseAccess<SqliteConnection, SqliteParameter>(DataBaseType.Sqlite,TestHelper.SqliteConnectionString);


        public  string SerializeObject<T>( T toSerialize)
        {
            var xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }
        public object DeSerializeObject(string obj,Type type)
        {
            var ser = new XmlSerializer(type);

            using (var sr = new StringReader(obj))
            {
                return ser.Deserialize(sr);
            }
        }

        public EmployeeSerialize GetEmployeeSerialize(int index)
        {
             var employee = MockEmployee.Hashset.Take(index).First();
            return  new EmployeeSerialize()
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

            var assemblyResources = Assembly.GetExecutingAssembly().GetManifestResourceNames(); //example DotNetHelper.Database.Tests.Scripts.Sqlite.sql
            var sqls = assemblyResources.Where(str => str.Contains($"{DatabaseAccess.SqlSyntaxHelper.DataBaseType}")).ToList();
            sqls.ForEach(delegate (string s)
            {
                var result = DatabaseAccess.ExecuteNonQuery(TestHelper.GetEmbeddedResourceFile(s), CommandType.Text);
            });
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
            var result = DatabaseAccess.ExecuteNonQuery("DROP TABLE Employee", CommandType.Text);
            var result2 = DatabaseAccess.ExecuteNonQuery("DROP TABLE Employee2", CommandType.Text);
        }

        [Test]
        [Order(1)]
        public void Test_Object_With_Serialize_Attribute_Insert_Without_Error()
        {

            var obj = GetEmployeeSerialize(1);
            var recordCount = DatabaseAccess.Execute(obj,ActionType.Insert,null,SerializeObject,Json.SerializeToString,Csv.SerializeToString);
        }

        [Test]
        public void Test_Get_Throws_NullArgument_When_No_Deserializer_Func_Is_Not_Provided()
        {
            var obj = GetEmployeeSerialize(1);
            Assert.That(() => DatabaseAccess.Execute(obj,ActionType.Insert),
                Throws.Exception
                    .TypeOf<ArgumentNullException>());
        }

        [Test]
        [Order(2)]
        public void Test_Object_With_Serialized_Value_Pull_From_DataBase_Deserialize()
        {
            var mockObj = GetEmployeeSerialize(1);
            var objs = DatabaseAccess.Get<EmployeeSerialize>(DeSerializeObject,(s, type) => Json.Deserialize(s,type),(s, type) => Csv.Deserialize(s,type)  );
            var obj = objs.First();

  
            Assert.IsTrue(CompareEmployees(obj.EmployeeAsCsv, mockObj.EmployeeAsCsv));
            Assert.IsTrue(CompareEmployees(obj.EmployeeAsJson, mockObj.EmployeeAsJson));
            Assert.IsTrue(CompareEmployees(obj.EmployeeAsXml, mockObj.EmployeeAsXml));
            Assert.IsTrue(CompareEmployees(obj.EmployeeListAsCsv, mockObj.EmployeeListAsCsv));
            Assert.IsTrue(CompareEmployees(obj.EmployeeListAsJson, mockObj.EmployeeListAsJson));
            Assert.IsTrue(CompareEmployees(obj.EmployeeListAsXml, mockObj.EmployeeListAsXml));
         

        }

        public bool CompareEmployees(Employee one, Employee two)
        {
            var match =
                one.LastName == two.LastName
                && one.CreatedAt == two.CreatedAt
                && one.DateOfBirth == two.DateOfBirth
                && one.FavoriteColor == two.FavoriteColor
                && one.FirstName == two.FirstName;
            return match;
        }
        public bool CompareEmployees(List<Employee> one, List<Employee> two)
        {
            var index = 0;
             foreach(var employee in one)
            {
                if (!CompareEmployees(employee, two[index]))
                    return false;
            }

             return true;
        }

    }
}

#endif