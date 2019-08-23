using System;
using System.IO;
#if SUPPORTSQLITE
using MySql.Data.MySqlClient;
#endif

namespace DotNetHelper.Database.Tests.Helpers
{
    public static class TestHelper
    {


        public static string GetCurrentDirectory
        {
            get
            {
#if NET452
                if (Environment.MachineName == "DESKTOP-MEON7CL")
                {
                    return $@"C:\Temp";
                }
                else
                {
                    return $"{Environment.CurrentDirectory}";
                }
#else
                return $"{Environment.CurrentDirectory}";
#endif
            }
        }

        public static string LocalDatabaseFile { get; } = $"{Path.Combine(GetCurrentDirectory, "data.db")}";
        public static string SqliteConnectionString { get; set; } = $"Data Source={LocalDatabaseFile};";
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
        public static string SQLServerConnectionString { get; set; } = GetCS();


        private static string GetMySqlCS()
        {
            if (Environment.MachineName == "DESKTOP-MEON7CL" || Environment.MachineName == "JMCNEAL-W8")
            {
#if SUPPORTSQLITE
                var csBuilder = new MySqlConnectionStringBuilder();
                csBuilder.Port = 3306;
                csBuilder.Password = "password";
                csBuilder.UserID = "test";
                csBuilder.Server = "172.19.27.154";//"172.17.0.2";
                csBuilder.Database = "sys";
                return csBuilder.GetConnectionString(true);
#endif
                return null;
            }
            else
            {
                var csBuilder = new MySqlConnectionStringBuilder();
                csBuilder.Port = 3306;
                csBuilder.Password = "Password12!";
                csBuilder.UserID = "root";
                csBuilder.Server = "localhost";//"172.17.0.2";
                csBuilder.Database = "sys";
                return csBuilder.GetConnectionString(true);
            }
        }
        public static string MySqlConnectionString { get; set; } = GetMySqlCS();



        public static string GetEmbeddedResourceFile(string filename)
        {
            var a = System.Reflection.Assembly.GetExecutingAssembly();
            using (var s = a.GetManifestResourceStream(filename))
            using (var r = new StreamReader(s))
            {
                string result = r.ReadToEnd();
                return result;
            }
        }


    }



}
