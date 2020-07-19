using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using MySql.Data.MySqlClient;

namespace DotNetHelper.Database.Tests.Base.Providers
{
	public class MySqlProvider : IDatabaseProvider
	{

		public DbConnection Instance => GetOpenConnection();
		public string GetConnectionString() => GetMySqlConnectionString();

		string GetMySqlConnectionString()
		{
			var csBuilder = new MySqlConnectionStringBuilder
			{
				Port = 3306,
				Password = "Password12!",
				UserID = "root",
				Server = "localhost",
				Database = "sys"
			};
			return csBuilder.GetConnectionString(true);
		}

		public DbConnection GetClosedConnection()
		{
			var conn = new MySqlConnection(GetConnectionString());
			if (conn.State != ConnectionState.Closed)
				throw new InvalidOperationException("should be closed!");
			return conn;
		}

		public DbConnection GetOpenConnection()
		{
			var conn = new MySqlConnection(GetConnectionString());
			conn.Open();
			if (conn.State != ConnectionState.Open)
				throw new InvalidOperationException("should be open!");
			return conn;
		}

		public MySqlProvider()
		{
		}
	}
}
