using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using CsvHelper.Configuration;
using DotNetHelper.Database.Interface;
using DotNetHelper.Database.Tests.Helpers;
using DotNetHelper.Database.Tests.MockData;
using DotNetHelper.Database.Tests.Models;
using DotNetHelper.Database.Tests.Services;
using DotNetHelper.Serialization.Abstractions.Interface;
using DotNetHelper.Serialization.Csv;
using DotNetHelper.Serialization.Json;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DotNetHelper.Database.Tests
{


    public enum ScriptType
    {
        Initialize
        ,CleanUp
    }
    public class BaseTest
    {
        private readonly IDatabaseAccess _databaseAccess;

        public ISerializer Json { get; } = new DataSourceJson(new JsonSerializerSettings() { Formatting = Formatting.None });
        public ISerializer Csv { get; } = new DataSourceCsv(new Configuration());
        public ISerializer Xml { get; } = new DataSourceXML();



        public BaseTest(IDatabaseAccess databaseAccess)
        {
            _databaseAccess = databaseAccess;
        }

  

        public void EnsureExpectedExceptionIsThrown<T>(Action action) where T : Exception
        {
            Assert.That(action.Invoke, Throws.Exception.TypeOf<T>());
        }


        public List<string> GetDBScripts(ScriptType scriptType) 
        {
            var assemblyResources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var keyword = scriptType == ScriptType.CleanUp ? "cleanup" : "init";
            var sqls = assemblyResources.Where(str => str.StartsWith($"DotNetHelper.Database.Tests.Scripts.{_databaseAccess.DatabaseType}_{keyword}", StringComparison.OrdinalIgnoreCase)).ToList();
            return sqls;
        }

        public void CleanUp()
        {
            var sqls = GetDBScripts(ScriptType.CleanUp);
            sqls.ForEach(delegate (string s)
            {
                try
                {
                    var result = _databaseAccess.ExecuteNonQuery(TestHelper.GetEmbeddedResourceFile(s), CommandType.Text);
                }
                catch (Exception w)
                {

                }
            });
        }








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
