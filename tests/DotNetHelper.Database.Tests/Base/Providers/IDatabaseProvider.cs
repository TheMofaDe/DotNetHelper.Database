using System;
using System.Data.Common;

namespace DotNetHelper.Database.Tests.Base.Providers
{
	public interface IDatabaseProvider
	{
		DbConnection Instance { get; }
		string GetConnectionString();
		DbConnection GetClosedConnection();
		DbConnection GetOpenConnection();
	}
}