using System.Data.Common;

namespace DotNetHelper.Database.Tests.Services.Providers
{
	public interface IDatabaseProvider
	{
		string GetConnectionString();
		DbConnection GetClosedConnection();
		DbConnection GetOpenConnection();
	}
}