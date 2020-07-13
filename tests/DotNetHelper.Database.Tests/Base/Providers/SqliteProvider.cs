using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Microsoft.Data.Sqlite;

namespace DotNetHelper.Database.Tests.Base.Providers
{
	public class SqliteProvider : DatabaseProvider
	{
		public override DbProviderFactory Factory => SqliteFactory.Instance;
		public override string GetConnectionString() => "Data Source=:memory:";
	}
}
