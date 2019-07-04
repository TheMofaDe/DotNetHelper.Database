using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DotNetHelper.Database.Tests
{
    public static class TestHelper
    {
        public static string SQLServerConnectionString { get; set; } = GetCS();

        public static string SqliteConnectionString { get; set; } = $"Data Source={Path.Combine(Environment.CurrentDirectory, "data.db")};";
        private static string GetCS()
        {
            if (Environment.MachineName == "DESKTOP-MEON7CL" || Environment.MachineName == "JMCNEAL-W8")
            {
                return $"Server=localhost;Initial Catalog=master;Integrated Security=True";
            }
            else
            {
                return $@"Server=(local)\SQL2014;Database=master;UID=sa;PWD=Password12!";
            }
        }

        public static string GetEmbeddedResourceFile(string filename)
        {
            var a = System.Reflection.Assembly.GetExecutingAssembly();
            using (var s = a.GetManifestResourceStream(filename))
            using (var r = new System.IO.StreamReader(s))
            {
                string result = r.ReadToEnd();
                return result;
            }
            return "";
        }


    }



}
