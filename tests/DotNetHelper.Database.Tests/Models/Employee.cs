using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DotNetHelper.Database.Tests.Models
{
    public class Employee
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdentityField { get; set; }

        [MaxLength(400)]
        public string FirstName { get; set; }

        [MaxLength(400)]
        public string LastName { get; set; }
        [NotMapped]
        public string FullName => FirstName + " " + LastName;

        public DateTime DateOfBirth { get; set; }
        [MaxLength(400)]
        public string FavoriteColor { get; set; }

        public DateTime CreatedAt { get; } = new DateTime();
        public Employee()
        {

        }



    }


    [Table("Employee2")]
    public class EmployeeWithKey
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdentityField { get; set; }
        [Key]
        public string PrimaryKey { get; set; }
        [MaxLength(400)]
        public string FirstName { get; set; }

        [MaxLength(400)]
        public string LastName { get; set; }
        [NotMapped]
        public string FullName => FirstName + " " + LastName;

        public DateTime DateOfBirth { get; set; }
        [MaxLength(400)]
        public string FavoriteColor { get; set; }

        public DateTime CreatedAt { get; } = new DateTime();

        public EmployeeWithKey()
        {

        }



    }




}
