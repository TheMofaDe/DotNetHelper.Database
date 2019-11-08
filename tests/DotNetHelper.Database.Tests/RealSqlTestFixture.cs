using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetHelper.Database.DataSource;
using DotNetHelper.Database.Extension;
using DotNetHelper.Database.Interface;
using DotNetHelper.Database.Tests.Helpers;
using DotNetHelper.Database.Tests.MockData;
using DotNetHelper.Database.Tests.Models;
using DotNetHelper.ObjectToSql.Enum;
using DotNetHelper.ObjectToSql.Model;
#if SUPPORTSQLITE
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
#endif
using NUnit.Framework;


namespace DotNetHelper.Database.Tests
{

    [TestFixtureSource("TestObjects")]
    public class DatabaseAccessShould : BaseTest
    {
        private static List<IDatabaseAccess> TestObjects { get; } = GetTestObjects();

        private static bool HasConnectionIssue { get; set; }

        private static List<IDatabaseAccess> GetTestObjects()
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var list = new List<IDatabaseAccess>() { };

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


        public IDatabaseAccess DatabaseAccess { get; }


        public DatabaseAccessShould(IDatabaseAccess databaseAccess) : base(databaseAccess)
        {
            DatabaseAccess = databaseAccess;
        }




        [SetUp]
        public void Setup()
        {
            //if(DatabaseAccess.DatabaseType == DataBaseType.Sqlite)
            //{
            //    if(File.Exists(TestHelper.LocalDatabaseFile))
            //    File.Delete(TestHelper.LocalDatabaseFile);
            //}
            var sqls = GetDBScripts(ScriptType.Initialize);
            sqls.ForEach(delegate (string s)
            {
                var result = DatabaseAccess.ExecuteNonQuery(TestHelper.GetEmbeddedResourceFile(s), CommandType.Text);
            });
        }


        [TearDown]
        public void Teardown()
        {
            CleanUp();
        }



        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {

        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
            CleanUp();
        }





        [Test]

        public void Insert_All_Records_In_List()
        {
            var list = MockEmployee.Hashset;
            var recordsAffected = DatabaseAccess.Execute(list, ActionType.Insert);
            Assert.That(recordsAffected == MockEmployee.Hashset.Count, "");

        }



    }
}