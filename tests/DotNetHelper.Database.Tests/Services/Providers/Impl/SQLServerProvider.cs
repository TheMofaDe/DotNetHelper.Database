using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace DotNetHelper.Database.Tests.Services.Providers.Impl
{
	public class SqlServerProvider : IDatabaseProvider
	{

#if IsRunningOnAppVeyor
		public string GetConnectionString() =>  @"Server=(local)\SQL2017;Database=master;User ID=sa;Password=Password12!";
#else
		public string GetConnectionString() => "Data Source=localhost;Initial Catalog=master;Integrated Security=False;User Id=sa;Password=Password12!";
#endif
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