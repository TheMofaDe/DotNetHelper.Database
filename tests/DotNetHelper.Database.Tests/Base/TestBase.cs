using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using DotNetHelper.Database.DataSource;
using DotNetHelper.Database.Tests.Base.Providers;
using DotNetHelper.Database.Tests.MockData;
using DotNetHelper.Database.Tests.Models;
using DotNetHelper.Database.Tests.Services;
using DotNetHelper.Serialization.Abstractions.Interface;
using DotNetHelper.Serialization.Csv;
using DotNetHelper.Serialization.Json;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DotNetHelper.Database.Tests.Base
{
	public abstract class TestBase<TProvider> where TProvider : IDatabaseProvider
	{

		public ISerializer Json { get; } = new DataSourceJson(new JsonSerializerSettings() { Formatting = Formatting.None });
		public DataSourceCsv Csv { get; } = new DataSourceCsv();
		public ISerializer Xml { get; } = new DataSourceXML();

		protected DbConnection GetOpenConnection() => Provider.GetOpenConnection();
		protected DbConnection GetClosedConnection() => Provider.GetClosedConnection();
		protected DbConnection _connection;
		protected DbConnection Connection => _connection ??= Provider.GetOpenConnection();

		public static TProvider Provider { get; } = DatabaseProvider<TProvider>.Instance;

	
		protected static CultureInfo ActiveCulture
		{
			get { return Thread.CurrentThread.CurrentCulture; }
			set { Thread.CurrentThread.CurrentCulture = value; }
		}

		static TestBase()
		{
			Console.WriteLine("Using Provider: {0}", Provider.GetType().FullName);
			Console.WriteLine("Using Connectionstring: {0}", Provider.GetConnectionString());
			Console.WriteLine(".NET: " + Environment.Version);

		}

		public virtual void EnsureExpectedExceptionIsThrown<T>(Action action) where T : Exception
		{
			Assert.That(action.Invoke, Throws.Exception.TypeOf<T>());
		}





		public EmployeeSerialize GetEmployeeSerialize(int index)
		{
			var employee = MockEmployee.Hashset.Take(index).First();
			return new EmployeeSerialize()
			{
				EmployeeAsCsv = employee
				,
				EmployeeAsJson = employee
				,
				EmployeeAsXml = employee
				,
				EmployeeListAsCsv = new List<Employee>() { employee }
				,
				EmployeeListAsJson = new List<Employee>() { employee }
				,
				EmployeeListAsXml = new List<Employee>() { employee }
			};
		}


		public bool CompareEmployees(List<Employee> one, List<Employee> two)
		{
			const int index = 0;
			foreach (var employee in one)
			{
				if (!CompareEmployees(employee, two[index]))
					return false;
			}

			return true;
		}


		public bool CompareEmployees(Employee one, Employee two)
		{
			var match =
				one.LastName == two.LastName
				&& one.CreatedAt == two.CreatedAt
				&& one.DateOfBirth == two.DateOfBirth
				&& one.FavoriteColor == two.FavoriteColor
				&& one.FirstName == two.FirstName
				&& one.IdentityField == two.IdentityField;
	

			return match;
		}




	}


}
