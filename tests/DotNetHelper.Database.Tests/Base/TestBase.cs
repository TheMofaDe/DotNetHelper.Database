using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Threading;
using DotNetHelper.Database.DataSource;

namespace DotNetHelper.Database.Tests.Base
{
	public abstract class TestBase<TProvider> : IDisposable where TProvider : DatabaseProvider
	{

		protected DbConnection GetOpenConnection() => Provider.GetOpenConnection();
		protected DbConnection GetClosedConnection() => Provider.GetClosedConnection();
		protected DbConnection _connection;
		protected DbConnection Connection => _connection ??= Provider.GetOpenConnection();

		public TProvider Provider { get; } = DatabaseProvider<TProvider>.Instance;

	
		protected static CultureInfo ActiveCulture
		{
			get { return Thread.CurrentThread.CurrentCulture; }
			set { Thread.CurrentThread.CurrentCulture = value; }
		}

		static TestBase()
		{
		
			var provider = DatabaseProvider<TProvider>.Instance;
			Console.WriteLine("Using Connectionstring: {0}", provider.GetConnectionString());
			var factory = provider.Factory;
			Console.WriteLine("Using Provider: {0}", factory.GetType().FullName);
			Console.WriteLine(".NET: " + Environment.Version);

		}



		public virtual void Dispose()
		{
			_connection?.Dispose();
			_connection = null;
			Provider?.Dispose();
		}
	}


}
