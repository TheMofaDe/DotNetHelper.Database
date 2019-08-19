using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using CsvHelper.Configuration;
using DotNetHelper.Database.Tests.MockData;
using DotNetHelper.Database.Tests.Models;
using DotNetHelper.Serialization.Abstractions.Interface;
using DotNetHelper.Serialization.Csv;
using DotNetHelper.Serialization.Json;
using Newtonsoft.Json;

namespace DotNetHelper.Database.Tests
{
    public class BaseTest
    {
        public string WorkingDirectory { get; }

        public BaseTest()
        {
            WorkingDirectory = $"{Environment.CurrentDirectory}";
        }

        public ISerializer Json { get; } = new DataSourceJson(new JsonSerializerSettings() { Formatting = Formatting.None });
        public ISerializer Csv { get; } = new DataSourceCsv(new Configuration());


        public string SerializeObject<T>(T toSerialize)
        {
            var xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }
        public object DeSerializeObject(string obj, Type type)
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
