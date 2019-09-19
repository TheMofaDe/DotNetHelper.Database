using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using DotNetHelper.Database.DataSource;
using DotNetHelper.Database.Extension;
using DotNetHelper.Database.Interface;
using DotNetHelper.Database.Tests.Helpers;
using DotNetHelper.ObjectToSql.Enum;
using NUnit.Framework;
#if SUPPORTSQLITE
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
#endif

namespace DotNetHelper.Database.Tests
{


    [TestFixtureSource("TestObjects")]
    public class DBParameterTextFixture : BaseTest
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
            list.Add(new SqlConnection(TestHelper.SQLServerConnectionString).DatabaseAccess());

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


        public DBParameterTextFixture(IDatabaseAccess databaseAccess) : base(databaseAccess)
        {
            DatabaseAccess = databaseAccess;
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

        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {

        }


        [Test]
        public void TestAnonyousObjectToParameterHasCorrectValues()
        {
            var parameters = new { Name = "Bro", Id = 2 };
            var dbParams = DatabaseAccess.GetNewParameter(parameters);

            Assert.AreEqual(dbParams.Count, 2);
            Assert.AreEqual(dbParams.First().ParameterName, "@Id");
            Assert.AreEqual(dbParams.Last().ParameterName, "@Name");

            Assert.AreEqual(dbParams.First().Value, 2);
            Assert.AreEqual(dbParams.Last().Value, "Bro");

        }



        [Test]
        public void TestDynamicObjectToParameterHasCorrectValues()
        {
            dynamic parameters = new ExpandoObject();
            parameters.Id = 2;
            parameters.Name = "Bro";


            List<DbParameter> dbParams = DatabaseAccess.GetNewParameter(parameters);

            Assert.AreEqual(dbParams.Count, 2);
            Assert.AreEqual(dbParams.First().ParameterName, "@Id");
            Assert.AreEqual(dbParams.Last().ParameterName, "@Name");

            Assert.AreEqual(dbParams.First().Value, 2);
            Assert.AreEqual(dbParams.Last().Value, "Bro");

        }

    }
}
