using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
#if NET452
using Microsoft.Data.Sqlite;

namespace DotNetHelper.Database.Tests.Base.Providers
{
	public class SqliteProvider : IDatabaseProvider
	{
		public DbConnection Instance => GetOpenConnection();
		public string GetConnectionString() => "Data Source=:memory:";
		public DbConnection GetClosedConnection()
		{
			var conn = new SqliteConnection(GetConnectionString());
			if (conn.State != ConnectionState.Closed)
				throw new InvalidOperationException("should be closed!");
			return conn;
		}

		public DbConnection GetOpenConnection()
		{
			var conn = new SqliteConnection(GetConnectionString());
			conn.Open();
			if (conn.State != ConnectionState.Open)
				throw new InvalidOperationException("should be open!");
			return conn;
		}

		protected SqliteProvider()
		{
		}
		
	}

}
#endif