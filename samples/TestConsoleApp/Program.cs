
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DotNetHelper.Database.Extension;
using DotNetHelper.ObjectToSql.Attribute;
using DotNetHelper.ObjectToSql.Enum;

namespace TestConsoleApp
{

	public class Employee
	{
		[SqlColumn(SetIsIdentityKey = true)] // Specify that this column is an Identity field
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // or you can use data annotation attribute for same behavior
		public int Id { get; set; }
		public string Name { get; set; }

		[SqlColumn(SetIsReadOnly = true)] // If true this property will never be included when creating insert sql. This is useful for senarios where you want to use the database default value
		[NotMapped] // or you can use data annotation attribute for same behavior
		public DateTime CreatedAt { get; set; }
	}

	class Program
	{

		static async Task Main(string[] args)
		{
			using (var db = new SqlConnection("Data Source=localhost;Initial Catalog=master;Integrated Security=False;User Id=sa;Password=Password12!").DB())
			{

				var dropTableSql = $@"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Employee]') AND type in (N'U'))
					DROP TABLE [dbo].[Employee]";

				var createTableSql = $@"CREATE TABLE [Employee](
						[Id] [int] NOT NULL IDENTITY (1,1) PRIMARY KEY,
						[Name] [varchar](400) NOT NULL,
						[CreatedAt] DateTime NOT NULL DEFAULT GETDATE()
					);";

				var allowRollbackOnFail = true;
				var recordAffected = await db.ExecuteTransactionAsync(new List<string>(){dropTableSql,createTableSql}, allowRollbackOnFail);
			

				var employee = new Employee() {Name = "Generic Name"};
				var outputEmployee = await db.ExecuteAndGetOutputAsync(employee, ActionType.Insert,emp => emp.Id, emp => emp.CreatedAt);

				// Set the identity and database default value back to my original unchanged object
				employee.Id = outputEmployee.Id;
				employee.CreatedAt = outputEmployee.CreatedAt;


				Console.WriteLine(employee.Id); // 1
				Console.WriteLine(employee.CreatedAt); // 7/24/2020 7:55:45 PM
				Console.WriteLine(employee.Name); // Generic Name
			}

			Console.ReadLine();
		}

	}
}
