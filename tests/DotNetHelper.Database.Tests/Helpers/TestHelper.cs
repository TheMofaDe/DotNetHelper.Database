using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using DotNetHelper.Database.DataSource;
using DotNetHelper.Database.Extension;
using DotNetHelper.ObjectToSql.Enum;
using Microsoft.Data.Sqlite;
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
            var csBuilder = new SqlConnectionStringBuilder
            {
                Password = "Password12!",
                UserID = "sa",
                DataSource = "(local)\\SQL2016",
                InitialCatalog = "master",
                IntegratedSecurity = false
            };
            if (Environment.MachineName == "DESKTOP-MEON7CL" || Environment.MachineName == "JMCNEAL-W8")
            {
                csBuilder.DataSource = "127.0.0.1"; // localhost works && 127.0.0.1 

                var useLocalInstance = false;
                if (useLocalInstance)
                {
                    csBuilder.Password = string.Empty;
                    csBuilder.UserID = string.Empty;
                    csBuilder.DataSource = "localhost";
                    csBuilder.IntegratedSecurity = true;
                }

            }
            else
            {
                return @"Server=(local)\SQL2017;Database=master;User ID=sa;Password=Password12!";
            }
            return csBuilder.ToString();
        }
        public static string SQLServerConnectionString { get; set; } = GetCS();


        private static string GetMySqlCS()
        {
#if SUPPORTSQLITE
            var csBuilder = new MySqlConnectionStringBuilder();
            csBuilder.Port = 3306;
            csBuilder.Password = "Password12!";
            csBuilder.UserID = "root";
            csBuilder.Server = "localhost";
            csBuilder.Database = "sys";
            return csBuilder.GetConnectionString(true);
#endif
            return null;
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



        private static List<DatabaseAccess<DbConnection>> GetDatabaseAccesses()
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var list = new List<DatabaseAccess<DbConnection>>() { };

            list.Add(new DatabaseAccess<SqlConnection>(TestHelper.SQLServerConnectionString));
            list.Add(new DatabaseAccess<SqlConnection>(DataBaseType.SqlServer, TestHelper.SQLServerConnectionString));
            list.Add(new DatabaseAccess<SqlConnection>(DataBaseType.SqlServer, TestHelper.SQLServerConnectionString, TimeSpan.FromSeconds(35)));
            list.Add(new DatabaseAccess<SqlConnection>(DataBaseType.SqlServer, TestHelper.SQLServerConnectionString, TimeSpan.FromSeconds(40)));
            list.Add(new SqlConnection(TestHelper.SQLServerConnectionString) { ConnectionString = TestHelper.SQLServerConnectionString }.DatabaseAccess());


#if SUPPORTSQLITE
            list.Add(new DatabaseAccess<SqliteConnection>(DataBaseType.Sqlite, TestHelper.SqliteConnectionString));
            list.Add(new DatabaseAccess<MySqlConnection>(DataBaseType.MySql, TestHelper.MySqlConnectionString, TimeSpan.FromSeconds(5)));
            list.Add(new SqliteConnection(TestHelper.SqliteConnectionString).DatabaseAccess());
            list.Add(new MySqlConnection(TestHelper.MySqlConnectionString).DatabaseAccess());
#endif


#if SUPPORTDBFACTORIES
            list.Add(new DatabaseAccessFactory(DataBaseType.SqlServer, TestHelper.SQLServerConnectionString));
            list.Add(new DatabaseAccessFactory(DataBaseType.SqlServer, TestHelper.SQLServerConnectionString, TimeSpan.FromSeconds(40)));
#endif
            return list;
        }
    }



}
