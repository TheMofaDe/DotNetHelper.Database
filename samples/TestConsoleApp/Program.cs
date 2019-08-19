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

       // [NotMapped] // This property will be ignore when performing database actions
       // public string FullName => FirstName + " " + LastName;

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


        }

    }
}
