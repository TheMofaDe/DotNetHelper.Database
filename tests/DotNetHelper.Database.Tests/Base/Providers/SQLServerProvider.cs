using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using DotNetHelper.Database.DataSource;

namespace DotNetHelper.Database.Tests.Base.Providers
{
	public class SqlServerProvider : IDatabaseProvider
	{
		public DbConnection Instance => GetOpenConnection();
		public string GetConnectionString() => "Data Source=localhost;Initial Catalog=master;Integrated Security=False;User Id=sa;Password=Password12!";
		public DbConnection GetClosedConnection()
		{
			var conn = new SqlConnection(GetConnectionString());
			if (conn.State != ConnectionState.Closed)
				throw new InvalidOperationException("should be closed!");
			return conn;
		}

		public DbConnection GetOpenConnection()
		{
			var conn = new SqlConnection(GetConnectionString());
			conn.Open();
			if (conn.State != ConnectionState.Open)
				throw new InvalidOperationException("should be open!");
			return conn;
		}

		public SqlServerProvider()
		{
		}
	}
}