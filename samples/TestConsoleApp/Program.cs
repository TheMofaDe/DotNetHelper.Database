using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using DotNetHelper.Database.DataSource;
using DotNetHelper.ObjectToSql.Attribute;
using DotNetHelper.ObjectToSql.Enum;

namespace TestConsoleApp
{

    public class Employee
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Marks this property as an identity field 
        [Key]  // marks this property as a primary key
        public int IdentityField { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        [NotMapped] // This property will be ignore when performing database actions
        public string FullName => FirstName + " " + LastName;

        [SqlColumn(MapTo = "DOB")]  // This property is actually name DOB in the database
        public DateTime DateOfBirth { get; set; }

        public string FavoriteColor { get; set; }
        public DateTime CreatedAt { get; } = new DateTime();
        
    }
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            dynamic dynamicEmployee = new ExpandoObject(); // A DYNAMIC EMPLOYEE
                    dynamicEmployee.FirstName = "Joe Sister";
                    dynamicEmployee.LastName = "Dynamic";
                    dynamicEmployee.DOB = DateTime.Today.AddDays(-2);

            var employee = new Employee() {DateOfBirth = DateTime.Today, FavoriteColor = "Blue", FirstName = "Joe" , LastName = "Generic"}; // A GENERIC EMPLOYEE
            var anonymousEmployee = new {FirstName = "Joe Brother", DOB = DateTime.Today.AddDays(-1) , LastName = "Anonymous"}; // A ANONYMOUS EMPLOYEE

            var dbAccess = new DatabaseAccess<SqlConnection, SqlParameter>(DataBaseType.SqlServer, "Server=localhost;Initial Catalog=master;Integrated Security=True");

            // Lets add our 3 employees to the database note 
            var recordAffected = dbAccess.Execute(employee, ActionType.Insert); // ActionType is a enum of Insert,Update,Delete,Upsert
            recordAffected  += dbAccess.Execute(anonymousEmployee, ActionType.Insert,"Employee");
            recordAffected  += dbAccess.Execute<ExpandoObject>(dynamicEmployee, ActionType.Insert,"Employee");


        }

        // This example assumes a reference to System.Data.Common.
        static DataTable GetProviderFactoryClasses()
        {
            // Retrieve the installed providers and factories.
            DataTable table = DbProviderFactories.GetFactoryClasses();

            // Display each row and column value.
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in table.Columns)
                {
                    Console.WriteLine(row[column]);
                }
            }
            return table;
        }
    }
}
