using System;
using System.Data;
using System.Data.Common;
using System.IO;
using Microsoft.Data.Sqlite;

namespace DotNetHelper.Database.Tests.Services.Providers.Impl
{
	public class SqliteProvider : IDatabaseProvider
	{

		public string GetConnectionString() => $"Data Source={Path.Combine(Directory.GetCurrentDirectory(), "data.db")};";
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

		public SqliteProvider()
		{
		}

	}

}
