using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using DotNetHelper.Database.DataSource;

namespace DotNetHelper.Database.Tests.Base.Providers
{
	public abstract class SqlServerDatabaseProvider : DatabaseProvider
	{
		public virtual DB<DbConnection> Database { get; }
		public override string GetConnectionString() => GetConnectionString("SqlServerConnectionString", "Data Source=172.17.0.3;Initial Catalog=tempdb;Integrated Security=True");

		public DbConnection GetOpenConnection(bool mars)
		{
			if (!mars)
				return GetOpenConnection();

			var scsb = Factory.CreateConnectionStringBuilder();
			scsb.ConnectionString = GetConnectionString();
			((dynamic)scsb).MultipleActiveResultSets = true;
			var conn = Factory.CreateConnection();
			conn.ConnectionString = scsb.ConnectionString;
			conn.Open();
			if (conn.State != ConnectionState.Open)
				throw new InvalidOperationException("should be open!");
			return conn;
		}

		protected SqlServerDatabaseProvider()
		{
			Database = new DB<DbConnection>(GetOpenConnection());
		}
	}
}
