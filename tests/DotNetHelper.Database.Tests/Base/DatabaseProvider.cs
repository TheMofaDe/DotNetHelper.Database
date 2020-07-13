using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using DotNetHelper.Database.DataSource;

namespace DotNetHelper.Database.Tests.Base
{
	public abstract class DatabaseProvider
	{

		
		public abstract DbProviderFactory Factory { get; }

		public virtual void Dispose() { }
		public abstract string GetConnectionString();

		protected string GetConnectionString(string name, string defaultConnectionString) =>
			Environment.GetEnvironmentVariable(name) ?? defaultConnectionString;

		public DbConnection GetOpenConnection()
		{
			var conn = Factory.CreateConnection();
			conn.ConnectionString = GetConnectionString();
			conn.Open();
			if (conn.State != ConnectionState.Open)
				throw new InvalidOperationException("should be open!");
			return conn;
		}

		public DbConnection GetClosedConnection()
		{
			var conn = Factory.CreateConnection();
			conn.ConnectionString = GetConnectionString();
			if (conn.State != ConnectionState.Closed)
				throw new InvalidOperationException("should be closed!");
			return conn;
		}



		public DbParameter CreateRawParameter(string name, object value)
		{
			var p = Factory.CreateParameter();
			p.ParameterName = name;
			p.Value = value ?? DBNull.Value;
			return p;
		}

	}

	public static class DatabaseProvider<TProvider> where TProvider : DatabaseProvider
	{
		public static TProvider Instance { get; } = Activator.CreateInstance<TProvider>();
	}

}
