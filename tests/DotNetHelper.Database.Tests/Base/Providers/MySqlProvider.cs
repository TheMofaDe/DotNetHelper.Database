using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace DotNetHelper.Database.Tests.Base.Providers
{
	public sealed class MySqlProvider : DatabaseProvider
	{
		public override DbProviderFactory Factory => MySql.Data.MySqlClient.MySqlClientFactory.Instance;
		public override string GetConnectionString() =>
			GetConnectionString("MySqlConnectionString", "Server=localhost;Database=tests;Uid=test;Pwd=pass;");

		public DbConnection GetMySqlConnection(bool open = true,
			bool convertZeroDatetime = false, bool allowZeroDatetime = false)
		{
			string cs = GetConnectionString();
			var csb = Factory.CreateConnectionStringBuilder();
			csb.ConnectionString = cs;
			((dynamic)csb).AllowZeroDateTime = allowZeroDatetime;
			((dynamic)csb).ConvertZeroDateTime = convertZeroDatetime;
			var conn = Factory.CreateConnection();
			conn.ConnectionString = csb.ConnectionString;
			if (open)
				conn.Open();
			return conn;
		}
	}
}
